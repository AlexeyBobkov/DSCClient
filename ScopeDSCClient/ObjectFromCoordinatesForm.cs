using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SkyObjectPosition;

namespace ScopeDSCClient
{
    public partial class ObjectFromCoordinatesForm : Form
    {
        public struct LastSettings
        {
            public double ra_hour_, dec_;
            public Epoch epoch_;
            public static LastSettings Default { get { return new LastSettings { ra_hour_ = 0, dec_ = 0, epoch_ = Epoch.J2000 }; } }
        }

        public ObjectFromCoordinatesForm(bool nightMode, double latitude, double longitude, LastSettings settings)
        {
            nightMode_ = nightMode;
            latitude_ = latitude;
            longitude_ = longitude;
            settings_ = settings;
            InitializeComponent();
        }

        public SkyObjectPosCalc.SkyPosition Object { get { return object_; } }
        public LastSettings Settings { get { return settings_; } }

        // implementation

        private enum RADecFormat
        {
            HMS,
            DMS,
            Degree
        }
        public enum Epoch
        {
            J2000,
            Today
        }
        
        private bool init_ = false;
        private bool nightMode_ = false;
        private double latitude_, longitude_;
        private int textBoxRADegWidth_, textBoxDecDegWidth_;
        private bool ignoreRAChange_ = false;
        private bool ignoreDecChange_ = false;
        private double ra_hour_ = 0, dec_ = 0;
        private RADecFormat raFormat_ = RADecFormat.HMS;
        private RADecFormat decFormat_ = RADecFormat.DMS;
        private Epoch epoch_;
        private LastSettings settings_;
        private SkyObjectPosCalc.SkyPosition object_;

        private void ObjectFromCoordinatesForm_Load(object sender, EventArgs e)
        {
            Location = new Point(0, 0);

            if (nightMode_)
                ClientCommonAPI.EnterNightMode(this);

            ra_hour_ = settings_.ra_hour_;
            dec_ = settings_.dec_;
            epoch_ = settings_.epoch_;
                
            textBoxRADegWidth_ = textBoxRADeg.Width;
            radioButtonRAHMS.Checked = true;
            SetRAFormat(RADecFormat.HMS, true);

            textBoxDecDegWidth_ = textBoxDecDeg.Width;
            radioButtonDecDMS.Checked = true;
            SetDecFormat(RADecFormat.DMS, true);

            checkBoxJ2000.Checked = (epoch_ == Epoch.J2000);

            timer1.Enabled = true;

            init_ = true;
            ObjectChanged();
        }

        private SkyObjectPosCalc.SkyPosition GetObject() { return object_; }

        private void CalcAndOutputResults()
        {
            if (!init_)
                return;

            string s = "";

            double d = ClientCommonAPI.CalcTime();
            if (object_ == null)
            {
                buttonOK.Enabled = false;
                s += "No Object Selected";
            }
            else
            {
                s += object_.Name + ":" + Environment.NewLine;

                double dec, ra;
                object_.CalcTopoRaDec(d, latitude_, longitude_, out dec, out ra);
                s += "R.A.\t= " + ClientCommonAPI.PrintTime(ra) + " (" + ra.ToString("F5") + "\x00B0)" + Environment.NewLine;
                s += "Dec.\t= " + ClientCommonAPI.PrintAngle(dec, true) + " (" + ClientCommonAPI.PrintDec(dec, "F5") + "\x00B0)" + Environment.NewLine;

                double azm, alt;
                object_.CalcAzimuthal(d, latitude_, longitude_, out azm, out alt);
                s += "Azm.\t= " + ClientCommonAPI.PrintAngle(azm) + " (" + azm.ToString("F5") + "\x00B0)" + Environment.NewLine;
                s += "Alt.\t= " + ClientCommonAPI.PrintAngle(alt) + " (" + alt.ToString("F5") + "\x00B0)" + Environment.NewLine;

                buttonOK.Enabled = (alt > 0);
            }

            textBoxResults.Text = s;
        }

