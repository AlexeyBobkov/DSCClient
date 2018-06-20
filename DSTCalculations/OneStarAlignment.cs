using System;

namespace DSCCalculations
{
    // one star alignment
    public class OneStarAlignment : Alignment
    {
        private Vect3 equAxis_;
        private double azmOffset_;
        private double altOffset_;
        private AlignStar star_;
        private Precisions precesions_;

        private void MakeAlignment(AlignStar star)
        {
            Vect3 v = (new Rotation3(star.EquAngle, equAxis_)).Apply(star.Horz);
            azmOffset_ = star.Scope.Azm - v.Azm;
            altOffset_ = star.Scope.Alt - v.Alt;
            star_ = star;
        }

        public OneStarAlignment(Vect3 equAxis, Precisions precesions)
        {
            equAxis_ = equAxis;
            precesions_ = precesions;
        }

        public OneStarAlignment(Vect3 equAxis, Precisions precesions, AlignStar star)
        {
            equAxis_ = equAxis;
            precesions_ = precesions;
            MakeAlignment((AlignStar)star.Clone());
        }

        public override PairA Scope2Horz(PairA scope, double equAngle)
        {
            return (new Rotation3(-equAngle, equAxis_)).Apply(scope.Offset(-azmOffset_, -altOffset_));
        }
        public override PairA Horz2Scope(PairA horz, double equAngle)
        {
            return (new Rotation3(equAngle, equAxis_)).Apply(horz).Offset(+azmOffset_, +altOffset_);
        }
        public override Alignment AddStar(AlignStar newStar)
        {
            if (newStar == null)
                return this;

            if (star_ != null && Math.Abs(newStar.EquAngle - star_.EquAngle) <= precesions_.iterEquAngleDiff_)
                return new TwoStarsAlignment(equAxis_, precesions_, star_, newStar);
            else
            {
                MakeAlignment(newStar);
                return this;
            }
        }
        public override bool CorrectOffsets(AlignStar star)
        {
            if (star == null || star_ == null)
                return false;
            PairA scopeOld = Horz2Scope((PairA)star.Horz, star.EquAngle);
            double deltaAlt = star.Scope.Alt - scopeOld.Alt;
            double deltaAzm = star.Scope.Azm - scopeOld.Azm;
            altOffset_ += deltaAlt;
            azmOffset_ += deltaAzm;

            if (star_ != null)
                star_.Scope = star_.Scope.Offset(+deltaAzm, +deltaAlt);

            return true;
        }
        public override void ForceAlignment() { }
        public override bool IsAligned { get { return star_ != null; } }
        public override AlignStar[] Stars { get { return new AlignStar[] {star_}; } }
        public override Vect3 EquAxis { get { return equAxis_; } }
        public override double EquAngleFactor { get { return 1; } }
        public override Alignment CorrectEquAxis(Vect3 newEquAxis) { return this; }

        public override string ToString() {return ToString(false);}
        public override string ToString(bool verbose)
        {
            string s = "";
            if (star_ == null)
            {
                s += "No Alignment" + Environment.NewLine;
                s += "\tEquatorial Axis: " + equAxis_.ToString() + Environment.NewLine;
            }
            else
            {
                s += "OneStarAlignment" + Environment.NewLine;
                s += "\tObject: " + star_.Name + Environment.NewLine;
                s += "\tEquatorial Axis: " + equAxis_.ToString() + Environment.NewLine;
                if (verbose)
                {
                    s += "\tA Offset: " + (azmOffset_ * Const.toDeg).ToString("F5") + Environment.NewLine;
                    s += "\th Offset: " + (altOffset_ * Const.toDeg).ToString("F5") + Environment.NewLine;
                }
            }
            return s;
        }

        // deep cloning
        private OneStarAlignment(Vect3 equAxis, double azmOffset, double altOffset, AlignStar star, Precisions precesions)
        {
            equAxis_ = equAxis;
            azmOffset_ = azmOffset;
            altOffset_ = altOffset;
            if(star != null)
                star_ = (AlignStar)star.Clone();
            precesions_ = precesions;
        }
        public override Object Clone() { return new OneStarAlignment(equAxis_, azmOffset_, altOffset_, star_, precesions_); }
    }
}
