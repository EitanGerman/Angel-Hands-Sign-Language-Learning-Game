using Assets.CameraFeed;
using Assets.Logger;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.WebCam
{
    public class WebCamGrabber : MonoBehaviour
    {
        public RawImage rawImage;

        void Start()
        {
            InitImageTexture();
        }

        void OnEnable()
        {
            InitImageTexture();
        }

        private void InitImageTexture()
        {
            WebCamTexture webCamTexture = CameraManager.Instance.GetCameraFeed();
            if (webCamTexture != null)
            {
                rawImage.texture = webCamTexture;
                rawImage.rectTransform.sizeDelta = new Vector2(webCamTexture.width, webCamTexture.height);
                rawImage.uvRect = new Rect(1, 0, -1, 1);
            }
            else
            {
                FileLogger.LogWarning("No WebCamTexture available from CameraManager");
            }
        }


        //void Update()
        //{
        //    WebCamTexture webCamTexture = CameraManager.Instance.GetCameraFeed();
        //    if (webCamTexture != null)
        //    {
        //        rawImage.texture = webCamTexture;
        //        rawImage.rectTransform.sizeDelta = new Vector2(webCamTexture.width, webCamTexture.height);
        //    }
        //    else
        //    {
        //        FileLogger.LogWarning("No WebCamTexture available from CameraManager");
        //    }
        //}
    }
}