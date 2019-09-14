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
    public partial class ObjectLastSelectionForm : Form
    {
        public ObjectLastSelectionForm(bool nightMode, double latitude, double longitude, SkyObjectPosCalc.SkyPosition[] lastObjects)
        {
            nightMode_ = nightMode;
            latitude_ = latitude;
            longitude_ = longitude;
            lastObjects_ = lastObjects;
            InitializeComponent();
        }

        public SkyObjectPosCalc.SkyPosition Object { get { return object_; } }

        // implementation

        private bool init_ = false;
        private bool nightMode_ = false;
        private double latitude_, longitude_;
        SkyObjectPosCalc.SkyPosition[] lastObjects_;
        private SkyObjectPosCalc.SkyPosition object_;

        private void ObjectLastSelectionForm_Load(object sender, EventArgs e)
        {
            if (nightMode_)
                ClientCommonAPI.EnterNightMode(this);

            if (lastObjects_.Length == 0)
                buttonOK.Enabled = false;
            else
            {
                foreach (var obj in lastObjects_)
                    if (obj != null)
                        listBoxLastObjs.Items.Add(obj.Name);

                if (listBoxLastObjs.Items.Count <= 0)
                    buttonOK.Enabled = false;
                else
                {
                    listBoxLastObjs.SelectedIndex = 0;
                    object_ = lastObjects_[0];
                    buttonOK.Enabled = true;
                }
            }

            timer1.Enabled = true;

            init_ = true;
            ObjectChanged();
        }

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
            CalcAndOutputResults();
        }

        private void listBoxLastObjs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!init_ || listBoxLastObjs.SelectedIndex >= lastObjects_.Length)
                return;
            object_ = lastObjects_[listBoxLastObjs.SelectedIndex];
            ObjectChanged();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            CalcAndOutputResults();
        }
    }
}
