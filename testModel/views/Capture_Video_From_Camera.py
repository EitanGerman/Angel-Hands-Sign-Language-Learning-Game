import tkinter as tk
from tkinter import ttk, messagebox
import cv2
import mediapipe as mp
import os
import numpy as np

# Define constants
actions = np.array(['hello', 'thanks', 'iloveyou'])
DATA_PATH = 'MP_Data'
no_sequences = 30
sequence_length = 30

mp_holistic = mp.solutions.holistic


def mediapipe_detection(image, model):
    image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)  # Color conversion BGR to RGB
    image.flags.writeable = False  # Image is no longer writable
    results = model.process(image)  # Make prediction
    image.flags.writeable = True  # Image is now writable
    image = cv2.cvtColor(image, cv2.COLOR_RGB2BGR)  # Color conversion RGB to BGR
    return image, results


def draw_landmarks(image, results):
    mp_drawing = mp.solutions.drawing_utils
    mp_drawing.draw_landmarks(image, results.face_landmarks, mp_holistic.FACEMESH_CONTOURS)
    mp_drawing.draw_landmarks(image, results.pose_landmarks, mp_holistic.POSE_CONNECTIONS)
    mp_drawing.draw_landmarks(image, results.left_hand_landmarks, mp_holistic.HAND_CONNECTIONS)
    mp_drawing.draw_landmarks(image, results.right_hand_landmarks, mp_holistic.HAND_CONNECTIONS)


def draw_styled_landmarks(image, results):
    # Draw face connections
    mp_drawing = mp.solutions.drawing_utils
    mp_drawing.draw_landmarks(image, results.face_landmarks, mp_holistic.FACEMESH_CONTOURS,
                              mp_drawing.DrawingSpec(color=(80, 110, 10), thickness=1, circle_radius=1),
                              mp_drawing.DrawingSpec(color=(80, 256, 121), thickness=1, circle_radius=1)
                              )
    # Draw pose connections
    mp_drawing.draw_landmarks(image, results.pose_landmarks, mp_holistic.POSE_CONNECTIONS,
                              mp_drawing.DrawingSpec(color=(80, 22, 10), thickness=2, circle_radius=4),
                              mp_drawing.DrawingSpec(color=(80, 44, 121), thickness=2, circle_radius=2)
                              )
    # Draw left hand connections
    mp_drawing.draw_landmarks(image, results.left_hand_landmarks, mp_holistic.HAND_CONNECTIONS,
                              mp_drawing.DrawingSpec(color=(121, 22, 76), thickness=2, circle_radius=4),
                              mp_drawing.DrawingSpec(color=(121, 44, 250), thickness=2, circle_radius=2)
                              )
    # Draw right hand connections
    mp_drawing.draw_landmarks(image, results.right_hand_landmarks, mp_holistic.HAND_CONNECTIONS,
                              mp_drawing.DrawingSpec(color=(245, 117, 66), thickness=2, circle_radius=4),
                              mp_drawing.DrawingSpec(color=(245, 66, 230), thickness=2, circle_radius=2)
                              )


def extract_keypoints(results):
    pose = np.array([[res.x, res.y, res.z, res.visibility] for res in results.pose_landmarks.landmark]).flatten() if results.pose_landmarks else np.zeros(33 * 4)
    face = np.array([[res.x, res.y, res.z] for res in results.face_landmarks.landmark]).flatten() if results.face_landmarks else np.zeros(468 * 3)
    lh = np.array([[res.x, res.y, res.z] for res in results.left_hand_landmarks.landmark]).flatten() if results.left_hand_landmarks else np.zeros(21 * 3)
    rh = np.array([[res.x, res.y, res.z] for res in results.right_hand_landmarks.landmark]).flatten() if results.right_hand_landmarks else np.zeros(21 * 3)
    return np.concatenate([pose, face, lh, rh])


