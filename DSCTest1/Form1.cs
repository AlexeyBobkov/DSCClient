using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;

using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

using DSCCalculations;

namespace DSCTest1
{
    public partial class Form1 : Form
    {
        private const double toRad = Math.PI / 180.0;
        private const double toDeg = 1.0 / toRad;

        private bool init_ = false;

        // all angles in Radians
        private double azmOff_, altOff_;
        private double standRotationAxisAzm_, standRotationAngle_;
        private double equAxisAzm_, equAxisAlt_, equAngle_, equAngle2_, equAngleD_, equAngleD2_;
        private double equAngleFactor_;
        private double randomError_;
        private double star1Azm_, star1Alt_;
        private double star2Azm_, star2Alt_;
        private double objAzm_, objAlt_;
        private double objEquAngle_;

        private double minAlt_;
        private double latitude_;
        private Precisions precesions_ = Precisions.Default;
        private Random rnd_ = new Random();

        private Alignment alignment_;
        private TestSettings settings_ = new TestSettings();

        private Rotation3 StandRotation
        {
            get
            {
                Vect3 axis = new Vect3(Math.Cos(standRotationAxisAzm_), -Math.Sin(standRotationAxisAzm_), 0);
                return new Rotation3(standRotationAngle_, axis);
            }
        }
        private Vect3 EquAxis
        {
            get
            {
                return new Vect3(equAxisAzm_, equAxisAlt_);
            }
        }


        private PairA Star1 { get { return new PairA(star1Azm_, star1Alt_); } }
        private PairA Star2 { get { return new PairA(star2Azm_, star2Alt_); } }
        private PairA Obj { get { return new PairA(objAzm_, objAlt_); } }

        private PairA FromScope(PairA s, double equAngle)
        {
            return FromScope(s, equAngle, EquAxis, StandRotation);
        }

        private PairA FromScope(PairA s, double equAngle, Vect3 equAxis, Rotation3 standRotation)
        {
            return (new Rotation3(-equAngle, equAxis) * standRotation).Apply(s.Offset(-azmOff_, -altOff_));
        }

        private PairA ToScope(PairA h, double equAngle)
        {
            return ToScope(h, equAngle, EquAxis, StandRotation);
        }

        private PairA ToScope(PairA h, double equAngle, Vect3 equAxis, Rotation3 standRotation)
        {
            return (new Rotation3(-equAngle, equAxis) * standRotation).Conj.Apply(h).Offset(azmOff_, altOff_); ;
        }

        private string PrintV(PairA v)
        {
            return "{A = " + (v.Azm * toDeg).ToString("F5") + ", h = " + (v.Alt * toDeg).ToString("F5") + "}";
        }

        private string PrintV(Vect3 v)
        {
            return "{A = " + (v.Azm * toDeg).ToString("F5") + ", h = " + (v.Alt * toDeg).ToString("F5") + "}";
        }

        private double RndAngleErr()
        {
            return randomError_ * (rnd_.NextDouble() - 0.5);
        }

        public static bool IsEquAxisCorrectionNeeded(double latitude, Alignment alignment)
        {
            if (alignment == null)
                return false;
            Vect3 equAxis = alignment.EquAxis;
            return (Math.Abs(-equAxis.Azm) > 0.0003 || Math.Abs(latitude - equAxis.Alt) > 0.0003);
        }

        public static string AddEquAxisCorrectionText(double latitude, Alignment alignment)
        {
            if (alignment == null)
                return "";
            Vect3 equAxis = alignment.EquAxis;
            return "Polar Axis Correction Needed: " + (-equAxis.Azm * toDeg).ToString("F5") + ", " + ((latitude - equAxis.Alt) * toDeg).ToString("F5");
        }

