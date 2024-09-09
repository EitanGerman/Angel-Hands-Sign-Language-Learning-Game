import json
import cv2
import numpy as np
import os
import mediapipe as mp
import yt_dlp as youtube_dl
import Common.Utils as Utils
import shutil
import random
from pathlib import Path


# Constants
SEQUENCE_LENGTH = 30
NO_SEQUENCES = 30
DATA_PATH = 'MP_Data'


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


def process_dataset_entry(entry, video_path, sequence_count, start_sequence, folder_path, recordings_folder, preview=False):
    # Open video capture
    cap = cv2.VideoCapture(video_path)
    fps = cap.get(cv2.CAP_PROP_FPS)

    start_frame = int(entry['start_time'] * fps)
    end_frame = int(entry['end_time'] * fps)
    total_frames = end_frame - start_frame

    snippet_path = os.path.join(recordings_folder, f"{entry['org_text']}_{start_sequence}.mp4")

    # Check if the snippet already exists
    if not os.path.exists(snippet_path):
        print(f"Recording new snippet: {snippet_path}")
        fourcc = cv2.VideoWriter_fourcc(*'mp4v')
        out = cv2.VideoWriter(snippet_path, fourcc, fps, (int(cap.get(3)), int(cap.get(4))))

        # Save the unique snippet video from start to end times
        for frame_num in range(start_frame, end_frame):
            cap.set(cv2.CAP_PROP_POS_FRAMES, frame_num)
            ret, frame = cap.read()
            if not ret:
                print(f"Error reading frame {frame_num}")
                break
            out.write(frame)
        out.release()
    else:
        print(f"Snippet {snippet_path} already exists, skipping video recording.")

    # Process the snippet and record npy files (keypoints) for each sequence
    with mp.solutions.holistic.Holistic(min_detection_confidence=0.5, min_tracking_confidence=0.5) as holistic:
        for sequence in range(start_sequence, start_sequence + sequence_count):
            sequence_path = os.path.join(folder_path, str(sequence))
            os.makedirs(sequence_path, exist_ok=True)

            for frame_num in range(SEQUENCE_LENGTH):
                current_frame = start_frame + int(frame_num * total_frames / SEQUENCE_LENGTH)
                cap.set(cv2.CAP_PROP_POS_FRAMES, current_frame)

                ret, frame = cap.read()
                if not ret:
                    print(f"Error reading frame {current_frame}")
                    break


                # Process frame for keypoints
                image, results = Utils.mediapipe_detection(frame, holistic)
                keypoints = Utils.extract_keypoints(results)
                npy_path = os.path.join(sequence_path, str(frame_num))
                np.save(npy_path, keypoints)

                if preview:  # preview learning
                    # Draw landmarks on the frame
                    if results.pose_landmarks:
                        Utils.draw_styled_landmarks(image, results)

                    # Display the frame with landmarks
                    cv2.imshow('Video Frame', image)

                    if cv2.waitKey(int(1000 / fps)) & 0xFF == ord('q'):
                        break

    cap.release()
    cv2.destroyAllWindows()


def process_dataset_entry_list(dataset_entries, action, preview=False):
    folder_path = os.path.join(DATA_PATH, action)
    os.makedirs(folder_path, exist_ok=True)
    metadataFolder = os.path.join(folder_path, "MetaData")
    signFolder = os.path.join(metadataFolder, "SignData")
    recordingsFolder = os.path.join(metadataFolder, "Recordings")
    os.makedirs(signFolder, exist_ok=True)
    os.makedirs(recordingsFolder, exist_ok=True)
    print(folder_path)

    video_paths = {}
    valid_entries = []
    used_urls = []

    # First, attempt to download all videos
    for entry in dataset_entries:
        url = entry['url']
        if url not in video_paths:
            video_path = download_youtube_video(url)
            if video_path:
                video_paths[url] = video_path
                valid_entries.append(entry)
                used_urls.append(url)
            else:
                print(f"Failed to download video for URL: {url}. Skipping this entry.")

    # Determine how many sequences per valid entry
    num_valid_entries = len(valid_entries)
    if num_valid_entries == 0:
        print("No valid videos were downloaded. Exiting.")
        return

    # Store metadata for the sign
    metadata_file_path = os.path.join(signFolder, f"{action}.json")
    metadata = {
        "word": action,
        "urls": used_urls,
        "recordings_path": f"Resources/{action}/Recordings"
    }
    with open(metadata_file_path, 'w') as metadata_file:
        json.dump(metadata, metadata_file, indent=4)

    # Determine how many sequences per valid entry
    sequences_per_entry = NO_SEQUENCES // num_valid_entries
    extra_sequences = NO_SEQUENCES % num_valid_entries

    cnt = 1

    for idx, entry in enumerate(valid_entries):
        url = entry['url']
        video_path = video_paths[url]

        # Allocate sequences
        sequence_count = sequences_per_entry + (1 if idx < extra_sequences else 0)

        # Process entry and capture both keypoints and video snippets
        process_dataset_entry(entry, video_path, sequence_count, cnt, folder_path, recordingsFolder, preview)
        cnt += sequence_count

    # Clean up downloaded videos
    for path in video_paths.values():
        try:
            os.remove(path)
        except Exception as e:
            pass

    copy_and_replace(folder_path)


