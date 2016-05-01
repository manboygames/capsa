using UnityEngine;
using System.Collections.Generic;

public class Test : MonoBehaviour {

    void Update()
    {
        Vector3 screenPoint = Input.mousePosition;
        screenPoint.z = 2f; //distance of the plane from the camera
        transform.position = Camera.main.ScreenToWorldPoint(screenPoint);
    }

}
