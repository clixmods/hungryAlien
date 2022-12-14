using UnityEngine;
using DG.Tweening;

public class ShipViewModel : MonoBehaviour
{
    [SerializeField] private float speedRotation = 5;
    [SerializeField] private GameObject viewModel;
    [SerializeField] private GameObject viewModelMesh;

    private ShipController _shipController;
     // Start is called before the first frame update
    void Start()
    {
        _shipController = GetComponent<ShipController>();
    }

    // Update is called once per frame
    void Update()
    {
        var euler = viewModelMesh.transform.localRotation.eulerAngles;
        euler.y += Time.deltaTime * speedRotation * 100;
        viewModelMesh.transform.localRotation = Quaternion.Euler( euler);
        viewModel.transform.DORotate(new Vector3(_shipController.Direction.z, 0, -_shipController.Direction.x), 0.1f) ;
    }
}
