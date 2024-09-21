using Assets.Scripts.GameManager;
using System;
using System.IO.MemoryMappedFiles;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class SharedMemoryDisplay : MonoBehaviour
{
    public RawImage rawImage;
    [SerializeField] private Texture2D noVideoFeedImage; // Add this serialized field for NoVideoFeedImage
    private const int FRAME_WIDTH = 640;
    private const int FRAME_HEIGHT = 360;
    private const int FRAME_SIZE = FRAME_WIDTH * FRAME_HEIGHT * 3; // RGB format

    private MemoryMappedFile mmf;
    private MemoryMappedViewAccessor accessor;
    private byte[] frameBuffer = new byte[FRAME_SIZE];
    private Texture2D texture;
    private Thread readThread;
    private bool keepReading = true;

    void Start()
    {
        InitializeTexture();
        StartReadingThread();
    }

    private void InitializeTexture()
    {
        texture = new Texture2D(FRAME_WIDTH, FRAME_HEIGHT, TextureFormat.RGB24, false);
        rawImage.texture = texture;
        rawImage.rectTransform.sizeDelta = new Vector2(FRAME_WIDTH, FRAME_HEIGHT); // Adjust size if needed
    }

    private void StartReadingThread()
    {
        readThread = new Thread(ReadSharedMemory);
        readThread.IsBackground = true; // Make sure the thread doesn't prevent application exit
        readThread.Start();
    }

    private void ReadSharedMemory()
    {
        while (keepReading)
        {
            bool frameReadSuccess = false; // Track if a frame was successfully read

            try
            {
                mmf = MemoryMappedFile.OpenExisting("shm_cam_feed");
                accessor = mmf.CreateViewAccessor(0, FRAME_SIZE, MemoryMappedFileAccess.Read);

                try
                {
                    // Read frame data from shared memory
                    accessor.ReadArray(0, frameBuffer, 0, frameBuffer.Length);

                    // If read is successful, set flag
                    frameReadSuccess = true;

                    // Queue the texture update on the main thread
                    UnityMainThreadDispatcher.Instance.Enqueue(() =>
                    {
                        texture.LoadRawTextureData(frameBuffer);
                        texture.Apply();
                        rawImage.texture = texture; // Set the rawImage to use the video feed texture
                    });
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error reading from shared memory: " + ex.Message);
                }
                finally
                {
                    accessor.Dispose();
                    mmf.Dispose();
                }

                // Optional: Add a small delay to control the frame rate
                //Thread.Sleep(16); // Approx. 60 FPS
            }
            catch (Exception ex)
            {
                Debug.LogError("Error initializing shared memory access: " + ex.Message);
            }

            // If no frame was read, show the NoVideoFeedImage
            if (!frameReadSuccess)
            {
                UnityMainThreadDispatcher.Instance.Enqueue(() =>
                {
                    rawImage.texture = noVideoFeedImage; // Switch to the NoVideoFeedImage
                });

                Thread.Sleep(1000); // Optional delay before retrying to avoid spamming logs
            }
        }
    }

    void OnDestroy()
    {
        keepReading = false;
        readThread?.Join();
        accessor?.Dispose();
        mmf?.Dispose();
    }
}
