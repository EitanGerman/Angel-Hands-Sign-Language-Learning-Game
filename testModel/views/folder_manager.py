import os
import tkinter as tk
from tkinter import ttk, messagebox

class FolderManager:
    def __init__(self, root):
        self.root = root
        self.selected_folders = self.load_selected_folders()
        self.show_empty_folders = False
        self.create_widgets()

    def create_widgets(self):
        # Create main frame
        self.frame = tk.Frame(self.root)
        self.frame.pack(expand=1, fill="both")

        # Create a horizontal layout frame
        self.main_layout = tk.Frame(self.frame)
        self.main_layout.pack(expand=1, fill="both")

        # Create a vertical frame for the scroll pane
        self.scroll_frame = tk.Frame(self.main_layout)
        self.scroll_frame.pack(side="left", fill="both", expand=1)

        # Create a vertical separator
        self.separator = tk.Frame(self.main_layout, width=2, bg="grey")
        self.separator.pack(side="left", fill="y")

        # Create a frame for the right-side buttons
        self.right_frame = tk.Frame(self.main_layout)
        self.right_frame.pack(side="left", fill="y")

        # Create a canvas for the scroll pane
        self.canvas = tk.Canvas(self.scroll_frame)
        self.canvas.pack(side="left", fill="both", expand=1)

        # Create a vertical scrollbar linked to the canvas
        self.scrollbar = tk.Scrollbar(self.scroll_frame, orient="vertical", command=self.canvas.yview)
        self.scrollbar.pack(side="right", fill="y")
        self.canvas.configure(yscrollcommand=self.scrollbar.set)

        # Create a frame inside the canvas to hold the checkboxes and sizes
        self.checkbox_frame = tk.Frame(self.canvas)
        self.canvas.create_window((0, 0), window=self.checkbox_frame, anchor="nw")

        # Store the variable references
        self.var_dict = {}

        # Create a button to toggle empty folder visibility
        self.toggle_empty_folders_button = tk.Button(self.right_frame, text="Show Empty Folders", command=self.toggle_empty_folders)
        self.toggle_empty_folders_button.pack(pady=10)

        # Create refresh button
        self.refresh_button = tk.Button(self.right_frame, text="Refresh List", command=self.update_folder_list)
        self.refresh_button.pack(pady=10)

        # Create save selection button
        self.save_button = tk.Button(self.right_frame, text="Save Selection", command=self.save_selection)
        self.save_button.pack(pady=10)

        # Create horizontal separator below the save button
        self.h_separator = tk.Frame(self.right_frame, height=2, bg="grey")
        self.h_separator.pack(fill="x", pady=5)

        # Create folder name entry
        self.create_folder_label = tk.Label(self.right_frame, text="Add Folder Name:")
        self.create_folder_label.pack(pady=5)
        self.create_folder_entry = tk.Entry(self.right_frame)
        self.create_folder_entry.pack(pady=5)

        # Create number of subfolders entry with default value 30
        self.num_subfolders_label = tk.Label(self.right_frame, text="Number of Subfolders:")
        self.num_subfolders_label.pack(pady=5)
        self.num_subfolders_entry = tk.Entry(self.right_frame)
        self.num_subfolders_entry.insert(0, '30')  # Set default number to 30
        self.num_subfolders_entry.pack(pady=5)

        # Create button to create new folder(s)
        self.create_folder_button = tk.Button(self.right_frame, text="Create Folder(s)", command=self.create_folders)
        self.create_folder_button.pack(pady=10)

        # Add horizontal separator for Delete Folder section
        separator_horizontal_delete = ttk.Separator(self.right_frame, orient=tk.HORIZONTAL)
        separator_horizontal_delete.pack(side=tk.TOP, fill=tk.X, pady=5)

        # Delete folder name entry and button
        delete_folder_label = tk.Label(self.right_frame, text="Delete Folder Name:")
        delete_folder_label.pack(side=tk.TOP, anchor="w")

        self.delete_folder_name_entry = tk.Entry(self.right_frame)
        self.delete_folder_name_entry.pack(side=tk.TOP, fill=tk.X, pady=5)

        delete_folder_button = tk.Button(self.right_frame, text="Delete Folder", command=self.delete_folder)
        delete_folder_button.pack(side=tk.TOP, fill=tk.X, pady=5)

        # Initial update of folder list
        self.update_folder_list()

    def update_folder_list(self):
        # Clear existing widgets in the checkbox frame
        for widget in self.checkbox_frame.winfo_children():
            widget.destroy()

        folders = [f for f in os.listdir('MP_Data') if os.path.isdir(os.path.join('MP_Data', f))]
        for folder in folders:
            folder_path = os.path.join('MP_Data', folder)
            size = self.get_folder_size(folder_path)

            # Only display folders based on current setting
            if size > 0 or self.show_empty_folders:
                var = tk.BooleanVar(value=folder in self.selected_folders)
                chk = tk.Checkbutton(self.checkbox_frame, text=f"{folder} ({size} bytes)", variable=var)
                if size > 0:
                    chk.pack(anchor="w")
                else:
                    chk.config(state=tk.DISABLED)
                    chk.pack(anchor="w")

                # Store the variable and widget name
                self.var_dict[folder] = var
                var.widget_name = chk.winfo_name()

        # Update canvas scroll region
        self.canvas.update_idletasks()
        self.canvas.config(scrollregion=self.canvas.bbox("all"))

    def get_folder_size(self, folder_path):
        total_size = 0
        for dirpath, dirnames, filenames in os.walk(folder_path):
            for f in filenames:
                filepath = os.path.join(dirpath, f)
                total_size += os.path.getsize(filepath)
        return total_size

    def save_selection(self):
        selected_folders = []
        for folder, var in self.var_dict.items():
            chk = self.checkbox_frame.nametowidget(var.widget_name)
            if var.get() and chk.cget('state') != tk.DISABLED:
                selected_folders.append(folder)

        with open('selected_folders.txt', 'w') as file:
            file.write('\n'.join(selected_folders))

        messagebox.showinfo("Saved", "Selected folders saved to 'selected_folders.txt'.")

    def create_folders(self):
        folder_name = self.create_folder_entry.get().strip()
        num_subfolders = self.num_subfolders_entry.get().strip()

        if not folder_name:
            messagebox.showwarning("Input Error", "Folder name cannot be empty.")
            return

        if not num_subfolders.isdigit() or int(num_subfolders) < 1:
            messagebox.showwarning("Input Error", "Number of subfolders must be a positive integer.")
            return

        num_subfolders = int(num_subfolders)
        base_path = 'MP_Data'
        full_path = os.path.join(base_path, folder_name)

        if os.path.exists(full_path):
            response = messagebox.askyesno("Folder Exists", "Folder already exists. Do you want to override it?")
            if response:
                self._create_new_folders(full_path, num_subfolders)
        else:
            self._create_new_folders(full_path, num_subfolders)

    def _create_new_folders(self, path, num_subfolders):
        try:
            os.makedirs(path)
            for i in range(1, num_subfolders + 1):
                subfolder_path = os.path.join(path, str(i))
                os.makedirs(subfolder_path)
            messagebox.showinfo("Success", f"Folder '{path}' created with {num_subfolders} subfolders.")
            self.update_folder_list()  # Update the folder list to reflect the new folder
        except Exception as e:
            messagebox.showerror("Error", f"An error occurred: {e}")

    def load_selected_folders(self):
        selected_folders = set()
        if os.path.exists('selected_folders.txt'):
            with open('selected_folders.txt', 'r') as file:
                selected_folders = set(file.read().splitlines())
        return selected_folders

    def toggle_empty_folders(self):
        self.show_empty_folders = not self.show_empty_folders
        if self.show_empty_folders:
            self.toggle_empty_folders_button.config(text="Hide Empty Folders")
        else:
            self.toggle_empty_folders_button.config(text="Show Empty Folders")
        self.update_folder_list()

    def delete_folder(self):
        folder_name = self.delete_folder_name_entry.get().strip()
        folder_path = os.path.join('MP_Data', folder_name)

        if not folder_name or not os.path.exists(folder_path):
            messagebox.showwarning("Warning", "Folder does not exist.")
            return

        if messagebox.askyesno("Delete", f"Are you sure you want to delete folder '{folder_name}'?"):
            for root, dirs, files in os.walk(folder_path, topdown=False):
                for name in files:
                    os.remove(os.path.join(root, name))
                for name in dirs:
                    os.rmdir(os.path.join(root, name))
            os.rmdir(folder_path)
            messagebox.showinfo("Success", f"Folder '{folder_name}' deleted.")
            self.update_folder_list()