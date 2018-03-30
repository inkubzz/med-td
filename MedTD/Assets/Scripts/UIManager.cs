﻿using System;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Transform uICanvas;
    public Text textSelectSS;
    public Text textSelectedSSCount;
    public Text textMaxSSSelected;
    public Button buttonDoneWithSS;
    public GameObject buildingMenuPrefab;

    private GameObject xGO;
    private bool interruptXAtTouch = false;
    private bool interruptTextMaxSSSelected = false;

    private void Start()
    {
        BuildManager buildManager = BuildManager.instance;
        textSelectSS.text = "Pick " + buildManager.numberOfLymphNodes + " strategic sites.";
        textSelectedSSCount.text = "Number of selected sites: 0/" + buildManager.numberOfLymphNodes + ".";
        textMaxSSSelected.text = "Can't pick more than " + buildManager.numberOfLymphNodes + " strategic sites.";
    }


    private void SetEnabledTextMaxSSSelected(bool newActiveState)
    {
        if (textMaxSSSelected.gameObject.activeSelf != newActiveState)
            textMaxSSSelected.gameObject.SetActive(newActiveState);
    }
    private void DisableTextMaxSSSelected()
    {
        if (interruptTextMaxSSSelected)
        {
            interruptTextMaxSSSelected = false;
            return;
        }

        if (textMaxSSSelected.gameObject.activeSelf)
            textMaxSSSelected.gameObject.SetActive(false);
    }
    public void SetEnabledButtonDoneWithSS(bool newActiveState)
    {
        if (buttonDoneWithSS.gameObject.activeSelf != newActiveState)
            buttonDoneWithSS.gameObject.SetActive(newActiveState);
    }
    public void UpdateSelectedSSCount(int count)
    {
        SetTextSelectedSSCount("Number of selected sites: " + count + "/" + BuildManager.instance.numberOfLymphNodes + ".");
    }
    private void SetTextSelectedSSCount(string newText)
    {
        textSelectedSSCount.text = newText;
    }
    public void DestroySSUIElements()
    {
        Destroy(textSelectSS.gameObject);
        Destroy(textSelectedSSCount.gameObject);
        Destroy(buttonDoneWithSS.gameObject);
    }

    public GameObject ShowBuildingMenu(Transform lymphNode)
    {
        //Debug.Log("UIManager.ShowBuildingMenu");
        GameObject buildingMenu = Instantiate(buildingMenuPrefab, new Vector3(0f, 0f, -1.2f), uICanvas.rotation);
        buildingMenu.transform.SetParent(uICanvas);
        
        RectTransform buildingMenuRT = buildingMenu.GetComponent<RectTransform>();
        RectTransform canvasRT = uICanvas.GetComponent<RectTransform>();
        Vector2 viewportPosition = Camera.main.WorldToViewportPoint(lymphNode.position);
        Vector2 uiOffset = new Vector2((float)canvasRT.sizeDelta.x / 2f, (float)canvasRT.sizeDelta.y / 2f); // screen offset for the canvas
        Vector2 proportionalPosition = new Vector2(viewportPosition.x * canvasRT.sizeDelta.x, viewportPosition.y * canvasRT.sizeDelta.y); // position on the canvas

        // set the position and remove the screen offset
        buildingMenuRT.localPosition = proportionalPosition - uiOffset;
        
        return buildingMenu;
    }

    public void FlashXAtTouch(float delay)
    {
        ShowXAtTouch(delay);
    }
    private void ShowXAtTouch(float delay)
    {
        if (xGO != null)
        {
            Destroy(xGO);
            interruptXAtTouch = true;
        }

        Vector3 mousePosWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosWorld.z = -2f;
        var xSprite = Resources.Load<Sprite>("Sprites/xSprite");

        xGO = new GameObject();
        xGO.transform.SetPositionAndRotation(mousePosWorld, Quaternion.identity);
        xGO.AddComponent<SpriteRenderer>().sprite = xSprite;

        Invoke("DestroyXAtTouch", delay);
    }
    private void DestroyXAtTouch()
    {
        if (interruptXAtTouch)
        {
            interruptXAtTouch = false;
            return;
        }
        if (xGO != null)
            Destroy(xGO);
    }
    public void FlashMaxSSSelected(float delay)
    {
        if (textMaxSSSelected.gameObject.activeSelf)
        {
            DisableTextMaxSSSelected();
            interruptTextMaxSSSelected = true;
        }
        SetEnabledTextMaxSSSelected(true);
        Invoke("DisableTextMaxSSSelected", delay);
    }
}
