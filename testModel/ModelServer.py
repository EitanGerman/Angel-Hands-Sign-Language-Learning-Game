import argparse
from Common.Action_recognition import ActionRecognition


def main(model_path,show_video=False,port=None):
    ac = ActionRecognition()
    ac.load_model(model_path)
    ac.run_model(show_video,port)


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
        type=bool,
        default=False,
        help="Flag to show video output (default: True)"
    )

    parser.add_argument(
        "--server_port",
        type=int,
        default=None,
        help="Flag to show video output (default: True)"
    )

    # Parse the arguments
    args = parser.parse_args()

    # Run the main function with the provided model path
    main(args.model_path,show_video=args.show_video,port=args.server_port)


# from Common.Action_recognition import ActionRecognition
#
# model_path = "Models/2024-08-04_19-29(500EP)"
#
# ac = ActionRecognition()
# ac.load_model(model_path)
# ac.run_model(True)
