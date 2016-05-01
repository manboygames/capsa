using UnityEngine;
using System.Collections;
using System;
using System.IO;

public class ScreenshotUtils : MonoBehaviour
{

    private static ScreenshotUtils instance;

    public static ScreenshotUtils Instance
    {
        get { return ScreenshotUtils.instance; }
    }

    public KeyCode keyCode;
    public string prefix = "Screenshot";
    public string path = "Screenshots";

    void Awake()
    {
        if (ScreenshotUtils.Instance != null)
        {
            Destroy(gameObject);
        }

        instance = this;
        DontDestroyOnLoad(this);
    }

    void Update()
    {
        if (Input.GetKeyDown(keyCode))
        {
            DateTime dateTime = DateTime.Now;
            string date = dateTime.ToString("yyyy-MM-dd");
            string time = dateTime.ToString("HH.mm.ss");
            string fileName = prefix + " " + date + " " + time + ".png";

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            fileName = path + "/" + fileName;
            Application.CaptureScreenshot(fileName);
        }
    }
}