        private void ObjectChanged()
        {
            settings_.ra_hour_ = ra_hour_;
            settings_.dec_ = dec_;
            settings_.epoch_ = epoch_;

            object_ = new SkyObjectPosCalc.StarPosition("Unknown", ra_hour_, dec_, epoch_ == Epoch.J2000);
            CalcAndOutputResults();
        }

        private void CalcRA()
        {
            try
            {
                switch (raFormat_)
                {
                    case RADecFormat.HMS:
                    case RADecFormat.DMS:
                        {
                            double min = Convert.ToDouble(textBoxRAMin.Text);
                            if (min < 0)
                                min = 0;
                            else if (min > 59)
                                min = 59;

                            double sec = Convert.ToDouble(textBoxRASec.Text);
                            if (sec < 0)
                                sec = 0;
                            else if (sec >= 60)
                                sec = 59.9;

                            double ra;
                            if (ClientCommonAPI.ParseSignedValue(textBoxRADeg.Text, out ra))
                                ra += (min + sec/60.0)/60.0;
                            else
                                ra -= (min + sec / 60.0) / 60.0;
                            ra_hour_ = SkyObjectPosCalc.Rev((raFormat_ == RADecFormat.HMS) ? ra * 15.0 : ra) / 15.0;
                        }
                        break;
                        
                    case RADecFormat.Degree:
                        ra_hour_ = SkyObjectPosCalc.Rev(Convert.ToDouble(textBoxRADeg.Text)) / 15.0;
                        break;
                }
            }
            catch (System.FormatException)
            {
                ra_hour_ = 0;
            }
            ObjectChanged();
        }

        private void CalcDec()
        {
            try
            {
                switch (decFormat_)
                {
                    case RADecFormat.HMS:
                    case RADecFormat.DMS:
                        {
                            double min = Convert.ToDouble(textBoxDecMin.Text);
                            if (min < 0)
                                min = 0;
                            else if (min > 59)
                                min = 59;

                            double sec = Convert.ToDouble(textBoxDecSec.Text);
                            if (sec < 0)
                                sec = 0;
                            else if (sec >= 60)
                                sec = 59.9;

                            double dec;
                            if (ClientCommonAPI.ParseSignedValue(textBoxDecDeg.Text, out dec))
                                dec += (min + sec / 60.0) / 60.0;
                            else
                                dec -= (min + sec / 60.0) / 60.0;
                            dec_ = dec;
                        }
                        break;

                    case RADecFormat.Degree:
                        dec_ = Convert.ToDouble(textBoxDecDeg.Text);
                        break;
                }
                if (dec_ < -90)
                    dec_ = -90;
                else if (dec_ > 90)
                    dec_ = 90;
            }
            catch (System.FormatException)
            {
                dec_ = 0;
            }
            ObjectChanged();
        }

        private void SetRA_HMS_DMS(double ra_hour)
        {
            bool pos = (ra_hour >= 0);
            ra_hour = Math.Abs(ra_hour);

            double a_deg = Math.Floor(ra_hour);
            double min = (ra_hour - a_deg) * 60, a_min = Math.Floor(min);
            double sec = (min - a_min) * 60;
            textBoxRADeg.Text = (pos ? "" : "-") + a_deg.ToString("F0");
            textBoxRAMin.Text = a_min.ToString("F0");
            textBoxRASec.Text = sec.ToString("F0");
        }

        private void SetRAFormat(RADecFormat raFmt, bool force)
        {
            if (raFmt != raFormat_ || force)
            {
                ignoreRAChange_ = true;
                switch (raFmt)
                {
                    case RADecFormat.HMS:
                    case RADecFormat.DMS:
                        textBoxRADeg.Width = textBoxRADegWidth_;
                        textBoxRAMin.Visible = true;
                        textBoxRASec.Visible = true;
                        SetRA_HMS_DMS(raFmt == RADecFormat.HMS ? ra_hour_ : ra_hour_ * 15);
                        break;
                    case RADecFormat.Degree:
                        textBoxRADeg.Width = textBoxRASec.Right - textBoxRADeg.Left;
                        textBoxRAMin.Visible = false;
                        textBoxRASec.Visible = false;
                        textBoxRADeg.Text = (ra_hour_ * 15).ToString("F5");
                        break;
                }
                raFormat_ = raFmt;
                ignoreRAChange_ = false;
            }
        }

