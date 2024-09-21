using Assets.Logger;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.CameraFeed
{
    public class CameraManager : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        private static CameraManager _instance;
        private WebCamTexture _webCamTexture;
        private bool _isInitialized = false;

        public static CameraManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject obj = new GameObject("CameraManager");
                    _instance = obj.AddComponent<CameraManager>();
                    DontDestroyOnLoad(obj);
                }
                return _instance;
            }
        }


        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
                DontDestroyOnLoad(this.gameObject);
                InitCamera();
                //InitShaerdMemoryCamera();
            }
        }

        private void InitShaerdMemoryCamera()
        {

        }

        private void InitCamera()
        {

            if (WebCamTexture.devices.Length > 0)
            {
                _webCamTexture = new WebCamTexture();
                _webCamTexture.Play();
                _isInitialized = true;
            }
            else
            {
                FileLogger.LogError("No camera device found");
            }
        }

        public WebCamTexture GetCameraFeed()
        {
            if (!_isInitialized)
            {
                InitCamera();
            }
            return _webCamTexture;
        }

        public void OnApplicationQuit()
        {
            Cleanup();
        }

        public void Cleanup()
        {
            if (_webCamTexture != null)
            {
                _webCamTexture.Stop();
                _webCamTexture = null;
                _isInitialized = false;
            }

            if (_instance != null)
            {
                Destroy(_instance.gameObject);
                _instance = null;
            }
        }

    }
}
