import argparse
from Common.Action_recognition import ActionRecognition
from Common.SharedMemoryPlugin import CameraFeed


def main(model_path,show_video=False,port=None,width=640,height=360,threshold=0.7,sensitivity=10):
    shmp = CameraFeed(show_video,width,height)
    shmp.start()
    ac = ActionRecognition()
    ac.load_model(model_path)
    ac.run_model(show_video,port,width,height,threshold,sensitivity)
    shmp.stop()
    exit(0)


if __name__ == "__main__":
    # Create the parser
    parser = argparse.ArgumentParser(description="Run Action Recognition model")

    # Add the argument for the model path
    parser.add_argument(
        "model_path",
        type=str,
        help="Path to the model file"
    )

    parser.add_argument(
        "--show_video",
        action="store_true",
        help="Flag to show video output"
    )

    parser.add_argument(
        "--server_port",
        type=int,
        default=None,
        help="Tcp sign transmition port (default: 65431)"
    )

    parser.add_argument(
        "--Width",
        type=int,
        default=640,
        help="Video Width (default: 640)"
    )

    parser.add_argument(
        "--Height",
        type=int,
        default=360,
        help="Video Height (default: 360)"
    )

    parser.add_argument(
        "--Threshold",
        type=float,
        default=0.7,
        help="Model Threshold (default: 0.7)",
        choices=[x / 100.0 for x in range(101)]  # Limits the range from 0 to 1
    )

    parser.add_argument(
        "--Sensitivity",
        type=int,
        default=10,
        help="Model Sensitivity (default: 10)",
    )

    # Parse the arguments
    args = parser.parse_args()

    # Run the main function with the provided model path
    main(args.model_path,show_video=args.show_video,port=args.server_port,width=args.Width,height=args.Height,threshold=args.Threshold,sensitivity=args.Sensitivity)


# from Common.Action_recognition import ActionRecognition
#
# model_path = "Models/2024-08-04_19-29(500EP)"
#
# ac = ActionRecognition()
# ac.load_model(model_path)
# ac.run_model(True)
