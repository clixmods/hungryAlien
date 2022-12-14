using System;
using UnityEngine;

namespace Player
{
    public class AbsorberColor : MonoBehaviour
    {
        public static Action<Color> CallbackColorChange;
        private float _colorLerpMultiplier = 5;
        private Light _light;
        public Color color;
        
        // Materials
        private static readonly int EmissiveColorID = Shader.PropertyToID("_EmissiveColor");
        [SerializeField] private MeshRenderer meshLight;
        private MaterialPropertyBlock _materialProperty;
        [SerializeField] private float emissiveMultiplier = 6f;

        private void Awake()
        {
            _materialProperty = new MaterialPropertyBlock();
            CallbackColorChange = null;
            
            
        }

        public void SetColor(Color color)
        {
            if (!this.color.Equals(color))
            {
                this.color = color;
                CallbackColorChange?.Invoke(color);
            }
            
        }

        private void Start()
        {
            _light = GetComponent<Light>();
        }

        private void Update()
        {
            var generatedColor = Color.Lerp(_light.color, color, _colorLerpMultiplier * Time.deltaTime);
            _light.color =  generatedColor;
            // Get the current value of the material properties in the renderer.
            meshLight.GetPropertyBlock(_materialProperty);
            // Assign our new value.
            _materialProperty.SetColor(EmissiveColorID, generatedColor * emissiveMultiplier);
            // Apply the edited values to the renderer.
            meshLight.SetPropertyBlock(_materialProperty, 0);
        }
    }
}