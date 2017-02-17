import sys,threading,time
import DobotDllType as dType

from socketIO_client import SocketIO, LoggingNamespace
ble_button = None
def on_message(*args):
    print('on_response', args)
    ble_button = args
    print('ble_button is:')
    print(ble_button)

    # Listen
    api = dType.load()

    def GetPoseTask():
        pos = dType.GetPose(api)
        threading.Timer(0.2, GetPoseTask).start()

    threading.Timer(0.5, GetPoseTask).start()

    errorString = [
        'Success',
        'NotFound',
        'Occupied']

    result = dType.ConnectDobot(api, "", 115200)
    print("Connect status:",errorString[result[0]])

    print('ble_button is:')
    print(ble_button)

    if (result[0] == 0 and ble_button is not None):
        # Set command timeout
        dType.SetCmdTimeout(api, 3000)

        dType.SetJOGJointParams(api, 200, 200, 200, 200, 200, 200, 200, 200)
        dType.SetJOGCoordinateParams(api, 200, 200, 200, 200, 200, 200, 200, 200)
        dType.SetJOGCommonParams(api, 100, 100)

        #while 1:
        dType.SetJOGCmd(api, 1, 1)
        time.sleep(0.2)
        dType.SetJOGCmd(api, 1, 0)
        time.sleep(1)
        dType.SetJOGCmd(api, 1, 2)
        time.sleep(0.2)
        dType.SetJOGCmd(api, 1, 0)
        time.sleep(1)
        dType.DisconnectDobot(api)


def on_connect():
    print('it is connectted to edison1_pepper')

socketIO = SocketIO('edison1_pepper.local', 3000, LoggingNamespace)
socketIO.on('connect', on_connect)
socketIO.on('ble_button', on_message)
socketIO.wait()
