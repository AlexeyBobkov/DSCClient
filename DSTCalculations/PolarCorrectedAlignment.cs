using System;

namespace DSCCalculations
{
    class PolarCorrectedAlignment : Alignment
    {
        private AlignStar[] stars_;
        private Vect3 equAxis_;
        private double altOffset_;
        private Rotation3 rotationToStand_;
        private double equAngleFactor_;
        private Rotation3 correction_;   // correction rotation to be done

        public PolarCorrectedAlignment(Vect3 newEquAxis, Vect3 prevEquAxis, double altOffset, Rotation3 prevRotationToStand, double equAngle0, double equAngleFactor, AlignStar[] stars)
        {
            equAxis_ = newEquAxis;
            altOffset_ = altOffset;
            equAngleFactor_ = equAngleFactor;

            // calculate RotationToStand for equatorial angle = 0
            prevRotationToStand = prevRotationToStand * new Rotation3(-equAngle0 * equAngleFactor_, prevEquAxis);

            // convert coordinates of pole to scope coordinates
            Vect3 oldScopePoleAxis = prevRotationToStand.Apply(prevEquAxis);
            Vect3 newScopePoleAxis = prevRotationToStand.Apply(equAxis_);

            // correct first altitude, then correct azimuth
            correction_ = new Rotation3(newScopePoleAxis.Alt - oldScopePoleAxis.Alt, new Vect3(newScopePoleAxis.Azm + Math.PI / 2, 0));
            correction_ = new Rotation3(newScopePoleAxis.Azm - oldScopePoleAxis.Azm, new Vect3(0, 0, -1)) * correction_;

            rotationToStand_ = correction_.Conj * prevRotationToStand;

            if (stars != null && stars.Length > 0)
            {
                stars_ = new AlignStar[stars.Length];
                for (int i = 0; i < stars.Length; ++i)
                    if (stars[i] != null)
                    {
                        PairA newScope = DoHorz2Scope((PairA)stars[i].Horz, stars[i].EquAngle);
                        stars_[i] = new AlignStar(stars[i].Name, stars[i].Horz, newScope, stars[i].EquAngle);
                    }
            }
        }

        private PairA DoHorz2Scope(PairA horz, double equAngle)
        {
            return (rotationToStand_ * new Rotation3(equAngle * equAngleFactor_, equAxis_)).Apply(horz).Offset(0, altOffset_);
        }

        public override PairA Scope2Horz(PairA scope, double equAngle)
        {
            return (new Rotation3(-equAngle * equAngleFactor_, equAxis_) * rotationToStand_.Conj).Apply(scope.Offset(0, -altOffset_));
        }
        public override PairA Horz2Scope(PairA horz, double equAngle)
        {
            return DoHorz2Scope(horz, equAngle);
        }
        public override Alignment AddStar(AlignStar newStar) { return this; }
        public override void ForceAlignment() {}
        public override bool IsAligned { get { return true; } }
        public override AlignStar[] Stars { get { return stars_; } }
        public override Vect3 EquAxis { get { return equAxis_; } }
        public override double EquAngleFactor { get { return equAngleFactor_; } }
        public override Alignment CorrectEquAxis(Vect3 newEquAxis) { return this; }

        public override string ToString() { return ToString(false); }
        public override string ToString(bool verbose)
        {
            string s = "PolarCorrectedAlignment" + Environment.NewLine;

            if (stars_ != null && stars_.Length > 0)
            {
                s += "\tObjects: ";
                bool first = true;
                for (int i = 0; i < stars_.Length; ++i)
                    if (stars_[i] != null)
                    {
                        if (first)
                            first = false;
                        else
                            s += ",";
                        s += stars_[i].Name;
                    }
                s += Environment.NewLine;
            }

            s += "\tEquatorial Axis: " + equAxis_.ToString() + Environment.NewLine;
            if (verbose)
            {
                s += "\th Offset: " + (altOffset_ * Const.toDeg).ToString("F5") + Environment.NewLine;
                s += "\tRotation: " + rotationToStand_.ToString() + Environment.NewLine;
            }
            s += "\tCorrection: " + correction_.ToString() + Environment.NewLine;
            s += "\tEqu Angle Factor: " + equAngleFactor_.ToString("F5") + Environment.NewLine;
            return s;
        }

        private PolarCorrectedAlignment(Vect3 equAxis, AlignStar[] stars, double altOffset, Rotation3 rotationToStand, double equAngleFactor, Rotation3 correction)
        {
            equAxis_ = equAxis;

            if (stars != null && stars.Length > 0)
            {
                stars_ = new AlignStar[stars.Length];
                for (int i = 0; i < stars.Length; ++i)
                    if (stars[i] != null)
                        stars_[i] = (AlignStar)stars[i].Clone();
            }

            altOffset_ = altOffset;
            rotationToStand_ = rotationToStand;
            equAngleFactor_ = equAngleFactor;
            correction_ = correction;
        }
        public override Object Clone() { return new PolarCorrectedAlignment(equAxis_, stars_, altOffset_, rotationToStand_, equAngleFactor_, correction_); }
    }
}
