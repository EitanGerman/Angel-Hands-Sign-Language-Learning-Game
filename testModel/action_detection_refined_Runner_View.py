import cv2
import mediapipe as mp
import numpy as np
import server as srvr
from keras.models import Sequential
from keras.layers import LSTM, Dense
import os

class ActionRecognition:
    def __init__(self):
        # Initialize MediaPipe and Keras model
        self.mp_holistic = mp.solutions.holistic
        self.mp_drawing = mp.solutions.drawing_utils
        self.server = srvr.Server()

        self.actions = None
        self.model = None
        # self.actions = np.array(['hello', 'thanks', 'iloveyou'])
        # self.model = self._build_model()
        # self.model.load_weights('action.keras')

        self.colors = [(245, 117, 16), (117, 245, 16), (16, 117, 245), (150, 23, 245)]
        self.threshold = 0.5

    def load_model(self, model_path, folders):
        self.actions = np.array(folders)
        print('actions:' +str(len(self.actions)))
        self.model = self._build_model()
        self.model.load_weights(model_path)

    def _build_model(self):
        model = Sequential()
        model.add(LSTM(64, return_sequences=True, activation='relu', input_shape=(30, 554*3)))  #i662
        model.add(LSTM(128, return_sequences=True, activation='relu'))
        model.add(LSTM(64, return_sequences=False, activation='relu'))
        model.add(Dense(64, activation='relu'))
        model.add(Dense(32, activation='relu'))
        model.add(Dense(self.actions.shape[0], activation='softmax'))
        model.compile(optimizer='Adam', loss='categorical_crossentropy', metrics=['categorical_accuracy'])
        return model

    def mediapipe_detection(self, image, model):
        image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
        image.flags.writeable = False
        results = model.process(image)
        image.flags.writeable = True
        image = cv2.cvtColor(image, cv2.COLOR_RGB2BGR)
        return image, results

    def draw_landmarks(self, image, results):
        self.mp_drawing.draw_landmarks(image, results.face_landmarks, self.mp_holistic.FACEMESH_CONTOURS)
        self.mp_drawing.draw_landmarks(image, results.pose_landmarks, self.mp_holistic.POSE_CONNECTIONS)
        self.mp_drawing.draw_landmarks(image, results.left_hand_landmarks, self.mp_holistic.HAND_CONNECTIONS)
        self.mp_drawing.draw_landmarks(image, results.right_hand_landmarks, self.mp_holistic.HAND_CONNECTIONS)

    def draw_styled_landmarks(self, image, results):
        self.mp_drawing.draw_landmarks(image, results.face_landmarks, self.mp_holistic.FACEMESH_CONTOURS,
                                       self.mp_drawing.DrawingSpec(color=(80, 110, 10), thickness=1, circle_radius=1),
                                       self.mp_drawing.DrawingSpec(color=(80, 256, 121), thickness=1, circle_radius=1))
        self.mp_drawing.draw_landmarks(image, results.pose_landmarks, self.mp_holistic.POSE_CONNECTIONS,
                                       self.mp_drawing.DrawingSpec(color=(80, 22, 10), thickness=2, circle_radius=4),
                                       self.mp_drawing.DrawingSpec(color=(80, 44, 121), thickness=2, circle_radius=2))
        self.mp_drawing.draw_landmarks(image, results.left_hand_landmarks, self.mp_holistic.HAND_CONNECTIONS,
                                       self.mp_drawing.DrawingSpec(color=(121, 22, 76), thickness=2, circle_radius=4),
                                       self.mp_drawing.DrawingSpec(color=(121, 44, 250), thickness=2, circle_radius=2))
        self.mp_drawing.draw_landmarks(image, results.right_hand_landmarks, self.mp_holistic.HAND_CONNECTIONS,
                                       self.mp_drawing.DrawingSpec(color=(245, 117, 66), thickness=2, circle_radius=4),
                                       self.mp_drawing.DrawingSpec(color=(245, 66, 230), thickness=2, circle_radius=2))

    def extract_keypoints(self, results):
        pose = np.array([[res.x, res.y, res.z, res.visibility] for res in results.pose_landmarks.landmark]).flatten() \
            if results.pose_landmarks else np.zeros(33 * 4)
        face = np.array([[res.x, res.y, res.z] for res in results.face_landmarks.landmark]).flatten() \
            if results.face_landmarks else np.zeros(468 * 3)
        lh = np.array([[res.x, res.y, res.z] for res in results.left_hand_landmarks.landmark]).flatten() \
            if results.left_hand_landmarks else np.zeros(21 * 3)
        rh = np.array([[res.x, res.y, res.z] for res in results.right_hand_landmarks.landmark]).flatten() \
            if results.right_hand_landmarks else np.zeros(21 * 3)
        return np.concatenate([pose, face, lh, rh])

    def prob_viz(self, res, actions, input_frame, colors):
        output_frame = input_frame.copy()
        for num, prob in enumerate(res):
            cv2.rectangle(output_frame, (0, 60 + num * 40), (int(prob * 100), 90 + num * 40),
                          colors[num] if colors[num] is not None else (0, 0, 255), -1)
            cv2.putText(output_frame, actions[num], (0, 85 + num * 40), cv2.FONT_HERSHEY_SIMPLEX, 1, (255, 255, 255), 2,
                        cv2.LINE_AA)
        return output_frame

    def start_server(self):
        self.server.start()

    def run_model(self):
        currentAction = ""
        previousAction = ""
        self.start_server()
        sequence = []
        sentence = []
        predictions = []

        cap = cv2.VideoCapture(0)
        with self.mp_holistic.Holistic(min_detection_confidence=0.5, min_tracking_confidence=0.5) as holistic:
            while cap.isOpened():
                ret, frame = cap.read()
                image, results = self.mediapipe_detection(frame, holistic)
                self.draw_styled_landmarks(image, results)
                keypoints = self.extract_keypoints(results)
                sequence.append(keypoints)
                sequence = sequence[-30:]

                showVideo = True

                if len(sequence) == 30:
                    res = self.model.predict(np.expand_dims(sequence, axis=0))[0]
                    print(self.actions[np.argmax(res)])
                    currentAction = self.actions[np.argmax(res)]
                    if previousAction != currentAction:
                        previousAction = currentAction
                        self.server.broadcast_variable(previousAction)
                    predictions.append(np.argmax(res))

                    if showVideo:
                        if np.unique(predictions[-10:])[0] == np.argmax(res):
                            if res[np.argmax(res)] > self.threshold:
                                if len(sentence) > 0:
                                    if self.actions[np.argmax(res)] != sentence[-1]:
                                        sentence.append(self.actions[np.argmax(res)])
                                else:
                                    sentence.append(self.actions[np.argmax(res)])

                        if len(sentence) > 5:
                            sentence = sentence[-5:]

                        image = self.prob_viz(res, self.actions, image, self.colors)

                if showVideo:
                    cv2.rectangle(image, (0, 0), (640, 40), (245, 117, 16), -1)
                    cv2.putText(image, ' '.join(sentence), (3, 30), cv2.FONT_HERSHEY_SIMPLEX, 1, (255, 255, 255), 2,
                                cv2.LINE_AA)
                    cv2.imshow('OpenCV Feed', image)

                    if cv2.waitKey(10) & 0xFF == ord('q'):
                        break
            cap.release()
            cv2.destroyAllWindows()


# To run the model
if __name__ == "__main__":
    action_recognition = ActionRecognition()
    action_recognition.run_model()

