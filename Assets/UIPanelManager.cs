using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIPanelManager : MonoBehaviour
{
    [SerializeField] private GameObject buttonToSelect;
    // Start is called before the first frame update
    void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(buttonToSelect);
    }

}
