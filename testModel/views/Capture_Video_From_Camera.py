import tkinter as tk
from tkinter import ttk, messagebox
import cv2
import mediapipe as mp
import os
import numpy as np
import Common.Utils as utils

# Define constants
actions = np.array(['hello', 'thanks', 'iloveyou'])
DATA_PATH = 'MP_Data'
no_sequences = 30
sequence_length = 30

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
        with mp.solutions.holistic.Holistic(min_detection_confidence=0.5, min_tracking_confidence=0.5) as holistic:
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
                    image, results = utils.mediapipe_detection(frame, holistic)

                    # Draw landmarks
                    utils.draw_styled_landmarks(image, results)

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
                    keypoints = utils.extract_keypoints(results)
                    npy_path = os.path.join(sequence_path, str(frame_num))
                    np.save(npy_path, keypoints)

                    # Break gracefully
                    if cv2.waitKey(10) & 0xFF == ord('q'):
                        break

            cap.release()
            cv2.destroyAllWindows()

        cap.release()
        cv2.destroyAllWindows()