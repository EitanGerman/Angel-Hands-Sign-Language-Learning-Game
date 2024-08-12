import cv2
import numpy as np
import os
import mediapipe as mp
import Common.Utils as utils
import yt_dlp as youtube_dl


# Constants
SEQUENCE_LENGTH = 30
NO_SEQUENCES = 30
DATA_PATH = '../MP_Data'


def download_youtube_video(url, output_path='temp'):
    ydl_opts = {
        'outtmpl': f'{output_path}/%(title)s.%(ext)s',
        'format': 'bestvideo[ext=mp4]/best',  # Download the best video up to 720p //bestvideo[height<=720][ext=mp4]/
        'noplaylist': True,  # Do not download playlists
    }
    try:
        with youtube_dl.YoutubeDL(ydl_opts) as ydl:
            info_dict = ydl.extract_info(url, download=True)
            filename = f"{output_path}/{info_dict['title']}.mp4"
            return filename
    except Exception as e:
        print(f"Error downloading video: {e}")
        raise


def process_dataset_entry(entry):
    # Extract relevant information from the dataset entry
    url = entry['url']
    start_time = entry['start_time']
    end_time = entry['end_time']
    action = entry['clean_text']
    label = entry['label']

    # Create folder for the action
    folder_path = os.path.join(DATA_PATH, f"{action}_{label}")
    os.makedirs(folder_path, exist_ok=True)

    # Download YouTube video
    try:
        video_path = download_youtube_video(url)
    except Exception as e:
        print(f"Error downloading video: {e}")
        return

    # Process video
    cap = cv2.VideoCapture(video_path)
    fps = cap.get(cv2.CAP_PROP_FPS)

    start_frame = int(start_time * fps)
    end_frame = int(end_time * fps)
    total_frames = end_frame - start_frame

    with mp.solutions.holistic.Holistic(min_detection_confidence=0.5, min_tracking_confidence=0.5) as holistic:
        for sequence in range(NO_SEQUENCES + 1):
            sequence_path = os.path.join(folder_path, str(sequence))
            os.makedirs(sequence_path, exist_ok=True)

            for frame_num in range(SEQUENCE_LENGTH):
                # Calculate which frame to read
                current_frame = start_frame + int(frame_num * total_frames / SEQUENCE_LENGTH)
                cap.set(cv2.CAP_PROP_POS_FRAMES, current_frame)

                ret, frame = cap.read()
                if not ret:
                    print(f"Error reading frame {current_frame}")
                    break

                # Resize frame to match the dataset dimensions
                frame = cv2.resize(frame, (int(entry['width']), int(entry['height'])))

                # # Apply bounding box
                # x1, y1, x2, y2 = [int(entry['box'][i] * dim) for i, dim in
                #                   enumerate([entry['width'], entry['height'], entry['width'], entry['height']])]
                # frame = frame[y1:y2, x1:x2]

                # Make detections
                image, results = utils.mediapipe_detection(frame, holistic)

                # Extract keypoints
                keypoints = utils.extract_keypoints(results)
                npy_path = os.path.join(sequence_path, str(frame_num))
                np.save(npy_path, keypoints)

                if(True):#preview learning
                    # Draw landmarks on the frame
                    if results.pose_landmarks:
                        utils.draw_styled_landmarks(image, results)

                    # Display the frame with landmarks
                    cv2.imshow('Video Frame', image)

                    if cv2.waitKey(int(1000 / fps)) & 0xFF == ord('q'):
                        break

    cap.release()
    cv2.destroyAllWindows()
    os.remove(video_path)  # Remove the temporary video file
    print(f"Processed {action} (label: {label})")


# Example usage
dataset_entry = {"org_text": "rainbow", "clean_text": "rainbow", "start_time": 315.916, "signer_id": 94, "signer": 2, "start": 9468, "end": 9548, "file": "Colors and Springtime signs in ASL", "label": 669, "height": 720.0, "fps": 29.97, "end_time": 318.585, "url": "https://www.youtube.com/watch?v=9RE2NLd_Sgw", "review": 1, "text": "rainbow", "box": [0.03182029724121094, 0.1610688716173172, 1.0, 0.6403075456619263], "width": 1280.0}

