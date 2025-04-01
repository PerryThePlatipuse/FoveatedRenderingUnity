import os
import sys
import cv2
import pygame
import numpy as np
import socket

# Настройка связи с Unity через UDP
UNITY_HOST = '127.0.0.1'
UNITY_PORT = 50666
udp_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

pygame.init()
pygame.font.init()

screen_info = pygame.display.Info()
screen_width = screen_info.current_w
screen_height = screen_info.current_h
screen = pygame.display.set_mode((screen_width, screen_height))
pygame.display.set_caption("EyeGestures v3 example")
font_size = 48
bold_font = pygame.font.Font(None, font_size)
bold_font.set_bold(True)

dir_path = os.path.dirname(os.path.realpath(__file__))
sys.path.append(f'{dir_path}/..')

from eyeGestures.utils import VideoCapture
from eyeGestures import EyeGestures_v3

gestures = EyeGestures_v3()
cap = VideoCapture(0)

# Создаем сетку точек для калибровки
x = np.arange(0, 1.1, 0.2)
y = np.arange(0, 1.1, 0.2)
xx, yy = np.meshgrid(x, y)
calibration_map = np.column_stack([xx.ravel(), yy.ravel()])
n_points = min(len(calibration_map), 25)
np.random.shuffle(calibration_map)

# Загружаем карту калибровки
gestures.uploadCalibrationMap(calibration_map, context="my_context")
gestures.setFixation(1.0)

RED = (255, 0, 100)
BLUE = (100, 0, 255)
GREEN = (0, 255, 0)
BLANK = (0, 0, 0)
WHITE = (255, 255, 255)

clock = pygame.time.Clock()

running = True
calibration_complete = False
iterator = 0
prev_x = 0
prev_y = 0

while running:
    for event in pygame.event.get():
        if event.type == pygame.QUIT:
            running = False
        elif event.type == pygame.KEYDOWN:
            if event.key == pygame.K_q and pygame.key.get_mods() & pygame.KMOD_CTRL:
                running = False

    # Считываем кадр, меняем цвет и зеркально отражаем изображение
    ret, frame = cap.read()
    frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
    frame = np.flip(frame, axis=1)

    calibrate = (iterator <= n_points) and not calibration_complete

    # Обрабатываем кадр для получения данных о взгляде и калибровке
    event, calibration = gestures.step(
        frame,
        calibrate,
        screen_width,
        screen_height,
        context="my_context"
    )

    screen.fill((0, 0, 0))
    frame_surface = pygame.surfarray.make_surface(frame)
    frame_surface = pygame.transform.scale(frame_surface, (400, 400))
    screen.blit(frame_surface, (0, 0))

    if event is not None or calibration is not None:
        if calibrate:
            # Если точка калибровки изменилась, обновляем счётчик
            if (calibration.point[0] != prev_x or calibration.point[1] != prev_y):
                iterator += 1
                prev_x = calibration.point[0]
                prev_y = calibration.point[1]

            # Рисуем точку калибровки и выводим текущий счётчик
            pygame.draw.circle(screen, BLUE, calibration.point, calibration.acceptance_radius)
            text = bold_font.render(f"{iterator}/{n_points}", True, WHITE)
            text_rect = text.get_rect(center=calibration.point)
            screen.blit(text, text_rect)

            if iterator >= n_points:
                calibration_complete = True
                print("Капибровка завершена! Начинаю отслеживание...")

        if calibration_complete and event is not None:
            # Нормализуем координаты взгляда и выводим в консоль
            x, y = event.point
            norm_x = -((x / screen_width) * 2 - 1)
            norm_y = -((y / screen_height) * 2 - 1)
            print(f"Координаты взгляда: X={norm_x:.1f}, Y={norm_y:.1f}")

            # Отправляем координаты в Unity через UDP
            try:
                data = f"{norm_x:.4f},{norm_y:.4f}"
                udp_socket.sendto(data.encode(), (UNITY_HOST, UNITY_PORT))
            except Exception as e:
                print(f"Ошибка передачи: {e}")

            # Рисуем круг в точке взгляда, цвет зависит от типа движения глаз
            color = GREEN if event.saccades else RED
            pygame.draw.circle(screen, color, event.point, 50)

            algorithm = gestures.whichAlgorithm(context="my_context")
            text = bold_font.render(algorithm, True, WHITE)
            screen.blit(text, event.point)

    pygame.display.flip()
    clock.tick(60)

pygame.quit()
udp_socket.close()
