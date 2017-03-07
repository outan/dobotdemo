# -*- coding: utf-8 -*-
import DobotDllType as dType
import atexit

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

def all_done():
    dType.DisconnectDobot(api)
    print("disconnect from the dobot")

atexit.register(all_done)

def on_message_drink(*args):
    order = args[0]
    print('order from ble_button is: ' + order)
    if (dobotConnecResult == 0):
        get_drink(order)

def get_drink(order):
    if order == "water":
        #dType.SetPTPCmdEx(api, 0, 200,  0,  75, 0, 1)
        print("going to water")
        dType.SetPTPCmdEx(api, 0, -96,  -276,  67, 0, 1)
        dType.SetWAITCmdEx(api, 0.5, 1)
        print("suction cup on and get water")
        dType.SetEndEffectorSuctionCupEx(api, 1, 1)
        dType.SetWAITCmdEx(api, 0.5, 1)
        print("get back to arm")
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
        print("suction cup on and get tee")
        dType.SetEndEffectorSuctionCupEx(api, 1, 1)
        dType.SetWAITCmdEx(api, 0.5, 1)
        print("get back to arm")
        dType.SetPTPCmdEx(api, 2, -13,  -208,  90, 0, 1)
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
socketIO_edison.on('drink_button', on_message_drink)


#マイクからの入力を5秒間録音し、ファイル名：voice.wavで保存する。
import requests
import pyaudio
import sys
import time
import wave
import os.path
import json

try:
    import apiai
except ImportError:
    sys.path.append(
        os.path.join(os.path.dirname(os.path.realpath(__file__)), os.pardir)
    )
    import apiai
CLIENT_ACCESS_TOKEN = '3d93f36cf47f421386d517df3be45c07'

chunk = 1024*2
FORMAT = pyaudio.paInt16
CHANNELS = 1
#サンプリングレート、マイク性能に依存
RATE = 16000
#録音時間
RECORD_SECONDS =  4

def on_message_speech_order(*args):
    print("get into on_message_speech_order")
    time.sleep(4)
    print 'マイクに話しかけてください >>>'
    p = pyaudio.PyAudio()
    input_device_index = 0  #マイク0番を設定
    stream = p.open(  #マイクからデータ取得
        format = FORMAT,
        channels = CHANNELS,
        rate = RATE,
        input = True,
        input_device_index = input_device_index,
        frames_per_buffer = chunk
        )

    all = []
    for i in range(0, RATE / chunk * RECORD_SECONDS):
            data = stream.read(chunk)
            all.append(data)

    stream.close()
    data = ''.join(all)
    out = wave.open('voice.wav','w')
    out.setnchannels(1) #mono
    out.setsampwidth(2) #16bit
    out.setframerate(RATE)
    out.writeframes(data)
    out.close()

    p.terminate()

    print '<<< 録音完了'

    path = 'voice.wav'
    APIKEY = '484232446270726f464a6c3831737349657478796a4654717970364859574458656f78552e3974686b4631'
    url = "https://api.apigw.smt.docomo.ne.jp/amiVoice/v1/recognize?APIKEY={}".format(APIKEY)
    files = {"a": open(path, 'rb'), "v":"on"}
    print("files is ", files)
    r = requests.post(url, files=files)
    print("order text is: ")
    print r.json()['text']
    text = r.json()['text']

    water = "水".decode("utf-8")
    tee = "茶".decode("utf-8")
    coffee = "コーヒー".decode("utf-8")

    if water in text:
        socketIO_edison.emit('speech_order', 'water')
        print("emit speech_order: " + 'water')
        get_drink("water")
    elif tee in text:
        socketIO_edison.emit('speech_order', 'tee')
        print("emit speech_order: " + 'tee')
        get_drink("tee")
    elif coffee in text:
        socketIO_edison.emit('speech_order', 'coffee')
        print("emit speech_order: " + 'coffee')
        get_drink("coffee")
    else:
        socketIO_edison.emit('speech_order', 'notUnderstand')
        print("emit speech_order: " + 'notUnderstand')

        # ai = apiai.ApiAI(CLIENT_ACCESS_TOKEN)
        #
        # request = ai.text_request()
        #
        # request.lang = 'ja'  # optional, default value equal 'en'
        #
        # request.session_id = "<SESSION ID, UNIQUE FOR EACH USER>"
        #
        # request.query = text
        #
        # response1 = request.getresponse().read()
        # print ("response1 is: ")
        # print(response1)
        # response2 = json.loads(response1)
        # print ("response2 is: ")
        # print(response2)
        #
        # intent = response2['result']['metadata']['intentName']
        # print("intent is: ")
        # print(intent)
        # drinks = ['water', 'tee', 'coffee']
        # if intent in drinks:
        #   get_drink(intent)
        # elif intent == "Default Fallback Intent":
        #     #speech = response2['result']['fulfillment']['messages']['speech']
        #     speech = response2['result']['fulfillment']['messages'][0]['speech']
        #     print(speech)
        #     socketIO_edison.emit('notUnderstand', speech)
        #     print("emit notUnderstand:" + speech)


socketIO_edison.on('speech_order_button',on_message_speech_order)
socketIO_edison.on('speech_order',on_message_speech_order)

socketIO_edison.wait()
