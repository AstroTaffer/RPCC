import socket
import datetime
import threading
import numpy as np
from donuts import Donuts

time_last_message = 0
time_wait_sec = 3
server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)


def timer_loop():
    now = datetime.datetime.utcnow()
    if time_last_message != 0:
        if (now - time_last_message).total_seconds() > time_wait_sec:
            server.close()
            print('Donuts timeout, closing server')


def pars_req(req: str) -> str:
    if 'ping' in req:
        return 'pong'
    data = req.split(';')
    if len(data) != 2:
        return 'fail'
    if "\n" in data[1]:
        data[1] = data[1][:-1]

    try:
        donuts = Donuts(refimage=data[0], image_ext=0, overscan_width=24, prescan_width=24,
                        border=64, normalise=True, exposure='EXPTIME', subtract_bkg=True, ntiles=32)
        shift_result = donuts.measure_shift(data[1])
        return f'{np.round(shift_result.x.value, 2)} {np.round(shift_result.y.value, 2)}'
    except:
        return 'fail'


def handle_client(reader, writer):
    t = threading.Timer(time_wait_sec, timer_loop)
    t.start()
    while True:
        request = reader.readline()
        time_last_message = datetime.datetime.utcnow()
        if request == 'quit\n':
            break
        if (request is not None) and (request != '') and ('\ufeff' not in request):
            response = pars_req(request) + '\n'
            writer.writelines(response)
            writer.flush()
    writer.close()


def run_server():
    server.bind((socket.gethostname(), 3030))
    server.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)  # реюз порта
    server.listen(1)
    # while True:
    client_socket, _ = server.accept()
    # client_socket.settimeout(60)
    try:
        with client_socket:
            handle_client(client_socket.makefile('r'), client_socket.makefile('w'))
    except ConnectionAbortedError:
        pass


if __name__ == '__main__':
    run_server()
