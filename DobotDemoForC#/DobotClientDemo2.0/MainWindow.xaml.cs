using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using DobotClientDemo.CPlusDll;

namespace DobotClientDemo
{
    /// <summary>
    /// MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private byte isJoint = (byte)0;
        private bool isConnectted = false;
        private JogCmd currentCmd;
        private Pose pose = new Pose();
        private System.Timers.Timer posTimer = new System.Timers.Timer();

        public MainWindow()
        {
            InitializeComponent();
            
            sld.Value = 30;
            sld.AddHandler(Slider.MouseLeftButtonUpEvent, new MouseButtonEventHandler(blurSlider_MouseLeftButtonUp), true);
            sld1.Value = 30;
            sld1.AddHandler(Slider.MouseLeftButtonUpEvent, new MouseButtonEventHandler(blurSlider_MouseLeftButtonUp), true);
            sldAcc.Value = 30;
            sldAcc.AddHandler(Slider.MouseLeftButtonUpEvent, new MouseButtonEventHandler(blurSlider_MouseLeftButtonUp), true);

            modeStyle.SelectedIndex = 2;

            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            StartGetPose();

            StartDobot();
        }

        /// <summary>
        /// StartDobot
        /// </summary>
        private void StartDobot()
        {
            int ret = DobotDll.ConnectDobot("", 115200);
            // start connect
            if (ret != (int)DobotConnect.DobotConnect_NoError)
            {
                Msg("Connect error", MsgInfoType.Error);
                return;
            }
            Msg("Connect success", MsgInfoType.Info);

            isConnectted = true;
            DobotDll.SetCmdTimeout(3000);

            // Must set when sensor is not exist
            DobotDll.ResetPose(true, 45, 45);

            // Get name
            string deviceName = "Dobot Magician";
            DobotDll.SetDeviceName(deviceName);

            StringBuilder deviceSN = new StringBuilder(64);
            DobotDll.GetDeviceName(deviceSN, 64);

            UInt64 cmdIndex = 0;
            JOGJointParams jsParam;
            jsParam.velocity = new float[]{200, 200, 200, 200};
            jsParam.acceleration = new float[] { 200, 200, 200, 200 };
            DobotDll.SetJOGJointParams(ref jsParam, false, ref cmdIndex);

            JOGCommonParams jdParam;
            jdParam.velocityRatio = 100;
            jdParam.accelerationRatio = 100;
            DobotDll.SetJOGCommonParams(ref jdParam, false, ref cmdIndex);

            PTPJointParams pbsParam;
            pbsParam.velocity = new float[] { 200, 200, 200, 200 };
            pbsParam.acceleration = new float[] { 200, 200, 200, 200 };
            DobotDll.SetPTPJointParams(ref pbsParam, false, ref cmdIndex);

            PTPCoordinateParams cpbsParam;
            cpbsParam.xyzVelocity = 100;
            cpbsParam.xyzAcceleration = 100;
            cpbsParam.rVelocity = 100;
            cpbsParam.rAcceleration = 100;
            DobotDll.SetPTPCoordinateParams(ref cpbsParam, false, ref cmdIndex);

            PTPJumpParams pjp;
            pjp.jumpHeight= 20;
            pjp.zLimit = 100;
            DobotDll.SetPTPJumpParams(ref pjp, false, ref cmdIndex);

            PTPCommonParams pbdParam;
            pbdParam.velocityRatio = 30;
            pbdParam.accelerationRatio = 30;
            DobotDll.SetPTPCommonParams(ref pbdParam, false, ref cmdIndex);
        }

        /// <summary>
        /// StartPeriodic
        /// </summary>
        private void StartGetPose()
        {
            posTimer.Elapsed += new System.Timers.ElapsedEventHandler(PosTimer_Tick);
            posTimer.Interval = 600;
            posTimer.Start();
        }

