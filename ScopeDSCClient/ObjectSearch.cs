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
    public partial class ObjectSearch : Form
    {
        public ObjectSearch(bool nightMode, double latitude, double longitude, List<ClientCommonAPI.ObjDatabaseEntry> database, ObjectFromListForm.LastSettings settings)
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
        public ObjectFromListForm.LastSettings Settings { get { return settings_; } }

        // implementation
        private struct ListEntry
        {
            public int objTypeIdx_, objIdx_;
            public ListEntry(int objTypeIdx, int objIdx)
            {
                objTypeIdx_ = objTypeIdx;
                objIdx_ = objIdx;
            }
        }
        
        private bool init_ = false;
        private bool nightMode_ = false;
        private double latitude_, longitude_;
        private List<ClientCommonAPI.ObjDatabaseEntry> database_;
        private ObjectFromListForm.LastSettings settings_;
        private List<ListEntry> matchingObjList_ = new List<ListEntry>();
        private SkyObjectPosCalc.SkyPosition object_;
        private UInt16 timerCnt_;
        private int timerStart_ = -1;

        private void ObjectSearch_Load(object sender, EventArgs e)
        {
            Location = new Point(0, 0);

            if (nightMode_)
                ClientCommonAPI.EnterNightMode(this);

            if (settings_.objTypeIdx_ >= 0 && settings_.objTypeIdx_ < database_.Count)
            {
                ClientCommonAPI.ObjDatabaseEntry database = database_[settings_.objTypeIdx_];
                if (settings_.objIdx_ >= 0 && settings_.objIdx_ < database.objects_.Length)
                    object_ = database.objects_[settings_.objIdx_];
            }

            if (object_ != null)
            {
                textBoxSearch.Text = object_.Name;
                CorrectMatchingList();
            }

            timer1.Enabled = true;

            if (keyboard_ == null)
            {
                keyboard_ = new KeyboardClassLibrary.TouchscreenKeyboardForm(nightMode_);
                keyboard_.Show();
                keyboard_.FormClosed += keyboard_FormClosed;
            }

            init_ = true;
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

        private void ObjectSearch_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (keyboard_ != null)
                keyboard_.Close();
        }

        private void textBoxSearch_TextChanged(object sender, EventArgs e)
        {
            if (!init_)
                return;
            timerStart_ = timerCnt_;
        }

        private void listBoxMatch_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!init_)
                return;

            int i = listBoxMatch.SelectedIndex;
            if (i >= 0 && i < matchingObjList_.Count)
            {
                ListEntry entry = matchingObjList_[i];
                object_ = database_[entry.objTypeIdx_].objects_[entry.objIdx_];
                settings_.objTypeIdx_ = entry.objTypeIdx_;
                settings_.objIdx_ = entry.objIdx_;
            }
            CalcAndOutputResults();
        }

        private void CorrectMatchingList()
        {
            if (textBoxSearch.Text.Length <= 0)
                matchingObjList_.Clear();
            else
                matchingObjList_ = BuildMatchingList(textBoxSearch.Text);
            listBoxMatch.Items.Clear();
            for (int i = 0; i < matchingObjList_.Count; ++i)
            {
                ListEntry entry = matchingObjList_[i];
                SkyObjectPosCalc.SkyPosition obj = database_[entry.objTypeIdx_].objects_[entry.objIdx_];
                listBoxMatch.Items.Add(obj.NameInfo);
                if (object_ == obj)
                    listBoxMatch.SelectedIndex = i;
            }
        }

        public static string RemoveWhitespace(string input)
        {
            return new string(input.ToCharArray()
                .Where(c => !Char.IsWhiteSpace(c))
                .ToArray());
        }

        private List<ListEntry> BuildMatchingList(string text)
        {
            text = RemoveWhitespace(text);
            List<ListEntry> list0 = new List<ListEntry>();
            List<ListEntry> list1 = new List<ListEntry>();
            List<ListEntry> list2 = new List<ListEntry>();
            List<ListEntry> list3 = new List<ListEntry>();
            for (int i = 0; i < database_.Count; ++i)
            {
                var entry = database_[i]; 
                for (int j = 0; j < entry.objects_.Length; ++j)
                {
                    var obj = entry.objects_[j];
                    string name = obj.Name;
                    if (text == name)
                    {
                        list0.Add(new ListEntry(i, j));
                        continue;
                    }
                    switch (RemoveWhitespace(name).IndexOf(text, StringComparison.CurrentCultureIgnoreCase))
                    {
                    case -1:    break;
                    case 0:     list1.Add(new ListEntry(i, j)); continue;
                    default:    list2.Add(new ListEntry(i, j)); continue;
                    }
                    if (RemoveWhitespace(obj.Info).IndexOf(text, StringComparison.CurrentCultureIgnoreCase) >= 0)
                        list3.Add(new ListEntry(i, j));
                }
            }
            list0.AddRange(list1);
            list0.AddRange(list2);
            list0.AddRange(list3);
            return list0;
        }

        private void CalcAndOutputResults()
        {
            if (!init_)
                return;

            string s = "";

            if (object_ == null)
                buttonOK.Enabled = false;
            else
            {
                double d = ClientCommonAPI.CalcTime();
                s += object_.Name + ":" + Environment.NewLine;
                if (object_.Info != null && object_.Info.Length > 0)
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
            }

            textBoxResults.Text = s;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            ++timerCnt_;
            if ((timerCnt_ & 0x07) == 0)
                CalcAndOutputResults();

            if (timerStart_ < 0)
                timerCnt_ = (UInt16)(timerCnt_ & 0x07);
            else if (timerCnt_ - timerStart_ > 8)
            {
                CorrectMatchingList();
                timerStart_ = -1;
                timerCnt_ = (UInt16)(timerCnt_ & 0x07);
            }
        }
    }
}
