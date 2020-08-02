using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class TouchBehavior : MonoBehaviour
{
    [SerializeField]
    Text debugInfoText;

    [SerializeField]
    Image leftPanelBackGround;

    [SerializeField]
    Image leftPanelRoller;
    float radiusleftPanelRoller;
    bool leftPanelPressed = false;
    bool someTouchLeavesLeftPanel = false;

    [SerializeField]
    Image leftPanelPointer;
    float radiusleftPanelPointer;

    [SerializeField]
    PlayerBehavior playerBehavior;

    private string debugInfoTextDefault = "Touch Debug Info";
    private string debugInfoTextLeftPanelTouched = "LeftPanel Touched";
    private string debugInfoTextLeftPanelMoved = "LeftPanel Moved";
    private string debugInfoTextUpperRightPanelTouched = "UpperRightPanel Touched";
    private string debugInfoTextUpperRightPanelMoved = "UpperRightPanel Moved";

    private Color color1 = new Color(57/255f, 167/255f, 217/255f, 60/255f);
    private Color color2 = new Color(57/255f, 167/255f, 217/255f, 150/255f);

    int touchCounts;
    private Touch theTouch;
    private Vector2 touchLastPosition, touchCurrentPosition;
    private Vector2 moveLeftPanelPointerVector;
    float screenToUiScaler;

    [SerializeField]
    Image cameraRoller;
    [SerializeField]
    Camera mainCamera;

    private Vector2 upperRightTouchLastPosition, upperRightTouchCurrentPosition;
    private Vector2 cameraRollerPointerVector;
    bool cameraRollerPressed;

    private void Awake()
    {
        radiusleftPanelRoller = leftPanelRoller.GetComponent<RectTransform>().rect.width / 2;
        radiusleftPanelPointer = leftPanelPointer.GetComponent<RectTransform>().rect.width / 2;

        screenToUiScaler = 1;
    }

    // Update is called once per frame
    void Update()
    {
        //set initial state
        leftPanelPressed = false;
        someTouchLeavesLeftPanel = false;
        debugInfoText.text = null;
        cameraRollerPressed = false;

        touchCounts = Input.touchCount;
        
        //touch screen
        if (touchCounts > 0)
        {  

            for(int iTouch =0; iTouch< touchCounts; iTouch++)
            {
                theTouch = Input.GetTouch(iTouch);

                //touch scope on some game object
                if (EventSystem.current.IsPointerOverGameObject(theTouch.fingerId))
                {

                    GameObject tmpRefPointedObject = GetFirstPickGameObject(theTouch.position);
                    
                    //the touch is inside left panel
                    if (tmpRefPointedObject.name == leftPanelBackGround.name)
                    {   
                        //left panel is not pressed
                        if (leftPanelPressed == false)
                        {
                            leftPanelPressed = true;
                            processLeftPanel(theTouch);
                        }
                        else if (leftPanelPressed == true)
                        {
                            //select another touch inside left panel area
                            if (someTouchLeavesLeftPanel == true && 
                                (theTouch.phase == TouchPhase.Stationary || theTouch.phase == TouchPhase.Moved))
                            {   
                                someTouchLeavesLeftPanel = false;
                                //leftPanelPressed = true;
                                processLeftPanel(theTouch);
                            }

                            //the touched finger in left panel leaves
                            else if (theTouch.phase == TouchPhase.Ended)
                            {
                                someTouchLeavesLeftPanel = true;
                            }
                        }
                    }

                    else if (tmpRefPointedObject.name == cameraRoller.name)
                    {
                        cameraRollerPressed = true;
                        processCameraRoller(theTouch);
                    }


                }

            }//end iTouch = 0~touchCounts

            if (leftPanelPressed == false)
            {
                leftPanelPointer.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                leftPanelRoller.color = color1;
            }

            if(cameraRollerPressed == false && mainCamera.GetComponent<CameraBehavior>().state == 2)
            {
                mainCamera.GetComponent<CameraBehavior>().stopRotate();
            }

        }
       
    }

    private void processLeftPanel(Touch theTouch)
    {
        if (theTouch.phase == TouchPhase.Began)
        {
            //began touch
            touchLastPosition = theTouch.position;
            touchCurrentPosition = theTouch.position;

            debugInfoText.text = debugInfoTextLeftPanelTouched;
            leftPanelRoller.color = color2;
            
        }

        if (theTouch.phase == TouchPhase.Stationary || theTouch.phase == TouchPhase.Moved)
        {
            //moved touch
            touchCurrentPosition = theTouch.position;

            debugInfoText.text = debugInfoTextLeftPanelMoved;

            moveLeftPanelPointerVector.x = touchCurrentPosition.x - touchLastPosition.x;
            moveLeftPanelPointerVector.y = touchCurrentPosition.y - touchLastPosition.y;

            Vector2 tmpDir = moveLeftPanelPointerVector.normalized;
            float tmpLength = moveLeftPanelPointerVector.magnitude;

            float moveLength = 0;

            if (tmpLength > radiusleftPanelRoller / 2)
            {
                moveLength = radiusleftPanelRoller - radiusleftPanelPointer;
            }
            else
            {
                moveLength = tmpLength * 2;  //*2 to extend to maximum at radiusleftPanelRoller
                if (moveLength > radiusleftPanelRoller - radiusleftPanelPointer)
                {
                    moveLength = radiusleftPanelRoller - radiusleftPanelPointer;
                }
            }

            Vector2 tmpMoveVector = screenToUiScaler * moveLength * tmpDir;


            leftPanelPointer.GetComponent<RectTransform>().anchoredPosition = tmpMoveVector;

            Vector3 moveDir = new Vector3(tmpDir.x, 0, tmpDir.y);
            Quaternion cameraYAxisRotation = Quaternion.Euler(0,mainCamera.transform.rotation.eulerAngles.y,0);
            moveDir = cameraYAxisRotation * moveDir;

            //player control
            playerBehavior.playerMove(moveDir);
        }


    }


    private GameObject GetFirstPickGameObject(Vector2 position)
    {
        EventSystem eventSystem = EventSystem.current;
        PointerEventData pointerEventData = new PointerEventData(eventSystem);
        pointerEventData.position = position;

        List<RaycastResult> uiRaycastResultCache = new List<RaycastResult>();
        eventSystem.RaycastAll(pointerEventData, uiRaycastResultCache);
        if (uiRaycastResultCache.Count > 0)
            return uiRaycastResultCache[0].gameObject;
        return null;
    }


    private void processCameraRoller(Touch theTouch)
    {
        if (theTouch.phase == TouchPhase.Began)
        {
            //began touch
            upperRightTouchLastPosition = theTouch.position;
            upperRightTouchCurrentPosition = theTouch.position;

            debugInfoText.text = debugInfoTextUpperRightPanelTouched;
        }

        if (theTouch.phase == TouchPhase.Stationary || theTouch.phase == TouchPhase.Moved)
        {
            //moved touch
            upperRightTouchCurrentPosition = theTouch.position;

            debugInfoText.text = debugInfoTextUpperRightPanelMoved;

            cameraRollerPointerVector.x = upperRightTouchCurrentPosition.x - upperRightTouchLastPosition.x;
            cameraRollerPointerVector.y = upperRightTouchCurrentPosition.y - upperRightTouchLastPosition.y;

            Vector2 tmpDir = cameraRollerPointerVector.normalized;
            float tmpLength = cameraRollerPointerVector.magnitude;

            if (tmpLength > 50f)
            {
                mainCamera.GetComponent<CameraBehavior>().startRotate(tmpDir);
            }
        }

    }
}
