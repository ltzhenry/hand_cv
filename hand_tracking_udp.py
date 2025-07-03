import cv2
import mediapipe as mp
import socket
import json

# 初始化 MediaPipe
mp_hands = mp.solutions.hands
hands = mp_hands.Hands(max_num_hands=2, min_detection_confidence=0.7)
mp_drawing = mp.solutions.drawing_utils

# 初始化 UDP socket
udp_ip = "127.0.0.1"
udp_port = 9999
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

# 打开摄像头
cap = cv2.VideoCapture(0)
print("🟢 开始捕捉双手动作，按 q 键退出")

while cap.isOpened():
    success, frame = cap.read()
    if not success:
        print("❌ 摄像头读取失败")
        break

    # 翻转图像（自拍模式）
    frame = cv2.flip(frame, 1)
    rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
    results = hands.process(rgb)

    multi_hand_data = []

    if results.multi_hand_landmarks:
        for hand_landmarks in results.multi_hand_landmarks:
            single_hand = []
            for lm in hand_landmarks.landmark:
                single_hand.append({
                    'x': lm.x,
                    'y': lm.y,
                    'z': lm.z
                })
            multi_hand_data.append(single_hand)

        # 将数据打包成 JSON 并发送
        json_data = json.dumps(multi_hand_data)
        sock.sendto(json_data.encode(), (udp_ip, udp_port))

        # 可视化手部
        for hand_landmarks in results.multi_hand_landmarks:
            mp_drawing.draw_landmarks(frame, hand_landmarks, mp_hands.HAND_CONNECTIONS)

    cv2.imshow("Hand Tracking", frame)

    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

# 释放资源
cap.release()
cv2.destroyAllWindows()
sock.close()