        private void PosTimer_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!isConnectted)
                return;

            DobotDll.GetPose(ref pose);

            this.Dispatcher.BeginInvoke((Action)delegate()
            {
                tbJoint1Angle.Text = pose.jointAngle[0].ToString();
                tbJoint2Angle.Text = pose.jointAngle[1].ToString();
                tbJoint3Angle.Text = pose.jointAngle[2].ToString();
                tbJoint4Angle.Text = pose.jointAngle[3].ToString();

                if (sync.IsChecked == true)
                {
                    X.Text = pose.x.ToString();
                    Y.Text = pose.y.ToString();
                    Z.Text = pose.z.ToString();
                    rHead.Text = pose.rHead.ToString();
                    pauseTime.Text = "0";
                }
            });
        }

        // event handle
        private void ProcessEvt(object sender, EventArgs e)
        {
            if (!isConnectted)
                return;

            Button obj = (Button)sender;
            String con = obj.Content.ToString();
            UInt64 cmdIndex = 0;

            float x, y, z, r, gripper, pTime;

            if (!float.TryParse(X.Text, out x) || !float.TryParse(Y.Text, out y) || !float.TryParse(Z.Text, out z) || !float.TryParse(rHead.Text, out r)
                || !float.TryParse(isGripper.Text, out gripper) || !float.TryParse(pauseTime.Text, out pTime))
            {
                Msg("Please input float formate", MsgInfoType.Error);
                return;
            }
            Msg("", MsgInfoType.Info);

            switch(con)
            {
                case "SendPlaybackCmd":
                    {
                        PTPCmd pdbCmd;

                        obj.IsEnabled = false;
                        pdbCmd.ptpMode = (byte)modeStyle.SelectedIndex;
                        pdbCmd.x = x;
                        pdbCmd.y = y;
                        pdbCmd.z = z;
                        pdbCmd.rHead = r;
                        int ret = DobotDll.SetPTPCmd(ref pdbCmd, true, ref cmdIndex);
                        while (true) {
                            UInt64 retIndex = 0;
                            int ind = DobotDll.GetQueuedCmdCurrentIndex(ref retIndex);
                            if (ind == 0 && cmdIndex <= retIndex)
                            {
                                obj.IsEnabled = true;
                                break;
                            }
                        }

                        float waitTime = 0;
                        if (float.TryParse(pauseTime.Text, out waitTime) && waitTime > 0)
                        {
                            WAITCmd waitcmd;
                            waitcmd.timeout = (uint)waitTime;
                            DobotDll.SetWAITCmd(ref waitcmd, false, ref cmdIndex);
                        }
                    }break;
                case "SendCPCmd":
                    {
                        CPCmd cpCmd;
                        cpCmd.x = x;
                        cpCmd.y = y;
                        cpCmd.z = z;
                        cpCmd.velocity = 100;
                        cpCmd.cpMode = (byte)ContinuousPathMode.CPAbsoluteMode;
                        int ret = DobotDll.SetCPCmd(ref cpCmd, true, ref cmdIndex);
                        while (true)
                        {
                            UInt64 retIndex = 0;
                            int ind = DobotDll.GetQueuedCmdCurrentIndex(ref retIndex);
                            if (ind == 0 && cmdIndex <= retIndex)
                            {
                                obj.IsEnabled = true;
                                break;
                            }
                        }
                    }break;
                default:
                    break;
            }
        }

        // control event handle
        private void OnEvent(object sender, MouseButtonEventArgs e)
        {
            if (!isConnectted)
                return;

            UInt64 cmdIndex = 0;
            Button obj = (Button)sender;
            String con = obj.Content.ToString();
            switch(con)
            {
                case "X+":
                case "Joint1+":
                    {
                        currentCmd.isJoint = isJoint;
                        currentCmd.cmd = e.ButtonState == MouseButtonState.Pressed ? (byte)JogCmdType.JogAPPressed : (byte)JogCmdType.JogIdle;
                        DobotDll.SetJOGCmd(ref currentCmd, false, ref cmdIndex);
                    }
                    break;
                case "X-":
                case "Joint1-":
                    {
                        currentCmd.isJoint = isJoint;
                        currentCmd.cmd = e.ButtonState == MouseButtonState.Pressed ? (byte)JogCmdType.JogANPressed : (byte)JogCmdType.JogIdle;
                        DobotDll.SetJOGCmd(ref currentCmd, false, ref cmdIndex);
                    }
                    break;
                case "Y+":
                case "Joint2+":
                    {
                        currentCmd.isJoint = isJoint;
                        currentCmd.cmd = e.ButtonState == MouseButtonState.Pressed ? (byte)JogCmdType.JogBPPressed : (byte)JogCmdType.JogIdle;
                        DobotDll.SetJOGCmd(ref currentCmd, false, ref cmdIndex);
                    }
                    break;
                case "Y-":
                case "Joint2-":
                    {
                        currentCmd.isJoint = isJoint;
                        currentCmd.cmd = e.ButtonState == MouseButtonState.Pressed ? (byte)JogCmdType.JogBNPressed : (byte)JogCmdType.JogIdle;
                        DobotDll.SetJOGCmd(ref currentCmd, false, ref cmdIndex);
                    }
                    break;
                case "Z+":
                case "Joint3+":
                    {
                        currentCmd.isJoint = isJoint;
                        currentCmd.cmd = e.ButtonState == MouseButtonState.Pressed ? (byte)JogCmdType.JogCPPressed : (byte)JogCmdType.JogIdle;
                        DobotDll.SetJOGCmd(ref currentCmd, false, ref cmdIndex);
                    }
                    break;
                case "Z-":
                case "Joint3-":
                    {
                        currentCmd.isJoint = isJoint;
                        currentCmd.cmd = e.ButtonState == MouseButtonState.Pressed ? (byte)JogCmdType.JogCNPressed : (byte)JogCmdType.JogIdle;
                        DobotDll.SetJOGCmd(ref currentCmd, false, ref cmdIndex);
                    }
                    break;
                case "R+":
                case "Joint4+":
                    {
                        currentCmd.isJoint = isJoint;
                        currentCmd.cmd = e.ButtonState == MouseButtonState.Pressed ? (byte)JogCmdType.JogDPPressed : (byte)JogCmdType.JogIdle;
                        DobotDll.SetJOGCmd(ref currentCmd, false, ref cmdIndex);
                    }
                    break;
                case "R-":
                case "Joint4-":
                    {
                        currentCmd.isJoint = isJoint;
                        currentCmd.cmd = e.ButtonState == MouseButtonState.Pressed ? (byte)JogCmdType.JogDNPressed : (byte)JogCmdType.JogIdle;
                        DobotDll.SetJOGCmd(ref currentCmd, false, ref cmdIndex);
                    }
                    break;
                case "Gripper+":
                    {
                        
                    }
                    break;
                case "Gripper-":
                    {
                        
                    }
                    break;
                default: 
                    break;
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isConnectted)
                return;

            ComboBox obj = (ComboBox)sender;
            String tag = obj.Tag.ToString();
            if(tag == "mode")
            {
                bool isJ = ((ComboBoxItem)obj.SelectedItem).Content.ToString() == "Axis";
                isJoint = isJ ? (byte)1 : (byte)0;
                if(isJ)
                {
                    XI.Content = "Joint1+";
                    YI.Content = "Joint2+";
                    ZI.Content = "Joint3+";
                    RI.Content = "Joint4+";

                    XN.Content = "Joint1-";
                    YN.Content = "Joint2-";
                    ZN.Content = "Joint3-";
                    RN.Content = "Joint4-";
                }
                else
                {
                    XI.Content = "X+";
                    YI.Content = "Y+";
                    ZI.Content = "Z+";
                    RI.Content = "R+";

                    XN.Content = "X-";
                    YN.Content = "Y-";
                    ZN.Content = "Z-";
                    RN.Content = "R-";
                }
            }
            else if(tag == "headType")
            {
                string str = ((ComboBoxItem)obj.SelectedItem).Content.ToString();
                if (str == "SuctionCup")
                {
                    cbGrab.IsEnabled = false;
                    cbLaser.IsEnabled = false;
                    cbSuctionCup.IsEnabled = true;
                    EndTypeParams endType;
                    endType.xBias = 59.7f;
                    endType.yBias = 0;
                    endType.zBias = 0;
                    DobotDll.SetEndEffectorParams(ref endType);
                }
                else if (str == "Gripper")
                {
                    cbGrab.IsEnabled = true;
                    cbLaser.IsEnabled = false;
                    cbSuctionCup.IsEnabled = false;
                    EndTypeParams endType;
                    endType.xBias = 59.7f;
                    endType.yBias = 0;
                    endType.zBias = 0;
                    DobotDll.SetEndEffectorParams(ref endType);
                }
                else if (str == "Laser")
                {
                    cbGrab.IsEnabled = false;
                    cbLaser.IsEnabled = true;
                    cbSuctionCup.IsEnabled = false;
                    EndTypeParams endType;
                    endType.xBias = 70f;
                    endType.yBias = 0;
                    endType.zBias = 0;
                    DobotDll.SetEndEffectorParams(ref endType);
                }
                
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            DobotDll.DisconnectDobot();
        }

        private void Msg(string str, MsgInfoType infoType)
        {
            lbTip.Content = str;
            switch(infoType)
            {
                case MsgInfoType.Error:
                    lbTip.Foreground = new SolidColorBrush(Colors.Red);
                    break;
                case MsgInfoType.Info:
                    lbTip.Foreground = new SolidColorBrush(Colors.Black);
                    break;
                default: 
                    break;
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!isConnectted)
                return;

            CheckBox obj = (CheckBox)sender;
            String con = obj.Content.ToString();
            UInt64 cmdIndex = 0;
            if (con == "Grab") // grab
            {
                DobotDll.SetEndEffectorGripper(true, true, false, ref cmdIndex);
            }
            else if (con == "Laser") // Shutting
            {
                DobotDll.SetEndEffectorLaser(true, true, false, ref cmdIndex);
            }
            else if (con == "SuctionCup")
            {
                DobotDll.SetEndEffectorSuctionCup(true, true, false, ref cmdIndex);
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!isConnectted)
                return;

            CheckBox obj = (CheckBox)sender;
            String con = obj.Content.ToString();
            UInt64 cmdIndex = 0;
            if (con == "Grab") // cancel grab
            {
                DobotDll.SetEndEffectorGripper(true, false, false, ref cmdIndex);
            }
            else if (con == "Laser") // release laser
            {
                DobotDll.SetEndEffectorLaser(false, false, false, ref cmdIndex);
            }
            else if (con == "SuctionCup")
            {
                DobotDll.SetEndEffectorSuctionCup(false, false, false, ref cmdIndex);
            }
        }

        private void blurSlider_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!isConnectted)
                return;

            UInt64 cmdIndex = 0;
            Slider obj = (Slider)sender;
            if (obj.Name == "sld")
            {
                JOGCommonParams jdParam;
                jdParam.velocityRatio = (float)sld.Value;
                jdParam.accelerationRatio = 100;
                DobotDll.SetJOGCommonParams(ref jdParam, false, ref cmdIndex);
            }
            else if (obj.Name == "sld1" || obj.Name == "sldAcc") // playback
            {
                PTPCommonParams pbdParam;
                pbdParam.velocityRatio = (float)sld1.Value;
                pbdParam.accelerationRatio = (float)sldAcc.Value;
                DobotDll.SetPTPCommonParams(ref pbdParam, false, ref cmdIndex);
            }
        }
    }
}
