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

        public ObjectFromListForm(bool nightMode, double latitude, double longitude, List<ClientCommonAPI.ObjDatabaseEntry> database, LastSettings settings)
        {
            System.Diagnostics.Debug.Assert(database != null);

            nightMode_ = nightMode;
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
        private double latitude_, longitude_;
        private List<ClientCommonAPI.ObjDatabaseEntry> database_;
        private LastSettings settings_;
        private SkyObjectPosCalc.SkyPosition object_;

        private void ObjectFromListForm_Load(object sender, EventArgs e)
        {
            if (nightMode_)
                ClientCommonAPI.EnterNightMode(this);

            foreach (var entry in database_)
                comboBoxType.Items.Add(entry.name_);
            
            if (settings_.objTypeIdx_ >= 0 && settings_.objTypeIdx_ < comboBoxType.Items.Count)
                comboBoxType.SelectedIndex = settings_.objTypeIdx_;
            else
                comboBoxType.SelectedIndex = 0;

            FillObjectListBox();
            if (settings_.objIdx_ >= 0 && settings_.objIdx_ < listBoxObj.Items.Count)
                listBoxObj.SelectedIndex = settings_.objIdx_;
            else
                listBoxObj.SelectedIndex = 0;

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
            int index = listBoxObj.SelectedIndex;
            SkyObjectPosCalc.SkyPosition[] objs = GetObjects();
            return objs[index < objs.Length ? index : 0];
        }

        private void CalcAndOutputResults()
        {
            if (!init_)
                return;

            string s = "";

            double d = ClientCommonAPI.CalcTime();
            object_ = GetObject();
            s += object_.Name + ":" + Environment.NewLine;

            if(object_.Info != null && object_.Info.Length > 0)
                s += object_.Info + Environment.NewLine;
            s += Environment.NewLine;

            double dec, ra;
            object_.CalcTopoRaDec(d, latitude_, longitude_, out dec, out ra);
            s += "R.A.\t= " + ClientCommonAPI.PrintTime(ra) + " (" + ra.ToString("F5") + "\x00B0)" + Environment.NewLine;
            s += "Dec.\t= " + ClientCommonAPI.PrintAngle(dec, true) + " (" + ClientCommonAPI.PrintDec(dec, "F5") + "\x00B0)" + Environment.NewLine;

            double azm, alt;
            object_.CalcAzimuthal(d, latitude_, longitude_, out azm, out alt);
            s += "Azm.\t= " + ClientCommonAPI.PrintAngle(azm) + " (" + azm.ToString("F5") + "\x00B0)" + Environment.NewLine;
            s += "Alt.\t= " + ClientCommonAPI.PrintAngle(alt) + " (" + alt.ToString("F5") + "\x00B0)" + Environment.NewLine;

            buttonOK.Enabled = (alt > 0);

            textBoxResults.Text = s;
        }

        private void FillObjectListBox()
        {
            listBoxObj.Items.Clear();
            SkyObjectPosCalc.SkyPosition[] objs = GetObjects();
            foreach (SkyObjectPosCalc.SkyPosition star in objs)
                listBoxObj.Items.Add(star.NameInfo);
        }

        private void comboBoxType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!init_)
                return;

            FillObjectListBox();
            listBoxObj.SelectedIndex = 0;

            settings_.objTypeIdx_ = comboBoxType.SelectedIndex;
            settings_.objIdx_ = listBoxObj.SelectedIndex;

            CalcAndOutputResults();
        }

        private void listBoxObj_SelectedIndexChanged(object sender, EventArgs e)
        {
            settings_.objIdx_ = listBoxObj.SelectedIndex;
            CalcAndOutputResults();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            CalcAndOutputResults();
        }
    }
}
