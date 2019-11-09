#define LOGGING_ON

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

#if LOGGING_ON
        private void SendLoggingMode()
        {
            if (connection_ != null)
            {
                byte mode;
                if (!started_ || !checkBoxLogging.Checked)
                    mode = 0;
                else
                    switch (mode_)
                    {
                    case M_ALT:
                    case A_ALT: mode = M_ALT; break;
                    case M_AZM:
                    case A_AZM: mode = M_AZM; break;
                    default:    mode = 0; break;
                    }

                SendCommand(connection_, new byte[] { (byte)'L', (byte)'m', mode }, 1, ReceiveDummy);
            }
        }
#endif

        private void SendSetNextPosCommand(Int32 sp, Int32 ts, byte dst, ReceiveDelegate receiveDelegate)
        {
            if (connection_ != null)
                SendCommand(connection_, new byte[] { (byte)'N',
                                                      dst,
                                                      (byte)sp,
                                                      (byte)(sp >> 8),
                                                      (byte)(sp >> 16),
                                                      (byte)(sp >> 24),
                                                      (byte)ts,
                                                      (byte)(ts >> 8),
                                                      (byte)(ts >> 16),
                                                      (byte)(ts >> 24)}, 8, receiveDelegate);
        }

        Int32 nextTs_;
        Int32 nextSp_;
        private void ShiftPos(Int32 value, byte dst)
        {
            if (started_)
            {
                if (checkBoxSetNextPos.Checked)
                {
                    startAltPos_ += value;
                    timerSendNextPosTicks_ = 0;
                }
                else
                {
                    Int32 elapsed = (Int32)(DateTime.Now - startDT_).TotalMilliseconds;
                    Int32 ts = startTs_ + elapsed + 30000;
                    Int32 sp = (Int32)Math.Round(startAltPos_ + (double)speed_ * (elapsed + 30000) / (24.0 * 60.0 * 60000.0)) + value;
                    nextTs_ = ts;
                    nextSp_ = sp;
                    SendSetNextPosCommand(sp, ts, dst, ReceiveShiftNextPos);
                }
            }
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
            SendLoggingMode();
            PrintPosition(altPos, ts, 0, 0, 0, 0);
        }

        private void ReceiveShiftNextPos(byte[] data)
        {
            Int32 altPos = (Int32)((((UInt32)data[3]) << 24) + (((UInt32)data[2]) << 16) + (((UInt32)data[1]) << 8) + data[0]);
            Int32 ts = (int)((((UInt32)data[7]) << 24) + (((UInt32)data[6]) << 16) + (((UInt32)data[5]) << 8) + data[4]);

            startTs_ = ts;
            startDT_ = DateTime.Now;
            startAltPos_ = altPos;
            speed_ = (int)((double)(nextSp_ - altPos) * 60000.0 * 60.0 * 24.0 / (double)(nextTs_ - ts));
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
            started_ = false;
            SendLoggingMode();
            UpdateUI();
            prevAltPos_.Clear();
            prevTs_.Clear();
        }

        private bool started_ = false;
        private Int32 speed_ = 0;
        private Int32 startAltPos_;
        private Int32 startTs_;
        private DateTime startDT_;
        private List<Int32> prevAltPos_ = new List<int>();
        private List<Int32> prevTs_ = new List<int>();
        private const int MAX_POSITIONS = 50;
        private int timerSendNextPosTicks_ = 0;
        private Timeout tmoSendPos_ = new Timeout(8000);

