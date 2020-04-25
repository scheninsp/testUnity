using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Text_Script : MonoBehaviour
{

    public Text textUI;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 fix_pos = new Vector3(0, 4, 2);
        Vector3 textUIpos = Camera.main.WorldToScreenPoint(this.transform.position + fix_pos);
        textUI.transform.position = textUIpos;
    }
}
