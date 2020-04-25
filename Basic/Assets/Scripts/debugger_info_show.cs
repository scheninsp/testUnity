using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class debugger_info_show : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Text>().text = "Touch Pressure Unsupported";
        if (Input.touchPressureSupported){
            GetComponent<Text>().text = "Touch Pressure Supported";
        }


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
