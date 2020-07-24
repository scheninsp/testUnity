using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Skill2ButtonBehavior : MonoBehaviour, IPointerClickHandler
{

    public Text debugText;

    public PlayerBehavior player;

    private string debugInfoTextSkill2DoubleClick = "Dash";

    float t1;
    float t2;

    public void OnPointerClick(PointerEventData eventData)
    {
        t2 = Time.realtimeSinceStartup;
        if (t2 - t1 < 0.2)
        {
            debugText.text = debugInfoTextSkill2DoubleClick;
            player.playerDash();
        }
        t1 = t2;
    }
}
