using UnityEngine;
using System.Collections;

public class GameUtils{

    public static void SetPosition2D(Vector3 pos3D, RectTransform rectTransform, Camera camera, Canvas canvas)
    {
        Vector2 viewportPoint = camera.WorldToViewportPoint(pos3D);
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        Vector2 pos2D = new Vector2(
        ((viewportPoint.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f)),
        ((viewportPoint.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f)));

        rectTransform.anchoredPosition = pos2D;
    }

}
