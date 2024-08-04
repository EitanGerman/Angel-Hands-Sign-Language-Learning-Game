import os
import tkinter as tk
from tkinter import ttk, filedialog, messagebox
import cv2
from PIL import Image, ImageTk
from action_detection_refined_Runner_View import ActionRecognition
from datetime import datetime


class ModelPreview:
    def __init__(self, root):
        self.root = root
        self.frame = tk.Frame(root)
        self.frame.pack(expand=1, fill="both")

        # Create a frame for the video and the buttons
        self.video_frame = tk.Frame(self.frame)
        self.video_frame.pack(side="left", expand=1, fill="both")

        self.control_frame = tk.Frame(self.frame)
        self.control_frame.pack(side="right", fill="y")

        self.model_running = False
        self.show_video = False

        self.video_label = tk.Label(self.video_frame)
        self.video_label.pack(pady=10)

        # Create a separator between video and control frames
        self.separator = ttk.Separator(self.frame, orient='vertical')
        self.separator.pack(side="left", fill="y", padx=5)

        self.model_button = tk.Button(self.control_frame, text="Start Model", command=self.toggle_model)
        self.model_button.pack(fill='x', pady=5, padx=10)

        self.preview_button = tk.Button(self.control_frame, text="Start Preview", command=self.toggle_preview)
        self.preview_button.pack(fill='x', pady=5, padx=10)

        # Create refresh button
        self.refresh_button = tk.Button(self.control_frame, text="Refresh List", command=self.load_model_folders)
        self.refresh_button.pack(pady=10)

        self.cap = None

        self.action_recognition = ActionRecognition()

        self.load_model_button = tk.Button(self.control_frame, text="Load Model Folder", command=self.load_model_folder)
        self.load_model_button.pack(fill='x', pady=5, padx=10)

        self.model_folder_var = tk.StringVar()
        self.model_folder_dropdown = ttk.Combobox(self.control_frame, textvariable=self.model_folder_var)
        self.model_folder_dropdown.pack(fill='x', pady=5, padx=10)

        self.model_folder_var.trace('w', self.on_model_folder_change)

        self.labels_listbox = tk.Listbox(self.control_frame, height=10)
        self.labels_listbox.pack(fill='both', pady=5, padx=10)

        self.load_model_folders()

    def load_model_folders(self):
        model_dir = 'Models'
        if not os.path.exists(model_dir):
            os.makedirs(model_dir)

        folders = sorted([f for f in os.listdir(model_dir) if os.path.isdir(os.path.join(model_dir, f))], reverse=True)
        self.model_folder_dropdown['values'] = folders
        if folders:
            self.model_folder_dropdown.current(0)

    def load_model_folder(self):
        folder = filedialog.askdirectory(initialdir='Models', title='Select Model Folder')
        if folder:
            folder_name = os.path.basename(folder)
            if folder_name not in self.model_folder_dropdown['values']:
                self.model_folder_dropdown['values'] = (*self.model_folder_dropdown['values'], folder_name)
            self.model_folder_var.set(folder_name)

    def on_model_folder_change(self, *args):
        selected_folder = self.model_folder_var.get()
        model_path = os.path.join('Models', selected_folder, 'actions.keras')
        folders_file = os.path.join('Models', selected_folder, 'selected_folders.txt')
        self.labels_listbox.delete(0, tk.END)
        if os.path.exists(model_path) and os.path.exists(folders_file):
            with open(folders_file, 'r') as f:
                folders = f.read().splitlines()
                self.action_recognition.load_model(model_path, folders)
                for folder in folders:
                    self.labels_listbox.insert(tk.END, folder)
            messagebox.showinfo("Model Loaded", f"Model and folders loaded from {selected_folder}")
        else:
            messagebox.showerror("Error", "Selected folder does not contain the required files")

    def toggle_model(self):
        if self.model_running:
            self.stop_model()
        else:
            self.start_model()

    def start_model(self):
        self.action_recognition.run_model()
        self.model_running = True
        self.model_button.config(text="Stop Model")

    def stop_model(self):
        self.model_running = False
        self.show_video = False
        if self.cap:
            self.cap.release()
            self.cap = None
        self.video_label.config(image='')
        self.model_button.config(text="Start Model")
        self.preview_button.config(text="Start Preview")

    def toggle_preview(self):
        if self.show_video:
            self.stop_preview()
        else:
            self.start_preview()

    def start_preview(self):
        self.show_video = True
        self.preview_button.config(text="Stop Video")

    def stop_preview(self):
        self.show_video = False
        self.preview_button.config(text="Start Preview")

    def update_video_stream(self):
        if self.model_running and self.cap and self.cap.isOpened():
            ret, frame = self.cap.read()
            if ret:
                if self.show_video:
                    image = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
                    image = Image.fromarray(image)
                    image = ImageTk.PhotoImage(image)
                    self.video_label.config(image=image)
                    self.video_label.image = image

            self.root.after(10, self.update_video_stream)


