import os
import subprocess
import sys


def create_virtual_environment(venv_name="venv"):
    # Create a virtual environment
    subprocess.check_call([sys.executable, "-m", "venv", venv_name])
    print(f"Virtual environment '{venv_name}' created successfully.")


def install_requirements(venv_name="venv"):
    # Install packages from requirements.txt using pip within the virtual environment
    pip_path = os.path.join(venv_name, "Scripts", "pip") if os.name == "nt" else os.path.join(venv_name, "bin", "pip")
    subprocess.check_call([pip_path, "install", "-r", "../requirements.txt"])
    print("All packages installed successfully from requirements.txt.")


def main():
    venv_name = "venv"

    # Check if virtual environment already exists
    if not os.path.exists(venv_name):
        create_virtual_environment(venv_name)
    else:
        print(f"Virtual environment '{venv_name}' already exists.")

    install_requirements(venv_name)

    print(f"To activate the virtual environment, run:")
    if os.name == "nt":  # Windows
        print(f".\\{venv_name}\\Scripts\\activate")
    else:  # macOS/Linux
        print(f"source ./{venv_name}/bin/activate")


if __name__ == "__main__":
    main()