        private int MakeDynamic4StarAlignment(Vect3 star0, string name0, PairA stand0, double eqAngle0,
                                                    Vect3 star1, string name1, PairA stand1, double eqAngle1,
                                                    Vect3 star2, string name2, PairA stand2, double eqAngle2,
                                                    Vect3 star3, string name3, PairA stand3, double eqAngle3,
                                                    out Alignment calc)
        {
            Vect3 eqNorth = new Vect3(0, latitude_);
            Vect3 eqAxis = eqNorth;
            double equAngleFactor = 1;
            calc = null;
            int i = 0;
            for (; i < 50; ++i)
            {
                Vect3 star1_corrected = new Rotation3((eqAngle1 - eqAngle0) * equAngleFactor, eqAxis).Apply(star1);
                Vect3 star3_corrected = new Rotation3((eqAngle3 - eqAngle2) * equAngleFactor, eqAxis).Apply(star3);
                calc = new DSCAlignment(new Vect3(0, latitude_), precesions_);
                calc.AddStar(new AlignStar(name0, star0, stand0, eqAngle0));
                calc.AddStar(new AlignStar(name1, star1_corrected, stand1, eqAngle0));
                calc.AddStar(new AlignStar(name2, star2, stand2, eqAngle2));
                calc.AddStar(new AlignStar(name3, star3_corrected, stand3, eqAngle2));

                Vect3 equAxis = calc.EquAxis;
                PairA correction = new PairA(-equAxis.Azm, latitude_ - equAxis.Alt);
                Vect3 eqAxisNew = new Vect3(eqNorth.Azm - correction.Azm, eqNorth.Alt - correction.Alt);
                double diff = Vect3.VMul(eqAxis, eqAxisNew).Abs;
                if (diff < toRad / 240)
                    break;
                eqAxis = eqAxisNew;
                equAngleFactor = calc.EquAngleFactor;
            }
            return i;
        }

        private void CalcAndOutputResults()
        {
            if (!init_)
                return;

            // if true, make four start alignment (i.e. actually the same two alignment starts, but twice for two equatorial angles)
            bool fourStars = textBoxPlatformA2.Enabled = textBoxPlatformDA2.Enabled = checkBox4StarAlignment.Checked;

            string s = "";

            try
            {
                {
                    //rnd_ = new Random(0); 
                    
                    PairA stand1 = ToScope(Star1, equAngle_).Offset(RndAngleErr(), RndAngleErr());

                    s += "Eq angle = " + (equAngle_*toDeg).ToString() + ", Star1 in scope coordinates = " + PrintV(stand1) + Environment.NewLine;
                    if (stand1.Below(minAlt_ + altOff_))
                        throw new ApplicationException("Star1 is below min altitude");

                    PairA stand2 = ToScope(Star2, equAngle_ + equAngleD_).Offset(RndAngleErr(), RndAngleErr());

                    s += "Eq angle = " + ((equAngle_ + equAngleD_) * toDeg).ToString() + ", Star2 in scope coordinates = " + PrintV(stand2) + Environment.NewLine;
                    if (stand2.Below(minAlt_ + altOff_))
                        throw new ApplicationException("Star2 is below min altitude");

                    s += Environment.NewLine;

                    PairA stand21 = new PairA(), stand22 = new PairA();
                    if (fourStars)
                    {
                        stand21 = ToScope(Star1, equAngle2_).Offset(RndAngleErr(), RndAngleErr());
                        stand22 = ToScope(Star2, equAngle2_ + equAngleD2_).Offset(RndAngleErr(), RndAngleErr());

                        s += "Eq angle = " + (equAngle2_ * toDeg).ToString() + ", Star1 in scope coordinates = " + PrintV(stand21) + Environment.NewLine;
                        s += "Eq angle = " + ((equAngle2_ + equAngleD2_) * toDeg).ToString() + ", Star2 in scope coordinates = " + PrintV(stand22) + Environment.NewLine;
                        s += Environment.NewLine;
                    }

                    Alignment calc;
                    /*
                    if (fourStars && (equAngleD_ != 0 || equAngleD2_ != 0))
                    {
                        Vect3 star1 = new Vect3(Star1);
                        Vect3 star2 = new Vect3(Star2);
                        int iterations = MakeDynamic4StarAlignment(star1, "Star1", stand1, equAngle_ * equAngleFactor_,
                                                         star2, "Star2", stand2, (equAngle_ + equAngleD_) * equAngleFactor_,
                                                         star1, "Star1", stand21, equAngle2_ * equAngleFactor_,
                                                         star2, "Star2", stand22, (equAngle2_ + equAngleD2_) * equAngleFactor_,
                                                         out calc);
                        s += "Iterations: " + iterations.ToString();
                        s += Environment.NewLine;
                    }
                    else
                     * */
                    {
                        calc = new DSCAlignment(new Vect3(0, latitude_), precesions_);
                        calc.AddStar(new AlignStar("Star1", new Vect3(Star1), stand1, equAngle_ * equAngleFactor_));
                        calc.AddStar(new AlignStar("Star2", new Vect3(Star2), stand2, (equAngle_ + equAngleD_) * equAngleFactor_));
                        if (fourStars)
                        {
                            calc.AddStar(new AlignStar("Star1", new Vect3(Star1), stand21, equAngle2_ * equAngleFactor_));
                            calc.AddStar(new AlignStar("Star2", new Vect3(Star2), stand22, (equAngle2_ + equAngleD2_) * equAngleFactor_));
                        }
                    }

                    s += calc.ToString(true);
                    if (IsEquAxisCorrectionNeeded(latitude_, calc))
                        s += Environment.NewLine + AddEquAxisCorrectionText(latitude_, calc);
                    s += Environment.NewLine;
                    s += Environment.NewLine;

                    s += "OBJECT POSITION" + Environment.NewLine;
                    PairA standObj = ToScope(Obj, objEquAngle_);
                    s += "Object in scope coordinates = " + PrintV(standObj);
                    s += Environment.NewLine;
                    if (standObj.Below(minAlt_ + altOff_))
                        throw new ApplicationException("Object is below min altitude");

                    PairA calcObj = calc.Scope2Horz(standObj, objEquAngle_ * equAngleFactor_);
                    double errorMin = Math.Asin(Vect3.VMul(new Vect3(Obj), new Vect3(calcObj)).Abs) * toDeg * 60;
                    s += "Calculated Horizontal Object: " + PrintV(calcObj) + ", error " + errorMin.ToString("F2") + " arc min";
                    s += Environment.NewLine;
                    s += Environment.NewLine;

                    alignment_ = calc; // (Alignment)calc.Clone();

                    if (IsEquAxisCorrectionNeeded(latitude_, calc))
                    {
                        calc.CorrectEquAxis(new Vect3(0, latitude_));
                        s += calc.ToString(true);
                    }
                }
            }
            catch (Exception e)
            {
                s += e.Message;
                s += Environment.NewLine;
            }

            textBoxResults.Text = s;
        }