# import tkinter as tk
# from tkinter import ttk
# import cv2
# from PIL import Image, ImageTk
# from action_detection_refined_Runner_View import ActionRecognition
#
# class ModelPreview:
#     def __init__(self, root):
#         self.root = root
#         self.frame = tk.Frame(root)
#         self.frame.pack(expand=1, fill="both")
#
#         # Create a frame for the video and the buttons
#         self.video_frame = tk.Frame(self.frame)
#         self.video_frame.pack(side="left", expand=1, fill="both")
#
#         self.control_frame = tk.Frame(self.frame)
#         self.control_frame.pack(side="right", fill="y")
#
#         self.model_running = False
#         self.show_video = False
#
#         self.video_label = tk.Label(self.video_frame)
#         self.video_label.pack(pady=10)
#
#         # Create a separator between video and control frames
#         self.separator = ttk.Separator(self.frame, orient='vertical')
#         self.separator.pack(side="left", fill="y", padx=5)
#
#         self.model_button = tk.Button(self.control_frame, text="Start Model", command=self.toggle_model)
#         self.model_button.pack(fill='x', pady=5, padx=10)
#
#         self.preview_button = tk.Button(self.control_frame, text="Start Preview", command=self.toggle_preview)
#         self.preview_button.pack(fill='x', pady=5, padx=10)
#
#         self.cap = None
#
#         self.action_recognition = ActionRecognition()
#
#     def toggle_model(self):
#         if self.model_running:
#             self.stop_model()
#         else:
#             self.start_model()
#
#     def start_model(self):
#         self.action_recognition.run_model()
#         self.model_running = True
#         #self.cap = cv2.VideoCapture(0)
#         #self.update_video_stream()
#         self.model_button.config(text="Stop Model")
#
#     def stop_model(self):
#         self.model_running = False
#         self.show_video = False
#         if self.cap:
#             self.cap.release()
#             self.cap = None
#         self.video_label.config(image='')
#         self.model_button.config(text="Start Model")
#         self.preview_button.config(text="Start Preview")
#
#     def toggle_preview(self):
#         if self.show_video:
#             self.stop_preview()
#         else:
#             self.start_preview()
#
#     def start_preview(self):
#         self.show_video = True
#         self.preview_button.config(text="Stop Video")
#
#     def stop_preview(self):
#         self.show_video = False
#         self.preview_button.config(text="Start Preview")
#
#     def update_video_stream(self):
#         if self.model_running and self.cap.isOpened():
#             ret, frame = self.cap.read()
#             if ret:
#                 if self.show_video:
#                     image = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
#                     image = Image.fromarray(image)
#                     image = ImageTk.PhotoImage(image)
#                     self.video_label.config(image=image)
#                     self.video_label.image = image
#
#             self.root.after(10, self.update_video_stream)
