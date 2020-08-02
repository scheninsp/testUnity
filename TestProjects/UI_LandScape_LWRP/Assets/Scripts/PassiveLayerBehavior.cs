using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveLayerBehavior : MonoBehaviour
{
    [SerializeField]
    LockImage lockImageClass;

    private LockImage lockImageInst = null;

    private Camera mainCamera;
    private RectTransform layer2PassiveCanvas;

    private void Start()
    {
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        layer2PassiveCanvas = GetComponent<RectTransform>();
    }

    public void generateLockImage(Shape targetLocked)
    {

        if (lockImageInst == null)
        {
            lockImageInst = Instantiate(lockImageClass) as LockImage ;
            lockImageInst.transform.SetParent(this.gameObject.transform, false);
            lockImageInst.Initialize(mainCamera, layer2PassiveCanvas);
        }

        lockImageInst.gameObject.SetActive(true);
        lockImageInst.setTargetLocked(targetLocked);
    }

    public void removeLockImage()
    {
        lockImageInst.gameObject.SetActive(false);
    }
}
