from cx_Freeze import setup, Executable

additional_files = ['Models/2024-08-04_19-29(500EP)/']

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


# run in console like that
# python setup.py build
