using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Util;
    
    public GameObject InputHelperPrefab;

    public static void CreateInputHelper(string text, Transform transformToTarget , out UIInputHelper component, 
        float displayTime = -1)
    {
        component = null;
        if (transformToTarget == null) return;
        
        var inputHelperObject =
            Instantiate(Util.InputHelperPrefab, Vector3.zero, Quaternion.identity, Util.transform);

        component = inputHelperObject.GetComponent<UIInputHelper>();
            
        component.Setup(text, transformToTarget , displayTime);
        
    }
    
    // Start is called before the first frame update
    void Start()
    {
        Util = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