        private void SetDec_DMS(double dec)
        {
            bool pos = (dec >= 0);
            dec = Math.Abs(dec);

            double a_deg = Math.Floor(dec);
            double min = (dec - a_deg) * 60, a_min = Math.Floor(min);
            double sec = (min - a_min) * 60;
            textBoxDecDeg.Text = (pos ? "" : "-") + a_deg.ToString("F0");
            textBoxDecMin.Text = a_min.ToString("F0");
            textBoxDecSec.Text = sec.ToString("F0");
        }

        private void SetDecFormat(RADecFormat decFmt, bool force)
        {
            if (decFmt != decFormat_ || force)
            {
                ignoreDecChange_ = true;
                switch (decFmt)
                {
                    case RADecFormat.HMS:
                    case RADecFormat.DMS:
                        textBoxDecDeg.Width = textBoxDecDegWidth_;
                        textBoxDecMin.Visible = true;
                        textBoxDecSec.Visible = true;
                        SetDec_DMS(dec_);
                        break;
                    case RADecFormat.Degree:
                        textBoxDecDeg.Width = textBoxDecSec.Right - textBoxDecDeg.Left;
                        textBoxDecMin.Visible = false;
                        textBoxDecSec.Visible = false;
                        textBoxDecDeg.Text = dec_.ToString("F5");
                        break;
                }
                decFormat_ = decFmt;
                ignoreDecChange_ = false;
            }
        }

        private void textBoxRADeg_TextChanged(object sender, EventArgs e)
        {
            if (!init_ || ignoreRAChange_)
                return;
            CalcRA();
        }

        private void textBoxRAMin_TextChanged(object sender, EventArgs e)
        {
            if (!init_ || ignoreRAChange_)
                return;
            CalcRA();
        }

        private void textBoxRASec_TextChanged(object sender, EventArgs e)
        {
            if (!init_ || ignoreRAChange_)
                return;
            CalcRA();
        }

        private void radioButtonRAHMS_CheckedChanged(object sender, EventArgs e)
        {
            if (!init_)
                return;
            SetRAFormat(RADecFormat.HMS, false);
        }

        private void radioButtonRADMS_CheckedChanged(object sender, EventArgs e)
        {
            if (!init_)
                return;
            SetRAFormat(RADecFormat.DMS, false);
        }

        private void radioButtonRADegree_CheckedChanged(object sender, EventArgs e)
        {
            if (!init_)
                return;
            SetRAFormat(RADecFormat.Degree, false);
        }

        private void textBoxDecDeg_TextChanged(object sender, EventArgs e)
        {
            if (!init_ || ignoreDecChange_)
                return;
            CalcDec();
        }

        private void textBoxDecMin_TextChanged(object sender, EventArgs e)
        {
            if (!init_ || ignoreDecChange_)
                return;
            CalcDec();
        }

        private void textBoxDecSec_TextChanged(object sender, EventArgs e)
        {
            if (!init_ || ignoreDecChange_)
                return;
            CalcDec();
        }

        private void radioButtonDecDMS_CheckedChanged(object sender, EventArgs e)
        {
            if (!init_)
                return;
            SetDecFormat(RADecFormat.DMS, false);
        }

        private void radioButtonDecDegree_CheckedChanged(object sender, EventArgs e)
        {
            if (!init_)
                return;
            SetDecFormat(RADecFormat.Degree, false);
        }

        private void checkBoxJ2000_CheckedChanged(object sender, EventArgs e)
        {
            if (!init_)
                return;
            epoch_ = checkBoxJ2000.Checked ? Epoch.J2000 : Epoch.Today;
            ObjectChanged();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            CalcAndOutputResults();
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
        private void ObjectFromCoordinatesForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (keyboard_ != null)
                keyboard_.Close();
        }
    }
}
