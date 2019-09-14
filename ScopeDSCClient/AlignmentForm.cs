using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SkyObjectPosition;
using DSCCalculations;
using AAB.UtilityLibrary;

namespace ScopeDSCClient
{
    public partial class AlignmentForm : Form
    {
        public struct LastSettings
        {
            public int objTypeIdx_, objIdx_;
            public static LastSettings Default { get { return new LastSettings { objTypeIdx_ = 0, objIdx_ = 0 }; } }
        }

        public AlignmentForm(ClientCommonAPI.IScopePositions angles, bool nightMode, double latitude, double longitude, LastSettings settings, DSCAlignment alignment)
        {
            nightMode_ = nightMode;
            angles_ = angles;
            latitude_ = latitude;
            longitude_ = longitude;
            settings_ = settings;
            alignment_ = (alignment != null) ? (DSCAlignment)alignment.Clone() : new DSCAlignment(new Vect3(0, latitude_ * Const.toRad), Precisions.Default);
            InitializeComponent();
        }

        public DSCAlignment Alignment
        {
            get { return alignment_.IsAligned ? alignment_ : null; }
        }
        public LastSettings Settings { get { return settings_; } }

        // implementation
        private bool init_ = false;
        private bool nightMode_ = false;
        private ClientCommonAPI.IScopePositions angles_;
        private double latitude_, longitude_;
        private LastSettings settings_;
        private DSCAlignment alignment_;

        private void AlignmentForm_Load(object sender, EventArgs e)
        {
            if (nightMode_)
                ClientCommonAPI.EnterNightMode(this);

            comboBoxType.Items.AddRange(new object[]
                {
                    "Solar System Object",
                    "Star"
                });
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

            buttonOK.Enabled = false;
            buttonSaveAlignment.Enabled = alignment_.IsAligned;

            init_ = true;
            CalcAndOutputResults();
        }

        private SkyObjectPosCalc.SkyPosition GetObject()
        {
            int index = comboBoxObj.SelectedIndex;

            SkyObjectPosCalc.SkyPosition[] objs;
            if (comboBoxType.SelectedIndex == 0)
            {
                // Sun System Objects
                objs = SkyObjectPosCalc.sunSystemObjects;
            }
            else
            {
                // stars
                objs = SkyObjectPosCalc.stars;
            }

            return objs[index < objs.Length ? index : 0];
        }

        private void CalcAndOutputResults()
        {
            if (!init_)
                return;

            string s = "";

            double d = ClientCommonAPI.CalcTime();
            SkyObjectPosCalc.SkyPosition obj = GetObject();
            s += obj.Name + ":" + Environment.NewLine;

            double dec, ra;
            obj.CalcTopoRaDec(d, latitude_, longitude_, out dec, out ra);
            s += "R.A.\t= " + ClientCommonAPI.PrintTime(ra) + " (" + ra.ToString("F5") + "\x00B0)" + Environment.NewLine;
            s += "Dec.\t= " + ClientCommonAPI.PrintAngle(dec, true) + " (" + ClientCommonAPI.PrintDec(dec, "F5") + "\x00B0)" + Environment.NewLine;
            
            double azm, alt;
            obj.CalcAzimuthal(d, latitude_, longitude_, out azm, out alt);
            s += "Azm.\t= " + ClientCommonAPI.PrintAngle(azm) + " (" + azm.ToString("F5") + "\x00B0)" + Environment.NewLine;
            s += "Alt.\t= " + ClientCommonAPI.PrintAngle(alt) + " (" + alt.ToString("F5") + "\x00B0)" + Environment.NewLine;
            s += Environment.NewLine;

            buttonAddObject.Enabled = (alt > 0);
            buttonCorrectPolarAxis.Enabled = ClientCommonAPI.IsEquAxisCorrectionNeeded(latitude_, alignment_);

            if (alignment_.IsAligned)
                s += "Alignment valid";
            else
                s += "Alignment not valid";
            s += Environment.NewLine;
            s += alignment_.ToString(true);

            if (ClientCommonAPI.IsEquAxisCorrectionNeeded(latitude_, alignment_))
                s += Environment.NewLine + ClientCommonAPI.AddEquAxisCorrectionText(latitude_, alignment_);

            textBoxResults.Text = s;
        }

        private void AlignmentChanged()
        {
            buttonOK.Enabled = true;
            buttonSaveAlignment.Enabled = alignment_.IsAligned;
            CalcAndOutputResults();
        }

