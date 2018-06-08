using System;

namespace DSCCalculations
{
    // two star alignment
    public class TwoStarsAlignment : Alignment
    {
        private Vect3 equAxis_;
        private AlignStar[] stars_ = new AlignStar[4];
        private bool alignmentDone_;
        private double altOffset_;
        private Rotation3 rotationToStand_;
        private double quality_;
        private Precisions precesions_;

        private static double CalcAltOffset(Vect3 horz0, PairA scope0, Vect3 horz1, PairA scope1)
        {
            double cosA1A2 = Math.Cos(scope0.Azm - scope1.Azm);
            double den = 1 - cosA1A2;
            if (den == 0)
                throw new ApplicationException("Error2");

            double x = (Math.Cos(scope0.Alt - scope1.Alt) * (cosA1A2 + 1) - 2 * Vect3.SMul(horz0, horz1)) / den;
            if (x < -1 || x > 1)
                throw new ApplicationException("Error3");

            return (-Math.Acos(x) + scope0.Alt + scope1.Alt) / 2;
        }

        private static double CalcRotationAngle(Vect3 n, Vect3 a0, Vect3 a1)
        {
            double an = Vect3.SMul(a0, n);
            return Math.Atan2(Vect3.SMul(n, Vect3.VMul(a0, a1)), Vect3.SMul(a0, a1) - an * an);
        }

        public static void Align(Vect3 horz0, PairA scope0, Vect3 horz1, PairA scope1, Precisions precesions,
                                    out double altOffset, out Rotation3 rotationToStand, out double quality)
        {
            altOffset = CalcAltOffset(horz0, scope0, horz1, scope1);
            Vect3 s0 = new Vect3(scope0.Offset(0, -altOffset));
            Vect3 s1 = new Vect3(scope1.Offset(0, -altOffset));

            double abs;

            Vect3 n0 = horz0 - s0;
            abs = n0.Abs;
            if (abs < precesions.rotation_)
            {
                double angle = CalcRotationAngle(s0, horz1, s1);
                if (angle == 0 || angle == Math.PI)
                    throw new ApplicationException("Error7");

                quality = -1;
                rotationToStand = new Rotation3(angle, s0);
                return;
            }
            n0 /= abs;

            Vect3 n1 = horz1 - s1;
            abs = n1.Abs;
            if (abs < precesions.rotation_)
            {
                double angle = CalcRotationAngle(s1, horz0, s0);
                if (angle == 0 || angle == Math.PI)
                    throw new ApplicationException("Error8");

                quality = -2;
                rotationToStand = new Rotation3(angle, s1);
                return;
            }
            n1 /= abs;

            // axis
            Vect3 axis = Vect3.VMul(n0, n1);
            abs = axis.Abs;
            if (abs < precesions.axis_)
                throw new ApplicationException("Error4");
            axis /= abs;

            double angle0 = CalcRotationAngle(axis, horz0, s0);
            if (angle0 == 0 || angle0 == Math.PI)
                throw new ApplicationException("Error5");

            double angle1 = CalcRotationAngle(axis, horz1, s1);
            if (angle1 == 0 || angle1 == Math.PI)
                throw new ApplicationException("Error6");

            quality = Math.Abs(angle0 - angle1);
            rotationToStand = new Rotation3((angle0 + angle1) / 2, axis);
        }

        public TwoStarsAlignment(Vect3 equAxis, Precisions precesions)
        {
            equAxis_ = equAxis;
            precesions_ = precesions;
            alignmentDone_ = false;
        }

        public TwoStarsAlignment(Vect3 equAxis, Precisions precesions, AlignStar star)
        {
            equAxis_ = equAxis;
            precesions_ = precesions;
            stars_[0] = star;
            alignmentDone_ = false;
        }

        public TwoStarsAlignment(Vect3 equAxis, Precisions precesions, AlignStar star0, AlignStar star1)
        {
            equAxis_ = equAxis;
            precesions_ = precesions;
            stars_[0] = (AlignStar)star0.Clone();
            stars_[1] = (AlignStar)star1.Clone();
            alignmentDone_ = false;
        }

        public Rotation3 RotationToStand
        {
            get
            {
                ForceAlignment();
                return rotationToStand_;
            }
        }

        public double EquAngle
        {
            get
            {
                ForceAlignment();
                return stars_[0].EquAngle;
            }
        }

