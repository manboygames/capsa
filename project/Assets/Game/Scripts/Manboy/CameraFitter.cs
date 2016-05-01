using UnityEngine;
using System.Collections;
using AdvancedInspector;

public class CameraFitter : MonoBehaviour {

    float defaultWidth = 1024;
    float defaultHeight = 768;

    void Start()
    {
        UpdateSize();
    }

    [Inspect]
    public void UpdateSize()
    {
        float x = Screen.width;
        float y = Screen.height;
        float defaultSize = Camera.main.orthographicSize;
        Camera.main.orthographicSize = y / defaultHeight * defaultSize;
    }

}