process_dataset_entry(dataset_entry)

# import cv2
# import numpy as np
# import os
# import json
# from pytube import YouTube
# import mediapipe as mp
# import Common.Utils as utils
# import time
# import yt_dlp as youtube_dl
#
# # Constants
# SEQUENCE_LENGTH = 30
# NO_SEQUENCES = 30
# DATA_PATH = '..\\MP_Data'
#
#
# def download_youtube_video(url, output_path='temp'):
#     ydl_opts = {
#         'outtmpl': f'{output_path}/%(title)s.%(ext)s',
#         'format': 'bestvideo[height<=720][ext=mp4]/bestvideo[ext=mp4]/best',  # Download the best video up to 720p
#         'noplaylist': True,  # Do not download playlists
#     }
#     try:
#         with youtube_dl.YoutubeDL(ydl_opts) as ydl:
#             info_dict = ydl.extract_info(url, download=True)
#             # Extract the filename from the info dictionary
#             filename = f"{output_path}/{info_dict['title']}.mp4"
#             return filename
#     except Exception as e:
#         print(f"Error downloading video: {e}")
#         raise
#
#
#
# def process_dataset_entry(entry):
#     # Extract relevant information from the dataset entry
#     url = entry['url']
#     start_time = entry['start_time']
#     end_time = entry['end_time']
#     action = entry['clean_text']
#     label = entry['label']
#
#     # Create folder for the action
#     folder_path = os.path.join(DATA_PATH, f"{action}_{label}")
#     os.makedirs(folder_path, exist_ok=True)
#
#     # Download YouTube video
#     try:
#         video_path = download_youtube_video(url)
#     except Exception as e:
#         print(f"Error downloading video: {e}")
#         return
#
#     # Process video
#     cap = cv2.VideoCapture(video_path)
#     fps = cap.get(cv2.CAP_PROP_FPS)
#
#     start_frame = int(start_time * fps)
#     end_frame = int(end_time * fps)
#     total_frames = end_frame - start_frame
#
#     with mp.solutions.holistic.Holistic(min_detection_confidence=0.5, min_tracking_confidence=0.5) as holistic:
#         for sequence in range(NO_SEQUENCES+1):
#             sequence_path = os.path.join(folder_path, str(sequence))
#             os.makedirs(sequence_path, exist_ok=True)
#
#             for frame_num in range(SEQUENCE_LENGTH):
#                 # Calculate which frame to read
#                 current_frame = start_frame + int(frame_num * total_frames / SEQUENCE_LENGTH)
#                 cap.set(cv2.CAP_PROP_POS_FRAMES, current_frame)
#
#                 ret, frame = cap.read()
#                 if not ret:
#                     print(f"Error reading frame {current_frame}")
#                     break
#
#                 # Resize frame to match the dataset dimensions
#                 frame = cv2.resize(frame, (int(entry['width']), int(entry['height'])))
#
#                 # Apply bounding box
#                 x1, y1, x2, y2 = [int(entry['box'][i] * dim) for i, dim in
#                                   enumerate([entry['width'], entry['height'], entry['width'], entry['height']])]
#                 frame = frame[y1:y2, x1:x2]
#
#                 # Make detections
#                 image, results = utils.mediapipe_detection(frame, holistic)
#
#                 # Extract keypoints
#                 keypoints = utils.extract_keypoints(results)
#                 npy_path = os.path.join(sequence_path, str(frame_num))
#                 np.save(npy_path, keypoints)
#
#     cap.release()
#     os.remove(video_path)  # Remove the temporary video file
#     print(f"Processed {action} (label: {label})")
#
#
# # Example usage
# dataset_entry = {"org_text": "rainbow ", "clean_text": "rainbow", "start_time": 1.804, "signer_id": 144, "signer": -1, "start": 54, "end": 165, "file": "rainbow - ASL sign for rainbow", "label": 669, "height": 360.0, "fps": 29.934, "end_time": 5.512, "url": "https://www.youtube.com/watch?v=WeAFuzYTdtU", "review": 1, "text": "rainbow", "box": [0.004803866147994995, 0.0, 1.0, 0.9657534956932068], "width": 480.0}
#
#
#
# process_dataset_entry(dataset_entry)