        public override PairA Scope2Horz(PairA scope, double equAngle)
        {
            ForceAlignment();
            return (new Rotation3(stars_[0].EquAngle - equAngle, equAxis_) * rotationToStand_.Conj).Apply(scope.Offset(0, -altOffset_));
        }
        public override PairA Horz2Scope(PairA horz, double equAngle)
        {
            ForceAlignment();
            return (rotationToStand_ * new Rotation3(equAngle - stars_[0].EquAngle, equAxis_)).Apply(horz).Offset(0, altOffset_);
        }
        public override Alignment AddStar(AlignStar newStar)
        {
            if (newStar == null)
                return this;

            if (stars_[0] == null)
                stars_[0] = newStar;
            else if (stars_[1] == null)
            {
                if (Math.Abs(newStar.EquAngle - stars_[0].EquAngle) > precesions_.iterEquAngleDiff_)
                    stars_[0] = newStar;
                else
                    stars_[1] = newStar;
            }
            else if (stars_[2] == null)
            {
                if (Math.Abs(newStar.EquAngle - stars_[1].EquAngle) > precesions_.iterEquAngleDiff_)
                    stars_[2] = newStar;
                else
                {
                    stars_[0] = stars_[1];
                    stars_[1] = newStar;
                    alignmentDone_ = false;
                }
            }
            else if (Math.Abs(newStar.EquAngle - stars_[2].EquAngle) > precesions_.iterEquAngleDiff_)
                stars_[2] = newStar;
            else
            {
                stars_[3] = newStar;

                ForceAlignment();
                return new FourStarsAlignment(equAxis_, stars_, rotationToStand_, altOffset_, quality_, precesions_);
            }
            return this;
        }

        public override void ForceAlignment()
        {
            if (!alignmentDone_)
            {
                if (stars_[0] == null || stars_[1] == null)
                    throw new ApplicationException("No alignment stars");
                Vect3 star1horz_corrected = new Rotation3(stars_[1].EquAngle - stars_[0].EquAngle, equAxis_).Apply(stars_[1].Horz);
                Align(stars_[0].Horz, stars_[0].Scope, star1horz_corrected, stars_[1].Scope, precesions_, out altOffset_, out rotationToStand_, out quality_);
                alignmentDone_ = true;
            }
        }

        public override bool IsAligned
        {
            get
            {
                if (stars_[0] == null || stars_[1] == null)
                    return false;
                try
                {
                    ForceAlignment();
                }
                catch (Exception)
                {
                    return false;
                }
                return true;
            }
        }

        public override AlignStar[] Stars { get { return stars_; } }
        public override Vect3 EquAxis { get { return equAxis_; } }
        public override double EquAngleFactor { get { return 1; } }
        public override Alignment CorrectEquAxis(Vect3 newEquAxis) { return this; }

        public override string ToString() { return ToString(false); }
        public override string ToString(bool verbose)
        {
            string s = "";
            if (stars_[0] == null || stars_[1] == null)
            {
                s += "No Alignment" + Environment.NewLine;
                s += "\tEquatorial Axis: " + equAxis_.ToString() + Environment.NewLine;
            }
            else
            {
                s = "TwoStarsAlignment" + Environment.NewLine;
                s += "\tEquatorial Axis: " + equAxis_.ToString() + Environment.NewLine;
                if(verbose)
                    s += "\tEquatorial Angle: " + (stars_[0].EquAngle * Const.toDeg).ToString("F5") + Environment.NewLine;
                s += "\tObjects: " + stars_[0].Name + ", " + stars_[1].Name + Environment.NewLine;

                try
                {
                    ForceAlignment();
                    if (verbose)
                    {
                        s += "\th Offset: " + (altOffset_ * Const.toDeg).ToString("F5") + Environment.NewLine;
                        s += "\tRotation: " + rotationToStand_.ToString() + Environment.NewLine;
                        s += "\tQuality: " + quality_.ToString() + Environment.NewLine;
                    }
                }
                catch (Exception e)
                {
                    s += "\tException during alignment: " + e.Message + Environment.NewLine;
                }
            }
            return s;
        }

        // deep cloning
        private TwoStarsAlignment(Vect3 equAxis, AlignStar[] stars, bool alignmentDone, double altOffset, Rotation3 rotationToStand, double quality, Precisions precesions)
        {
            equAxis_ = equAxis;
            for (int i = 0; i < stars.Length; ++i)
                if (stars[i] != null)
                    stars_[i] = (AlignStar)stars[i].Clone();
            alignmentDone_ = alignmentDone;
            altOffset_ = altOffset;
            rotationToStand_ = rotationToStand;
            quality_ = quality;
            precesions_ = precesions;
        }
        public override Object Clone() { return new TwoStarsAlignment(equAxis_, stars_, alignmentDone_, altOffset_, rotationToStand_, quality_, precesions_); }
    }
}
