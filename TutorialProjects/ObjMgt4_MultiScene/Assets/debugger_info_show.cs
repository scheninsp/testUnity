using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class debugger_info_show : MonoBehaviour
{
    void Start()
    {
        GetComponent<Text>().text = "Scene Level 1 Unloaded";
        Scene sceneTmp = SceneManager.GetSceneByName("Level 1");
        if (sceneTmp.isLoaded)
        {
            GetComponent<Text>().text = "Scene Level 1 Loaded";
        }
        else
        {
            GetComponent<Text>().text = sceneTmp.name;
        }
    }

    // Update is called once per frame
    void Update()
    {
        /*
        Scene sceneTmp = SceneManager.GetSceneByName("Level 1");
        if (sceneTmp.isLoaded)
        {
            GetComponent<Text>().text = "Scene Level 1 Loaded";
        }
        else
        {
            GetComponent<Text>().text = sceneTmp.name;
        }*/
    }
}
