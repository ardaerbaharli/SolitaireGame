using System;
using System.IO;
using UnityEngine;

namespace ardaerbaharli
{
    public class Screenshot : MonoBehaviour
    {
        private int _index;
        [SerializeField] private string _directoryName = "Screenshots";
        [SerializeField] private string _filePrefix = "screenshot_";
        [SerializeField] private string _fileType = ".png";
        [SerializeField] private KeyCode screenshotKey = KeyCode.F10;
        private string path;
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            _index = 0;
            path = Path.Combine(Application.dataPath, _directoryName);
            print(path);
            Directory.CreateDirectory(path);
        }

        private void Update()
        {
            if (Input.GetKeyDown(screenshotKey))
            {
                ScreenCapture.CaptureScreenshot(path + "/" + _filePrefix + _index + _fileType);
                _index++;
            }
        }
    }
}