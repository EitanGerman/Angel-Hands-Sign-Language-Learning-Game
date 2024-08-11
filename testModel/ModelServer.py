import cv2
import mediapipe as mp
import numpy as np
import server as srvr
import Common.Utils as Utils
from keras.models import Sequential
from keras.layers import LSTM, Dense
"""# 2. Keypoints using MP Holistic"""

currentAction = ""
server = srvr.Server()



actions = np.array(['hello', 'thanks', 'iloveyou'])


"""# 6. Preprocess Data and Create Labels and Features"""

# from tensorflow.keras.utils import to_categorical


model = Sequential()
model.add(LSTM(64, return_sequences=True, activation='relu', input_shape=(30,1662)))
model.add(LSTM(128, return_sequences=True, activation='relu'))
model.add(LSTM(64, return_sequences=False, activation='relu'))
model.add(Dense(64, activation='relu'))
model.add(Dense(32, activation='relu'))
model.add(Dense(actions.shape[0], activation='softmax'))

model.compile(optimizer='Adam', loss='categorical_crossentropy', metrics=['categorical_accuracy'])
model.load_weights('action.keras')


"""# 11. Test in Real Time"""

colors = []
# print(res)

# plt.figure(figsize=(18,18))
# plt.imshow(prob_viz(res, actions, image, colors))
def start_server():
 #   server = srvr.Server()
    server.start()


# 1. New detection variables
def run_model():
    start_server()
    sequence = []
    sentence = []
    predictions = []
    threshold = 0.5
    colors = Utils.generate_random_colors(5)

    cap = cv2.VideoCapture(0)
    # Set mediapipe model
    with mp.solutions.holistic.Holistic(min_detection_confidence=0.5, min_tracking_confidence=0.5) as holistic:
        while cap.isOpened():

            # Read feed
            ret, frame = cap.read()

            # Make detections
            image, results = Utils.mediapipe_detection(frame, holistic)
            #print(results)

            # Draw landmarks
            Utils.draw_styled_landmarks(image, results)

            # 2. Prediction logic
            keypoints = Utils.extract_keypoints(results)
            sequence.append(keypoints)
            sequence = sequence[-30:]

            showVideo = False

            if len(sequence) == 30:
                res = model.predict(np.expand_dims(sequence, axis=0))[0]
                print(actions[np.argmax(res)])
                currentAction = actions[np.argmax(res)]
                server.broadcast_variable(currentAction)
                predictions.append(np.argmax(res))

            # 3. Viz logic
                if showVideo:
                    if np.unique(predictions[-10:])[0] == np.argmax(res):
                        if res[np.argmax(res)] > threshold:

                            if len(sentence) > 0:
                                if actions[np.argmax(res)] != sentence[-1]:
                                    sentence.append(actions[np.argmax(res)])
                            else:
                                sentence.append(actions[np.argmax(res)])

                    if len(sentence) > 5:
                        sentence = sentence[-5:]

                    # Viz probabilities
                    image = Utils.prob_viz(res, actions, image, colors)

            if showVideo:
                cv2.rectangle(image, (0, 0), (640, 40), (245, 117, 16), -1)
                cv2.putText(image, ' '.join(sentence), (3, 30),
                            cv2.FONT_HERSHEY_SIMPLEX, 1, (255, 255, 255), 2, cv2.LINE_AA)

                # Show to screen
                cv2.imshow('OpenCV Feed', image)

                # Break gracefully
                if cv2.waitKey(10) & 0xFF == ord('q'):
                    break
        cap.release()
        cv2.destroyAllWindows()


run_model()
