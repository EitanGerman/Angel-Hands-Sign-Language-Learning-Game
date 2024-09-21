import cv2
import mediapipe as mp
import numpy as np
from keras import Input
from Common import server
from keras.models import Sequential
from keras.layers import LSTM, Dense
import Common.Utils as Utils
import os
import mmap
import time

SHM_NAME = "shm_cam_feed"
FRAME_WIDTH = 640
FRAME_HEIGHT = 360
FRAME_SIZE = FRAME_WIDTH * FRAME_HEIGHT * 3


class ActionRecognition:
    def __init__(self):
        # Initialize MediaPipe and Keras model
        self.mp_holistic = mp.solutions.holistic
        self.mp_drawing = mp.solutions.drawing_utils

        self.actions = None
        self.model = None

        self.colors = [(245, 117, 16), (117, 245, 16), (16, 117, 245), (150, 23, 245), (150, 23, 0)]
        self.threshold = 0.7

    def load_model(self, model_path):
        self.actions = Utils.get_action_array_from_file(model_path)
        print('actions:' + str(len(self.actions)))
        self.colors = Utils.generate_random_colors(len(self.actions))
        self.model = self.build_model(self.actions.shape[0])
        self.model.load_weights(os.path.join(model_path,'actions.keras'))

    @staticmethod
    def build_model(shape):
        model = Sequential()
        model.add(Input(shape=(30, 554*3)))
        model.add(LSTM(64, return_sequences=True, activation='relu'))  # input_shape=(30, 554*3)
        model.add(LSTM(128, return_sequences=True, activation='relu'))
        model.add(LSTM(64, return_sequences=False, activation='relu'))
        model.add(Dense(64, activation='relu'))
        model.add(Dense(32, activation='relu'))
        model.add(Dense(shape, activation='softmax'))
        model.compile(optimizer='Adam', loss='categorical_crossentropy', metrics=['categorical_accuracy'])
        return model

    def start_server(self,port=None):
        self.srvr = server.Server(port)
        self.srvr.start()

    def run_model(self, show_video=True, port=None, width=FRAME_WIDTH, height=FRAME_HEIGHT,threshold=0.7,sensitivity=10):
        self.threshold = threshold
        currentAction = ""
        previousAction = ""
        self.start_server(port)
        sequence = []
        sentence = []
        predictions = []
        frame_size = width * height * 3
        # Attempt to open shared memory with retry mechanism
        shm = None
        while shm is None:
            try:
                shm = mmap.mmap(-1, frame_size, SHM_NAME)
                print("Shared memory opened successfully.")
            except FileNotFoundError:
                print("Shared memory not found. Retrying in 1 second...")
                time.sleep(1)

        with self.mp_holistic.Holistic(min_detection_confidence=0.5, min_tracking_confidence=0.5) as holistic:
            while True:
                # Read frame from shared memory
                shm.seek(0)
                frame_bytes = shm.read(frame_size)

                # Convert byte data to numpy array
                frame = np.frombuffer(frame_bytes, dtype=np.uint8).reshape((height, width, 3))

                # Flip and process the frame
                # frame = cv2.flip(frame, 1)
                frame = cv2.cvtColor(frame, cv2.COLOR_RGB2BGR)
                image, results = Utils.mediapipe_detection(frame, holistic)
                Utils.draw_styled_landmarks(image, results)
                keypoints = Utils.extract_keypoints(results)
                sequence.append(keypoints)
                sequence = sequence[-30:]

                if len(sequence) == 30:
                    res = self.model.predict(np.expand_dims(sequence, axis=0),verbose=None)[0]
                    print(self.actions[np.argmax(res)])
                    currentAction = self.actions[np.argmax(res)]
                    if previousAction != currentAction:
                        previousAction = currentAction
                        #self.srvr.broadcast_variable(previousAction)
                    predictions.append(np.argmax(res))

                    if np.unique(predictions[-sensitivity:])[0] == np.argmax(res):
                        if res[np.argmax(res)] > self.threshold:
                            if len(sentence) > 0:
                                if self.actions[np.argmax(res)] != sentence[-1]:
                                    sentence.append(self.actions[np.argmax(res)])
                            else:
                                sentence.append(self.actions[np.argmax(res)])
                            self.srvr.broadcast_variable(self.actions[np.argmax(res)])

                    if len(sentence) > 5:
                        sentence = sentence[-5:]
                    if show_video:
                        image = Utils.prob_viz_sorted(res, self.actions, image, self.colors)

                if show_video:
                    cv2.rectangle(image, (0, 0), (640, 40), (245, 117, 16), -1)
                    cv2.putText(image, ' '.join(sentence), (3, 30), cv2.FONT_HERSHEY_SIMPLEX, 1, (255, 255, 255), 2,
                                cv2.LINE_AA)
                    cv2.imshow('OpenCV Feed', image)

                    if cv2.waitKey(10) & 0xFF == ord('q'):
                        break

        # Cleanup

        if cv2.getWindowProperty('OpenCV Feed', cv2.WND_PROP_VISIBLE) >= 1:
            cv2.destroyWindow('OpenCV Feed')
        else:
            cv2.destroyAllWindows()
        shm.close()
        self.srvr.stop()

    def run_model_old(self, show_video=True,port=None):
        currentAction = ""
        previousAction = ""
        self.start_server(port)
        sequence = []
        sentence = []
        predictions = []

        cap = Utils.open_capture_with_resolution(FRAME_WIDTH,FRAME_HEIGHT)  # Utils.open_16_9_video_capture()
        shm = mmap.mmap(-1, FRAME_SIZE, SHM_NAME)
        with self.mp_holistic.Holistic(min_detection_confidence=0.5, min_tracking_confidence=0.5) as holistic:
            while cap.isOpened():
                ret, frame = cap.read()
                frame = cv2.flip(frame, 1)
                image, results = Utils.mediapipe_detection(frame, holistic)
                Utils.draw_styled_landmarks(image, results)
                keypoints = Utils.extract_keypoints(results)
                sequence.append(keypoints)
                sequence = sequence[-30:]

                frame_rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
                # Check frame size
                if frame_rgb.nbytes != FRAME_SIZE:
                    print(f"Warning: Frame size mismatch! Expected {FRAME_SIZE}, got {frame_rgb.nbytes}")

                # Write frame to shared memory
                shm.seek(0)
                shm.write(frame_rgb.tobytes())

                if len(sequence) == 30:
                    res = self.model.predict(np.expand_dims(sequence, axis=0))[0]
                    print(self.actions[np.argmax(res)])
                    currentAction = self.actions[np.argmax(res)]
                    if previousAction != currentAction:
                        previousAction = currentAction
                        self.srvr.broadcast_variable(previousAction)
                    predictions.append(np.argmax(res))

                    if show_video:
                        if np.unique(predictions[-10:])[0] == np.argmax(res):
                            if res[np.argmax(res)] > self.threshold:
                                if len(sentence) > 0:
                                    if self.actions[np.argmax(res)] != sentence[-1]:
                                        sentence.append(self.actions[np.argmax(res)])
                                else:
                                    sentence.append(self.actions[np.argmax(res)])

                        if len(sentence) > 5:
                            sentence = sentence[-5:]

                        image = Utils.prob_viz(res, self.actions, image, self.colors)

                if show_video:
                    cv2.rectangle(image, (0, 0), (640, 40), (245, 117, 16), -1)
                    cv2.putText(image, ' '.join(sentence), (3, 30), cv2.FONT_HERSHEY_SIMPLEX, 1, (255, 255, 255), 2,
                                cv2.LINE_AA)
                    cv2.imshow('OpenCV Feed', image)

                    if cv2.waitKey(10) & 0xFF == ord('q'):
                        break
            cap.release()
            cv2.destroyAllWindows()
            self.srvr.stop()


# To run the model
if __name__ == "__main__":
    action_recognition = ActionRecognition()
    action_recognition.run_model()

