using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockImage : MonoBehaviour
{
    private Shape currentLockedTarget;

    private Vector2 DefaultPosition = new Vector2(1200f, -400f);

    private Camera mainCamera = null;
    private RectTransform layer2PassiveCanvas = null;
    
    public void Initialize(Camera main, RectTransform canvas)
    {
        mainCamera = main;
        layer2PassiveCanvas = canvas;
    }

    void Update()
    {
        Vector3 targetPosition = currentLockedTarget.transform.position;
        //Vector3 targetPosition = new Vector3(2f, 0.8f, 2f);

        
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(targetPosition);

        Vector2 localPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            layer2PassiveCanvas, screenPosition, null, out localPosition);  //overlay mode

        
        //GetComponent<RectTransform>().anchoredPosition = DefaultPosition;

        GetComponent<RectTransform>().localPosition = localPosition;
    }

    public void setTargetLocked(Shape targetLocked)
    {
        currentLockedTarget = targetLocked;
    }
}
