using System;

namespace DSCCalculations
{
    // two star alignment
    public class FourStarsAlignment : Alignment
    {
        private AlignStar[] stars_ = new AlignStar[6];
        private Rotation3 rotationToStand_;
        private double altOffset_;
        private Vect3 equAxis_;
        private double equScopeDiff_, equHorzDiff_;
        private double quality_;
        private Precisions precesions_;
        private int iterationCnt_;
        private bool alignmentDone_;

        public FourStarsAlignment(Vect3 equAxis, AlignStar[] stars, Rotation3 rotationToStand, double altOffset0, double quality0, Precisions precesions)
        {
            if (stars.Length < 4 || stars.Length > stars_.Length)
                throw new ApplicationException("too few alignment stars");

            // check equatorial angles: 1) should be within limits for stars 0,1, 2) should be within limits for stars 2,3, and 3) should be different for (0,1) and (2,3) star pairs
            if (Math.Abs(stars[0].EquAngle - stars[1].EquAngle) > precesions.iterEquAngleDiff_ ||
                Math.Abs(stars[2].EquAngle - stars[3].EquAngle) > precesions.iterEquAngleDiff_ ||
                stars[0].EquAngle == stars[2].EquAngle)
            {
                throw new ApplicationException("bad data for 4-stars alignment");
            }

            // check equatorial angles: 1) should be same for stars 0 and 1, 2) should be same for stars 2 and 3, and 3) should be different for (0,1) and (2,3) star pairs
            //if (stars[0].EquAngle != stars[1].EquAngle || stars[2].EquAngle != stars[3].EquAngle || stars[0].EquAngle == stars[2].EquAngle)
            //    throw new ApplicationException("bad data for 4-stars alignment");

            equAxis_ = equAxis;

            for (int i = 0; i < stars.Length; ++i)
                if (stars[i] != null)
                    stars_[i] = (AlignStar)stars[i].Clone();

            rotationToStand_ = rotationToStand;
            altOffset_ = altOffset0;
            equScopeDiff_ = stars_[2].EquAngle - stars_[0].EquAngle;

            quality_ = quality0;
            precesions_ = precesions;
            alignmentDone_ = false;
        }

        public override PairA Scope2Horz(PairA scope, double equAngle)
        {
            ForceAlignment();
            double da = (equAngle - stars_[0].EquAngle) * equHorzDiff_ / equScopeDiff_;
            return (new Rotation3(-da, equAxis_) * rotationToStand_.Conj).Apply(scope.Offset(0, -altOffset_));
        }
        public override PairA Horz2Scope(PairA horz, double equAngle)
        {
            ForceAlignment();
            double da = (equAngle - stars_[0].EquAngle) * equHorzDiff_ / equScopeDiff_;
            return (rotationToStand_ * new Rotation3(da, equAxis_)).Apply(horz).Offset(0, altOffset_);
        }
        public override Alignment AddStar(AlignStar newStar)
        {
            return this;
        }
        public override bool CorrectOffsets(AlignStar star)
        {
            if (star == null)
                return false;

            ForceAlignment();
            PairA scopeOld = Horz2Scope((PairA)star.Horz, star.EquAngle);
            double deltaAlt = star.Scope.Alt - scopeOld.Alt, deltaAzm = star.Scope.Azm - scopeOld.Azm;

            altOffset_ += deltaAlt;
            rotationToStand_ = new Rotation3(-deltaAzm, new Vect3(0, 0, 1)) * rotationToStand_;

            foreach (AlignStar s in stars_)
                if (s != null)
                    s.Scope = s.Scope.Offset(+deltaAzm, +deltaAlt);
            return true;
        }

        private void AlignStatic()
        {
            double altOffset1;
            Rotation3 rotationToStand1;
            double quality1;
            TwoStarsAlignment.Align(stars_[2].Horz, stars_[2].Scope, stars_[3].Horz, stars_[3].Scope, precesions_, out altOffset1, out rotationToStand1, out quality1);

            Rotation3 eqRotation = rotationToStand_.Conj * rotationToStand1;    // actual equatorial rotation

            equAxis_ = eqRotation.Axis;
            equHorzDiff_ = eqRotation.Angle;            // Generally, it isn't equal to equScopeDiff_. It depends on platform accuracy.
            altOffset_ = (altOffset_ + altOffset1) / 2; // average offset
            if (quality_ < quality1)
                quality_ = quality1;                    // maximal quality
            iterationCnt_ = 0;
        }

        private void AlignDynamic()
        {
            double equAngleFactor = 1;
            for (int i = 0; i < precesions_.iterCnt_; ++i)
            {
                Vect3 star1horz_corrected = new Rotation3((stars_[1].EquAngle - stars_[0].EquAngle) * equAngleFactor, equAxis_).Apply(stars_[1].Horz);
                Vect3 star3horz_corrected = new Rotation3((stars_[3].EquAngle - stars_[2].EquAngle) * equAngleFactor, equAxis_).Apply(stars_[3].Horz);

                double altOffset0;
                Rotation3 rotationToStand0;
                double quality0;
                TwoStarsAlignment.Align(stars_[0].Horz, stars_[0].Scope, star1horz_corrected, stars_[1].Scope, precesions_, out altOffset0, out rotationToStand0, out quality0);

                double altOffset1;
                Rotation3 rotationToStand1;
                double quality1;
                TwoStarsAlignment.Align(stars_[2].Horz, stars_[2].Scope, star3horz_corrected, stars_[3].Scope, precesions_, out altOffset1, out rotationToStand1, out quality1);

                Rotation3 eqRotation = rotationToStand0.Conj * rotationToStand1;    // actual equatorial rotation
                Vect3 equAxisNew = eqRotation.Axis;
                double diff = Vect3.VMul(equAxis_, equAxisNew).Abs;
                equAxis_ = equAxisNew;
                if (diff < precesions_.iterEquAxisDiff_)
                {
                    equHorzDiff_ = eqRotation.Angle;                        // Generally, it isn't equal to equScopeDiff_. It depends on platform accuracy.
                    rotationToStand_ = rotationToStand0;
                    altOffset_ = (altOffset0 + altOffset1) / 2;             // average offset
                    quality_ = (quality0 > quality1) ? quality0 : quality1; // maximal quality
                    iterationCnt_ = i + 1;
                    return;
                }
                equAngleFactor = eqRotation.Angle / equScopeDiff_;
            }
            throw new ApplicationException("too many iterations");
        }

