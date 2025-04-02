import cv2
import mediapipe as mp

cam = cv2.VideoCapture(0)

# Подключение модели FaceMesh с ключевыми точками (refine_landmarks=True)
face_mesh = mp.solutions.face_mesh.FaceMesh(refine_landmarks=True)

while True:
    ret, frame = cam.read()
    if not ret:
        break

    frame = cv2.flip(frame, 1)
    rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)  # Преобразование кадра из BGR в RGB (требуется длч MediaPipe)
    output = face_mesh.process(rgb_frame)
    landmark_points = output.multi_face_landmarks
    frame_h, frame_w, _ = frame.shape

    # Если обнаружено хотя бы одно лицо на кадре
    if landmark_points:
        landmarks = landmark_points[0].landmark  # Выбираем ключевые точки первого обнаруженного лица

        # Берем точки, соответствующих области глаза
        eye_points = landmarks[474:478]
        for id, landmark in enumerate(eye_points):
            # Перевод координат из нормализованных значений в пиксельные значения
            x = int(landmark.x * frame_w)
            y = int(landmark.y * frame_h)
            cv2.circle(frame, (x, y), 3, (0, 255, 0))

            if id == 1:
                # Нормализация координат от -1 до 1 для осей X и Y(стандарт для корректной передачи в unity для оптимизации)
                x_norm = landmark.x * 2 - 1
                y_norm = 1 - landmark.y * 2  # Инвертирование оси Y: верхняя часть экрана будет иметь положительное значение
                print(f"X: {x_norm:.2f}, Y: {y_norm:.2f}")

    cv2.imshow('Eye Tracking', frame)
    # Если нажата клавиша 'q', выходим
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

cam.release()
cv2.destroyAllWindows()
