import cv2
import mediapipe as mp
import numpy as np
from keras import Input
from Common import server
from keras.models import Sequential
from keras.layers import LSTM, Dense
import Common.Utils as Utils
import os


class ActionRecognition:
    def __init__(self):
        # Initialize MediaPipe and Keras model
        self.mp_holistic = mp.solutions.holistic
        self.mp_drawing = mp.solutions.drawing_utils

        self.actions = None
        self.model = None

        self.colors = [(245, 117, 16), (117, 245, 16), (16, 117, 245), (150, 23, 245), (150, 23, 0)]
        self.threshold = 0.5

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

    def run_model(self, show_video=True,port=None):
        currentAction = ""
        previousAction = ""
        self.start_server(port)
        sequence = []
        sentence = []
        predictions = []

        cap = Utils.open_16_9_video_capture()

        with self.mp_holistic.Holistic(min_detection_confidence=0.5, min_tracking_confidence=0.5) as holistic:
            while cap.isOpened():
                ret, frame = cap.read()
                image, results = Utils.mediapipe_detection(frame, holistic)
                Utils.draw_styled_landmarks(image, results)
                keypoints = Utils.extract_keypoints(results)
                sequence.append(keypoints)
                sequence = sequence[-30:]

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