        private void FillObjectComboBox()
        {
            comboBoxObj.Items.Clear();
            SkyObjectPosCalc.SkyPosition[] objs;
            switch (comboBoxType.SelectedIndex)
            {
                case 0:
                    // Sun System Objects
                    objs = SkyObjectPosCalc.sunSystemObjects;
                    break;
                case 1:
                default:
                    // stars
                    objs = SkyObjectPosCalc.stars;
                    break;
            }

            foreach (SkyObjectPosCalc.SkyPosition star in objs)
                comboBoxObj.Items.Add(star.Name);
        }

        private string MakeProfileName()
        {
            DateTime dt = DateTime.Now;
            return String.Format("Alignment{0}-{1}-{2}_{3}-{4}-{5}.xml",
                dt.Year.ToString("D4"), dt.Month.ToString("D2"), dt.Day.ToString("D2"), dt.Hour.ToString("D2"), dt.Minute.ToString("D2"), dt.Second.ToString("D2"));
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

        private void buttonAddObject_Click(object sender, EventArgs e)
        {
            if (!init_)
                return;

            double d = ClientCommonAPI.CalcTime();
            SkyObjectPosCalc.SkyPosition obj = GetObject();

            double azm, alt;
            obj.CalcAzimuthal(d, latitude_, longitude_, out azm, out alt);

            try
            {
                DSCAlignment alignmentNew = (DSCAlignment)alignment_.Clone();
                alignmentNew.AddStar(new AlignStar(obj.Name, new Vect3(azm * Const.toRad, alt * Const.toRad), new PairA(angles_.AzmAngle, angles_.AltAngle), angles_.EquAngle));
                alignmentNew.ForceAlignment();
                alignment_ = alignmentNew;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Alignment Error", MessageBoxButtons.OK);
                return;
            }

            AlignmentChanged();
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            alignment_ = new DSCAlignment(new Vect3(0, latitude_ * Const.toRad), Precisions.Default);
            AlignmentChanged();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            CalcAndOutputResults();
        }

        private void buttonCorrectPolarAxis_Click(object sender, EventArgs e)
        {
            alignment_.CorrectEquAxis(new Vect3(0, latitude_ * Const.toRad));
            AlignmentChanged();
        }

        private void buttonLoadAlignment_Click(object sender, EventArgs e)
        {
            OpenFileDialog openfile = new OpenFileDialog();
            openfile.InitialDirectory = Application.StartupPath + @"\";
            openfile.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            openfile.FilterIndex = 1;
            openfile.RestoreDirectory = true;

            try
            {
                DialogResult res = openfile.ShowDialog();
                if (res != DialogResult.OK)
                    return;

                XmlProfile profile = new XmlProfile(openfile.FileName);
                AlignStar[] stars = (AlignStar[])profile.GetValue("entries", "AlignmentStars", null, typeof(AlignStar[]));
                Vect3 alignmentEquAxis = (Vect3)profile.GetValue("entries", "AlignmentEquAxis", new Vect3());
                if (stars != null)
                {
                    alignment_ = new DSCAlignment(alignmentEquAxis, Precisions.Default);
                    for (int i = 0; i < stars.Length; ++i)
                        alignment_.AddStar(stars[i]);

                    alignment_.ForceAlignment();
                    AlignmentChanged();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void buttonSaveAlignment_Click(object sender, EventArgs e)
        {
            if (!alignment_.IsAligned)
                return;

            SaveFileDialog savefile = new SaveFileDialog();
            savefile.FileName = MakeProfileName();
            savefile.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";

            try
            {
                DialogResult res = savefile.ShowDialog();
                if (res != DialogResult.OK)
                    return;

                XmlProfile profile = new XmlProfile(savefile.FileName);
                profile.AddTypes = AddType.Short;
                using (profile.Buffer())
                {
                    profile.SetValue("entries", "AlignmentStars", alignment_.Stars);
                    profile.SetValue("entries", "AlignmentEquAxis", alignment_.EquAxis);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void buttonCorrectOffsets_Click(object sender, EventArgs e)
        {
            if (!alignment_.IsAligned)
                return;
            double d = ClientCommonAPI.CalcTime();
            SkyObjectPosCalc.SkyPosition obj = GetObject();

            double azm, alt;
            obj.CalcAzimuthal(d, latitude_, longitude_, out azm, out alt);

            try
            {
                alignment_.CorrectOffsets(new AlignStar(obj.Name, new Vect3(azm * Const.toRad, alt * Const.toRad), new PairA(angles_.AzmAngle, angles_.AltAngle), angles_.EquAngle));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Correct Alignment Error", MessageBoxButtons.OK);
                return;
            }

            AlignmentChanged();
        }
    }
}
