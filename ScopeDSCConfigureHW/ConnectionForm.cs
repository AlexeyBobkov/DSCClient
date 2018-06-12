using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Text.RegularExpressions;

namespace ScopeDSCConfigureHW
{
    public partial class ConnectionForm : Form
    {
        private bool init_ = false;
        private string portName_;
        private int baudRate_ = 9600;

        public ConnectionForm(string portName, int baudRate)
        {
            portName_ = portName;
            baudRate_ = baudRate;
            InitializeComponent();
        }

        public string PortName { get { return portName_; } }
        public int BaudRate { get { return baudRate_; } }

        private void ConnectionForm_Load(object sender, EventArgs e)
        {

            int i;

            string[] ports = AAB.UtilityLibrary.SerialConnection.MakeCorrectPortNames(SerialPort.GetPortNames());
            comboBoxSerialPort.Items.Add("Select Port");
            comboBoxSerialPort.Items.AddRange(ports);
            if (portName_ != null)
            {
                i = comboBoxSerialPort.Items.IndexOf(portName_);
                comboBoxSerialPort.SelectedIndex = i >= 0 ? i : 0;
            }
            else
                comboBoxSerialPort.SelectedIndex = 0;

            comboBoxBaudRate.Items.AddRange(new object[] { 300, 600, 1200, 2400, 4800, 9600, 14400, 19200, 28800, 38400, 57600, 115200 });
            i = comboBoxBaudRate.Items.IndexOf(baudRate_);
            if (i < 0)
                i = comboBoxBaudRate.Items.IndexOf(9600);
            comboBoxBaudRate.SelectedIndex = i >= 0 ? i : 0;

            init_ = true;
        }

        private void comboBoxSerialPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (init_)
                portName_ = comboBoxSerialPort.SelectedIndex > 0 ? (string)comboBoxSerialPort.SelectedItem : null;
        }

        private void comboBoxBaudRate_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (init_)
                baudRate_ = (int)comboBoxBaudRate.SelectedItem;
        }
    }
}
