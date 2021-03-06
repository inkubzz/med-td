﻿using UnityEngine;
using UnityEngine.EventSystems;

public class Scroller : MonoBehaviour
{
    // TODO this could and should be relative to the vertical offset (difference between screen height and field height)
    public float scrollSpeed = 0.025f;
    public Transform uICanvas;
    private static Transform staticCanvas;

    //public static Scroller instance;

    private static Camera cam;
    private static float currentAspect;

    private static float totalDrag;
    private static float prevPoint;
    private static float newPoint;
    private static float camLowerBound;
    private static float camUpperBound;

    private static bool dragging;

    // total drag allowed before the scrolling starts; useful for clicking buttons
    private const float totalDragAllowed = 30f;


    private void Awake()
    {
        staticCanvas = uICanvas;
    }

    private void Start()
    {
        if (Screen.width < 900)
            scrollSpeed = 0.025f;
        else if (Screen.width < 950)
            scrollSpeed = 0.0235f;
        else if (Screen.width < 980)
            scrollSpeed = 0.0208f;
        else if (Screen.width < 1000)
            scrollSpeed = 0.0222f;
        else if (Screen.width < 1100)
            scrollSpeed = 0.02f;
        else if (Screen.width < 1200)
            scrollSpeed = 0.0182f;
        else if (Screen.width < 1290)
            scrollSpeed = 0.01665f;
        else if (Screen.width < 1350)
            scrollSpeed = 0.0155f;
        else if (Screen.width < 2000)
            scrollSpeed = 0.013f; // approx. not calculated
        else if (Screen.width < 2560)
            scrollSpeed = 0.01f;
        else
            scrollSpeed = 0.0085f;


        cam = Camera.main;

        SetUpRatio();
    }
    
    private void Update()
    {
        // don't do anything if game is paused
        if (GameManager.instance.IsGamePaused()) return;

        if (cam == null) cam = Camera.main;

        // if the aspect ratio changes, update the camera so that it's fitted to the screen width
        if (currentAspect != cam.aspect)
        {
            SetUpRatio();
        }

        
        if (Input.GetMouseButtonDown(0))
        {
            totalDrag = 0f;
            prevPoint = Input.mousePosition.y;
            dragging = false;
        }
        else if (Input.GetMouseButton(0))
        {
            if (cam == null) cam = Camera.main;

            // if camHeight > fieldHeight (i.e. there's extra vertical space), then don't enable drag!
            if (cam.orthographicSize * 2 > this.GetComponent<BoxCollider2D>().bounds.size.y)
            {
                dragging = false;
                return;
            }


            newPoint = Input.mousePosition.y;

            float delta = prevPoint - newPoint;
            float moveDist = delta * scrollSpeed;
            prevPoint = newPoint;
            
            if (moveDist == 0) return;

            totalDrag += Mathf.Abs(delta);
            if (totalDrag < totalDragAllowed) return;

            dragging = true;

            bool tooLow = cam.transform.position.y + moveDist < camLowerBound;
            bool tooHigh = cam.transform.position.y + moveDist > camUpperBound;
            if (tooLow) // if it's too low, move it to camLowerBound
            {
                cam.transform.position = new Vector3(cam.transform.position.x, camLowerBound, cam.transform.position.z);
            }
            else if (tooHigh) // if it's too high, move it to camUpperBound
            {
                cam.transform.position = new Vector3(cam.transform.position.x, camUpperBound, cam.transform.position.z);
            }
            else // else just move it where it's going
            {
                cam.transform.Translate(new Vector2(0, moveDist));
            }

            // move any build menus along with the cam
            MoveMenuWithCam();
        }
        else if (Input.GetMouseButtonUp(0))
        {
               dragging = false;
        }
    }

