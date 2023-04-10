from donuts import Donuts
import socket
import numpy as np

if __name__ == '__main__':
    HOST = (socket.gethostname(), 3030)
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)  # создаем сокет
    sock.bind(HOST)  # связываем сокет с портом, где он будет ожидать сообщения
    sock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)  # реюз порта
    sock.listen(1)  # указываем сколько может сокет принимать соединений
    while True:
        print('Wait connection')
        conn, addr = sock.accept()  # начинаем принимать соединения
        print('Connected:', addr)  # выводим информацию о подключении
        size = conn.recv(2)  # принимаем данные от клиента, по 2 байт (2 цифры - длина сообщения пути к файлу)
        size = int(size.decode())
        print('Size of next msg:', size)
        data = conn.recv(size)  # принимаем сообщение размером в size байт
        data = data.decode()
        print('Path to ref file:', data)
        print('Set ref file')
        donuts = Donuts(refimage=data, image_ext=0, overscan_width=24, prescan_width=24,
                        border=64, normalise=True, exposure='EXPTIME', subtract_bkg=True, ntiles=32)
        while conn:
            try:
                data = conn.recv(size)
                if not data:
                    break
                data = data.decode()
                print('Path to measure file:', data)
                print('Calc shifts')
                shift_result = donuts.measure_shift(data)
                out = f'{np.round(shift_result.x.value, 2)} {np.round(shift_result.y.value, 2)}'
                print(out)
                conn.send(bytes(out, 'utf-8'))  # в ответ клиенту отправляем сообщение формата '+0.00 +0.00'
                print('Shifts sanded')
            except:
                print('Error in calc shifts')
                # break
        conn.close()  # закрываем соединение
