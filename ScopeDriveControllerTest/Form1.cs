#define LOGGING_ON
#define TEST_SLOW_PWM

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AAB.UtilityLibrary;

namespace ScopeDriveControllerTest
{
    public partial class TestForm : Form
    {
        public TestForm()
        {
            InitializeComponent();
        }

        private const int M_RESOLUTION = 4000;
        private const int S_RESOLUTION = 10000;

        private const byte A_ALT = 0;   // command for alt adapter
        private const byte A_AZM = 1;   // command for azm adapter
        private const byte M_ALT = 2;   // command for alt motor (debug only)
        private const byte M_AZM = 3;   // command for azm motor (debug only)
        private const byte PWM_ALT = 4; // PWM command for alt motor (debug only)
        private const byte PWM_AZM = 5; // PWM command for azm motor (debug only)

        private const UInt16 LMODE_ALT = 0;
        private const UInt16 LMODE_AZM = 0x8000;

        private const UInt16 LMODE_FIRST = 1;
        private const UInt16 LMODE_MPOS = 1;
        private const UInt16 LMODE_MLOG = 2;
        private const UInt16 LMODE_MSPD = 4;
        private const UInt16 LMODE_MERR = 8;
        private const UInt16 LMODE_APOS = 0x10;
        private const UInt16 LMODE_ALOG = 0x20;
        private const UInt16 LMODE_ASPD = 0x40;
        private const UInt16 LMODE_AERR = 0x80;
        private const UInt16 LMODE_LAST = 0x100;

        private const UInt16 LMODE_OFF = 0;

        private byte mode_ = M_AZM;
        private bool ignoreModeButtonChanged_ = false;
        
        private bool init_ = false;
        private MainFormSettings settings_ = new MainFormSettings();

        // connection
        private string portName_;
        private int baudRate_ = 115200;
        private SerialConnection connection_;

        private delegate void TimeoutDelegate(SerialConnection connection);
        private delegate void ReceiveDelegate(byte[] data);
        private class BaseConnectionHandler : SerialConnection.IReceiveHandler
        {
            public BaseConnectionHandler(TestForm parent, ReceiveDelegate receiveDelegate, SerialConnection connection)
            {
                parent_ = parent;
                receiveDelegate_ = receiveDelegate;
                connection_ = connection;
            }

            public void Error()
            {
                TimeoutDelegate d = new TimeoutDelegate(parent_.SerialError);
                parent_.BeginInvoke(d, new object[] { connection_ });
            }

            public void Received(byte[] data)
            {
                parent_.BeginInvoke(receiveDelegate_, new object[] { data });
            }

            private TestForm parent_;
            private ReceiveDelegate receiveDelegate_;
            private SerialConnection connection_;
        }

        // timeout class
        public class Timeout
        {
            private DateTime start_;
            private int timeoutInMS_;

            public Timeout(int timeoutInMS)
            {
                timeoutInMS_ = timeoutInMS;
                Restart();
            }

            public void Restart() { start_ = DateTime.Now; }

