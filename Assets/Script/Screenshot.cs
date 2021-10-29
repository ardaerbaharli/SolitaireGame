using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Screenshot : MonoBehaviour
{
    private int ssIndex;
    //here you can set the folder you want to use, 
    //IMPORTANT - use "@" before the string, because this is a verbatim string
    //IMPORTANT - the folder must exists
    string pathToYourFile = @"D:\Solitaire\";
    //this is the name of the file
    string fileName = "ss_";
    //this is the file type
    string fileType = ".png";


    private void Awake()
    {        
        ssIndex = 0;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F10))
        {
            UnityEngine.ScreenCapture.CaptureScreenshot(pathToYourFile + fileName + ssIndex + fileType);
            ssIndex++;
        }
    }
}
