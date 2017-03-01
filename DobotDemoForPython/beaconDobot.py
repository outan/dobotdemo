import DobotDllType as dType
api = dType.load()

# Set command timeout
dType.SetCmdTimeout(api, 3000)

errorString = ['Success','NotFound','Occupied']

dobotConnecResult = dType.ConnectDobot(api, "", 115200)
dobotConnecResult = dobotConnecResult[0]
print('trying to connet dobot.\nConnect status is: ')
print(errorString[dobotConnecResult])
if (dobotConnecResult == 0 ):
    print("going to SetPTPCommonParamsEx")
    dType.SetPTPCommonParamsEx(api,100,100,1)
    print("SetPTPCommonParamsEx complete")
else:
    print("can not connect to dobot. Please check the connection")

def on_message(*args):
    order = args[0]
    print('order from ble_button is: ' + order)
    if (dobotConnecResult == 0):
        if order == "water":
            #dType.SetPTPCmdEx(api, 0, 200,  0,  75, 0, 1)
            print("going to water")
            dType.SetPTPCmdEx(api, 0, -96,  -276,  67, 0, 1)
            dType.SetWAITCmdEx(api, 0.5, 1)
            print("suction cup on")
            dType.SetEndEffectorSuctionCupEx(api, 1, 1)
            dType.SetWAITCmdEx(api, 0.5, 1)
            print("moving near to water")
            dType.SetPTPCmdEx(api, 2, -56,  -168,  90, 0, 1)
            dType.SetWAITCmdEx(api, 0.5, 1)
            print("give the drink to customer ")
            dType.SetPTPCmdEx(api, 1, 200,  0,  70, 0, 1)
            dType.SetWAITCmdEx(api, 0.5, 1)
            print("suction cup off\n")
            dType.SetEndEffectorSuctionCupEx(api, 0, 1)
            dType.SetWAITCmdEx(api, 0.5, 1)
            print("get away from center\n")
            dType.SetPTPCmdEx(api, 0, 175,  105,  88, 0, 1)
        elif order == "tee":
            #dType.SetPTPCommonParamsEx(api,100,100,1)
            print("going to tee")
            dType.SetPTPCmdEx(api, 0, -1,  -274,  67, 0, 1)
            dType.SetWAITCmdEx(api, 0.5, 1)
            print("suction cup on")
            dType.SetEndEffectorSuctionCupEx(api, 1, 1)
            dType.SetWAITCmdEx(api, 0.5, 1)
            print("moving near")
            dType.SetPTPCmdEx(api, 2, -13,  -208,  75, 0, 1)
            dType.SetWAITCmdEx(api, 0.5, 1)
            print("give the drink to customer ")
            dType.SetPTPCmdEx(api, 1, 200,  0,  70, 0, 1)
            dType.SetWAITCmdEx(api, 0.5, 1)
            print("suction cup off\n")
            dType.SetEndEffectorSuctionCupEx(api, 0, 1)
            dType.SetWAITCmdEx(api, 0.5, 1)
            print("get away from center\n")
            dType.SetPTPCmdEx(api, 0, 175,  105,  88, 0, 1)
        elif order == "coffee":
            #dType.SetPTPCommonParamsEx(api,100,100,1)
            print("getting into coffee")
            dType.SetPTPCmdEx(api, 0, 85,  -271,  67, 0, 1)
            dType.SetWAITCmdEx(api, 0.5, 1)
            print("suction cup on")
            dType.SetEndEffectorSuctionCupEx(api, 1, 1)
            dType.SetWAITCmdEx(api, 0.5, 1)
            #dType.SetPTPCmdEx(api, 0, 98,  -198,  75, 0, 1)
            print("give the drink to customer ")
            dType.SetPTPCmdEx(api, 0, 200,  0,  70, 0, 1)
            dType.SetWAITCmdEx(api, 0.5, 1)
            print("suction cup off\n")
            dType.SetEndEffectorSuctionCupEx(api, 0, 1)
            dType.SetWAITCmdEx(api, 0.5, 1)
            print("get away from center\n")
            dType.SetPTPCmdEx(api, 0, 175,  105,  88, 0, 1)
        else:
            print("order is not in water,tee, coffee , disconnect dobot")
            dType.DisconnectDobot(api)


from socketIO_client import SocketIO, LoggingNamespace
def on_connect():
    print('python script on Macbook is connectted to edison1_pepper')

def on_connect_audio():
    print('it is connectted to itself audio')

socketIO_edison = SocketIO('edison1_pepper.local', 3000, LoggingNamespace)
socketIO_edison.on('connect', on_connect)
socketIO_edison.on('ble_button', on_message)
socketIO_edison.wait()
