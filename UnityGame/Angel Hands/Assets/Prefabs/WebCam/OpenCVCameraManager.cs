//using UnityEngine;
//using OpenCVForUnity.CoreModule;
//using OpenCVForUnity.ImgprocModule;
//using OpenCVForUnity.UnityUtils;
//using OpenCVForUnity.ObjdetectModule;
//using OpenCVForUnity.DnnModule;
//using System.Collections.Generic;

//public class CameraManager : MonoBehaviour
//{
//    private static CameraManager _instance;
//    public static CameraManager Instance
//    {
//        get
//        {
//            if (_instance == null)
//            {
//                GameObject obj = new GameObject("CameraManager");
//                _instance = obj.AddComponent<CameraManager>();
//                DontDestroyOnLoad(obj);
//            }
//            return _instance;
//        }
//    }

//    private WebCamTexture _webCamTexture;
//    private Mat _frame;
//    private CascadeClassifier _faceCascade;
//    private Net _faceNet;
//    private Net _handNet;
//    private string _faceCascadeFilePath;
//    private string _faceModelFilePath;
//    private string _handModelFilePath;
//    private bool _isInitialized = false;

//    void Awake()
//    {
//        if (_instance != null && _instance != this)
//        {
//            Destroy(this.gameObject);
//        }
//        else
//        {
//            _instance = this;
//            DontDestroyOnLoad(this.gameObject);
//            InitCamera();
//        }
//    }

//    private void InitCamera()
//    {
//        if (WebCamTexture.devices.Length > 0)
//        {
//            _webCamTexture = new WebCamTexture();
//            _webCamTexture.Play();
//            _frame = new Mat(_webCamTexture.height, _webCamTexture.width, CvType.CV_8UC3);

//            _faceCascadeFilePath = Utils.getFilePath("haarcascade_frontalface_alt.xml");
//            _faceCascade = new CascadeClassifier(_faceCascadeFilePath);

//            // Load face detection model
//            _faceModelFilePath = Utils.getFilePath("deploy.prototxt");
//            _faceNet = Dnn.readNetFromCaffe(_faceModelFilePath, Utils.getFilePath("res10_300x300_ssd_iter_140000_fp16.caffemodel"));

//            // Load hand detection model (adjust paths as necessary)
//            _handModelFilePath = Utils.getFilePath("hand_model.pb");
//            _handNet = Dnn.readNetFromTensorflow(_handModelFilePath);

//            _isInitialized = true;
//        }
//        else
//        {
//            Debug.LogError("No camera device found");
//        }
//    }

//    public Texture2D GetProcessedFrame()
//    {
//        if (!_isInitialized || _webCamTexture == null)
//        {
//            return null;
//        }

//        Utils.webCamTextureToMat(_webCamTexture, _frame);

//        // Detect faces
//        MatOfRect faces = new MatOfRect();
//        _faceCascade.detectMultiScale(_frame, faces);

//        foreach (Rect face in faces.toArray())
//        {
//            // Draw rectangle around the face
//            Imgproc.rectangle(_frame, new Point(face.x, face.y), new Point(face.x + face.width, face.y + face.height), new Scalar(255, 0, 0), 2);

//            // Process each face for landmarks using DNN model
//            Mat faceROI = new Mat(_frame, face);
//            Mat blob = Dnn.blobFromImage(faceROI, 1.0, new Size(300, 300), new Scalar(104, 177, 123));
//            _faceNet.setInput(blob);
//            Mat detections = _faceNet.forward();

//            // Draw landmarks on the face
//            for (int i = 0; i < detections.size(2); i++)
//            {
//                float confidence = detections.get(0, 0, i, 2)[0];
//                if (confidence > 0.5)
//                {
//                    int x1 = (int)(detections.get(0, 0, i, 3)[0] * faceROI.cols());
//                    int y1 = (int)(detections.get(0, 0, i, 4)[0] * faceROI.rows());
//                    int x2 = (int)(detections.get(0, 0, i, 5)[0] * faceROI.cols());
//                    int y2 = (int)(detections.get(0, 0, i, 6)[0] * faceROI.rows());

//                    Imgproc.rectangle(faceROI, new Point(x1, y1), new Point(x2, y2), new Scalar(0, 255, 0), 2);
//                }
//            }
//        }

//        // Convert Mat to Texture2D
//        Texture2D texture = new Texture2D(_frame.cols(), _frame.rows(), TextureFormat.RGBA32, false);
//        Utils.matToTexture2D(_frame, texture);

//        return texture;
//    }

//    public void Cleanup()
//    {
//        if (_webCamTexture != null)
//        {
//            _webCamTexture.Stop();
//            _webCamTexture = null;
//            _isInitialized = false;
//        }

//        if (_instance != null)
//        {
//            Destroy(_instance.gameObject);
//            _instance = null;
//        }
//    }

//    void OnApplicationQuit()
//    {
//        Cleanup();
//    }
//}
//}


///////use case//////
///
//using Assets.CameraFeed;
//using UnityEngine;
//using UnityEngine.UI;

//public class DisplayCameraFeed : MonoBehaviour
//{
//    public RawImage rawImage;

//    private Texture2D _texture;

//    void OnEnable()
//    {
//        _texture = CameraManager.Instance.GetProcessedFrame();
//        if (_texture != null)
//        {
//            rawImage.texture = _texture;
//            rawImage.rectTransform.sizeDelta = new Vector2(_texture.width, _texture.height);

//            // Apply horizontal flip if needed
//            rawImage.uvRect = new Rect(1, 0, -1, 1);
//        }
//        else
//        {
//            Debug.LogError("No processed frame available from CameraManager");
//        }
//    }
//}
