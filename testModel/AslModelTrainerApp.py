import tkinter as tk
from tkinter import ttk
from views.Model_Preview import ModelPreview
from views.Model_Trainer import ModelTrainer
from views.folder_manager import FolderManager
from views.Capture_Video_From_Camera import FolderFiller


class App:
    def __init__(self, root):
        self.notebook = ttk.Notebook(root)
        self.notebook.pack(expand=1, fill="both")

        # Create frames for each tab
        self.model_preview_tab = tk.Frame(self.notebook)
        self.notebook.add(self.model_preview_tab, text="Model Preview")
        self.model_preview = ModelPreview(self.model_preview_tab)

        # Tab 2 - Folder Manager
        self.folder_manager_tab = tk.Frame(self.notebook)
        self.notebook.add(self.folder_manager_tab, text="Folder Manager")
        self.folder_manager = FolderManager(self.folder_manager_tab)

        # Tab 3 - Folder Creator
        self.folder_manager_tab = tk.Frame(self.notebook)
        self.notebook.add(self.folder_manager_tab, text="Capture Sign")
        self.folder_filler = FolderFiller(self.folder_manager_tab)

        # Tab 4 - Model Trainer
        self.model_trainer_tab = tk.Frame(self.notebook)
        self.notebook.add(self.model_trainer_tab, text="Model Trainer")
        self.model_trainer = ModelTrainer(self.model_trainer_tab)


if __name__ == "__main__":
    root = tk.Tk()
    root.title("My Application")
    root.geometry("800x600")

    app = App(root)
    root.mainloop()
