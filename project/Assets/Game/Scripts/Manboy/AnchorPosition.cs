using UnityEngine;
using System.Collections;
using AdvancedInspector;
using UnityEngine.Events;

public class AnchorPosition : MonoBehaviour {

    public enum Anchor
    {
        Top,
        Left,
        Bottom,
        Right
    }

    public Anchor defaultAnchor;
    public Camera cam;
    public bool setAtStart;

    public UnityEvent onPositionUpdated = new UnityEvent();

    void Start()
    {
        if (setAtStart)
        {
            SetPosition();
        }
    }

    [Inspect]
    public void SetPosition()
    {
        SetPosition(defaultAnchor);
    }

    public void SetPosition(Anchor anchor)
    {
        if(cam == null)
        {
            cam = Camera.main;
        }

        if(!cam.orthographic)
        {
            Debug.LogWarning("Camera is not orthographic");
        }

        float pixelFactor = cam.orthographicSize / (Screen.height / 2);

        float posX = transform.position.x;
        float posY = transform.position.y;

        float nudgeX = Screen.width / 2 * pixelFactor;
        float nudgeY = Screen.height / 2 * pixelFactor;

        float camX = cam.transform.position.x;
        float camY = cam.transform.position.y;

        switch(anchor)
        {
            case Anchor.Top:
                posX = 0;
                posY = nudgeY;
                break;

            case Anchor.Left:
                posX = -nudgeX;
                posY = 0;
                break;

            case Anchor.Bottom:
                posX = 0;
                posY = -nudgeY;
                break;

            case Anchor.Right:
                posX = nudgeX;
                posY = 0;
                break;
        }

        transform.position = new Vector3(posX, posY, transform.position.z);

        onPositionUpdated.Invoke();
    }

    void OnDestroy()
    {
        onPositionUpdated.RemoveAllListeners();
    }

}
