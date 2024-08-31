import tkinter as tk
from tkinter import ttk, messagebox
from Common import Capture_From_youTube_List as capYT
import json

class YouTubeCaptureView(tk.Frame):
    def __init__(self, parent):
        super().__init__(parent)
        self.pack(fill=tk.BOTH, expand=True)
        self.sign_data = self.load_grouped_data()
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
        self.log("Enter a list of signs from MS-ASL.\n")

        # Create frame for controls
        control_frame = tk.Frame(main_frame)
        control_frame.pack(side=tk.RIGHT, fill=tk.Y, padx=10)

        # Sign name
        tk.Label(control_frame, text="Enter Sign Name:").pack(anchor="w")
        self.name_entry = tk.Entry(control_frame)
        self.name_entry.pack(fill=tk.X, pady=5)

        # Sign name search and select
        tk.Label(control_frame, text="Select Sign:").pack(anchor="w")
        self.sign_combobox = ttk.Combobox(control_frame, values=list(self.sign_data.keys()))
        self.sign_combobox.pack(fill=tk.X, pady=5)
        self.sign_combobox.bind("<<ComboboxSelected>>", self.on_sign_selected)

        self.preview = tk.Checkbutton(control_frame, text="Preview Capture")
        self.preview.pack(fill=tk.X, pady=5)
        # Create start button
        start_button = tk.Button(control_frame, text="Start", command=self.start_capture)
        start_button.pack(fill=tk.X, pady=5)

    def on_sign_selected(self, event):
        selected_sign = self.sign_combobox.get()
        if selected_sign in self.sign_data:
            sign_list = self.sign_data[selected_sign]
            self.text_entry.delete("1.0", tk.END)
            self.text_entry.insert(tk.END, json.dumps(sign_list, indent=4))
            self.log(f"Selected sign '{selected_sign}' with {len(sign_list)} entries.")

    def log(self, message):
        self.log_text.configure(state='normal')
        self.log_text.insert(tk.END, message + "\n")
        self.log_text.configure(state='disabled')
        self.log_text.see(tk.END)

    def start_capture(self):
        self.log("starting")
        input_text = self.text_entry.get("1.0", tk.END).strip()
        if input_text is None or input_text == '':
            self.log("Please enter at least one dataset entry!")
            messagebox.showinfo("Invalid Input", f"Please enter at least one dataset entry!")
            return
        # Wrap the input with square brackets if not already present
        if not (input_text.startswith("[") and input_text.endswith("]")):
            input_text = f"[{input_text}]"

        try:
            dataset_entries = json.loads(input_text)

            # Ensure the input is a list of dictionaries
            if isinstance(dataset_entries, list) and all(isinstance(entry, dict) for entry in dataset_entries):
                self.log("Valid dataset entries received.")
                action = self.name_entry.get().strip()
                if action:
                    self.log(f"Processing entries with action: {action}")
                    capYT.process_dataset_entry_list(dataset_entries, action,True)
                    self.log(f"Done Processing action: {action}")
                else:
                    self.log("Please enter a sign name to proceed.")
            else:
                raise ValueError("Input is not a valid list of dictionaries.")

        except json.JSONDecodeError:
            self.log("Invalid JSON format. Please correct the input.")
        except ValueError as ve:
            self.log(str(ve))

    @staticmethod
    def load_grouped_data():
        try:
            with open("Common/SourceData/grouped_ms_asl_dataset.json", "r") as file:
                return json.load(file)
        except Exception as e:
            messagebox.showinfo("Error", f"Error loading grouped data: {e}")
            return {}

