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
    public partial class ObjectFromListForm : Form
    {
        public struct LastSettings
        {
            public int objTypeIdx_, objIdx_;
            public static LastSettings Default { get { return new LastSettings { objTypeIdx_ = 0, objIdx_ = 0 }; } }
        }

        public ObjectFromListForm(ScopeDSCClient parent, bool nightMode, double latitude, double longitude, List<ScopeDSCClient.ObjDatabaseEntry> database, LastSettings settings)
        {
            System.Diagnostics.Debug.Assert(database != null);

            nightMode_ = nightMode;
            parent_ = parent;
            latitude_ = latitude;
            longitude_ = longitude;
            database_ = database;
            settings_ = settings;
            InitializeComponent();
        }

        public SkyObjectPosCalc.SkyPosition Object { get { return object_; } }
        public LastSettings Settings { get { return settings_; } }

        // implementation
        private bool init_ = false;
        private bool nightMode_ = false;
        private ScopeDSCClient parent_;
        private double latitude_, longitude_;
        private List<ScopeDSCClient.ObjDatabaseEntry> database_;
        private LastSettings settings_;
        private SkyObjectPosCalc.SkyPosition object_;

        private void ObjectFromListForm_Load(object sender, EventArgs e)
        {
            if (nightMode_)
                ScopeDSCClient.EnterNightMode(this);

            foreach (var entry in database_)
                comboBoxType.Items.Add(entry.name_);
            
            if (settings_.objTypeIdx_ >= 0 && settings_.objTypeIdx_ < comboBoxType.Items.Count)
                comboBoxType.SelectedIndex = settings_.objTypeIdx_;
            else
                comboBoxType.SelectedIndex = 0;

            FillObjectComboBox();
            if (settings_.objIdx_ >= 0 && settings_.objIdx_ < comboBoxObj.Items.Count)
                comboBoxObj.SelectedIndex = settings_.objIdx_;
            else
                comboBoxObj.SelectedIndex = 0;

            timer1.Enabled = true;

            init_ = true;
            CalcAndOutputResults();
        }

        private SkyObjectPosCalc.SkyPosition[] GetObjects()
        {
            int idx = comboBoxType.SelectedIndex;
            if (idx < 0 || idx >= database_.Count)
                return SkyObjectPosCalc.messier;
            else
                return database_[idx].objects_;
        }

        private SkyObjectPosCalc.SkyPosition GetObject()
        {
            int index = comboBoxObj.SelectedIndex;
            SkyObjectPosCalc.SkyPosition[] objs = GetObjects();
            return objs[index < objs.Length ? index : 0];
        }

        private void CalcAndOutputResults()
        {
            if (!init_)
                return;

            string s = "";

            double d = ScopeDSCClient.CalcTime();
            object_ = GetObject();
            s += object_.Name + ":" + Environment.NewLine;

            double dec, ra;
            object_.CalcTopoRaDec(d, latitude_, longitude_, out dec, out ra);
            s += "R.A.\t= " + ScopeDSCClient.PrintTime(ra) + " (" + ra.ToString("F5") + "\x00B0)" + Environment.NewLine;
            s += "Dec.\t= " + ScopeDSCClient.PrintAngle(dec, true) + " (" + ScopeDSCClient.PrintDec(dec, "F5") + "\x00B0)" + Environment.NewLine;

            double azm, alt;
            object_.CalcAzimuthal(d, latitude_, longitude_, out azm, out alt);
            s += "Azm.\t= " + ScopeDSCClient.PrintAngle(azm) + " (" + azm.ToString("F5") + "\x00B0)" + Environment.NewLine;
            s += "Alt.\t= " + ScopeDSCClient.PrintAngle(alt) + " (" + alt.ToString("F5") + "\x00B0)" + Environment.NewLine;

            buttonOK.Enabled = (alt > 0);

            textBoxResults.Text = s;
        }

        private void FillObjectComboBox()
        {
            comboBoxObj.Items.Clear();
            SkyObjectPosCalc.SkyPosition[] objs = GetObjects();
            foreach (SkyObjectPosCalc.SkyPosition star in objs)
                comboBoxObj.Items.Add(star.Name);
        }

        private void comboBoxType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!init_)
                return;

            FillObjectComboBox();
            comboBoxObj.SelectedIndex = 0;

            settings_.objTypeIdx_ = comboBoxType.SelectedIndex;
            settings_.objIdx_ = comboBoxObj.SelectedIndex;

            CalcAndOutputResults();
        }

        private void comboBoxObj_SelectedIndexChanged(object sender, EventArgs e)
        {
            settings_.objIdx_ = comboBoxObj.SelectedIndex;
            CalcAndOutputResults();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            CalcAndOutputResults();
        }
    }
}
