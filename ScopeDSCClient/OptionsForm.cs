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
        public ClientCommonAPI.LoggingType LoggingType0, LoggingType1;
        public List<int> LogData;
#endif

        public OptionsForm (ClientCommonAPI.IClientHost host,
                            List<ClientCommonAPI.PhysicalLocation> locations,
                            bool showNearestAzmRotation,
                            bool connectToStellarium,
                            int stellariumTcpPort,
                            bool oppositeHorzPositioningDir,
                            ClientCommonAPI.AutoTrack autoTrack,
                            ClientCommonAPI.LoggingState lstate,
                            ClientCommonAPI.LoggingChannel lchannel,
                            ClientCommonAPI.LoggingType ltype0,
                            ClientCommonAPI.LoggingType ltype1,
                            List<int> logData)
        {
            locations_ = locations;
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
            LoggingType0 = ltype0;
            LoggingType1 = ltype1;
            LogData = logData;
#endif
            InitializeComponent();
        }

        private bool init_ = false;
        private List<ClientCommonAPI.PhysicalLocation> locations_;
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
            for (int i = 0; i < locations_.Count; ++i)
            {
                //comboBoxLocation.Items.Add(locations_[i].name_ + " (" + locations_[i].latitude_ + "," + locations_[i].longitude_ + ")");
                comboBoxLocation.Items.Add(locations_[i].name_ + " (" + ClientCommonAPI.PrintAngle(locations_[i].latitude_) + ", " + ClientCommonAPI.PrintAngle(locations_[i].longitude_) + ")");
                if (idx == 0 && Latitude == locations_[i].latitude_ && Longitude == locations_[i].longitude_)
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
            comboBoxLoggingType0.Items.Add("M POS");
            comboBoxLoggingType0.Items.Add("M LOG");
            comboBoxLoggingType0.Items.Add("M SPD");
            comboBoxLoggingType0.Items.Add("M ERR");
            comboBoxLoggingType0.Items.Add("A POS");
            comboBoxLoggingType0.Items.Add("A LOG");
            comboBoxLoggingType0.Items.Add("A SPD");
            comboBoxLoggingType0.Items.Add("A ERR");

            comboBoxLoggingType1.Items.Add("M POS");
            comboBoxLoggingType1.Items.Add("M LOG");
            comboBoxLoggingType1.Items.Add("M SPD");
            comboBoxLoggingType1.Items.Add("M ERR");
            comboBoxLoggingType1.Items.Add("A POS");
            comboBoxLoggingType1.Items.Add("A LOG");
            comboBoxLoggingType1.Items.Add("A SPD");
            comboBoxLoggingType1.Items.Add("A ERR");
            switch (LoggingState)
            {
                case ClientCommonAPI.LoggingState.OFF:
                    checkBoxLogging.Checked = false;
                    checkBoxLoggingAZM.Checked = LoggingChannel == ClientCommonAPI.LoggingChannel.AZM;
                    comboBoxLoggingType0.SelectedIndex = (int)LoggingType0;
                    comboBoxLoggingType1.SelectedIndex = (int)LoggingType1;
                    break;
                case ClientCommonAPI.LoggingState.ON:
                    checkBoxLogging.Checked = true;
                    checkBoxLoggingAZM.Checked = LoggingChannel == ClientCommonAPI.LoggingChannel.AZM;
                    comboBoxLoggingType0.SelectedIndex = (int)LoggingType0;
                    comboBoxLoggingType1.SelectedIndex = (int)LoggingType1;
                    break;
                default:
                    checkBoxLogging.Visible = false;
                    buttonSaveLog.Visible = false;
                    checkBoxLoggingAZM.Visible = false;
                    comboBoxLoggingType0.Visible = false;
                    comboBoxLoggingType1.Visible = false;
                    break;
            }
#else
            checkBoxLogging.Visible = false;
            buttonSaveLog.Visible = false;
            checkBoxLoggingAZM.Visible = false;
            comboBoxLoggingType0.Visible = false;
            comboBoxLoggingType1.Visible = false;
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
        private const double ASPEED_SCALE = 600000.0;
        private void buttonSaveLog_Click(object sender, EventArgs e)
        {
#if LOGGING_ON
            if (LogData == null || LogData.Count == 0)
                return;

            string name = "";

            ClientCommonAPI.LoggingType[] lt = (LoggingType0 < LoggingType1) ?
                new ClientCommonAPI.LoggingType[2] { LoggingType0, LoggingType1 } :
                new ClientCommonAPI.LoggingType[2] { LoggingType1, LoggingType0 };
            string[] posName = new string[2] {"", ""};
            double[] factor = new double[2] {1.0, 1.0};
            for (int i = 0; i < 2; ++i)
            {
                switch (lt[i])
                {
                default: name += "UNKN_"; posName[i] = ""; break;
                case ClientCommonAPI.LoggingType.M_POS: name += "MPOS_"; posName[i] = "MPOS"; break;
                case ClientCommonAPI.LoggingType.M_LOG: name += "MLOG_"; posName[i] = "MLOG"; break;
                case ClientCommonAPI.LoggingType.M_SPD: name += "MSPD_"; posName[i] = "MSPD(u/s)"; factor[i] = 1000.0 / MSPEED_SCALE; break;
                case ClientCommonAPI.LoggingType.M_ERR: name += "MERR_"; posName[i] = "MERR"; break;
                case ClientCommonAPI.LoggingType.A_POS: name += "APOS_"; posName[i] = "APOS"; break;
                case ClientCommonAPI.LoggingType.A_LOG: name += "ALOG_"; posName[i] = "ALOG"; break;
                case ClientCommonAPI.LoggingType.A_SPD: name += "ASPD_"; posName[i] = "ASPD(u/s)"; factor[i] = 1000.0 / ASPEED_SCALE; break;
                case ClientCommonAPI.LoggingType.A_ERR: name += "AERR_"; posName[i] = "AERR"; break;
                }
            }
            if (LoggingChannel == ClientCommonAPI.LoggingChannel.AZM)
                name += "Client_AZM_";
            else
                name += "Client_ALT_";
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
                    double startTs = (double)LogData[0] / 1000.0;
                    double[] prev = new double[2] {0.0, 0.0};
                    for (int i = 0; i < LogData.Count; i += 3)
                    {
                        string s = ((double)LogData[i] / 1000.0 - startTs).ToString("F3");
                        double x = LogData[i + 1] * factor[0];
                        s += "," + x.ToString();
                        s += "," + (i == 0 ? 0 : x - prev[0]).ToString("F3");
                        prev[0] = x;

                        s += ",";

                        x = LogData[i + 2] * factor[1];
                        s += "," + x.ToString();
                        s += "," + (i == 0 ? 0 : x - prev[1]).ToString("F3");
                        prev[1] = x;

                        sw.WriteLine(s);
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

        private void comboBoxLoggingType0_SelectedIndexChanged(object sender, EventArgs e)
        {
#if LOGGING_ON
            switch (comboBoxLoggingType0.SelectedIndex)
            {
            case 0: LoggingType0 = ClientCommonAPI.LoggingType.M_POS; break;
            case 1: LoggingType0 = ClientCommonAPI.LoggingType.M_LOG; break;
            case 2: LoggingType0 = ClientCommonAPI.LoggingType.M_SPD; break;
            case 3: LoggingType0 = ClientCommonAPI.LoggingType.M_ERR; break;
            case 4: LoggingType0 = ClientCommonAPI.LoggingType.A_POS; break;
            case 5: LoggingType0 = ClientCommonAPI.LoggingType.A_LOG; break;
            case 6: LoggingType0 = ClientCommonAPI.LoggingType.A_SPD; break;
            case 7: LoggingType0 = ClientCommonAPI.LoggingType.A_ERR; break;
            default: break;
            }
#endif
        }

        private void comboBoxLoggingType1_SelectedIndexChanged(object sender, EventArgs e)
        {
#if LOGGING_ON
            switch (comboBoxLoggingType1.SelectedIndex)
            {
            case 0: LoggingType1 = ClientCommonAPI.LoggingType.M_POS; break;
            case 1: LoggingType1 = ClientCommonAPI.LoggingType.M_LOG; break;
            case 2: LoggingType1 = ClientCommonAPI.LoggingType.M_SPD; break;
            case 3: LoggingType1 = ClientCommonAPI.LoggingType.M_ERR; break;
            case 4: LoggingType1 = ClientCommonAPI.LoggingType.A_POS; break;
            case 5: LoggingType1 = ClientCommonAPI.LoggingType.A_LOG; break;
            case 6: LoggingType1 = ClientCommonAPI.LoggingType.A_SPD; break;
            case 7: LoggingType1 = ClientCommonAPI.LoggingType.A_ERR; break;
            default: break;
            }
#endif
        }
    }
}