    private void OnMouseUpAsButton()
    {
        // if there's a ui element above, don't do anything
        if (EventSystem.current.IsPointerOverGameObject() || Utility.IsPointerOverUIObject())
        {
            return;
        }

        // if we're not scrolling, and there's a lymph node selected, deselect it
        // if we're not scrolling, and we're selecting SS, then this is a click outside any valid SS
        if (!dragging)
        {
            BuildManager buildManager = BuildManager.instance;
            if (buildManager.IsFinishedWithSS()) // done selecting SS
            {
                buildManager.DeselectLymphNode();
            }
            else // still selecting SS
            {
                // this is a click outside any valid strategic sites
                UIManager uim = buildManager.gameObject.GetComponent<UIManager>();
                uim.FlashXAtTouch(0.5f);
            }
        }
    }

    public static void ScrollToTop()
    {
        if (cam == null) cam = Camera.main;
        cam.transform.position = new Vector3(cam.transform.position.x, camUpperBound, cam.transform.position.z);
        MoveMenuWithCam();
    }
    public static void ScrollToBottom()
    {
        if (cam == null) cam = Camera.main;
        cam.transform.position = new Vector3(cam.transform.position.x, camLowerBound, cam.transform.position.z);
        MoveMenuWithCam();
    }

    private static void MoveMenuWithCam()
    {
        // move any build menus along with the cam
        BuildManager buildManager = BuildManager.instance;
        if (buildManager.IsFinishedWithSS())
        {
            LymphNode selLN = buildManager.GetSelectedLymphNode();
            if (selLN != null)
            {
                GameObject buildingMenu = selLN.GetBuildingMenu();
                if (buildingMenu != null)
                {   // there's a building menu shown; move the menu
                    RectTransform buildingMenuRT = buildingMenu.GetComponent<RectTransform>();
                    RectTransform canvasRT = staticCanvas.GetComponent<RectTransform>();
                    Vector2 viewportPosition = Camera.main.WorldToViewportPoint(selLN.transform.position);
                    Vector2 uiOffset = new Vector2((float)canvasRT.sizeDelta.x / 2f, (float)canvasRT.sizeDelta.y / 2f); // screen offset for the canvas
                    Vector2 proportionalPosition = new Vector2(viewportPosition.x * canvasRT.sizeDelta.x, viewportPosition.y * canvasRT.sizeDelta.y); // position on the canvas

                    // set the position and remove the screen offset
                    buildingMenuRT.localPosition = proportionalPosition - uiOffset;
                }
            }
        }
    }

    /// <summary>
    /// Saves the aspect ratio value;
    /// Resizes the camera so it fits the field's width;
    /// Positions the camera vertically in the middle of the field;
    /// Calculates the camera's top and bottom bounds.
    /// </summary>
    private void SetUpRatio()
    {
        if (cam == null) cam = Camera.main;

        // save the current aspect ratio
        currentAspect = cam.aspect;
        
        // get the width of the field and use it to calculate the camera width (which is exactly half that)
        float scrollFieldWidth = this.GetComponent<BoxCollider2D>().bounds.size.x;
        float cameraWidth = scrollFieldWidth / 2;
        // resize the camera so it fits the field's width
        cam.orthographicSize = cameraWidth / currentAspect;

        // position the camera in the middle of the field (vertically)
        cam.transform.position = new Vector3(cam.transform.position.x, this.transform.position.y, cam.transform.position.z);

        // calculate and define the upper and lower bounds for the camera
        BoxCollider2D fieldCollider = this.GetComponent<BoxCollider2D>();
        float height = fieldCollider.bounds.size.y;
        float fieldTop = fieldCollider.transform.position.y - (height / 2);
        float fieldBottom = fieldCollider.transform.position.y + (height / 2);

        float camHeight = cam.orthographicSize * 2;
        float camTop = cam.transform.position.y - (camHeight / 2);
        float camBottom = cam.transform.position.y + (camHeight / 2);

        float diffBottom = camBottom - fieldBottom;
        float diffTop = camTop - fieldTop;

        camLowerBound = cam.transform.position.y + diffBottom;
        camUpperBound = cam.transform.position.y + diffTop;
    }

    internal static bool IsDragging()
    {
        return dragging;
    }
}
