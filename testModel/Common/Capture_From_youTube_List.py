import cv2
import numpy as np
import os
import mediapipe as mp
import yt_dlp as youtube_dl
from collections import defaultdict
import Common.Utils as Utils


# Constants
SEQUENCE_LENGTH = 30
NO_SEQUENCES = 30
DATA_PATH = '../MP_Data'


def download_youtube_video(url, output_path='temp'):
    ydl_opts = {
        'outtmpl': f'{output_path}/%(title)s.%(ext)s',
        'format': 'bestvideo[height<=720][ext=mp4]/bestvideo[ext=mp4]/best',  # Download the best video up to 720p
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

def process_dataset_entry(entry, video_path, sequence_count,start_sequence, folder_path,preview=False):
    # Open video capture
    cap = cv2.VideoCapture(video_path)
    fps = cap.get(cv2.CAP_PROP_FPS)

    start_frame = int(entry['start_time'] * fps)
    end_frame = int(entry['end_time'] * fps)
    total_frames = end_frame - start_frame

    with mp.solutions.holistic.Holistic(min_detection_confidence=0.5, min_tracking_confidence=0.5) as holistic:
        for sequence in range(start_sequence,start_sequence+sequence_count):
            sequence_path = os.path.join(folder_path, str(sequence))
            os.makedirs(sequence_path, exist_ok=True)

            for frame_num in range(SEQUENCE_LENGTH):
                current_frame = start_frame + int(frame_num * total_frames / SEQUENCE_LENGTH)
                cap.set(cv2.CAP_PROP_POS_FRAMES, current_frame)

                ret, frame = cap.read()
                if not ret:
                    print(f"Error reading frame {current_frame}")
                    break

                # frame = cv2.resize(frame, (int(entry['width']), int(entry['height'])))
                # x1, y1, x2, y2 = [int(entry['box'][i] * dim) for i, dim in
                #                   enumerate([entry['width'], entry['height'], entry['width'], entry['height']])]
                # frame = frame[y1:y2, x1:x2]

                image, results = Utils.mediapipe_detection(frame, holistic)
                keypoints = Utils.extract_keypoints(results)
                npy_path = os.path.join(sequence_path, str(frame_num))
                np.save(npy_path, keypoints)

                if(preview):#preview learning
                    # Draw landmarks on the frame
                    if results.pose_landmarks:
                        Utils.draw_styled_landmarks(image, results)

                    # Display the frame with landmarks
                    cv2.imshow('Video Frame', image)

                    if cv2.waitKey(int(1000 / fps)) & 0xFF == ord('q'):
                        break

    cap.release()


def process_dataset_entry_list(dataset_entries, action):
    folder_path = os.path.join(DATA_PATH, action)
    os.makedirs(folder_path, exist_ok=True)

    # Determine how many sequences per entry
    num_entries = len(dataset_entries)
    sequences_per_entry = NO_SEQUENCES // num_entries
    extra_sequences = NO_SEQUENCES % num_entries

    video_paths = {}

    cnt = 1
    for idx, entry in enumerate(dataset_entries):
        # Download each video only once per unique label
        url = entry['url']
        if url not in video_paths:
            video_paths[url] = download_youtube_video(url)

        video_path = video_paths[url]

        # Allocate sequences
        sequence_count = sequences_per_entry + (1 if idx < extra_sequences else 0)
        process_dataset_entry(entry, video_path, sequence_count, cnt, folder_path)
        cnt += sequence_count

    # Clean up downloaded videos
    for path in video_paths.values():
        os.remove(path)

if __name__ == "__main__":
    # Example usage
    dataset_entries = [
        {"org_text": "rainbow ", "clean_text": "rainbow", "start_time": 1.804, "signer_id": 144, "signer": -1, "start": 54, "end": 165, "file": "rainbow - ASL sign for rainbow", "label": 669, "height": 360.0, "fps": 29.934, "end_time": 5.512, "url": "https://www.youtube.com/watch?v=WeAFuzYTdtU", "review": 1, "text": "rainbow", "box": [0.004803866147994995, 0.0, 1.0, 0.9657534956932068], "width": 480.0},
        {"org_text": "rainbow ", "clean_text": "rainbow", "start_time": 5.813, "signer_id": 144, "signer": -1, "start": 174, "end": 269, "file": "rainbow - ASL sign for rainbow", "label": 669, "height": 360.0, "fps": 29.934, "end_time": 8.986, "url": "https://www.youtube.com/watch?v=WeAFuzYTdtU", "review": 1, "text": "rainbow", "box": [0.004803866147994995, 0.0, 1.0, 0.9657534956932068], "width": 480.0},
        {"org_text": "rainbow ", "clean_text": "rainbow", "start_time": 9.354, "signer_id": 144, "signer": -1, "start": 280, "end": 435, "file": "rainbow - ASL sign for rainbow", "label": 669, "height": 360.0, "fps": 29.934, "end_time": 14.532, "url": "https://www.youtube.com/watch?v=WeAFuzYTdtU", "review": 1, "text": "rainbow", "box": [0.004803866147994995, 0.0, 1.0, 0.9657534956932068], "width": 480.0},
        {"org_text": "rainbow", "clean_text": "rainbow", "start_time": 312.913, "signer_id": 94, "signer": 2, "start": 9378, "end": 9468, "file": "Colors and Springtime signs in ASL", "label": 669, "height": 720.0, "fps": 29.97, "end_time": 315.916, "url": "https://www.youtube.com/watch?v=9RE2NLd_Sgw", "review": 1, "text": "rainbow", "box": [0.03182029724121094, 0.1610688716173172, 1.0, 0.6403075456619263], "width": 1280.0},
        {"org_text": "rainbow", "clean_text": "rainbow", "start_time": 315.916, "signer_id": 94, "signer": 2, "start": 9468, "end": 9548, "file": "Colors and Springtime signs in ASL", "label": 669, "height": 720.0, "fps": 29.97, "end_time": 318.585, "url": "https://www.youtube.com/watch?v=9RE2NLd_Sgw", "review": 1, "text": "rainbow", "box": [0.03182029724121094, 0.1610688716173172, 1.0, 0.6403075456619263], "width": 1280.0}
    ]

    process_dataset_entry_list(dataset_entries, 'rainbow')