        public override void ForceAlignment()
        {
            if (!alignmentDone_)
            {
                // Make four star alignment, i.e. calculate actual equatorial axis and
                // angle difference between two pairs of two star alignments
                if (stars_[0].EquAngle == stars_[1].EquAngle && stars_[2].EquAngle == stars_[3].EquAngle)
                    AlignStatic();
                else
                    AlignDynamic();
                alignmentDone_ = true;
            }
        }

        public override bool IsAligned
        {
            get
            {
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
        public override double EquAngleFactor { get { return equHorzDiff_ / equScopeDiff_; } }
        public override Alignment CorrectEquAxis(Vect3 newEquAxis)
        {
            if (!IsAligned)
                return this;
            return new PolarCorrectedAlignment(newEquAxis, equAxis_, altOffset_, rotationToStand_, stars_[0].EquAngle, equHorzDiff_ / equScopeDiff_, stars_);
        }

        public override string ToString() { return ToString(false); }
        public override string ToString(bool verbose)
        {
            string s = "";
            s = "FourStarsAlignment" + Environment.NewLine;
            s += "\tObjects: (" + stars_[0].Name + "," + stars_[1].Name + ") - (" + stars_[2].Name + "," + stars_[3].Name + ")" + Environment.NewLine;
            if (verbose)
            {
                s += "\tEquatorial Angles: " + (stars_[0].EquAngle * Const.toDeg).ToString("F5") + ", " + (stars_[2].EquAngle * Const.toDeg).ToString("F5") + Environment.NewLine;
                s += "\tRotation: " + rotationToStand_.ToString() + Environment.NewLine;
            }

            try
            {
                ForceAlignment();
                if (verbose)
                {
                    if (iterationCnt_ <= 0)
                        s += "\tType: static" + Environment.NewLine;
                    else
                        s += "\tType: dynamic, " + iterationCnt_.ToString() + " iterations" + Environment.NewLine;
                    s += "\th Offset: " + (altOffset_ * Const.toDeg).ToString("F5") + Environment.NewLine;
                }
                s += "\tEquatorial Axis: " + equAxis_.ToString() + Environment.NewLine;
                if (verbose)
                {
                    s += "\tEqu Angle Factor: " + (equHorzDiff_ / equScopeDiff_).ToString("F5") + Environment.NewLine;
                    s += "\tReported Equ Angle Diff: " + ((stars_[2].EquAngle - stars_[0].EquAngle) * Const.toDeg).ToString("F5") + Environment.NewLine;
                    s += "\tActual Equ Angle Diff: " + (equHorzDiff_ * Const.toDeg).ToString("F5") + Environment.NewLine;
                    s += "\tQuality: " + quality_.ToString() + Environment.NewLine;
                }
            }
            catch (Exception e)
            {
                s += "\tException during alignment: " + e.Message + Environment.NewLine;
            }
            return s;
        }

        // deep cloning
        private FourStarsAlignment(Vect3 equAxis, AlignStar[] stars, Rotation3 rotationToStand, double altOffset, double equHorzDiff, double quality, Precisions precesions, int iterationCnt, bool alignmentDone)
        {
            if (stars.Length < 4 || stars.Length > stars_.Length)
                throw new ApplicationException("too few alignment stars");

            // check equatorial angles: 1) should be within limits for stars 0,1, 2) should be within limits for stars 2,3, and 3) should be different for (0,1) and (2,3) star pairs
            if (Math.Abs(stars[0].EquAngle - stars[1].EquAngle) > precesions.iterEquAngleDiff_ ||
                Math.Abs(stars[2].EquAngle - stars[3].EquAngle) > precesions.iterEquAngleDiff_ ||
                stars[0].EquAngle == stars[2].EquAngle)
            {
                throw new ApplicationException("bad data for 4-stars alignment");
            }
                
            // check equatorial angles: 1) should be same for stars 0 and 1, 2) should be same for stars 2 and 3, and 3) should be different for (0,1) and (2,3) star pairs
            //if (stars[0].EquAngle != stars[1].EquAngle || stars[2].EquAngle != stars[3].EquAngle || stars[0].EquAngle == stars[2].EquAngle)
            //    throw new ApplicationException("bad data for 4-stars alignment");

            equAxis_ = equAxis;

            for (int i = 0; i < stars.Length; ++i)
                if (stars[i] != null)
                    stars_[i] = (AlignStar)stars[i].Clone();

            rotationToStand_ = rotationToStand;
            altOffset_ = altOffset;
            equScopeDiff_ = stars_[2].EquAngle - stars_[0].EquAngle;
            equHorzDiff_ = equHorzDiff;
            quality_ = quality;
            precesions_ = precesions;
            iterationCnt_ = iterationCnt;
            alignmentDone_ = alignmentDone;
        }
        public override Object Clone()
        {
            return new FourStarsAlignment(equAxis_, stars_, rotationToStand_, altOffset_, equHorzDiff_, quality_, precesions_, iterationCnt_, alignmentDone_);
        }
    }
}