def copy_and_replace(base_directory):
    base_path = Path(base_directory)

    subdirs = [d for d in base_path.iterdir() if d.is_dir() and d.name[0].isdigit()]

    less_than_SEQUENCE_LENGTH = [d for d in subdirs if sum(1 for _ in d.iterdir() if _.is_file()) < SEQUENCE_LENGTH]
    at_least_SEQUENCE_LENGTH = [d for d in subdirs if sum(1 for _ in d.iterdir() if _.is_file()) >= SEQUENCE_LENGTH]

    if not less_than_SEQUENCE_LENGTH:
        print(f"No subdirectory has less than {SEQUENCE_LENGTH} files.")
        return

    if not at_least_SEQUENCE_LENGTH:
        print(f"No subdirectory has {SEQUENCE_LENGTH} or more files.")
        return

    source_dir = random.choice(at_least_SEQUENCE_LENGTH)

    for target_dir in less_than_SEQUENCE_LENGTH:
        # Remove all files in target_dir
        for file in target_dir.iterdir():
            if file.is_file():
                file.unlink()

        source_dir = random.choice(at_least_SEQUENCE_LENGTH)

        # Copy files from source_dir to target_dir
        for file in source_dir.iterdir():
            if file.is_file():
                shutil.copy(file, target_dir / file.name)

        print(f"Replaced files in {target_dir.name} with files from {source_dir.name}")


def check_folders(srcDir):
    for entry in os.scandir(srcDir):
        if entry.is_dir():
            subdirectory = entry.path
            num_files = sum([1 for _ in os.scandir(subdirectory) if _.is_file()])
            print(f"{entry.name}: {num_files} files")


if __name__ == "__main__":
    #Example usage
    dataset_entries_list = [
        {"org_text": "rainbow ", "clean_text": "rainbow", "start_time": 1.804, "signer_id": 144, "signer": -1, "start": 54, "end": 165, "file": "rainbow - ASL sign for rainbow", "label": 669, "height": 360.0, "fps": 29.934, "end_time": 5.512, "url": "https://www.youtube.com/watch?v=WeAFuzYTdtU", "review": 1, "text": "rainbow", "box": [0.004803866147994995, 0.0, 1.0, 0.9657534956932068], "width": 480.0},
        {"org_text": "rainbow ", "clean_text": "rainbow", "start_time": 5.813, "signer_id": 144, "signer": -1, "start": 174, "end": 269, "file": "rainbow - ASL sign for rainbow", "label": 669, "height": 360.0, "fps": 29.934, "end_time": 8.986, "url": "https://www.youtube.com/watch?v=WeAFuzYTdtU", "review": 1, "text": "rainbow", "box": [0.004803866147994995, 0.0, 1.0, 0.9657534956932068], "width": 480.0},
        {"org_text": "rainbow ", "clean_text": "rainbow", "start_time": 9.354, "signer_id": 144, "signer": -1, "start": 280, "end": 435, "file": "rainbow - ASL sign for rainbow", "label": 669, "height": 360.0, "fps": 29.934, "end_time": 14.532, "url": "https://www.youtube.com/watch?v=WeAFuzYTdtU", "review": 1, "text": "rainbow", "box": [0.004803866147994995, 0.0, 1.0, 0.9657534956932068], "width": 480.0},
        {"org_text": "rainbow", "clean_text": "rainbow", "start_time": 312.913, "signer_id": 94, "signer": 2, "start": 9378, "end": 9468, "file": "Colors and Springtime signs in ASL", "label": 669, "height": 720.0, "fps": 29.97, "end_time": 315.916, "url": "https://www.youtube.com/watch?v=9RE2NLd_Sgw", "review": 1, "text": "rainbow", "box": [0.03182029724121094, 0.1610688716173172, 1.0, 0.6403075456619263], "width": 1280.0},
        {"org_text": "rainbow", "clean_text": "rainbow", "start_time": 315.916, "signer_id": 94, "signer": 2, "start": 9468, "end": 9548, "file": "Colors and Springtime signs in ASL", "label": 669, "height": 720.0, "fps": 29.97, "end_time": 318.585, "url": "https://www.youtube.com/watch?v=9RE2NLd_Sgw", "review": 1, "text": "rainbow", "box": [0.03182029724121094, 0.1610688716173172, 1.0, 0.6403075456619263], "width": 1280.0}
    ]

    process_dataset_entry_list(dataset_entries_list, 'rainbows')
    directory = '../MP_Data/MOM'
    check_folders(directory)

    copy_and_replace(directory)

    check_folders(directory)
