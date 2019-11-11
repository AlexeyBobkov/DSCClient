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

        public double Latitude = 37.28203, Longitude = -121.85925;
        public bool ShowNearestAzmRotation = false;
        public bool ConnectToStellarium = false;
        public int StellariumTcpPort = 0;
        public bool OppositeHorzPositioningDir = false;
        public ClientCommonAPI.AutoTrack AutoTrack;
#if LOGGING_ON
        public ClientCommonAPI.LoggingState LoggingState;
        public ClientCommonAPI.LoggingChannel LoggingChannel;
        public ClientCommonAPI.LoggingType LoggingType;
        public List<int> LogData;
#endif

        public OptionsForm (ClientCommonAPI.IClientHost host,
                            bool showNearestAzmRotation,
                            bool connectToStellarium,
                            int stellariumTcpPort,
                            bool oppositeHorzPositioningDir,
                            ClientCommonAPI.AutoTrack autoTrack,
                            ClientCommonAPI.LoggingState lstate,
                            ClientCommonAPI.LoggingChannel lchannel,
                            ClientCommonAPI.LoggingType ltype,
                            List<int> logData)
        {
            nightMode_ = host.NightMode;
            Latitude = host.Latitude;
            Longitude = host.Longitude;
            ShowNearestAzmRotation = showNearestAzmRotation;
            ConnectToStellarium = connectToStellarium;
            StellariumTcpPort = stellariumTcpPort;
            OppositeHorzPositioningDir = oppositeHorzPositioningDir;
            AutoTrack = autoTrack;
#if LOGGING_ON
            LoggingState = lstate;
            LoggingChannel = lchannel;
            LoggingType = ltype;
            LogData = logData;
#endif
            InitializeComponent();
        }

        private bool init_ = false;
        private bool nightMode_ = false;
        private LocFormat locFmt_ = LocFormat.DMS;
        private int textBoxLatDegWidth_, textBoxLonDegWidth_;
        private bool ignoreLocChange_ = false;

        private void OptionsForm_Load(object sender, EventArgs e)
        {
            Location = new Point(0, 0); 

            if (nightMode_)
                ClientCommonAPI.EnterNightMode(this);

            textBoxLatDegWidth_ = textBoxLatDeg.Width;
            textBoxLonDegWidth_ = textBoxLonDeg.Width;

            comboBoxLocation.Items.Add("Enter Coordinates");
            int idx = 0;
            for (int i = 0; i < locations_.Length; ++i)
            {
                //comboBoxLocation.Items.Add(locations_[i].name_ + " (" + locations_[i].latitude_ + "," + locations_[i].longitude_ + ")");
                comboBoxLocation.Items.Add(locations_[i].name_ + " (" + ClientCommonAPI.PrintAngle(locations_[i].latitude_) + ", " + ClientCommonAPI.PrintAngle(locations_[i].longitude_) + ")");
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
            textBoxStellariumTCPPort.Text = StellariumTcpPort.ToString();
            checkBoxOppHorzPositionDirection.Checked = OppositeHorzPositioningDir;

            switch (AutoTrack)
            {
                default:
                case ClientCommonAPI.AutoTrack.DISABLED:
                    checkBoxAutoTrack.Visible = checkBoxAutoTrack.Enabled = false;
                    break;

                case ClientCommonAPI.AutoTrack.ON:
                    checkBoxAutoTrack.Checked = true;
                    break;

                case ClientCommonAPI.AutoTrack.OFF:
                    checkBoxAutoTrack.Checked = false;
                    break;
            }

#if LOGGING_ON
            comboBoxLoggingType.Items.Add("M Pos");
            comboBoxLoggingType.Items.Add("M Spd");
            comboBoxLoggingType.Items.Add("A Spd");
            switch (LoggingState)
            {
                case ClientCommonAPI.LoggingState.OFF:
                    checkBoxLogging.Checked = false;
                    checkBoxLoggingAZM.Checked = LoggingChannel == ClientCommonAPI.LoggingChannel.AZM;
                    comboBoxLoggingType.SelectedIndex = (int)LoggingType;
                    break;
                case ClientCommonAPI.LoggingState.ON:
                    checkBoxLogging.Checked = true;
                    checkBoxLoggingAZM.Checked = LoggingChannel == ClientCommonAPI.LoggingChannel.AZM;
                    comboBoxLoggingType.SelectedIndex = (int)LoggingType;
                    break;
                default:
                    checkBoxLogging.Visible = false;
                    buttonSaveLog.Visible = false;
                    checkBoxLoggingAZM.Visible = false;
                    comboBoxLoggingType.Visible = false;
                    break;
            }
#else
            checkBoxLogging.Visible = false;
            buttonSaveLog.Visible = false;
            checkBoxLoggingAZM.Visible = false;
            comboBoxLoggingType.Visible = false;
#endif
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
                    if (ClientCommonAPI.ParseSignedValue(tbD.Text, out deg))
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
                StellariumTcpPort = Convert.ToInt32(textBoxStellariumTCPPort.Text);
            }
            catch
            {
                StellariumTcpPort = 0;
            }
        }

        private void checkBoxOppHorzPositionDirection_CheckedChanged(object sender, EventArgs e)
        {
            if (!init_)
                return;
            OppositeHorzPositioningDir = checkBoxOppHorzPositionDirection.Checked;
        }

        private void checkBoxAutoTrack_CheckedChanged(object sender, EventArgs e)
        {
            if (!init_)
                return;
            AutoTrack = checkBoxAutoTrack.Checked ? ClientCommonAPI.AutoTrack.ON : ClientCommonAPI.AutoTrack.OFF;
        }

        private void checkBoxLogging_CheckedChanged(object sender, EventArgs e)
        {
#if LOGGING_ON
            LoggingState = checkBoxLogging.Checked ? ClientCommonAPI.LoggingState.ON : ClientCommonAPI.LoggingState.OFF;
#endif
        }

        private const double MSPEED_SCALE = 4000.0;
        private void buttonSaveLog_Click(object sender, EventArgs e)
        {
#if LOGGING_ON
            if (LogData == null || LogData.Count == 0)
                return;

            SaveFileDialog savefile = new SaveFileDialog();

            DateTime dt = DateTime.Now;
            string name;
            switch (LoggingType)
            {
            default:
            case ClientCommonAPI.LoggingType.M_POS: name = "ClientLogMotorPos"; break;
            case ClientCommonAPI.LoggingType.M_SPD: name = "ClientLogMotorSpeed"; break;
            case ClientCommonAPI.LoggingType.A_SPD: name = "ClientLogAdapterSpeed"; break;
            }
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
                    if (LoggingType == ClientCommonAPI.LoggingType.M_POS)
                    {
                        sw.WriteLine("Time(s),Position,Angle,Diff");
                        double startTs = (double)LogData[0] / 1000.0;
                        double prevAngle = 0;
                        for (int i = 0; i < LogData.Count; i += 2)
                        {
                            int pos = LogData[i + 1];
                            double angle = (double)pos * 360.0 * 60.0 / ((double)10000 * 22.0 * 20.0);  // some hardcoded approximate values

                            string s = ((double)LogData[i] / 1000.0 - startTs).ToString("F3");
                            s += "," + pos.ToString();
                            s += "," + angle.ToString("F3");
                            s += "," + (i == 0 ? 0 : angle - prevAngle).ToString("F3");
                            sw.WriteLine(s);

                            prevAngle = angle;
                        }
                    }
                    else
                    {
                        sw.WriteLine("Time(s),Speed(u/sec),Diff");
                        double startTs = (double)LogData[0] / 1000.0;
                        double prevSpeed = 0;
                        for (int i = 0; i < LogData.Count; i += 2)
                        {
                            int pos = LogData[i + 1];
                            double speed = (double)pos * 1000.0 / MSPEED_SCALE;

                            string s = ((double)LogData[i] / 1000.0 - startTs).ToString("F3");
                            s += "," + speed.ToString("F3");
                            s += "," + (i == 0 ? 0 : speed - prevSpeed).ToString("F3");
                            sw.WriteLine(s);
                            prevSpeed = speed;
                        }
                    }
                }
                LogData = new List<int>();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
#endif
        }

        private void checkBoxLoggingAZM_CheckedChanged(object sender, EventArgs e)
        {
#if LOGGING_ON
            LoggingChannel = checkBoxLoggingAZM.Checked ? ClientCommonAPI.LoggingChannel.AZM : ClientCommonAPI.LoggingChannel.ALT;
#endif
        }

        private void comboBoxLoggingType_SelectedIndexChanged(object sender, EventArgs e)
        {
#if LOGGING_ON
            switch (comboBoxLoggingType.SelectedIndex)
            {
            case 0: LoggingType = ClientCommonAPI.LoggingType.M_POS; break;
            case 1: LoggingType = ClientCommonAPI.LoggingType.M_SPD; break;
            case 2: LoggingType = ClientCommonAPI.LoggingType.A_SPD; break;
            default: break;
#endif
            }
        }
    }
}
