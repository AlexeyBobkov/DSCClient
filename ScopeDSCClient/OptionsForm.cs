using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ScopeDSCClient
{
    public partial class OptionsForm : Form
    {
        private struct LocationData
        {
            public string name_;
            public double latitude_, longitude_;
            public LocationData(string name, double latitude, double longitude)
            {
                name_ = name;
                latitude_ = latitude;
                longitude_ = longitude;
            }
        }

        private static LocationData[] locations_ = new LocationData[]
        {
            new LocationData("San Jose Houge Park", 37.257471, -121.942246),
            new LocationData("Fremont Peak SP CA", 36.760441, -121.504472),
            new LocationData("Adin CA, Frosty Acres", 41.130625, -120.976339)
        };

        private enum LocFormat
        {
            DMS,
            Degree
        }

        private bool init_ = false;
        private bool nightMode_ = false;
        private LocFormat locFmt_ = LocFormat.DMS;
        private int textBoxLatDegWidth_, textBoxLonDegWidth_;
        private bool ignoreLocChange_ = false;

        public double Latitude = 37.28203, Longitude = -121.85925;
        public bool ShowNearestAzmRotation = false;
        public bool ConnectToStellarium = false;
        public int TcpPort = 0;
        public bool OppositeHorzPositioningDir = false;

        public OptionsForm(bool nightMode, bool showNearestAzmRotation, bool connectToStellarium, int tcpPort, bool oppositeHorzPositioningDir)
        {
            nightMode_ = nightMode;
            ShowNearestAzmRotation = showNearestAzmRotation;
            ConnectToStellarium = connectToStellarium;
            TcpPort = tcpPort;
            OppositeHorzPositioningDir = oppositeHorzPositioningDir;
            InitializeComponent();
        }

        private void OptionsForm_Load(object sender, EventArgs e)
        {
            Location = new Point(0, 0); 

            if (nightMode_)
                ScopeDSCClient.EnterNightMode(this);

            textBoxLatDegWidth_ = textBoxLatDeg.Width;
            textBoxLonDegWidth_ = textBoxLonDeg.Width;

            comboBoxLocation.Items.Add("Enter Coordinates");
            int idx = 0;
            for (int i = 0; i < locations_.Length; ++i)
            {
                //comboBoxLocation.Items.Add(locations_[i].name_ + " (" + locations_[i].latitude_ + "," + locations_[i].longitude_ + ")");
                comboBoxLocation.Items.Add(locations_[i].name_ + " (" + ScopeDSCClient.PrintAngle(locations_[i].latitude_) + ", " + ScopeDSCClient.PrintAngle(locations_[i].longitude_) + ")");
                if (Latitude == locations_[i].latitude_ && Longitude == locations_[i].longitude_)
                    idx = i + 1;
            }
            comboBoxLocation.SelectedIndex = idx;
            EnableLocationCtrls(idx == 0);

            radioButtonDMS.Checked = (locFmt_ == LocFormat.DMS);
            SetLocationText();

            checkBoxShowNearestAzmRotation.Checked = ShowNearestAzmRotation;
            checkBoxConnectToStellarium.Checked = ConnectToStellarium;
            labelStellariumTcpPort.Enabled = ConnectToStellarium;
            textBoxStellariumTCPPort.Enabled = ConnectToStellarium;
            textBoxStellariumTCPPort.Text = TcpPort.ToString();
            checkBoxOppHorzPositionDirection.Checked = OppositeHorzPositioningDir;

            init_ = true;
        }

        private void SetLocationDMS(TextBox tbD, TextBox tbM, TextBox tbS, double val)
        {
            bool pos = (val >= 0);
            val = Math.Abs(val);

            double a_deg = Math.Floor(val);
            double min = (val - a_deg) * 60, a_min = Math.Floor(min);
            double sec = (min - a_min) * 60;
            tbD.Text = (pos ? "" : "-") + a_deg.ToString("F0");
            tbM.Text = a_min.ToString("F0");
            tbS.Text = sec.ToString("F1");
        }

        private void EnableLocationCtrls(bool enable)
        {
            textBoxLatDeg.Enabled =
            textBoxLatMin.Enabled =
            textBoxLatSec.Enabled =
            textBoxLonDeg.Enabled =
            textBoxLonMin.Enabled =
            textBoxLonSec.Enabled =
            radioButtonDegree.Enabled =
            radioButtonDMS.Enabled =
            labelLat.Enabled =
            labelLon.Enabled =
            groupBoxLocUnits.Enabled = enable;
        }

        private void SetLocationText()
        {
            ignoreLocChange_ = true;
            switch (locFmt_)
            {
                case LocFormat.DMS:
                    textBoxLatDeg.Width = textBoxLatDegWidth_;
                    textBoxLatMin.Visible = true;
                    textBoxLatSec.Visible = true;
                    SetLocationDMS(textBoxLatDeg, textBoxLatMin, textBoxLatSec, Latitude);

                    textBoxLonDeg.Width = textBoxLonDegWidth_;
                    textBoxLonMin.Visible = true;
                    textBoxLonSec.Visible = true;
                    SetLocationDMS(textBoxLonDeg, textBoxLonMin, textBoxLonSec, Longitude);

                    break;
                case LocFormat.Degree:
                default:
                    textBoxLatDeg.Width = textBoxLatSec.Right - textBoxLatDeg.Left;
                    textBoxLatMin.Visible = false;
                    textBoxLatSec.Visible = false;
                    textBoxLatDeg.Text = Latitude.ToString("F6");

                    textBoxLonDeg.Width = textBoxLonSec.Right - textBoxLonDeg.Left;
                    textBoxLonMin.Visible = false;
                    textBoxLonSec.Visible = false;
                    textBoxLonDeg.Text = Longitude.ToString("F6");
                    break;
            }
            ignoreLocChange_ = false;
        }

        private double CalcLocation(TextBox tbD, TextBox tbM, TextBox tbS, bool dms)
        {
            try
            {
                if(dms)
                {
                    double min = Convert.ToDouble(tbM.Text);
                    if (min < 0)
                        min = 0;
                    else if (min > 59)
                        min = 59;

                    double sec = Convert.ToDouble(tbS.Text);
                    if (sec < 0)
                        sec = 0;
                    else if (sec >= 60)
                        sec = 59.9;

                    double deg;
                    if (ScopeDSCClient.ParseSignedValue(tbD.Text, out deg))
                        deg += (min + sec / 60.0) / 60.0;
                    else
                        deg -= (min + sec / 60.0) / 60.0;
                    return deg;
                }
                else
                {
                    return Convert.ToDouble(tbD.Text);
                }
            }
            catch (System.FormatException)
            {
                return 0;
            }
        }

        private void comboBoxLocation_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!init_)
                return;

            int idx = comboBoxLocation.SelectedIndex;
            EnableLocationCtrls(idx == 0);
            if (idx > 0)
            {
                Latitude = locations_[idx - 1].latitude_;
                Longitude = locations_[idx - 1].longitude_;
                SetLocationText();
            }
        }

        private void textBoxLatDeg_TextChanged(object sender, EventArgs e)
        {
            if (!init_ || ignoreLocChange_)
                return;
            Latitude = CalcLocation(textBoxLatDeg, textBoxLatMin, textBoxLatSec, locFmt_ == LocFormat.DMS);
        }

        private void textBoxLatMin_TextChanged(object sender, EventArgs e)
        {
            if (!init_ || ignoreLocChange_)
                return;
            Latitude = CalcLocation(textBoxLatDeg, textBoxLatMin, textBoxLatSec, locFmt_ == LocFormat.DMS);
        }

        private void textBoxLatSec_TextChanged(object sender, EventArgs e)
        {
            if (!init_ || ignoreLocChange_)
                return;
            Latitude = CalcLocation(textBoxLatDeg, textBoxLatMin, textBoxLatSec, locFmt_ == LocFormat.DMS);
        }

        private void textBoxLonDeg_TextChanged(object sender, EventArgs e)
        {
            if (!init_ || ignoreLocChange_)
                return;
            Longitude = CalcLocation(textBoxLonDeg, textBoxLonMin, textBoxLonSec, locFmt_ == LocFormat.DMS);
        }

        private void textBoxLonMin_TextChanged(object sender, EventArgs e)
        {
            if (!init_ || ignoreLocChange_)
                return;
            Longitude = CalcLocation(textBoxLonDeg, textBoxLonMin, textBoxLonSec, locFmt_ == LocFormat.DMS);
        }

        private void textBoxLonSec_TextChanged(object sender, EventArgs e)
        {
            if (!init_ || ignoreLocChange_)
                return;
            Longitude = CalcLocation(textBoxLonDeg, textBoxLonMin, textBoxLonSec, locFmt_ == LocFormat.DMS);
        }

        private void radioButtonDMS_CheckedChanged(object sender, EventArgs e)
        {
            if (!init_)
                return;
            locFmt_ = LocFormat.DMS;
            SetLocationText();
        }

        private void radioButtonDegree_CheckedChanged(object sender, EventArgs e)
        {
            if (!init_)
                return;
            locFmt_ = LocFormat.Degree;
            SetLocationText();
        }

        // Screen Keyboard
        private Form keyboard_;
        private void keyboard_FormClosed(Object sender, FormClosedEventArgs e) { keyboard_ = null; }
        private void buttonScreenKbd_Click(object sender, EventArgs e)
        {
            if (keyboard_ == null)
            {
                keyboard_ = new KeyboardClassLibrary.TouchscreenKeyboardForm(nightMode_);
                keyboard_.Show();
                keyboard_.FormClosed += keyboard_FormClosed;
            }
        }
        private void OptionsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (keyboard_ != null)
                keyboard_.Close();
        }

        private void checkBoxShowNearestAzmRotation_CheckedChanged(object sender, EventArgs e)
        {
            if (!init_)
                return;
            ShowNearestAzmRotation = checkBoxShowNearestAzmRotation.Checked;
        }

        private void checkBoxConnectToStellarium_CheckedChanged(object sender, EventArgs e)
        {
            ConnectToStellarium = checkBoxConnectToStellarium.Checked;
            labelStellariumTcpPort.Enabled = ConnectToStellarium;
            textBoxStellariumTCPPort.Enabled = ConnectToStellarium;
        }

        private void textBoxStellariumTCPPort_TextChanged(object sender, EventArgs e)
        {
            try
            {
                TcpPort = Convert.ToInt32(textBoxStellariumTCPPort.Text);
            }
            catch
            {
                TcpPort = 0;
            }
        }

        private void checkBoxOppHorzPositionDirection_CheckedChanged(object sender, EventArgs e)
        {
            if (!init_)
                return;
            OppositeHorzPositioningDir = checkBoxOppHorzPositionDirection.Checked;
        }
    }
}
