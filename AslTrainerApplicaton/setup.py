from cx_Freeze import setup, Executable
import sys

# Determine if the base should be set to "Win32GUI"
# base = None
# if sys.platform == "win32":
#     base = "Win32GUI"  # This hides the console window

additional_files = ['Models/2024-08-04_19-29(500EP)/']

setup(
    name="ModelServer",
    version="1.0",
    description="My Python script",
    executables=[Executable("ModelServer.py")],#, base=base
    options={
        'build_exe': {
            'include_files': additional_files,
            # 'optimize': 2  # Uncomment if you want to enable optimizations
        }
    }
)



# run in console like that
# python setup.py build
# python setup.py build -O
