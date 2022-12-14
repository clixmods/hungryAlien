using UnityEngine;

namespace Level
{
    public class MaterialPropertyBlockManager : MonoBehaviour
    {
        private MaterialPropertyBlock[] _propBlocks;
        private MeshRenderer _meshRenderer;
        private static readonly int Amount = Shader.PropertyToID("_Amount");

        MaterialPropertyBlock[] PropBlocks
        {
            get
            {
                if (_propBlocks != null)
                {
                    return _propBlocks;
                }
                _propBlocks = new MaterialPropertyBlock[_meshRenderer.sharedMaterials.Length];
                for (int i = 0; i < _propBlocks.Length; i++)
                {
                    _propBlocks[i] = new MaterialPropertyBlock();
                }

                return _propBlocks;
            }
        }

        public void Init(MeshRenderer meshRenderer)
        {
            _meshRenderer = meshRenderer;
            _propBlocks = new MaterialPropertyBlock[meshRenderer.sharedMaterials.Length];
            for (int i = 0; i < _propBlocks.Length; i++)
            {
                _propBlocks[i] = new MaterialPropertyBlock();
            }
        }

        public void SetDissolve(float amount)
        {
            for (int i = 0; i < PropBlocks.Length; i++)
            {
                // Get the current value of the material properties in the renderer.
                _meshRenderer.GetPropertyBlock(PropBlocks[i],i);
                // Assign our new value.
                var value = Mathf.Clamp(amount, 0, 1);
                PropBlocks[i].SetFloat(Amount, value);
                // Apply the edited values to the renderer.
                _meshRenderer.SetPropertyBlock(PropBlocks[i], i );
            }
        }
        
  
        public void AddDissolve(float amount)
        {
            for (int i = 0; i < PropBlocks.Length; i++)
            {
                // Get the current value of the material properties in the renderer.
                _meshRenderer.GetPropertyBlock(PropBlocks[i],i);
                // Assign our new value.
                float currentAmount = PropBlocks[i].GetFloat(Amount);
                // Assign our new value.
                var value = Mathf.Clamp(currentAmount + amount, 0, 1);
                PropBlocks[i].SetFloat(Amount, value);
                // Apply the edited values to the renderer.
                _meshRenderer.SetPropertyBlock(PropBlocks[i], i );
            }
        }

        public bool FloatsIsLessThan(float value)
        {
            bool boolean = false;
            for (int index = 0; index < _meshRenderer.materials.Length; index++)
            {
                // Get the current value of the material properties in the renderer.
                _meshRenderer.GetPropertyBlock(PropBlocks[index],index);
                if (PropBlocks[index].GetFloat(Amount) < value)
                {
                    boolean = true;
                }
                _meshRenderer.SetPropertyBlock(PropBlocks[index], index);
            }

            return boolean;
        }
        
    }
}