class FolderFiller(tk.Frame):
    def __init__(self, parent):
        super().__init__(parent)
        self.pack(fill=tk.BOTH, expand=True)
        self.create_widgets()

    def create_widgets(self):
        # Create main frame
        main_frame = tk.Frame(self)
        main_frame.pack(fill=tk.BOTH, expand=True, padx=10, pady=10)

        # Create canvas for OpenCV window
        self.canvas = tk.Canvas(main_frame, width=640, height=480)
        self.canvas.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)

        # Create frame for controls
        control_frame = tk.Frame(main_frame)
        control_frame.pack(side=tk.RIGHT, fill=tk.Y, padx=10)

        # Create folder selection combobox
        tk.Label(control_frame, text="Select Folder:").pack(anchor="w")
        self.folder_combobox = ttk.Combobox(control_frame)
        self.folder_combobox.pack(fill=tk.X, pady=5)
        self.update_folder_list()

        # Create start button
        start_button = tk.Button(control_frame, text="Start", command=self.start_capture)
        start_button.pack(fill=tk.X, pady=5)

        start_button = tk.Button(control_frame, text="Refresh List", command=self.update_folder_list)
        start_button.pack(fill=tk.X, pady=5)

    def update_folder_list(self):
        folders = [f for f in os.listdir(DATA_PATH) if os.path.isdir(os.path.join(DATA_PATH, f))]
        self.folder_combobox['values'] = folders

    def start_capture(self):
        selected_folder = self.folder_combobox.get()
        folder_path = os.path.join(DATA_PATH, selected_folder)

        if not selected_folder:
            messagebox.showwarning("Warning", "Please select a folder.")
            return

        size = self.get_folder_size(folder_path)

        if size > 0:
            if not messagebox.askyesno("Overwrite",
                                       f"Folder '{selected_folder}' already contains data. Do you want to overwrite it?"):
                return

        self.capture_video(folder_path, selected_folder)

    def get_folder_size(self, folder):
        total_size = 0
        for dirpath, dirnames, filenames in os.walk(folder):
            for f in filenames:
                fp = os.path.join(dirpath, f)
                total_size += os.path.getsize(fp)
        return total_size

    def capture_video(self, folder_path, action):
        cap = cv2.VideoCapture(0)
        # Set mediapipe model
        with mp_holistic.Holistic(min_detection_confidence=0.5, min_tracking_confidence=0.5) as holistic:
            os.makedirs(folder_path, exist_ok=True)
            # Loop through sequences aka videos
            for sequence in range(no_sequences + 1):
                sequence_path = os.path.join(folder_path, str(sequence))
                os.makedirs(sequence_path, exist_ok=True)

                # Loop through video length aka sequence length
                for frame_num in range(sequence_length):
                    # Read feed
                    ret, frame = cap.read()

                    # Make detections
                    image, results = mediapipe_detection(frame, holistic)

                    # Draw landmarks
                    draw_styled_landmarks(image, results)

                    # NEW Apply wait logic
                    if frame_num == 0:
                        cv2.putText(image, 'STARTING COLLECTION', (120, 200),
                                    cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 4, cv2.LINE_AA)
                        cv2.putText(image, 'Collecting frames for {} Video Number {}'.format(action, sequence),
                                    (15, 12),
                                    cv2.FONT_HERSHEY_SIMPLEX, 0.5, (0, 0, 255), 1, cv2.LINE_AA)
                        # Show to screen
                        cv2.imshow('OpenCV Feed', image)
                        cv2.waitKey(500)
                    else:
                        cv2.putText(image, 'Collecting frames for {} Video Number {}'.format(action, sequence),
                                    (15, 12),
                                    cv2.FONT_HERSHEY_SIMPLEX, 0.5, (0, 0, 255), 1, cv2.LINE_AA)
                        # Show to screen
                        cv2.imshow('OpenCV Feed', image)

                    # NEW Export keypoints
                    keypoints = extract_keypoints(results)
                    npy_path = os.path.join(sequence_path, str(frame_num))
                    np.save(npy_path, keypoints)

                    # Break gracefully
                    if cv2.waitKey(10) & 0xFF == ord('q'):
                        break

            cap.release()
            cv2.destroyAllWindows()

        cap.release()
        cv2.destroyAllWindows()