        public Form1()
        {
            azmOff_ = 123 * toRad;
            altOff_ = 65 * toRad;
            //azmOff_ = 0 * toRad;
            //altOff_ = 0 * toRad;

            standRotationAxisAzm_ = 90 * toRad;
            //standRotationAxisAzm_ = 270 * toRad;
            standRotationAngle_ = 45 * toRad;
            //standRotationAngle_ = 50 * toRad;
            equAxisAzm_ = 0 * toRad;
            equAxisAlt_ = 37.28203 * toRad;
            equAngle_ = 0 * toRad;
            equAngle2_ = 10 * toRad;
            equAngleD_ = 0 * toRad;
            equAngleD2_ = 0 * toRad;
            equAngleFactor_ = 1;
            randomError_ = 0;

            star1Azm_ = 10 * toRad;
            star1Alt_ = 90 * toRad;

            star2Azm_ = 58 * toRad;
            star2Alt_ = 46 * toRad;

            objAzm_ = 45 * toRad;
            objAlt_ = 45 * toRad;
            //objAzm_ = 90 * toRad;
            //objAlt_ = 0 * toRad;
            objEquAngle_ = 0 * toRad;

            minAlt_ = -10 * toRad;
            latitude_ = 37.28203 * toRad;
            //latitude_ = 40 * toRad;

            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBoxLatitude.Text = (latitude_ * toDeg).ToString();
            
            textBoxAzmOff.Text = (azmOff_ * toDeg).ToString();
            textBoxAltOff.Text = (altOff_ * toDeg).ToString();

            textBoxAxisAzm.Text = (standRotationAxisAzm_ * toDeg).ToString();
            textBoxRotationAngle.Text = (standRotationAngle_ * toDeg).ToString();
            textBoxPlatformEquAxisAzm.Text = (equAxisAzm_ * toDeg).ToString();
            textBoxPlatformEquAxisAlt.Text = (equAxisAlt_ * toDeg).ToString();
            textBoxPlatformA.Text = (equAngle_ * toDeg).ToString();
            textBoxPlatformA2.Text = (equAngle2_ * toDeg).ToString();
            textBoxPlatformDA.Text = (equAngleD_ * toDeg).ToString();
            textBoxPlatformDA2.Text = (equAngleD2_ * toDeg).ToString();
            textBoxEquAngleFactor.Text = equAngleFactor_.ToString();
            textBoxRandomError.Text = (randomError_ * toDeg * 60).ToString();

            textBoxStar1Azm.Text = (star1Azm_ * toDeg).ToString();
            textBoxStar1Alt.Text = (star1Alt_ * toDeg).ToString();

            textBoxStar2Azm.Text = (star2Azm_ * toDeg).ToString();
            textBoxStar2Alt.Text = (star2Alt_ * toDeg).ToString();

            textBoxObjectAzm.Text = (objAzm_ * toDeg).ToString();
            textBoxObjectAlt.Text = (objAlt_ * toDeg).ToString();
            textBoxObjectEquA.Text = (objEquAngle_ * toDeg).ToString();

            init_ = true;
            CalcAndOutputResults();
        }