#if LOGGING_ON
        UInt32 logStart_ = 0;
        Int32 logAbsPos_ = 0, logAbsTs_ = 0;
        int logNextBlockSize_ = 0;
        List<Int32> logData_ = new List<int>();
        private Timeout tmoAddLogData_ = new Timeout(1000);

        private void AddLogData(byte[] data)
        {
            int reported = (int)data[1];
            int stillInBuffer = (int)data[2];
            if (data[3] != 0)   // ring buffer was overflowed
            {
                // force restart and re-synchronization
                logStart_ = 0;
                logAbsPos_ = logAbsTs_ = 0;
            }

            logNextBlockSize_ = stillInBuffer;

            for (int i = 0; i < reported; ++i)
            {
                int start = i * 4 + 4;
                if ((data[start + 3] & 0x80) != 0)
                {
                    // re-sync!
                    logStart_ = (((UInt32)data[start + 3]) << 24) + (((UInt32)data[start + 2]) << 16) + (((UInt32)data[start + 1]) << 8) + (UInt32)data[start];
                    logAbsPos_ = logAbsTs_ = 0;
                    continue;
                }
                if (logStart_ == 0)
                    continue;       // skip it: waiting for re-sync
                if (logAbsPos_ == 0)
                {

                    logAbsPos_ = (((Int32)(byte)logStart_) << 24) + (((Int32)data[start + 2]) << 16) + (((Int32)data[start + 1]) << 8) + (Int32)data[start];
                    continue;
                }

                Int32 pos, ts;
                if (logAbsTs_ == 0)
                {
                    pos = logAbsPos_;
                    ts = logAbsTs_ = (((Int32)(byte)(logStart_ >> 8)) << 24) + (((Int32)data[start + 2]) << 16) + (((Int32)data[start + 1]) << 8) + (Int32)data[start];
                }
                else
                {
                    pos = logAbsPos_ + (((Int32)data[start + 1]) << 8) + (Int32)data[start];
                    ts = logAbsTs_ + (((Int32)data[start + 3]) << 8) + (Int32)data[start + 2];
                }
                logData_.Add(ts);
                logData_.Add(pos);
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
                    sp = startAltPos_ + (double)speed_ * (ts - startTs_) / (24.0 * 60.0 * 60000.0);
                    textBoxOutput.Text += "SP  = " + sp.ToString() + Environment.NewLine;
                }

                textBoxOutput.Text += Environment.NewLine;
                textBoxOutput.Text += "Speed_ = " + speed_.ToString() + Environment.NewLine;

                if (prevAltPos_.Count > 0)
                {
                    int delta = (int)(pos - prevAltPos_[0]);
                    int dt = ts - prevTs_[0];
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
                SendLoggingMode();
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
            case A_ALT: radioButtonALT.Checked = true; labelSpeed.Text = "Speed (units/day)"; break;
            case A_AZM: radioButtonAZM.Checked = true; labelSpeed.Text = "Speed (units/day)"; break;
            case M_ALT: radioButtonMALT.Checked = true; labelSpeed.Text = "Speed (rpm)"; break;
            case M_AZM: radioButtonMAZM.Checked = true; labelSpeed.Text = "Speed (rpm)"; break;
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
                    case A_ALT: case A_AZM: labelSpeed.Text = "Speed (units/day)"; break;
                    case M_ALT: case M_AZM: labelSpeed.Text = "Speed (rpm)"; break;
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

            //checkBoxSetNextPos.Checked = true;
#if LOGGING_ON
            checkBoxLogging.Checked = true;
#else
            checkBoxLogging.Visible = false;
            buttonSaveLog.Visible = false;
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
                }
            }
        }

        private void timerPoll_Tick(object sender, EventArgs e)
        {
            if (connection_ != null)
                SendCommand(connection_, new byte[] { (byte)'P', mode_ }, 25, ReceivePosition);

            if (started_)
            {
                if (checkBoxSetNextPos.Checked && tmoSendPos_.CheckExpired())
                {
                    Int32 elapsed = (Int32)(DateTime.Now - startDT_).TotalMilliseconds;
                    Int32 ts = startTs_ + elapsed + 10000;
                    Int32 sp = (Int32)Math.Round(startAltPos_ + (double)speed_ * (elapsed + 10000) / (24.0 * 60.0 * 60000.0));
                    SendSetNextPosCommand(sp, ts, mode_, ReceiveSetNextPos);
                }
#if LOGGING_ON
                if (connection_ != null && checkBoxLogging.Checked && tmoAddLogData_.CheckExpired())
                {
                    int logCnt = logNextBlockSize_ + 5;
                    if (logCnt > 14)
                        logCnt = 14;
                    SendCommand(connection_, new byte[] { (byte)'L', (byte)'w', (byte)logCnt }, 4 + logCnt * 4, AddLogData);
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

        private void buttonSetSpeed_Click(object sender, EventArgs e)
        {
            if (!init_)
                return;

            if (textBoxSpeed.Text.Length == 0)
            {
                Stop(mode_);
                return;
            }

            double dSpeed = 0;
            try
            {
                dSpeed = Convert.ToDouble(textBoxSpeed.Text);
            }
            catch
            {
            }

            switch (mode_)
            {
                default:
                case A_ALT:
                case A_AZM: ChangeSpeed((Int32)dSpeed, mode_); break;
                case M_ALT:
                case M_AZM: ChangeSpeed((Int32)(dSpeed * 60 * 24 * M_RESOLUTION), mode_); break;
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

        private void buttonSaveLog_Click(object sender, EventArgs e)
        {
#if LOGGING_ON
            if (started_ || logData_.Count == 0)
                return;

            SaveFileDialog savefile = new SaveFileDialog();

            DateTime dt = DateTime.Now;
            savefile.FileName = String.Format("LoggingData{0}-{1}-{2}_{3}-{4}-{5}.csv",
                dt.Year.ToString("D4"), dt.Month.ToString("D2"), dt.Day.ToString("D2"), dt.Hour.ToString("D2"), dt.Minute.ToString("D2"), dt.Second.ToString("D2"));
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
                    sw.WriteLine("Time(s),Position,Angle,Diff");
                    double startTs = (double)logData_[0]/1000.0;
                    double prevAngle = 0;
                    for (int i = 0; i < logData_.Count; i += 2)
                    {
                        int pos = logData_[i + 1];
                        double angle = (double)pos * 360.0 * 60.0 / ((double)M_RESOLUTION * 28.0 * 20.0);

                        string s = ((double)logData_[i] / 1000.0 - startTs).ToString("F3");
                        s += "," + pos.ToString();
                        s += "," + angle.ToString("F3");
                        s += "," + (i == 0 ? 0 : angle - prevAngle).ToString("F3");
                        sw.WriteLine(s);

                        prevAngle = angle;
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
