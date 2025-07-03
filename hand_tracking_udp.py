import cv2
import mediapipe as mp
import socket
import json

# UDP 设置
UDP_IP = "127.0.0.1"
UDP_PORT = 9999
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

# MediaPipe 手部追踪初始化
mp_hands = mp.solutions.hands
hands = mp_hands.Hands(static_image_mode=False, max_num_hands=1, min_detection_confidence=0.7)
mp_drawing = mp.solutions.drawing_utils

cap = cv2.VideoCapture(0)

while cap.isOpened():
    success, frame = cap.read()
    if not success:
        print("⚠️ Failed to grab frame")
        continue

    # 翻转和颜色空间转换
    frame = cv2.flip(frame, 1)
    rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
    results = hands.process(rgb)

    if results.multi_hand_landmarks:
        for hand_landmarks in results.multi_hand_landmarks:
            data = []
            for lm in hand_landmarks.landmark:
                data.append({
                    'x': lm.x,
                    'y': lm.y,
                    'z': lm.z
                })
            json_data = json.dumps(data)
            sock.sendto(json_data.encode(), (UDP_IP, UDP_PORT))

            mp_drawing.draw_landmarks(frame, hand_landmarks, mp_hands.HAND_CONNECTIONS)

    cv2.imshow("Hand Tracking", frame)
    if cv2.waitKey(1) & 0xFF == 27:
        break

cap.release()
