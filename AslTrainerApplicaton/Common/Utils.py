import random
import cv2
import mediapipe as mp
import numpy as np
import os

def mediapipe_detection(image, model):
    image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)  # Color conversion BGR to RGB
    image.flags.writeable = False  # Image is no longer writable
    results = model.process(image)  # Make prediction
    image.flags.writeable = True  # Image is now writable
    image = cv2.cvtColor(image, cv2.COLOR_RGB2BGR)  # Color conversion RGB to BGR
    return image, results


def draw_landmarks(image, results):
    mp_drawing = mp.solutions.drawing_utils
    mp_holistic = mp.solutions.holistic
    mp_drawing.draw_landmarks(image, results.face_landmarks, mp_holistic.FACEMESH_CONTOURS)
    mp_drawing.draw_landmarks(image, results.pose_landmarks, mp_holistic.POSE_CONNECTIONS)
    mp_drawing.draw_landmarks(image, results.left_hand_landmarks, mp_holistic.HAND_CONNECTIONS)
    mp_drawing.draw_landmarks(image, results.right_hand_landmarks, mp_holistic.HAND_CONNECTIONS)


def draw_styled_landmarks(image, results):
    # Draw face connections
    mp_holistic = mp.solutions.holistic
    mp_drawing = mp.solutions.drawing_utils
    mp_drawing.draw_landmarks(image, results.face_landmarks, mp_holistic.FACEMESH_CONTOURS,
                              mp_drawing.DrawingSpec(color=(80, 110, 10), thickness=1, circle_radius=1),
                              mp_drawing.DrawingSpec(color=(80, 256, 121), thickness=1, circle_radius=1)
                              )
    # Draw pose connections
    mp_drawing.draw_landmarks(image, results.pose_landmarks, mp_holistic.POSE_CONNECTIONS,
                              mp_drawing.DrawingSpec(color=(80, 22, 10), thickness=2, circle_radius=4),
                              mp_drawing.DrawingSpec(color=(80, 44, 121), thickness=2, circle_radius=2)
                              )
    # Draw left hand connections
    mp_drawing.draw_landmarks(image, results.left_hand_landmarks, mp_holistic.HAND_CONNECTIONS,
                              mp_drawing.DrawingSpec(color=(121, 22, 76), thickness=2, circle_radius=4),
                              mp_drawing.DrawingSpec(color=(121, 44, 250), thickness=2, circle_radius=2)
                              )
    # Draw right hand connections
    mp_drawing.draw_landmarks(image, results.right_hand_landmarks, mp_holistic.HAND_CONNECTIONS,
                              mp_drawing.DrawingSpec(color=(245, 117, 66), thickness=2, circle_radius=4),
                              mp_drawing.DrawingSpec(color=(245, 66, 230), thickness=2, circle_radius=2)
                              )


def extract_keypoints(results,use_faceLandmarks = True):
    pose = np.array([[res.x, res.y, res.z, res.visibility] for res in results.pose_landmarks.landmark]).flatten() if results.pose_landmarks else np.zeros(33 * 4)
    face = np.array([[res.x, res.y, res.z] for res in results.face_landmarks.landmark]).flatten() if results.face_landmarks and use_faceLandmarks else np.zeros(468 * 3)
    lh = np.array([[res.x, res.y, res.z] for res in results.left_hand_landmarks.landmark]).flatten() if results.left_hand_landmarks else np.zeros(21 * 3)
    rh = np.array([[res.x, res.y, res.z] for res in results.right_hand_landmarks.landmark]).flatten() if results.right_hand_landmarks else np.zeros(21 * 3)
    return np.concatenate([pose, face, lh, rh])


def prob_viz(res, actions, input_frame, colors=None):
    if colors is None:
        colors = [(0, 0, 255)]
    output_frame = input_frame.copy()
    for num, prob in enumerate(res):
        cv2.rectangle(output_frame, (0, 60 + num * 40), (int(prob * 100), 90 + num * 40),
                      colors[num] if colors[num] is not None else (0, 0, 255), -1)
        cv2.putText(output_frame, actions[num], (0, 85 + num * 40), cv2.FONT_HERSHEY_SIMPLEX, 1, (255, 255, 255), 2,
                    cv2.LINE_AA)
    return output_frame


def generate_random_colors(n):
    colors = []
    for _ in range(n):
        r = random.randint(0, 255)
        g = random.randint(0, 255)
        b = random.randint(0, 255)
        colors.append((r, g, b))
    return colors

def view_NP_Array():
    file_path = '../MP_Data/match_830/'

    # Load the NumPy array from the file
    try:
        for i in range(1, 30):
            for j in range(1, 30):
                array = np.load(file_path+str(i)+'/'+str(j)+'.npy')
                print("NumPy array contents:")
                print(array)
    except Exception as e:
        print(f"Error loading or printing NumPy array: {e}")


class TextRedirector:
    def __init__(self, widget, log_function):
        self.widget = widget
        self.log_function = log_function

    def write(self, message):
        self.log_function(message)

    def flush(self):
        pass


def get_action_array_from_file(base_path):
    file_path = ""
    try:
        file_path = os.path.join(base_path, 'selected_folders.txt')
        with open(file_path, 'r') as file:
            lines = file.readlines()
        folders = [line.strip() for line in lines]
        folder_array = np.array(folders)
        return folder_array

    except FileNotFoundError:
        print(f"Error: The file '{file_path}' was not found.")
        return None
    except Exception as e:
        print(f"An error occurred: {e}")
        return None


def open_capture_with_resolution(width=640,height=360,camera_index=0):
    # Open the video capture
    cap = cv2.VideoCapture(camera_index)
    cap.set(cv2.CAP_PROP_FRAME_WIDTH, width)
    cap.set(cv2.CAP_PROP_FRAME_HEIGHT, height)

    # Check if the camera opened successfully
    if not cap.isOpened():
        print(f"Error: Could not open camera with index {camera_index}")
        return None
    return cap


def open_16_9_video_capture(camera_index=0):
    # Open the video capture
    cap = cv2.VideoCapture(camera_index)

    # Check if the camera opened successfully
    if not cap.isOpened():
        print(f"Error: Could not open camera with index {camera_index}")
        return None

    # Retrieve the maximum height supported by the camera
    max_height = int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))

    # Calculate the corresponding width for a 16:9 aspect ratio
    width_16_9 = int(max_height * 16 / 9)

    # Set the 16:9 resolution
    cap.set(cv2.CAP_PROP_FRAME_WIDTH, width_16_9)
    cap.set(cv2.CAP_PROP_FRAME_HEIGHT, max_height)

    # Verify if the resolution was set successfully
    actual_width = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH))
    actual_height = int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))

    print(f"Camera resolution set to: {actual_width}x{actual_height}")

    return cap


if __name__ == "__main__":
    view_NP_Array()
