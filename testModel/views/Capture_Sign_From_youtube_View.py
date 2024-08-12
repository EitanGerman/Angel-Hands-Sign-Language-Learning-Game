import tkinter as tk
from tkinter import ttk, filedialog, messagebox
from Common import Capture_From_youTube_List as capYT

class YouTubeCaptureView(tk.Frame):
    def __init__(self, parent):
        super().__init__(parent)
        self.pack(fill=tk.BOTH, expand=True)
        self.create_widgets()

    def create_widgets(self):
        # Create main frame
        main_frame = tk.Frame(self)
        main_frame.pack(fill=tk.BOTH, expand=True, padx=10, pady=10)

        # Create canvas for OpenCV window
        self.left_frame = tk.Frame(main_frame, width=640, height=480)
        self.left_frame.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)

        self.text_entry = tk.Text(self.left_frame, wrap=tk.WORD, width=50, height=20)
        self.text_entry.pack(pady=10, fill=tk.BOTH, expand=True)

        tk.Label(self.left_frame, text="Run Log:").pack(anchor="w")

        self.log_text = tk.Text(self.left_frame, height=15, state='disabled',)
        self.log_text.pack(fill=tk.BOTH, expand=True, pady=5)
        self.log_text.bind("<Key>", lambda e: "break")
        self.log_text.configure(state='normal')
        self.log_text.insert(tk.END, "Enter a list of signs from MS-ASL.\n")
        self.log_text.see(tk.END)
        self.log_text.configure(state='disabled')

        # Create frame for controls
        control_frame = tk.Frame(main_frame)
        control_frame.pack(side=tk.RIGHT, fill=tk.Y, padx=10)

        # Sign name
        tk.Label(control_frame, text="Enter Sign Name:").pack(anchor="w")
        self.name_entry = tk.Entry(control_frame)
        self.name_entry.pack(fill=tk.X, pady=5)

        # Create start button
        start_button = tk.Button(control_frame, text="Start", command=self.start_capture)
        start_button.pack(fill=tk.X, pady=5)

    def start_capture(self):
        self.log("starting")


    def log(self, message):
        self.log_text.configure(state='normal')
        self.log_text.insert(tk.END, message + "\n")
        self.log_text.configure(state='disabled')
        self.log_text.see(tk.END)