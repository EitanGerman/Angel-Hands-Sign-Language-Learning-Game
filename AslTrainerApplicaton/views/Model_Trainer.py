import tkinter as tk
from pathlib import Path
from tkinter import messagebox
from datetime import datetime
import os
import numpy as np
from sklearn.model_selection import train_test_split
from sklearn.metrics import multilabel_confusion_matrix, accuracy_score
from keras.utils import to_categorical
from keras.callbacks import TensorBoard, Callback
import threading
from Common import Action_recognition
import json
import shutil

# Define constants
DATA_PATH = 'MP_Data'
no_sequences = 30
sequence_length = 30
Num_Epochs = 300
Train_Test_split = 0.1


class TextRedirector(Callback):
    def __init__(self, log_widget):
        super().__init__()
        self.log_widget = log_widget

    def on_epoch_end(self, epoch, logs=None):
        logs = logs or {}
        log_message = f"Epoch {epoch + 1}: " + ", ".join([f"{key}: {value:.4f}" for key, value in logs.items()])
        self.log(log_message + "\n")

    def log(self, message):
        self.log_widget.configure(state='normal')
        self.log_widget.insert(tk.END, message + "\n")
        self.log_widget.configure(state='disabled')
        self.log_widget.see(tk.END)


class ModelTrainer(tk.Frame):
    def __init__(self, parent):
        super().__init__(parent)
        self.pack(fill=tk.BOTH, expand=True)
        self.create_widgets()

    def create_widgets(self):
        main_frame = tk.Frame(self)
        main_frame.pack(fill=tk.BOTH, expand=True, padx=10, pady=10)

        list_frame = tk.Frame(main_frame)
        list_frame.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)

        # Create Epochs entry
        self.Epochs_label = tk.Label(list_frame, text="Epochs:")
        self.Epochs_label.pack(pady=5)
        self.Epochs_entry = tk.Entry(list_frame)
        self.Epochs_entry.insert(0, str(Num_Epochs))
        self.Epochs_entry.pack(pady=5)

        # Create Train Test split entry
        self.Train_Test_split_label = tk.Label(list_frame, text="Train test split:")
        self.Train_Test_split_label.pack(pady=5)
        self.Train_Test_split_entry = tk.Entry(list_frame)
        self.Train_Test_split_entry.insert(0, str(Train_Test_split))
        self.Train_Test_split_entry.pack(pady=5)

        tk.Label(list_frame, text="Folders to Train On:").pack(anchor="w")
        self.folder_listbox = tk.Listbox(list_frame, height=20)
        self.folder_listbox.pack(fill=tk.BOTH, expand=True, pady=5)

        # Create refresh button
        self.refresh_button = tk.Button(list_frame, text="Refresh List", command=self.update_folder_list)
        self.refresh_button.pack(pady=10)
        self.update_folder_list()

        control_frame = tk.Frame(main_frame)
        control_frame.pack(side=tk.RIGHT, fill=tk.Y, padx=10)

        self.train_button = tk.Button(control_frame, text="Run Training", command=self.run_training_thread)
        self.train_button.pack(fill=tk.X, pady=5)

        self.log_text = tk.Text(control_frame, height=20, state='disabled')
        self.log_text.pack(fill=tk.BOTH, expand=True, pady=5)
        self.log_text.bind("<Key>", lambda e: "break")

        self.log_text.insert(tk.END, "Ready to train the model.\n")
        self.log_text.see(tk.END)

    def run_training_thread(self):
        self.train_button.config(state=tk.DISABLED)
        threading.Thread(target=self.run_training).start()

    def update_folder_list(self):
        try:
            with open("selected_folders.txt", "r") as f:
                folders = f.read().splitlines()
                self.folder_listbox.delete(0, tk.END)
                for folder in folders:
                    self.folder_listbox.insert(tk.END, folder)
        except FileNotFoundError:
            messagebox.showerror("Error", "The selected_folders.txt file was not found.")

    def log(self, message):
        self.log_text.configure(state='normal')
        self.log_text.insert(tk.END, message + "\n")
        self.log_text.configure(state='disabled')
        self.log_text.see(tk.END)

    def run_training(self):
        self.log("Starting training...")
        selected_folders = self.folder_listbox.get(0, tk.END)

        if not selected_folders:
            messagebox.showwarning("Warning", "No folders selected for training.")
            return

        try:
            self.train_model(selected_folders)
        except Exception as e:
            messagebox.showerror("Error", f"An error occurred: {e}")
            self.log(f"Error: {e}")

    def train_model(self, selected_folders):
        label_map = {label: num for num, label in enumerate(selected_folders)}
        sequences, labels = [], []

        for action in selected_folders:
            numeric_dirs = [d for d in os.listdir(os.path.join(DATA_PATH, action))
                            if os.path.isdir(os.path.join(DATA_PATH, action, d))
                            and d.isdigit()]
            numeric_dirs_int = np.array(numeric_dirs).astype(int)
            for sequence in numeric_dirs_int:
                window = []
                for frame_num in range(sequence_length):
                    res = np.load(os.path.join(DATA_PATH, action, str(sequence), "{}.npy".format(frame_num)))
                    window.append(res)
                sequences.append(window)
                labels.append(label_map[action])

        X = np.array(sequences)
        y = to_categorical(labels).astype(int)

        X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=float(self.Train_Test_split_entry.get().strip()))

        log_dir = os.path.join('Logs')
        tb_callback = TensorBoard(log_dir=log_dir)
        log_callback = TextRedirector(self.log_text)

        model = Action_recognition.ActionRecognition.build_model(len(selected_folders))

        self.log("Training the model...")
        model.fit(X_train, y_train, epochs=int(self.Epochs_entry.get().strip()), callbacks=[tb_callback, log_callback])

        self.log("Training complete. Evaluating the model...")
        res = model.predict(X_test)
        ytrue = np.argmax(y_test, axis=1).tolist()
        yhat = np.argmax(res, axis=1).tolist()
        self.log(f"Accuracy: {accuracy_score(ytrue, yhat)}")

        # self.log("Saving the model...")
        # save_dir = os.path.join('Models', datetime.now().strftime('%Y-%m-%d_%H-%M'))
        # os.makedirs(save_dir, exist_ok=True)
        # model.save(os.path.join(save_dir, 'actions.keras'))
        # with open(os.path.join(save_dir, 'selected_folders.txt'), 'w') as f:
        #     for folder in selected_folders:
        #         f.write(folder + '\n')
        self.save_model_and_metadata(model, selected_folders)
        self.log("Model saved successfully.")

        self.train_button.config(state=tk.NORMAL)

    def save_model_and_metadata(self, model, selected_folders):
        self.log("Saving the model...")
        save_dir = os.path.join('Models', datetime.now().strftime('%Y-%m-%d_%H-%M'))
        os.makedirs(save_dir, exist_ok=True)

        # Save the model
        model.save(os.path.join(save_dir, 'actions.keras'))

        # Save the list of selected folders
        with open(os.path.join(save_dir, 'selected_folders.txt'), 'w') as f:
            for folder in selected_folders:
                f.write(folder + '\n')

        # Create Resources directory
        resources_dir = os.path.join(save_dir, 'Resources')
        os.makedirs(resources_dir, exist_ok=True)

        model_data = []
        expected_words = set()  # Keep track of all expected words

        for folder in selected_folders:
            metadata_folder = os.path.join('MP_Data\\'+folder, 'MetaData')
            recordings_folder = os.path.join(metadata_folder, 'Recordings')
            sign_data_folder = os.path.join(metadata_folder, 'SignData')

            # Extract folder name for Resources
            folder_name = os.path.basename(folder)
            resources_folder = os.path.join(resources_dir, folder_name)
            os.makedirs(resources_folder, exist_ok=True)

            # Copy Recordings folder
            if os.path.exists(recordings_folder):
                shutil.copytree(recordings_folder, os.path.join(resources_folder, 'Recordings'), dirs_exist_ok=True)

            expected_words.add(folder_name)
            # Combine JSON files
            for json_file in Path(sign_data_folder).glob('*.json'):
                with open(json_file, 'r') as f:
                    data = json.load(f)
                    data['recordings_path'] = os.path.join('Resources', folder_name, 'Recordings')
                    model_data.append(data)

        # Add placeholder entries for missing JSON files
        for word in expected_words:
            if not any(entry['word'] == word for entry in model_data):
                model_data.append({
                    "word": word,
                    "urls": [],
                    "recordings_path": ''#os.path.join('Resources', word, 'Recordings')
                })
        # Save combined JSON data
        with open(os.path.join(save_dir, 'ModelData.json'), 'w') as f:
            json.dump(model_data, f, indent=4)

        self.log("Model and metadata saved successfully.")