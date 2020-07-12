using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class TouchBehavior : MonoBehaviour
{

    public Text debugInfoText;

    public Image leftPanelBackGround;

    public Image leftPanelRoller;
    float radiusleftPanelRoller;
    bool leftPanelPressed = false;
    bool someTouchLeavesLeftPanel = false;

    public Image leftPanelPointer;
    float radiusleftPanelPointer;

    public PlayerBehavior playerBehavior;

    private string debugInfoTextDefault = "Touch Debug Info";
    private string debugInfoTextLeftPanelTouched = "LeftPanel Touched";
    private string debugInfoTextLeftPanelMoved = "LeftPanel Moved";
    private string debugInfoTextLeftPanelUntouched = "LeftPanel Untouched";


    private Color color1 = new Color(57/255f, 167/255f, 217/255f, 60/255f);
    private Color color2 = new Color(57/255f, 167/255f, 217/255f, 150/255f);

    int touchCounts;
    private Touch theTouch;
    private Vector2 touchLastPosition, touchCurrentPosition;
    private Vector2 moveLeftPanelPointerVector;
    float screenToUiScaler;


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
                                leftPanelPressed = true;
                                processLeftPanel(theTouch);
                            }

                            //the touched finger in left panel leaves
                            else if (theTouch.phase == TouchPhase.Ended)
                            {
                                someTouchLeavesLeftPanel = true;
                            }
                        }
                    }
                }

            }//end iTouch = 0~touchCounts

            if (leftPanelPressed == false)
            {
                debugInfoText.text = debugInfoTextLeftPanelUntouched;

                leftPanelPointer.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                leftPanelRoller.color = color1;
                leftPanelPressed = false;
            }

        }
       
    }

    public void processLeftPanel(Touch theTouch)
    {
        if (theTouch.phase == TouchPhase.Began)
        {
            //began touch
            debugInfoText.text = theTouch.phase.ToString();

            touchLastPosition = theTouch.position;
            touchCurrentPosition = theTouch.position;

            debugInfoText.text = debugInfoTextLeftPanelTouched;
            leftPanelRoller.color = color2;
            
        }

        if (theTouch.phase == TouchPhase.Moved)
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


            //player control
            playerBehavior.playerMove(tmpDir);
        }


    }


    public GameObject GetFirstPickGameObject(Vector2 position)
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

}
