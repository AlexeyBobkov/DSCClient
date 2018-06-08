using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SerialPortSupport;

namespace ScopeDSCConfigureHW
{
    public partial class ScopeDSCConfigureHW : Form
    {
        private string portName_;
        private int baudRate_ = 115200;
        private SerialConnection connection_;

        private delegate void TimeoutDelegate();
        private delegate void ReceiveDelegate(byte[] data);

        // connection capabilities flags
        private const byte CONNCAPS_ALTAZM = 1;
        private const byte CONNCAPS_EQU = 2;
        private const byte CONNCAPS_GPS = 4;
        private byte capabilities_ = 0;
        private double dMotorLastStartAngle_ = 0;
        private double dMotorLastEndAngle_ = 0;

        public ScopeDSCConfigureHW()
        {
            InitializeComponent();
        }

        private void ScopeDSCConfigureHW_Load(object sender, EventArgs e)
        {
            UpdateUI();
        }

        private void ScopeDSCConfigureHW_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseConnection();
        }

        private void UpdateUI()
        {
            string s = "";
            if (connection_ == null)
            {
                s += "No connection" + Environment.NewLine;
            }
            else
            {
                s += "Connected to Board on " + connection_.PortName + Environment.NewLine;
                if (capabilities_ != 0)
                {
                    s += "Capabilities:";
                    if ((capabilities_ & CONNCAPS_ALTAZM) != 0)
                        s += " Alt-Azm";

                    if ((capabilities_ & CONNCAPS_EQU) != 0)
                        s += " Equ";

                    if ((capabilities_ & CONNCAPS_GPS) != 0)
                        s += " GPS";

                    s += Environment.NewLine;
                }

                s += "Last Start Angle " + dMotorLastStartAngle_.ToString("F2") + Environment.NewLine;
                s += "Last End Angle " + dMotorLastEndAngle_.ToString("F2") + Environment.NewLine;
            }

            textBoxConnection.Text = s;
        }

        private void SerialError()
        {
            CloseConnection();
            UpdateUI();
        }

        private void CloseConnection()
        {
            if (connection_ != null)
            {
                connection_.Close();
                connection_.Dispose();
                connection_ = null;
            }
        }

        private class BaseConnectionHandler : SerialConnection.IReceiveHandler
        {
            public BaseConnectionHandler(ScopeDSCConfigureHW parent, ReceiveDelegate receiveDelegate)
            {
                parent_ = parent;
                receiveDelegate_ = receiveDelegate;
            }

            public void Error()
            {
                TimeoutDelegate d = new TimeoutDelegate(parent_.SerialError);
                parent_.BeginInvoke(d);
            }

            public void Received(byte[] data)
            {
                parent_.BeginInvoke(receiveDelegate_, new object[] { data });
            }

            private ScopeDSCConfigureHW parent_;
            private ReceiveDelegate receiveDelegate_;
        }

        private void SendCommand(char cmd, int receiveCnt, ReceiveDelegate receiveDelegate)
        {
            if (connection_ != null)
                connection_.SendReceiveRequest(new byte[] { (byte)cmd }, receiveCnt, new BaseConnectionHandler(this, receiveDelegate));
        }

        private void SendCommand(byte[] cmd, int receiveCnt, ReceiveDelegate receiveDelegate)
        {
            if (connection_ != null)
                connection_.SendReceiveRequest(cmd, receiveCnt, new BaseConnectionHandler(this, receiveDelegate));
        }

        private void ReceiveCapabilities(byte[] data)
        {
            capabilities_ = data[0];
            UpdateUI();
        }

        private void ReceiveMotorLastAngles(byte[] data)
        {
            dMotorLastStartAngle_ = (double)((((System.Int32)data[3]) << 24) + (((System.Int32)data[2]) << 16) + (((System.Int32)data[1]) << 8) + (System.Int32)data[0]) / 100.0;
            dMotorLastEndAngle_ = (double)((((System.Int32)data[7]) << 24) + (((System.Int32)data[6]) << 16) + (((System.Int32)data[5]) << 8) + (System.Int32)data[4]) / 100.0;
            UpdateUI();
        }

        private void ReceiveDummy(byte[] data)
        {
        }

        private void buttonConnection_Click(object sender, EventArgs e)
        {
            ConnectionForm form = new ConnectionForm(portName_, baudRate_);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            portName_ = form.PortName;
            baudRate_ = form.BaudRate;

            CloseConnection();
            if (portName_ != null)
            {
                try
                {
                    connection_ = new SerialConnection(portName_, baudRate_);
                }
                catch (Exception)
                {
                }

                if (connection_ != null)
                {
                    SendCommand('c', 1, this.ReceiveCapabilities);
                    SendCommand('t', 8, this.ReceiveMotorLastAngles);
                }
            }
            UpdateUI();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            // set capabilities
            byte new_capabilities = 0;
            if (checkBoxAltAzm.Checked)
                new_capabilities |= CONNCAPS_ALTAZM;
            if (checkBoxEqu.Checked)
                new_capabilities |= CONNCAPS_EQU;
            if (checkBoxGPS.Checked)
                new_capabilities |= CONNCAPS_GPS;

            SendCommand(new byte[] { (byte)'A', (byte)'2', (byte)('0' + new_capabilities) }, 1, this.ReceiveDummy);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            SendCommand('c', 1, this.ReceiveCapabilities);
            SendCommand('t', 8, this.ReceiveMotorLastAngles);
        }
    }
}
