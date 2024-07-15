from cx_Freeze import setup, Executable

additional_files = ['action.keras']

setup(
    name="ModelServer",
    version="1.0",
    description="My Python script",
    executables=[Executable("ModelServer.py")],
    options={
        'build_exe': {
            'include_files': additional_files,
        }
    }
)