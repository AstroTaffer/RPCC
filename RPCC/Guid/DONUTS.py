import asyncio
import socket
import numpy as np
from donuts import Donuts

if __name__ == '__main__':

    def pars_req(req: str) -> str:
        # req = 'path1;path2' \ufeff
        # print(req)
        if 'ping' in req:
            return 'pong'
        data = req.split(';')
        if "\r\n" in data[1]:
            data[1] = data[1][:-2]
        # print(data)
        if len(data) != 2:
            return 'fail'
        try:
            donuts = Donuts(refimage=data[0], image_ext=0, overscan_width=24, prescan_width=24,
                            border=64, normalise=True, exposure='EXPTIME', subtract_bkg=True, ntiles=32)
            shift_result = donuts.measure_shift(data[1])
            # в ответ клиенту отправляем сообщение формата '+0.00 +0.00'
            return f'{np.round(shift_result.x.value, 2)} {np.round(shift_result.y.value, 2)}'
        except:
            return 'fail'

    async def handle_client(reader, writer):
        request = None
        while request != 'quit':
            request = (await reader.read(255)).decode('utf8')
            if (request is not None) and (request != '') and ('\ufeff' not in request):
                response = pars_req(request) + '\r\n'
                writer.write(response.encode('utf8'))
                await writer.drain()
        writer.close()
        await writer.wait_closed()


    async def run_server():
        server = await asyncio.start_server(handle_client, socket.gethostname(), 3030)
        async with server:
            await server.serve_forever()

    asyncio.run(run_server())

    # sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)  # создаем сокет
    # sock.bind(HOST)  # связываем сокет с портом, где он будет ожидать сообщения
    # sock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)  # реюз порта
    # sock.listen(1)  # указываем сколько может сокет принимать соединений
    # while True:
    #     err = 0
    #     print('Wait connection')
    #     conn, addr = sock.accept()  # начинаем принимать соединения
    #     try:
    #         print('Connected:', addr)  # выводим информацию о подключении
    #         size = conn.recv(2)  # принимаем данные от клиента, по 2 байт (2 цифры - длина сообщения пути к файлу)
    #         size = int(size.decode())
    #         if size == 0:
    #             break
    #         print('Size of next msg:', size)
    #         data = conn.recv(size)  # принимаем сообщение размером в size байт
    #         data = data.decode()
    #         print('Path to ref file:', data)
    #         print('Set ref file')
    #         donuts = Donuts(refimage=data, image_ext=0, overscan_width=24, prescan_width=24,
    #                         border=64, normalise=True, exposure='EXPTIME', subtract_bkg=True, ntiles=32)
    #         while conn:
    #             try:
    #                 data = conn.recv(size)
    #                 if not data:
    #                     break
    #                 data = data.decode()
    #                 print('Path to measure file:', data)
    #                 print('Calc shifts')
    #                 shift_result = donuts.measure_shift(data)
    #                 out = f'{np.round(shift_result.x.value, 2)} {np.round(shift_result.y.value, 2)}'
    #                 print(out)
    #                 conn.send(bytes(out, 'utf-8'))  # в ответ клиенту отправляем сообщение формата '+0.00 +0.00'
    #                 print('Shifts sanded')
    #             except:
    #                 print('Error in calc shifts')
    #                 err = err + 1
    #                 if err > 5:
    #                     break
    #     except:
    #         pass
    #     conn.close()  # закрываем соединение
