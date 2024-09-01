import cv2
import mmap
import threading


class CameraFeed:
    SHM_NAME = "shm_cam_feed"
    FRAME_WIDTH = 640
    FRAME_HEIGHT = 360

    def __init__(self,show_video=False, width=640,height=360):
        FRAME_WIDTH = width
        FRAME_HEIGHT = height
        self.FRAME_SIZE = FRAME_WIDTH * FRAME_HEIGHT * 3
        self.ShowVideo = show_video
        self.shm = mmap.mmap(-1, self.FRAME_SIZE, self.SHM_NAME)
        self.cap = cv2.VideoCapture(0)
        self.cap.set(cv2.CAP_PROP_FRAME_WIDTH, FRAME_WIDTH)
        self.cap.set(cv2.CAP_PROP_FRAME_HEIGHT, FRAME_HEIGHT)
        self.running = False  # State to control the camera feed thread

        if not self.cap.isOpened():
            raise RuntimeError("Failed to open camera.")

    def start(self):
        """Starts the camera feed in a separate thread."""
        self.running = True
        self.thread = threading.Thread(target=self.run)
        self.thread.start()

    def run(self):
        """Captures frames from the camera and processes them."""
        while self.running:
            ret, frame = self.cap.read()
            if not ret:
                print("Failed to capture frame.")
                break

            frame = cv2.flip(frame, 1)
            text = "Sign Language Feed"
            cv2.putText(frame, text, (10, 50), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2, cv2.LINE_AA)

            frame_rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)

            # Check frame size
            if frame_rgb.nbytes != self.FRAME_SIZE:
                print(f"Warning: Frame size mismatch! Expected {self.FRAME_SIZE}, got {frame_rgb.nbytes}")

            # Write frame to shared memory
            self.shm.seek(0)
            self.shm.write(frame_rgb.tobytes())
            if self.ShowVideo is True:
                cv2.imshow('Camera Feed', frame)
                if cv2.waitKey(1) & 0xFF == ord('q'):
                    self.stop()
                    break  # Exit the loop to stop capturing

        self.cleanup()

    def stop(self):
        """Stops the camera feed thread."""
        self.running = False

    def cleanup(self):
        """Releases resources."""
        self.cap.release()
        self.shm.close()
        cv2.destroyAllWindows()

if __name__ == "__main__":
    try:
        cam_feed = CameraFeed()
        cam_feed.start()
        cam_feed.thread.join()  # Wait for the camera thread to finish
    except RuntimeError as e:
        print(e)


# import cv2
# import mmap
#
# SHM_NAME = "shm_cam_feed"
# FRAME_WIDTH = 640
# FRAME_HEIGHT = 360
# FRAME_SIZE = FRAME_WIDTH * FRAME_HEIGHT * 3
#
# def main():
#     shm = mmap.mmap(-1, FRAME_SIZE, SHM_NAME)
#     cap = cv2.VideoCapture(0)
#     cap.set(cv2.CAP_PROP_FRAME_WIDTH, FRAME_WIDTH)
#     cap.set(cv2.CAP_PROP_FRAME_HEIGHT, FRAME_HEIGHT)
#
#     if not cap.isOpened():
#         print("Failed to open camera.")
#         return
#
#     try:
#         while True:
#             ret, frame = cap.read()
#             if not ret:
#                 print("Failed to capture frame.")
#                 break
#             frame = cv2.flip(frame, 1)
#             text = "Sign Language Feed"
#             cv2.putText(frame, text, (10, 50), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2, cv2.LINE_AA)
#
#             frame_rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
#
#             # Check frame size
#             if frame_rgb.nbytes != FRAME_SIZE:
#                 print(f"Warning: Frame size mismatch! Expected {FRAME_SIZE}, got {frame_rgb.nbytes}")
#
#             # Write frame to shared memory
#             shm.seek(0)
#             shm.write(frame_rgb.tobytes())
#
#             cv2.imshow('Camera Feed', frame)
#             if cv2.waitKey(1) & 0xFF == ord('q'):
#                 break
#     finally:
#         cap.release()
#         shm.close()
#         cv2.destroyAllWindows()
#
# if __name__ == "__main__":
#     main()
