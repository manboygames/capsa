using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UITest : MonoBehaviour
{

    public RectTransform infoTransform;
    public List<Transform> chairs;
    public Camera uiCamera;

    void Update()
    {
       
        if(Input.GetKeyDown(KeyCode.F1))
        {
            Move(chairs[0].position);
        }
        else if(Input.GetKeyDown(KeyCode.F2))
        {
            Move(chairs[1].position);
        }
        else if (Input.GetKeyDown(KeyCode.F3))
        {
            Move(chairs[2].position);
        }
        else if (Input.GetKeyDown(KeyCode.F4))
        {
            Move(chairs[3].position);
        }
    }

    void Move(Vector3 chairPos)
    {
        /*Vector2 viewportPoint = uiCamera.WorldToViewportPoint(chairPos);

        infoTransform.anchorMin = viewportPoint;
        infoTransform.anchorMax = viewportPoint;*/

        //infoTransform.anchoredPosition = RectTransformUtility.WorldToScreenPoint(uiCamera, chairPos);

        RectTransform canvasRect = GetComponent<RectTransform>();

        Vector2 ViewportPosition = uiCamera.WorldToViewportPoint(chairPos);
        Vector2 WorldObject_ScreenPosition = new Vector2(
        ((ViewportPosition.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f)),
        ((ViewportPosition.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f)));

        //now you can set the position of the ui element
        infoTransform.anchoredPosition = WorldObject_ScreenPosition;
    }

    


}