        private void textBoxAzmOff_TextChanged(object sender, EventArgs e)
        {
            try
            {
                azmOff_ = Convert.ToDouble(textBoxAzmOff.Text) * toRad;
            }
            catch (System.FormatException)
            {
                azmOff_ = 0;
            }
            CalcAndOutputResults();
        }

        private void textBoxAltOff_TextChanged(object sender, EventArgs e)
        {
            try
            {
                altOff_ = Convert.ToDouble(textBoxAltOff.Text) * toRad;
            }
            catch (System.FormatException)
            {
                altOff_ = 0;
            }
            CalcAndOutputResults();
        }

        private void textBoxAxisAzm_TextChanged(object sender, EventArgs e)
        {
            try
            {
                standRotationAxisAzm_ = Convert.ToDouble(textBoxAxisAzm.Text);
                if (standRotationAxisAzm_ < 0 || standRotationAxisAzm_ > 360)
                    standRotationAxisAzm_ = 0;
                else
                    standRotationAxisAzm_ *= toRad;
            }
            catch (System.FormatException)
            {
                standRotationAxisAzm_ = 0;
            }
            CalcAndOutputResults();
        }

        private void textBoxRotationAngle_TextChanged(object sender, EventArgs e)
        {
            try
            {
                standRotationAngle_ = Convert.ToDouble(textBoxRotationAngle.Text);
                if (standRotationAngle_ < 0 || standRotationAngle_ > 90)
                    standRotationAngle_ = 0;
                else
                    standRotationAngle_ *= toRad;
            }
            catch (System.FormatException)
            {
                standRotationAngle_ = 0;
            }
            CalcAndOutputResults();
        }

        private void textBoxStar1Azm_TextChanged(object sender, EventArgs e)
        {
            try
            {
                star1Azm_ = Convert.ToDouble(textBoxStar1Azm.Text);
                if (star1Azm_ < 0 || star1Azm_ > 360)
                    star1Azm_ = 0;
                else
                    star1Azm_ *= toRad;
            }
            catch (System.FormatException)
            {
                star1Azm_ = 0;
            }
            CalcAndOutputResults();
        }

        private void textBoxStar1Alt_TextChanged(object sender, EventArgs e)
        {
            try
            {
                star1Alt_ = Convert.ToDouble(textBoxStar1Alt.Text);
                if (star1Alt_ < -90 || star1Alt_ > 90)
                    star1Alt_ = 0;
                else
                    star1Alt_ *= toRad;
            }
            catch (System.FormatException)
            {
                star1Alt_ = 0;
            }
            CalcAndOutputResults();
        }

        private void textBoxStar2Azm_TextChanged(object sender, EventArgs e)
        {
            try
            {
                star2Azm_ = Convert.ToDouble(textBoxStar2Azm.Text);
                if (star2Azm_ < 0 || star2Azm_ > 360)
                    star2Azm_ = 0;
                else
                    star2Azm_ *= toRad;
            }
            catch (System.FormatException)
            {
                star2Azm_ = 0;
            }
            CalcAndOutputResults();
        }