            public bool CheckExpired() { return CheckExpired(true); }
            public bool CheckExpired(bool restart)
            {
                DateTime now = DateTime.Now;
                if ((now - start_).TotalMilliseconds < timeoutInMS_)
                    return false;
                if (restart)
                    start_ = now;
                return true;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////
        private void CloseConnection(SerialConnection connection)
        {
            if (connection != null)
            {
                if (connection == connection_)
                {
                    connection_.Close();
                    connection_ = null;
                    buttonConnect.Text = "Connect";
                }
            }
            started_ = false;
            UpdateUI();
            prevAltPos_.Clear();
            prevTs_.Clear();
        }
        private void SerialError(SerialConnection connection)
        {
            CloseConnection(connection);
        }
        private void SendCommand(SerialConnection connection, char cmd, int receiveCnt, ReceiveDelegate receiveDelegate)
        {
            if (connection != null)
                connection.SendReceiveRequest(new byte[] { (byte)cmd }, receiveCnt, new BaseConnectionHandler(this, receiveDelegate, connection));
        }
        private void SendCommand(SerialConnection connection, byte[] cmd, int receiveCnt, ReceiveDelegate receiveDelegate)
        {
            if (connection_ != null)
                connection_.SendReceiveRequest(cmd, receiveCnt, new BaseConnectionHandler(this, receiveDelegate, connection));
        }

        private void ReceiveDummy(byte[] data)
        {
        }

        private void ChangeSpeed(Int32 speed, byte dst)
        {
            if (!started_)
            {
                if (connection_ != null)
                    SendCommand(connection_, new byte[] { (byte)'S',
                                                          dst,
                                                          (byte)speed,
                                                          (byte)(speed >> 8),
                                                          (byte)(speed >> 16),
                                                          (byte)(speed >> 24)}, 8, ReceiveStart);
                speed_ = speed;
                tmoSendPos_.Restart();
            }
            else
            {
                if (connection_ != null)
                    SendCommand(connection_, new byte[] { (byte)'V',
                                                          dst,
                                                          (byte)speed,
                                                          (byte)(speed >> 8),
                                                          (byte)(speed >> 16),
                                                          (byte)(speed >> 24)}, 8, ReceiveStart);
                speed_ = speed;
                tmoSendPos_.Restart();
            }
        }

        private void SendGetMotorAndAdapterConfigOptions()
        {
            if (connection_ != null)
                SendCommand(connection_, new byte[] { (byte)'Z' }, 4, ReceiveMotorConfigOptionsSizes);
        }
        private void SendGetMotorConfigOptions(byte dst)
        {
            if (connection_ != null)
                SendCommand(connection_, new byte[] { (byte)'O', dst }, motorOptionSize_ + 1, ReceiveMotorConfigOptions);
        }
        private void SendGetAdapterConfigOptions(byte dst)
        {
            if (connection_ != null)
                SendCommand(connection_, new byte[] { (byte)'O', dst }, adapterOptionSize_ + 1, ReceiveAdapterConfigOptions);
        }

#if LOGGING_ON
        private UInt16 GetLoggingMode()
        {
            switch (mode_)
            {
            case M_ALT:
            case A_ALT:
            case PWM_ALT:
                return (UInt16)((1 << comboBoxLoggingType0.SelectedIndex) | (1 << comboBoxLoggingType1.SelectedIndex));

            case M_AZM:
            case A_AZM:
            case PWM_AZM:
                return (UInt16)((1 << comboBoxLoggingType0.SelectedIndex) | (1 << comboBoxLoggingType1.SelectedIndex) | LMODE_AZM);

            default:
                return LMODE_OFF;
            }
        }

        private void SendLoggingMode()
        {
            if (connection_ != null)
            {
                UInt16 loggingMode = (!started_ || !checkBoxLogging.Checked) ? LMODE_OFF : GetLoggingMode();
                SendCommand(connection_, new byte[] { (byte)'L', (byte)'m', (byte)loggingMode, (byte)(loggingMode >> 8) }, 1, ReceiveDummy);
            }
        }
#endif

        private void SendSetNextPosCommand(float sp, Int32 ts, byte dst, ReceiveDelegate receiveDelegate)
        {
            byte[] spBytes = BitConverter.GetBytes(sp);
            if (connection_ != null)
                SendCommand(connection_, new byte[] { (byte)'N',
                                                      dst,
                                                      spBytes[0],
                                                      spBytes[1],
                                                      spBytes[2],
                                                      spBytes[3],
                                                      (byte)ts,
                                                      (byte)(ts >> 8),
                                                      (byte)(ts >> 16),
                                                      (byte)(ts >> 24)}, 8, receiveDelegate);
        }

        Int32 nextTs_;
        double nextSp_;
        private void ShiftPos(Int32 value, byte dst)
        {
            if (mode_ == PWM_ALT || mode_ == PWM_AZM)
                return;
            if (started_)
            {
                if (checkBoxSetNextPos.Checked)
                {
                    startAltPos_ += value;
                    //timerSendNextPosTicks_ = 0;
                }
                else
                {
                    Int32 elapsed = (Int32)(DateTime.Now - startDT_).TotalMilliseconds;
                    Int32 ts = startTs_ + elapsed + 30000;
                    double sp = startAltPos_ + speed_ * (elapsed + 30000) / (24.0 * 60.0 * 60000.0) + value;
                    nextTs_ = ts;
                    nextSp_ = sp;
                    SendSetNextPosCommand((float)sp, ts, dst, ReceiveShiftNextPos);
                }
            }
        }

        private void ChangePWMSpeed(Int16 value, Int16 period, float dutyCycle, byte dst)
        {
            byte[] dcBytes = BitConverter.GetBytes(dutyCycle);
            if (connection_ != null)
            {
                if (value != 0)
                    SendCommand(connection_, new byte[] { (byte)'W',
                                                      dst,
                                                      (byte)value,
                                                      (byte)(value >> 8),
                                                      (byte)period,
                                                      (byte)(period >> 8),
                                                      dcBytes[0],
                                                      dcBytes[1],
                                                      dcBytes[2],
                                                      dcBytes[3]},
                                                      8, ReceiveStart);
                else
                    SendCommand(connection_, new byte[] { (byte)'W',
                                                      dst,
                                                      (byte)0,
                                                      (byte)0,
                                                      (byte)0,
                                                      (byte)0,
                                                      dcBytes[0],
                                                      dcBytes[1],
                                                      dcBytes[2],
                                                      dcBytes[3]},
                                                      8, ReceiveStop);
            }
            speed_ = 0;
        }

        private void Stop(byte dst)
        {
            if (connection_ != null)
                SendCommand(connection_, new byte[] { (byte)'T', dst }, 1, ReceiveStop);
            speed_ = 0;
        }

        private void UpdateUI()
        {
#if LOGGING_ON
            if (started_)
            {
                checkBoxLogging.Enabled = false;
                buttonSaveLog.Enabled = false;
            }
            else
            {
                checkBoxLogging.Enabled = true;
                buttonSaveLog.Enabled = true;
            }
#endif
        }

        private void ReceiveStart(byte[] data)
        {
            Int32 altPos = (Int32)((((UInt32)data[3]) << 24) + (((UInt32)data[2]) << 16) + (((UInt32)data[1]) << 8) + data[0]);
            Int32 ts = (int)((((UInt32)data[7]) << 24) + (((UInt32)data[6]) << 16) + (((UInt32)data[5]) << 8) + data[4]);

            started_ = true;
            startTs_ = ts;
            startDT_ = DateTime.Now;
            startAltPos_ = altPos;

            UpdateUI();
#if LOGGING_ON
            SendLoggingMode();
#endif
            PrintPosition(altPos, ts, 0, 0, 0, 0);
        }

        private void ReceiveShiftNextPos(byte[] data)
        {
            Int32 altPos = (Int32)((((UInt32)data[3]) << 24) + (((UInt32)data[2]) << 16) + (((UInt32)data[1]) << 8) + data[0]);
            Int32 ts = (int)((((UInt32)data[7]) << 24) + (((UInt32)data[6]) << 16) + (((UInt32)data[5]) << 8) + data[4]);

            startTs_ = ts;
            startDT_ = DateTime.Now;
            startAltPos_ = altPos;
            speed_ = ((double)(nextSp_ - altPos) * 60000.0 * 60.0 * 24.0 / (double)(nextTs_ - ts));
            tmoSendPos_.Restart();
            
            prevAltPos_.Clear();
            prevTs_.Clear();
            PrintPosition(altPos, ts, 0, 0, 0, 0);
        }

        private void ReceiveSetNextPos(byte[] data)
        {
            Int32 altPos = (Int32)((((UInt32)data[3]) << 24) + (((UInt32)data[2]) << 16) + (((UInt32)data[1]) << 8) + data[0]);
            Int32 ts = (int)((((UInt32)data[7]) << 24) + (((UInt32)data[6]) << 16) + (((UInt32)data[5]) << 8) + data[4]);
            PrintPosition(altPos, ts, 0, 0, 0, 0);
        }

        private void ReceiveStop(byte[] data)
        {
            if (started_)
            {
                started_ = false;
#if LOGGING_ON
                SendLoggingMode();
#endif
                UpdateUI();
                prevAltPos_.Clear();
                prevTs_.Clear();
            }
        }

        private int motorOptionSize_ = 29, adapterOptionSize_ = 48;
        private void ReceiveMotorConfigOptionsSizes(byte[] data)
        {
            motorOptionSize_ = BitConverter.ToInt16(data, 0);
            adapterOptionSize_ = BitConverter.ToInt16(data, 2);
            SendGetMotorConfigOptions(M_AZM);
            SendGetMotorConfigOptions(M_ALT);
            SendGetAdapterConfigOptions(A_AZM);
            SendGetAdapterConfigOptions(A_ALT);
        }

        private MotorOptions motorAzmOptions_, motorAltOptions_;
        private void ReceiveMotorConfigOptions(byte[] data)
        {
            switch (data[0])
            {
            default:
            case M_ALT: ReadMotorOptions(data, 1, out motorAltOptions_); break;
            case M_AZM: ReadMotorOptions(data, 1, out motorAzmOptions_); break;
            }
        }

        private AdapterOptions adapterAzmOptions_, adapterAltOptions_;
        private void ReceiveAdapterConfigOptions(byte[] data)
        {
            switch (data[0])
            {
            default:
            case A_ALT: ReadAdapterOptions(data, 1, out adapterAltOptions_); break;
            case A_AZM: ReadAdapterOptions(data, 1, out adapterAzmOptions_); break;
            }
        }

        private enum ApproximationType
        {
            LINEAR,
            EXPONENTIAL
        };
        private struct PWMProfile   // size = 4
        {
            public byte value_;        // required value
            public byte magnitude_;    // PWM magnitude
            public Int16 period_;      // PWM (and PID) period
        }
        private struct MotorOptions             // size = 4 + 4*4 + 1 + 4*2 = 29
        {
            public Int32 encRes_;
            public float maxSpeedRPM_;
            public float Kp_, KiF_, Kd_;
            public ApproximationType approximationType_;
            public PWMProfile loProfile_, hiProfile_;
        }

        private void ReadMotorOptions(byte[] data, int offset, out MotorOptions opt)
        {
            opt = new MotorOptions();
            opt.encRes_                 = BitConverter.ToInt32(data, offset);
            opt.maxSpeedRPM_            = BitConverter.ToSingle(data, offset + 4);
            opt.Kp_                     = BitConverter.ToSingle(data, offset + 8);
            opt.KiF_                    = BitConverter.ToSingle(data, offset + 12);
            opt.Kd_                     = BitConverter.ToSingle(data, offset + 16);
            opt.approximationType_      = data[offset + 20] == 0 ? ApproximationType.LINEAR : ApproximationType.EXPONENTIAL;
            opt.loProfile_.value_       = data[offset + 21];
            opt.loProfile_.magnitude_   = data[offset + 22];
            opt.loProfile_.period_      = BitConverter.ToInt16(data, offset + 23);
            opt.hiProfile_.value_       = data[offset + 25];
            opt.hiProfile_.magnitude_   = data[offset + 26];
            opt.hiProfile_.period_      = BitConverter.ToInt16(data, offset + 27);
        }

        private struct AdapterOptions             // size = 4*12 = 48
        {
            public Int32 encRes_;
            public float scopeToMotor_;
            public float deviationSpeedFactor_, KiF_, KdF_, KpFast2F_, KpFast3F_;
            public float diff2_, diff3_;
            public Int32 pidPollPeriod_;    // ms
            public Int32 adjustPidTmo_;     // ms
            public Int32 speedSmoothTime_;  // ms
        }

        private void ReadAdapterOptions(byte[] data, int offset, out AdapterOptions opt)
        {
            opt = new AdapterOptions();
            opt.encRes_                 = BitConverter.ToInt32(data, offset);
            opt.scopeToMotor_           = BitConverter.ToSingle(data, offset + 4);
            opt.deviationSpeedFactor_   = BitConverter.ToSingle(data, offset + 8);
            opt.KiF_                    = BitConverter.ToSingle(data, offset + 12);
            opt.KdF_                    = BitConverter.ToSingle(data, offset + 16);
            opt.KpFast2F_               = BitConverter.ToSingle(data, offset + 20);
            opt.KpFast3F_               = BitConverter.ToSingle(data, offset + 24);
            opt.diff2_                  = BitConverter.ToSingle(data, offset + 28);
            opt.diff3_                  = BitConverter.ToSingle(data, offset + 32);
            opt.pidPollPeriod_          = BitConverter.ToInt32(data, offset + 36);
            opt.adjustPidTmo_           = BitConverter.ToInt32(data, offset + 40);
            opt.speedSmoothTime_        = BitConverter.ToInt32(data, offset + 44);
        }

        private bool started_ = false;
        private double speed_ = 0;
        private Int32 startAltPos_;
        private Int32 startTs_;
        private DateTime startDT_;
        private List<Int32> prevAltPos_ = new List<int>();
        private List<Int32> prevTs_ = new List<int>();
        private const int MAX_POSITIONS = 50;
        //private int timerSendNextPosTicks_ = 0;
        private Timeout tmoSendPos_ = new Timeout(8000);

#if LOGGING_ON
        private const int LOG_PERIOD = 200; //msec

        private UInt32 logStart_ = 0;
        private Int32 logTs_ = Int32.MinValue;
        private Int32 logAbs1_ = Int32.MinValue, logAbs2_ = Int32.MinValue;
        private int logNextBlockSize_ = 0;
        private List<Int32> logData_ = new List<int>();
        private Timeout tmoAddLogData_ = new Timeout(1000);

        private void AddLogData(byte[] data)
        {
            int reported = (int)data[1];
            int stillInBuffer = (int)data[2];
            if (data[3] != 0)   // ring buffer was overflowed
            {
                // force restart and re-synchronization
                logStart_ = 0;
                logAbs1_ = logAbs2_ = logTs_ = Int32.MinValue;
            }

            logNextBlockSize_ = stillInBuffer;

            for (int i = 0; i < reported; ++i)
            {
                int start = i * 4 + 4;
                if (data[start + 3] == 0x80)
                {
                    // re-sync!
                    logStart_ = (((UInt32)data[start + 3]) << 24) + (((UInt32)data[start + 2]) << 16) + (((UInt32)data[start + 1]) << 8) + (UInt32)data[start];
                    logAbs1_ = logAbs2_ = logTs_ = Int32.MinValue;
                    continue;
                }
                if (logStart_ == 0)
                    continue;       // skip it: waiting for re-sync

                if (logTs_ == Int32.MinValue)
                {
                    logTs_ = (Int32)((((UInt32)(byte)(logStart_ & 1L)) << 31) + (((UInt32)data[start + 3]) << 24) + 
                                     (((UInt32)data[start + 2]) << 16) + (((UInt32)data[start + 1]) << 8) + (UInt32)data[start]);
                    continue;
                }
                if (logAbs1_ == Int32.MinValue)
                {
                    logAbs1_ = (Int32)((((UInt32)(byte)(logStart_ & 2L)) << 30) + (((UInt32)data[start + 3]) << 24) +
                                       (((UInt32)data[start + 2]) << 16) + (((UInt32)data[start + 1]) << 8) + (UInt32)data[start]);
                    continue;
                }

                Int32 pos1, pos2;
                if (logAbs2_ == Int32.MinValue)
                {
                    pos1 = logAbs1_;
                    pos2 = logAbs2_ = (Int32)((((UInt32)(byte)(logStart_ & 4L)) << 29) + (((UInt32)data[start + 3]) << 24) +
                                              (((UInt32)data[start + 2]) << 16) + (((UInt32)data[start + 1]) << 8) + (UInt32)data[start]);
                }
                else
                {
                    pos1 = logAbs1_ + (Int16)((((UInt16)data[start + 1]) << 8) + (UInt16)data[start]);
                    pos2 = logAbs2_ + (Int16)((((UInt16)data[start + 3]) << 8) + (UInt16)data[start + 2]);
                }

                logData_.Add(logTs_);
                logData_.Add(pos1);
                logData_.Add(pos2);

                logTs_ += LOG_PERIOD;
            }
        }
#endif

        private void PrintPosition(Int32 pos, Int32 ts, Int32 rsp, Int32 dbg, Int32 motorAltPos, Int32 scopeAltPos)
        {
            if (connection_ == null)
            {
                textBoxOutput.Text = "";
                return;
            }
            textBoxOutput.Text = "DBG = " + dbg.ToString() + Environment.NewLine;
            textBoxOutput.Text += "RSP = " + rsp.ToString() + Environment.NewLine;
            textBoxOutput.Text += "POS = " + pos.ToString() + Environment.NewLine;
            //textBoxOutput.Text += "POS/22 = " + (altPos / 22).ToString() + Environment.NewLine + Environment.NewLine;

            if (started_)
            {
                double sp = 0;
                if (ts != startTs_)
                {
                    sp = startAltPos_ + speed_ * (ts - startTs_) / (24.0 * 60.0 * 60000.0);
                    textBoxOutput.Text += "SP  = " + sp.ToString() + Environment.NewLine;
                }

                textBoxOutput.Text += Environment.NewLine;
                textBoxOutput.Text += "Speed_ = " + speed_.ToString("F3") + Environment.NewLine;

                if (prevAltPos_.Count > 0)
                {
                    int delta = (int)(pos - prevAltPos_[0]);
                    int dt = ts - prevTs_[0];
                    if (dt == 0)
                        dt = 1;
                    double speed, err;
                    string unit;
                    switch (mode_)
                    {
                    default:
                    case A_ALT:
                    case A_AZM:
                        speed = (double)delta * 60 * 60000.0 * 24 / (double)dt;
                        unit = " units/day";
                        err = (double)(pos - sp) * 360.0 * 60.0 / ((double)S_RESOLUTION);
                        break;
                    case M_ALT:
                    case M_AZM:
                    case PWM_ALT:
                    case PWM_AZM:
                        speed = ((double)delta / M_RESOLUTION) * 60 * 1000.0 / (double)dt;
                        unit = " rpm";
                        err = (double)(pos - rsp) * 360.0 * 60.0 / ((double)M_RESOLUTION * 28.0 * 20.0);
                        break;
                    }

                    textBoxOutput.Text += "SPEED = " + speed.ToString() + unit + Environment.NewLine;
                    textBoxOutput.Text += "Scope Err = " + err.ToString() + " arc min" + Environment.NewLine;
                }

                prevAltPos_.Add(pos);
                prevTs_.Add(ts);
                if (prevAltPos_.Count > MAX_POSITIONS)
                {
                    prevAltPos_.RemoveAt(0);
                    prevTs_.RemoveAt(0);
                }
            }

            textBoxOutput.Text += Environment.NewLine;
            textBoxOutput.Text += "Motor POS = " + motorAltPos.ToString() + Environment.NewLine;
            textBoxOutput.Text += "Scope POS = " + scopeAltPos.ToString() + Environment.NewLine;
        }

        private void ReceivePosition(byte[] data)
        {
            Int32 pos = (Int32)((((UInt32)data[3]) << 24) + (((UInt32)data[2]) << 16) + (((UInt32)data[1]) << 8) + data[0]);
            Int32 ts = (int)((((UInt32)data[7]) << 24) + (((UInt32)data[6]) << 16) + (((UInt32)data[5]) << 8) + data[4]);
            Int32 sp = (int)((((UInt32)data[11]) << 24) + (((UInt32)data[10]) << 16) + (((UInt32)data[9]) << 8) + data[8]);
            Int32 dbg = (int)((((UInt32)data[15]) << 24) + (((UInt32)data[14]) << 16) + (((UInt32)data[13]) << 8) + data[12]);
            Int32 motorAltPos = (int)((((UInt32)data[19]) << 24) + (((UInt32)data[18]) << 16) + (((UInt32)data[17]) << 8) + data[16]);
            Int32 scopeAltPos = (int)((((UInt32)data[23]) << 24) + (((UInt32)data[22]) << 16) + (((UInt32)data[21]) << 8) + data[20]);

            if (data[24] == 0 && started_)
            {
                started_ = false;
#if LOGGING_ON
                SendLoggingMode();
#endif
                UpdateUI();
                prevAltPos_.Clear();
                prevTs_.Clear();
            }

            PrintPosition(pos, ts, sp, dbg, motorAltPos, scopeAltPos);
        }

        void SetMode(byte newMode)
        {
            switch(newMode)
            {
            case A_ALT: radioButtonALT.Checked = true; labelSpeed.Text = "Speed (units/day)"; labelPos.Text = "Position shift (units)"; break;
            case A_AZM: radioButtonAZM.Checked = true; labelSpeed.Text = "Speed (units/day)"; labelPos.Text = "Position shift (units)"; break;
            case M_ALT: radioButtonMALT.Checked = true; labelSpeed.Text = "Speed (rpm)"; labelPos.Text = "Position shift (units)"; break;
            case M_AZM: radioButtonMAZM.Checked = true; labelSpeed.Text = "Speed (rpm)"; labelPos.Text = "Position shift (units)"; break;
#if TEST_SLOW_PWM
            case PWM_ALT:
                radioButtonPWMALT.Checked = true;
                labelSpeed.Text = "Ref PWM (-255 - 255)";
                labelPos.Text = "Period (ms)"; 
                break;
            case PWM_AZM:
                radioButtonPWMAZM.Checked = true;
                labelSpeed.Text = "Ref PWM (-255 - 255)";
                labelPos.Text = "Period (ms)"; 
                break;
#endif
            default: return;
            }
            mode_ = newMode;
#if LOGGING_ON
            SendLoggingMode();
#endif
        }

        void CheckSetMode(byte newMode)
        {
            if (ignoreModeButtonChanged_)
                return;
            if (newMode != mode_)
            {
                ignoreModeButtonChanged_ = true;
                if (started_)
                    SetMode(mode_);
                else
                {
                    mode_ = newMode;
                    switch (newMode)
                    {
                    default:
                    case A_ALT: labelSpeed.Text = "Speed (units/day)"; labelPos.Text = "Position shift (units)"; break;
                    case A_AZM: labelSpeed.Text = "Speed (units/day)"; labelPos.Text = "Position shift (units)"; break;
                    case M_ALT: labelSpeed.Text = "Speed (rpm)"; labelPos.Text = "Position shift (units)"; break;
                    case M_AZM: labelSpeed.Text = "Speed (rpm)"; labelPos.Text = "Position shift (units)"; break;
#if TEST_SLOW_PWM
                    case PWM_ALT:
                        labelSpeed.Text = "Ref PWM (-255 - 255)";
                        labelPos.Text = "Period (ms)";
                        break;
                    case PWM_AZM:
                        labelSpeed.Text = "Ref PWM (-255 - 255)";
                        labelPos.Text = "Period (ms)";
                        break;
#endif
                    }
                }
                ignoreModeButtonChanged_ = false;
            }
        }

        private void TestForm_Load(object sender, EventArgs e)
        {
            portName_ = settings_.PortName;
            baudRate_ = settings_.BaudRate;
            SetMode(M_AZM);

#if LOGGING_ON
            checkBoxLogging.Checked = true;
            comboBoxLoggingType0.Items.Add("M POS");
            comboBoxLoggingType0.Items.Add("M LOG");
            comboBoxLoggingType0.Items.Add("M SPD");
            comboBoxLoggingType0.Items.Add("M ERR");
            comboBoxLoggingType0.Items.Add("A POS");
            comboBoxLoggingType0.Items.Add("A LOG");
            comboBoxLoggingType0.Items.Add("A SPD");
            comboBoxLoggingType0.Items.Add("A ERR");
            comboBoxLoggingType0.SelectedIndex = 0;

            comboBoxLoggingType1.Items.Add("M POS");
            comboBoxLoggingType1.Items.Add("M LOG");
            comboBoxLoggingType1.Items.Add("M SPD");
            comboBoxLoggingType1.Items.Add("M ERR");
            comboBoxLoggingType1.Items.Add("A POS");
            comboBoxLoggingType1.Items.Add("A LOG");
            comboBoxLoggingType1.Items.Add("A SPD");
            comboBoxLoggingType1.Items.Add("A ERR");
            comboBoxLoggingType1.SelectedIndex = 0;
#else
            checkBoxLogging.Visible = false;
            buttonSaveLog.Visible = false;
            comboBoxLoggingType0.Visible = false;
            comboBoxLoggingType1.Visible = false;
#endif

#if !TEST_SLOW_PWM
            radioButtonPWMAZM.Enabled = false;
            radioButtonPWMAZM.Visible = false;
            radioButtonPWMALT.Enabled = false;
            radioButtonPWMALT.Visible = false;
            textBoxPWMDutyCycle.Enabled = false;
            textBoxPWMDutyCycle.Visible = false;
#endif

            init_ = true;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseConnection(connection_);
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (!init_)
                return;

            ConnectionForm form = new ConnectionForm(portName_, baudRate_);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            if (form.DisconnectAll)
            {
                CloseConnection(connection_);
                return;
            }

            CloseConnection(connection_);

            using (settings_.Buffer())
            {
                settings_.PortName = portName_ = form.PortName;
                settings_.BaudRate = baudRate_ = form.BaudRate;
            }

            if (portName_ != null)
            {
                SerialConnection connection = null;
                try
                {
                    connection = new SerialConnection(portName_, baudRate_);
                }
                catch (Exception)
                {
                }

                if (connection != null)
                {
                    connection_ = connection;
                    buttonConnect.Text = "Disconnect";
#if LOGGING_ON
                    SendLoggingMode();
#endif
                    SendGetMotorAndAdapterConfigOptions();
                }
            }
        }

        private void timerPoll_Tick(object sender, EventArgs e)
        {
            if (connection_ != null)
            {
                byte mode;
                if (mode_ == PWM_ALT)
                    mode = M_ALT;
                else if (mode_ == PWM_AZM)
                    mode = M_AZM;
                else
                    mode = mode_;
                SendCommand(connection_, new byte[] { (byte)'P', mode }, 25, ReceivePosition);
            }

            if (started_)
            {
                if (checkBoxSetNextPos.Checked && tmoSendPos_.CheckExpired() && mode_ != PWM_ALT && mode_ != PWM_AZM)
                {
                    Int32 elapsed = (Int32)(DateTime.Now - startDT_).TotalMilliseconds;
                    Int32 ts = startTs_ + elapsed + 10000;
                    double sp = startAltPos_ + speed_ * (elapsed + 10000) / (24.0 * 60.0 * 60000.0);
                    SendSetNextPosCommand((float)sp, ts, mode_, ReceiveSetNextPos);
                }
#if LOGGING_ON
                if (connection_ != null && checkBoxLogging.Checked && tmoAddLogData_.CheckExpired())
                {
                    int logCnt = logNextBlockSize_ + 5;
                    if (logCnt > 14)
                        logCnt = 14;
                    SendCommand(connection_, new byte[] { (byte)'L', (byte)'w', (byte)logCnt, 0 }, 4 + logCnt * 4, AddLogData);
                }
#endif
            }
        }

        private void radioButtonMAZM_CheckedChanged(object sender, EventArgs e)
        {
            if (!init_)
                return;
            if (radioButtonMAZM.Checked)
                CheckSetMode(M_AZM);
        }

        private void radioButtonAZM_CheckedChanged(object sender, EventArgs e)
        {
            if (!init_)
                return;
            if (radioButtonAZM.Checked)
                CheckSetMode(A_AZM);
        }

        private void radioButtonMALT_CheckedChanged(object sender, EventArgs e)
        {
            if (!init_)
                return;
            if (radioButtonMALT.Checked)
                CheckSetMode(M_ALT);
        }

        private void radioButtonALT_CheckedChanged(object sender, EventArgs e)
        {
            if (!init_)
                return;
            if (radioButtonALT.Checked)
                CheckSetMode(A_ALT);
        }

        private void radioButtonPWMAZM_CheckedChanged(object sender, EventArgs e)
        {
            if (!init_)
                return;
            if (radioButtonPWMAZM.Checked)
                CheckSetMode(PWM_AZM);
        }

        private void radioButtonPWMALT_CheckedChanged(object sender, EventArgs e)
        {
            if (!init_)
                return;
            if (radioButtonPWMALT.Checked)
                CheckSetMode(PWM_ALT);
        }

        private void buttonSetSpeed_Click(object sender, EventArgs e)
        {
            if (!init_)
                return;

#if TEST_SLOW_PWM
            if (mode_ == PWM_ALT || mode_ == PWM_AZM)
            {
                Int16 value = 1;
                if (textBoxSpeed.Text.Length > 0)
                {
                    try
                    {
                        double x = Convert.ToDouble(textBoxSpeed.Text);
                        value = Convert.ToInt16(x);
                    }
                    catch
                    {
                    }
                }
                if (value < 1)
                    value = 1;
                else if (value > 255)
                    value = 255;

                Int16 period = 0;
                if (textBoxSetPos.Text.Length > 0)
                {
                    try
                    {
                        double x = Convert.ToDouble(textBoxSetPos.Text);
                        period = Convert.ToInt16(x);
                    }
                    catch
                    {
                    }
                }

                Int16 pwmSpeed = 0;
                if (textBoxPWMSpeed.Text.Length > 0)
                {
                    try
                    {
                        double x = Convert.ToDouble(textBoxPWMSpeed.Text);
                        pwmSpeed = Convert.ToInt16(x);
                    }
                    catch
                    {
                    }
                }
                if (pwmSpeed > value)
                    pwmSpeed = value;
                else if (pwmSpeed < -value)
                    pwmSpeed = (Int16)(-value);

                float dutyCycle = (float)pwmSpeed/(float)value;

                ChangePWMSpeed(value, period, dutyCycle, mode_ == PWM_ALT ? M_ALT : M_AZM);
                return;
            }
#endif
            
            if (textBoxSpeed.Text.Length == 0)
            {
                Stop(mode_);
                return;
            }

            double dValue = 0;
            try
            {
                dValue = Convert.ToDouble(textBoxSpeed.Text);
            }
            catch
            {
            }

            switch (mode_)
            {
            default:
            case A_ALT:
            case A_AZM: ChangeSpeed((Int32)dValue, mode_); break;
            case M_ALT:
            case M_AZM: ChangeSpeed((Int32)(dValue * 60 * 24 * M_RESOLUTION), mode_); break;
            }
        }

        private void buttonSetPos_Click(object sender, EventArgs e)
        {
            if (!init_)
                return;

            if (textBoxSetPos.Text.Length == 0)
                return;

            try
            {
                Int32 shift = (int)Convert.ToDouble(textBoxSetPos.Text);
                ShiftPos(shift, mode_);
            }
            catch
            {
                return;
            }
        }

        private const double MSPEED_SCALE = 4000.0;
        private const double ASPEED_SCALE = 600000.0;
        private void buttonSaveLog_Click(object sender, EventArgs e)
        {
#if LOGGING_ON
            if (started_ || logData_.Count == 0)
                return;

            UInt16 loggingMode = GetLoggingMode();
            string name = "";

            string[] posName = new string[2] {"", ""};
            double[] factor = new double[2] {1.0, 1.0};
            int i = 0;
            for(UInt16 mask = LMODE_FIRST; mask != LMODE_LAST; mask <<= 1)
            {
                if ((loggingMode & mask) != 0)
                {
                    switch(mask)
                    {
                    default:            name += "UNKN_"; posName[i] = ""; break;
                    case LMODE_MPOS:    name += "MPOS_"; posName[i] = "MPOS"; break;
                    case LMODE_MLOG:    name += "MLOG_"; posName[i] = "MLOG"; break;
                    case LMODE_MSPD:    name += "MSPD_"; posName[i] = "MSPD(u/s)"; factor[i] = 1000.0 / MSPEED_SCALE; break;
                    case LMODE_MERR:    name += "MERR_"; posName[i] = "MERR"; break;
                    case LMODE_APOS:    name += "APOS_"; posName[i] = "APOS"; break;
                    case LMODE_ALOG:    name += "ALOG_"; posName[i] = "ALOG"; break;
                    case LMODE_ASPD:    name += "ASPD_"; posName[i] = "ASPD(u/s)"; factor[i] = 1000.0 / ASPEED_SCALE; break;
                    case LMODE_AERR:    name += "AERR_"; posName[i] = "AERR"; break;
                    }
                    if (++i >= 2)
                        break;
                }
            }
            if ((loggingMode & LMODE_AZM) != 0)
                name += "AZM_";
            else
                name += "ALT_";
            string header = "Time(s)," + posName[0] + ",diff0,," + posName[1] + ",diff1";

            SaveFileDialog savefile = new SaveFileDialog();
            DateTime dt = DateTime.Now;
            savefile.FileName = String.Format("{6}{0}-{1}-{2}_{3}-{4}-{5}.csv",
                dt.Year.ToString("D4"), dt.Month.ToString("D2"), dt.Day.ToString("D2"), dt.Hour.ToString("D2"), dt.Minute.ToString("D2"), dt.Second.ToString("D2"), name);
            savefile.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";

            try
            {
                DialogResult res = savefile.ShowDialog();
                if (res != DialogResult.OK)
                    return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            try
            {
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(savefile.FileName))
                {
                    sw.WriteLine(header);
                    double startTs = (double)logData_[0] / 1000.0;
                    double[] prev = new double[2] {0.0, 0.0};
                    for (i = 0; i < logData_.Count; i += 3)
                    {
                        string s = ((double)logData_[i] / 1000.0 - startTs).ToString("F3");
                        double x = logData_[i+1] * factor[0];
                        s += "," + x.ToString();
                        s += "," + (i == 0 ? 0 : x - prev[0]).ToString("F3");
                        prev[0] = x;

                        s += ",";

                        x = logData_[i+2] * factor[1];
                        s += "," + x.ToString();
                        s += "," + (i == 0 ? 0 : x - prev[1]).ToString("F3");
                        prev[1] = x;

                        sw.WriteLine(s);
                    }
                }
                logData_ = new List<int>();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
#endif
        }

        private void checkBoxLogging_CheckedChanged(object sender, EventArgs e)
        {
#if LOGGING_ON
            SendLoggingMode();
#endif
        }

        private void comboBoxLoggingType0_SelectedIndexChanged(object sender, EventArgs e)
        {
#if LOGGING_ON
            SendLoggingMode();
#endif
        }

        private void comboBoxLoggingType1_SelectedIndexChanged(object sender, EventArgs e)
        {
#if LOGGING_ON
            SendLoggingMode();
#endif
        }

    }

    sealed class MainFormSettings
    {
        public MainFormSettings()
        {
            profile_.AddTypes = AddType.Short;
        }

        public string PortName
        {
            get { return (string)profile_.GetValue(section_, "PortName", ""); }
            set { profile_.SetValue(section_, "PortName", value); }
        }
        public int BaudRate
        {
            get { return profile_.GetValue(section_, "BaudRate", 115200); }
            set { profile_.SetValue(section_, "BaudRate", value); }
        }

        public XmlBuffer Buffer()
        {
            return profile_.Buffer();
        }

        private const string section_ = "entries";
        private XmlProfile profile_ = new XmlProfile();
    }
}