        private void textBoxStar2Alt_TextChanged(object sender, EventArgs e)
        {
            try
            {
                star2Alt_ = Convert.ToDouble(textBoxStar2Alt.Text);
                if (star2Alt_ < -90 || star2Alt_ > 90)
                    star2Alt_ = 0;
                else
                    star2Alt_ *= toRad;
            }
            catch (System.FormatException)
            {
                star2Alt_ = 0;
            }
            CalcAndOutputResults();
        }

        private void textBoxObjectAzm_TextChanged(object sender, EventArgs e)
        {
            try
            {
                objAzm_ = Convert.ToDouble(textBoxObjectAzm.Text);
                if (objAzm_ < 0 || objAzm_ > 360)
                    objAzm_ = 0;
                else
                    objAzm_ *= toRad;
            }
            catch (System.FormatException)
            {
                objAzm_ = 0;
            }
            CalcAndOutputResults();
        }

        private void textBoxObjectAlt_TextChanged(object sender, EventArgs e)
        {
            try
            {
                objAlt_ = Convert.ToDouble(textBoxObjectAlt.Text);
                if (objAlt_ < -90 || objAlt_ > 90)
                    objAlt_ = 0;
                else
                    objAlt_ *= toRad;
            }
            catch (System.FormatException)
            {
                objAlt_ = 0;
            }
            CalcAndOutputResults();
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            CalcAndOutputResults();
        }

        private void textBoxPlatformEquAxisAzm_TextChanged(object sender, EventArgs e)
        {
            try
            {
                equAxisAzm_ = Convert.ToDouble(textBoxPlatformEquAxisAzm.Text);
                if (equAxisAzm_ < 0 || equAxisAzm_ > 360)
                    equAxisAzm_ = 0;
                else
                    equAxisAzm_ *= toRad;
            }
            catch (System.FormatException)
            {
                equAxisAzm_ = 0;
            }
            CalcAndOutputResults();
        }

        private void textBoxPlatformEquAxisAlt_TextChanged(object sender, EventArgs e)
        {
            try
            {
                equAxisAlt_ = Convert.ToDouble(textBoxPlatformEquAxisAlt.Text);
                if (equAxisAlt_ < -90 || equAxisAlt_ > 90)
                    equAxisAlt_ = 0;
                else
                    equAxisAlt_ *= toRad;
            }
            catch (System.FormatException)
            {
                equAxisAlt_ = 0;
            }
            CalcAndOutputResults();
        }

        private void textBoxPlatformA_TextChanged(object sender, EventArgs e)
        {
            try
            {
                equAngle_ = Convert.ToDouble(textBoxPlatformA.Text);
                if (equAngle_ < -90 || equAngle_ > 90)
                    equAngle_ = 0;
                else
                    equAngle_ *= toRad;
            }
            catch (System.FormatException)
            {
                equAngle_ = 0;
            }
            CalcAndOutputResults();
        }

        private void textBoxPlatformA2_TextChanged(object sender, EventArgs e)
        {
            try
            {
                equAngle2_ = Convert.ToDouble(textBoxPlatformA2.Text);
                if (equAngle2_ < -90 || equAngle2_ > 90)
                    equAngle2_ = 0;
                else
                    equAngle2_ *= toRad;
            }
            catch (System.FormatException)
            {
                equAngle2_ = 0;
            }
            CalcAndOutputResults();
        }

        private void textBoxObjectEquA_TextChanged(object sender, EventArgs e)
        {
            try
            {
                objEquAngle_ = Convert.ToDouble(textBoxObjectEquA.Text);
                if (objEquAngle_ < -90 || objEquAngle_ > 90)
                    objEquAngle_ = 0;
                else
                    objEquAngle_ *= toRad;
            }
            catch (System.FormatException)
            {
                objEquAngle_ = 0;
            }
            CalcAndOutputResults();
        }

        private void textBoxLatitude_TextChanged(object sender, EventArgs e)
        {
            try
            {
                latitude_ = Convert.ToDouble(textBoxLatitude.Text);
                if (latitude_ < 0 || latitude_ > 90)
                    latitude_ = 0;
                else
                    latitude_ *= toRad;
            }
            catch (System.FormatException)
            {
                latitude_ = 0;
            }
            CalcAndOutputResults();
        }

        private void checkBox4StarAlignment_CheckedChanged(object sender, EventArgs e)
        {
            CalcAndOutputResults();
        }

        private void checkBoxRandomErrors_CheckedChanged(object sender, EventArgs e)
        {
            CalcAndOutputResults();
        }

        private void textBoxPlatformDA_TextChanged(object sender, EventArgs e)
        {
            try
            {
                equAngleD_ = Convert.ToDouble(textBoxPlatformDA.Text);
                if (equAngleD_ < -90 || equAngleD_ > 90)
                    equAngleD_ = 0;
                else
                    equAngleD_ *= toRad;
            }
            catch (System.FormatException)
            {
                equAngleD_ = 0;
            }
            CalcAndOutputResults();
        }

        private void textBoxPlatformDA2_TextChanged(object sender, EventArgs e)
        {
            try
            {
                equAngleD2_ = Convert.ToDouble(textBoxPlatformDA2.Text);
                if (equAngleD2_ < -90 || equAngleD2_ > 90)
                    equAngleD2_ = 0;
                else
                    equAngleD2_ *= toRad;
            }
            catch (System.FormatException)
            {
                equAngleD2_ = 0;
            }
            CalcAndOutputResults();
        }

        private void textBoxEquAngleFactor_TextChanged(object sender, EventArgs e)
        {
            try
            {
                equAngleFactor_ = Convert.ToDouble(textBoxEquAngleFactor.Text);
                if (equAngleFactor_ < 0.5 || equAngleFactor_ > 2)
                    equAngleFactor_ = 1;
            }
            catch (System.FormatException)
            {
                equAngleFactor_ = 1;
            }
            CalcAndOutputResults();
        }

        private void textBoxRandomError_TextChanged(object sender, EventArgs e)
        {
            try
            {
                randomError_ = Convert.ToDouble(textBoxRandomError.Text);
                if (randomError_ < -60 || randomError_ > 60)
                    randomError_ = 0;
                else
                    randomError_ *= toRad/60;
            }
            catch (System.FormatException)
            {
                randomError_ = 0;
            }
            CalcAndOutputResults();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            settings_.Vector = new Vect3(Obj);
            settings_.AlignStar1 = new AlignStar("StarStar", new Vect3(Obj), Obj, 21.098);
            settings_.AlignStars = alignment_.Stars;
            settings_.Save();

            /*
            using ( TextWriter writer = new StreamWriter( @"D:\Tmp\Xml.txt"))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(AlignStar));
                serializer.Serialize(writer, settings_.AlignStar1);

                XmlSerializer serializer1 = new XmlSerializer(typeof(Vect3));
                serializer1.Serialize(writer, settings_.Vector);
            }

            using (Stream writer = File.Create(@"D:\Tmp\Bin.bin"))
            {
                BinaryFormatter serializer = new BinaryFormatter();
                serializer.Serialize(writer, settings_.AlignStar1);
                serializer.Serialize(writer, settings_.Vector);
            }
             * */
        }

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            /*
            Vect3 v = settings_.Vector;
            AlignStar astar = settings_.AlignStar1;
            textBoxResults.Text = v.ToString() + " " + astar.Name +" " + astar.Horz.ToString() + " " + astar.Scope.ToString() + " " + astar.EquAngle;
             * */

            AlignStar[] stars = settings_.AlignStars;
            if (stars != null)
            {
                string s = "";
                for (int i = 0; i < stars.Length; ++i)
                    if (stars[i] != null)
                    {
                        s += stars[i].Name + " " + stars[i].Horz.ToString() + " " + stars[i].Scope.ToString() + " " + stars[i].EquAngle*toDeg + Environment.NewLine;

                    }
                textBoxResults.Text = s;
            }
        }
    }

    //Application settings wrapper class
    sealed class TestSettings : ApplicationSettingsBase
    {
        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute("")]
        public Vect3 Vector
        {
            get { return (Vect3)this["Vector"]; }
            set { this["Vector"] = value; }
        }

        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute("")]
        public AlignStar AlignStar1
        {
            get { return (AlignStar)this["AlignStar1"]; }
            set { this["AlignStar1"] = value; }
        }
        [UserScopedSettingAttribute()]
        [DefaultSettingValueAttribute("")]
        public AlignStar[] AlignStars
        {
            get { return (AlignStar[])this["AlignStars"]; }
            set { this["AlignStars"] = value; }
        }
    }
}
