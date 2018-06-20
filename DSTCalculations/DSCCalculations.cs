using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Xml.Serialization;

namespace DSCCalculations
{
    public abstract class TypeConverter<T> : TypeConverter where T : new()
    {
        public override bool CanConvertFrom(ITypeDescriptorContext ctxt, Type srcType)
        { return (srcType == typeof(string)) || base.CanConvertFrom(ctxt, srcType); }
        public override bool CanConvertTo(ITypeDescriptorContext ctxt, Type dstType)
        { return (dstType == typeof(string)) || base.CanConvertTo(ctxt, dstType); }

        public override object ConvertFrom(ITypeDescriptorContext ctxt, CultureInfo culture, object value)
        {
            if (value == null)
                return new T();

            string StringValue = value as string;
            if (StringValue != null)
                return (StringValue.Length <= 0) ? new T() : ParseFromString(StringValue);

            return base.ConvertFrom(ctxt, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext ctxt, CultureInfo culture, object value, Type dstType)
        {
            if (value != null)
                if (!(value is T))
                    throw new ArgumentException("Wrong Type", "value");

            // convert to a string
            if (dstType == typeof(string))
                return (value == null) ? String.Empty : PrintToString((T)value);

            return base.ConvertTo(ctxt, culture, value, dstType);
        }

        protected abstract T ParseFromString(string s);
        protected abstract string PrintToString(T obj);
    }

    public struct Const
    {
        public const double toRad = Math.PI / 180.0;
        public const double toDeg = 1.0 / toRad;
    }

    // azm-alt "vector"
    [TypeConverter(typeof(PairATypeConverter))]
    public struct PairA
    {
        [XmlAttribute]
        public double Azm;
        [XmlAttribute]
        public double Alt;

        public PairA(double azm, double alt)
        {
            //alt = Rev(alt + Math.PI / 2);
            //if (alt < 0)
            //{
            //    alt = -alt;
            //    azm += Math.PI;
            //}
            //Alt = alt - Math.PI / 2;
            Alt = alt;
            Azm = Rev(azm);
        }

        public PairA Offset(double azmOff, double altOff_)
        {
            return new PairA(Azm + azmOff, Alt + altOff_);
        }

        public bool Above(double height) { return Alt >= height; }
        public bool Below(double height) { return Alt < height; }

        public override string ToString()
        {
            return "{A = " + (Azm * Const.toDeg).ToString("F5") + ", h = " + (Alt * Const.toDeg).ToString("F5") + "}";
        }

        public static double Rev(double a)
        {
            a += Math.PI;
            return a - Math.Floor(a / (2 * Math.PI)) * (2 * Math.PI) - Math.PI;
        }
    }
    public class PairATypeConverter : TypeConverter<PairA>
    {
        protected override PairA ParseFromString(string s)
        {
            string[] parts = s.Split(',');
            if (parts.Length != 2)
                throw new ArgumentException("PairA must have 2 parts", "s");
            return new PairA(Convert.ToDouble(parts[0]), Convert.ToDouble(parts[1]));
        }
        protected override string PrintToString(PairA obj) { return String.Format("{0},{1}", obj.Azm, obj.Alt); }
    }

    // 3-vector
    [TypeConverter(typeof(Vect3TypeConverter))]
    public struct Vect3
    {
        private double x_, y_, z_;

        public Vect3(double x, double y, double z)
        {
            x_ = x;
            y_ = y;
            z_ = z;
        }
        public Vect3(double azm, double alt)
        {
            Polar2Cartesian(azm, alt, out x_, out y_, out z_);
        }
        public Vect3(PairA aa)
        {
            Polar2Cartesian(aa.Azm, aa.Alt, out x_, out y_, out z_);
        }

        [XmlAttribute]
        public double X { get { return x_; } set { x_ = value; } }
        [XmlAttribute]
        public double Y { get { return y_; } set { y_ = value; } }
        [XmlAttribute]
        public double Z { get { return z_; } set { z_ = value; } }

        public double Azm { get { return Cartesian2Azm(x_, y_, z_); } }
        public double Alt { get { return Cartesian2Alt(x_, y_, z_); } }

        public static void Polar2Cartesian(double azm, double alt, out double x, out double y, out double z)
        {
            double cos_alt = Math.Cos(alt);
            x = cos_alt * Math.Cos(azm);
            y = -cos_alt * Math.Sin(azm);
            z = Math.Sin(alt);
        }

        public static double Cartesian2Azm(double x, double y, double z) { return Math.Atan2(-y, x); }
        public static double Cartesian2Alt(double x, double y, double z) { return Math.Atan2(z, Math.Sqrt(x * x + y * y)); }

        public static void Cartesian2Polar(double x, double y, double z, out double azm, out double alt)
        {
            azm = Cartesian2Azm(x, y, z);
            alt = Cartesian2Alt(x, y, z);
        }

        public double Abs { get { return Math.Sqrt(x_ * x_ + y_ * y_ + z_ * z_); } }

        public bool Normalize()
        {
            double abs = Abs;
            if (abs == 1)
                return true;
            else if (abs > 0)
            {
                x_ /= abs;
                y_ /= abs;
                z_ /= abs;
                return true;
            }
            else
                return false;
        }

        public bool Above(double height) { return Alt >= height; }
        public bool Below(double height) { return Alt < height; }

        public static explicit operator PairA(Vect3 v)
        {
            return new PairA(Cartesian2Azm(v.x_, v.y_, v.z_), Cartesian2Alt(v.x_, v.y_, v.z_));
        }

        public static Vect3 operator -(Vect3 v)             { return new Vect3(-v.x_, -v.y_, -v.z_); }
        public static Vect3 operator +(Vect3 v0, Vect3 v1)  { return new Vect3(v0.x_ + v1.x_, v0.y_ + v1.y_, v0.z_ + v1.z_); }
        public static Vect3 operator -(Vect3 v0, Vect3 v1)  { return v0 + (-v1); }

        public static Vect3 operator *(double a, Vect3 v)   { return new Vect3(a * v.x_, a * v.y_, a * v.z_); }
        public static Vect3 operator *(Vect3 v, double a)   { return a * v; }
        public static Vect3 operator /(Vect3 v, double a)   { return (1 / a) * v; }

        public static double SMul(Vect3 v0, Vect3 v1)       { return v0.x_ * v1.x_ + v0.y_ * v1.y_ + v0.z_ * v1.z_; }
        public static Vect3 VMul(Vect3 v0, Vect3 v1)
        {
            return new Vect3(v0.y_ * v1.z_ - v0.z_ * v1.y_,
                             v0.z_ * v1.x_ - v0.x_ * v1.z_,
                             v0.x_ * v1.y_ - v0.y_ * v1.x_);
        }

        public override string ToString()
        {
            return "{A = " + (Azm * Const.toDeg).ToString("F5") + ", h = " + (Alt * Const.toDeg).ToString("F5") + "}";
        }
    }
    public class Vect3TypeConverter : TypeConverter<Vect3>
    {
        protected override Vect3 ParseFromString(string s)
        {
            string[] parts = s.Split(',');
            switch (parts.Length)
            {
                case 2: return new Vect3(Convert.ToDouble(parts[0]), Convert.ToDouble(parts[1]));
                case 3: return new Vect3(Convert.ToDouble(parts[0]), Convert.ToDouble(parts[1]), Convert.ToDouble(parts[2]));
                default:
                    throw new ArgumentException("Vect3 must have 3 parts", "s");
            }
        }
        protected override string PrintToString(Vect3 obj)
        {
            return obj.Abs == 1 ? String.Format("{0},{1}", obj.Azm, obj.Alt) : String.Format("{0},{1},{2}", obj.X, obj.Y, obj.Z);
        }
    }
    
    // quaternion representing 3D rotations
    public struct Rotation3
    {
        private double a_, b_, c_, d_;
        public Rotation3(double a, double b, double c, double d)
        {
            a_ = a;
            b_ = b;
            c_ = c;
            d_ = d;
        }

        // 3D rotation
        public Rotation3(double angle, Vect3 axis)
        {
            angle /= 2;
            double sin = Math.Sin(angle);
            a_ = Math.Cos(angle);
            b_ = axis.X * sin;
            c_ = axis.Y * sin;
            d_ = axis.Z * sin;
        }

        public double A { get { return a_; } }
        public double B { get { return b_; } }
        public double C { get { return c_; } }
        public double D { get { return d_; } }

        public Vect3 Axis
        {
            get
            {
                Vect3 v = Vect;
                v.Normalize();
                return (v.Z >= 0) ? v : -v;
            }
        }
        public double Angle
        {
            get
            {
                Vect3 v = Vect;
                double abs = v.Abs;
                return 2 * Math.Atan2(v.Z >= 0 ? abs : -abs, a_);
            }
        }


        public Vect3 Vect { get { return new Vect3(b_, c_, d_); } }

        public Rotation3 Conj { get { return new Rotation3(a_, -b_, -c_, -d_); } }

        public double Abs { get { return Math.Sqrt(a_ * a_ + b_ * b_ + c_ * c_ + d_ * d_); } }

        public bool Normalize()
        {
            double abs = Abs;
            if (abs == 1)
                return true;
            else if (abs > 0)
            {
                a_ /= abs;
                b_ /= abs;
                c_ /= abs;
                d_ /= abs;
                return true;
            }
            else
                return false;
        }

        public Vect3 Apply(Vect3 v)
        {
            return (this * v * Conj).Vect;
        }

        public PairA Apply(PairA v)
        {
            return (PairA)(this * new Vect3(v) * Conj).Vect;
        }

        public static Rotation3 operator *(Rotation3 x0, Rotation3 x1)
        {
            return new Rotation3(x0.a_ * x1.a_ - x0.b_ * x1.b_ - x0.c_ * x1.c_ - x0.d_ * x1.d_,
                                 x0.b_ * x1.a_ + x0.a_ * x1.b_ + x0.c_ * x1.d_ - x0.d_ * x1.c_,
                                 x0.a_ * x1.c_ - x0.b_ * x1.d_ + x0.c_ * x1.a_ + x0.d_ * x1.b_,
                                 x0.a_ * x1.d_ + x0.b_ * x1.c_ - x0.c_ * x1.b_ + x0.d_ * x1.a_);
        }
        public static Rotation3 operator *(Rotation3 x, Vect3 v)
        {
            return x * new Rotation3(0, v.X, v.Y, v.Z);
        }
        public static Rotation3 operator *(Vect3 v, Rotation3 x)
        {
            return new Rotation3(0, v.X, v.Y, v.Z) * x;
        }
        public override string ToString()
        {
            string s = "{";
            s += A.ToString("F5") + ",";
            s += B.ToString("F5") + ",";
            s += C.ToString("F5") + ",";
            s += D.ToString("F5") + "},";
            s += " Axis=" + Axis.ToString();
            s += " Angle=" + (Angle*Const.toDeg).ToString("F5");
            return s;
        }
    }

    public struct Precisions
    {
        public double rotation_;
        public double axis_;
        public double iterCnt_;
        public double iterEquAxisDiff_;
        public double iterEquAngleDiff_;

        public static Precisions Default { get { return new Precisions(0.0002, 0.01, 25, Const.toRad / 240, 1.5 * Const.toRad); } }

        public Precisions(double rotation, double axis, double iterCnt, double iterEquAxisDiff, double iterEquAngleDiff)
        {
            rotation_ = rotation;
            axis_ = axis;
            iterCnt_ = iterCnt;
            iterEquAxisDiff_ = iterEquAxisDiff;
            iterEquAngleDiff_ = iterEquAngleDiff;
        }
    }

    // alignment object
    public class AlignStar : ICloneable
    {
        private string name_;
        private Vect3 horz_;
        private PairA scope_;
        private double equAngle_;

        [XmlElement]
        public string Name { get { return name_; } set { name_ = value; } }
        [XmlElement]
        public Vect3 Horz { get { return horz_; } set { horz_ = value; } }
        [XmlElement]
        public PairA Scope { get { return scope_; } set { scope_ = value; } }
        [XmlElement]
        public double EquAngle { get { return equAngle_; } set { equAngle_ = value; } }

        public AlignStar() { }
        public AlignStar(string name, Vect3 horz, PairA scope, double equAngle)
        {
            name_ = name;
            horz_ = horz;
            scope_ = scope;
            equAngle_ = equAngle;
        }
        public Object Clone() { return new AlignStar(name_, horz_, scope_, equAngle_); }
    }

    // alignment interface
    public abstract class Alignment : ICloneable
    {
        public abstract Object Clone();
        public abstract PairA Scope2Horz(PairA scope, double equAngle);
        public abstract PairA Horz2Scope(PairA horz, double equAngle);
        public abstract Alignment AddStar(AlignStar newStar);
        public abstract bool CorrectOffsets(AlignStar star);
        public abstract void ForceAlignment();
        public abstract bool IsAligned { get; }
        public abstract AlignStar[] Stars { get; }
        public abstract Vect3 EquAxis { get; }
        public abstract double EquAngleFactor { get; }
        public abstract Alignment CorrectEquAxis(Vect3 newEquAxis);
        public abstract string ToString(bool verbose);
    }

    // generic Alignment
    public class DSCAlignment : Alignment
    {
        private Vect3           equAxis_;
        private Alignment       currAlignment_;
        private Precisions      precesions_;

        public DSCAlignment(Vect3 equAxis, Precisions precesions)
        {
            equAxis_ = equAxis;
            precesions_ = precesions;
            currAlignment_ = new OneStarAlignment(equAxis_, precesions_);
        }

        public override PairA Scope2Horz(PairA scope, double equAngle)  { return currAlignment_.Scope2Horz(scope, equAngle); }
        public override PairA Horz2Scope(PairA horz, double equAngle)   { return currAlignment_.Horz2Scope(horz, equAngle); }

        // always returns itself
        public override Alignment AddStar(AlignStar star)
        {
            currAlignment_ = currAlignment_.AddStar(star);
            return this;
        }

        public override bool CorrectOffsets(AlignStar star) { return currAlignment_.CorrectOffsets(star); }
        public override void ForceAlignment() { currAlignment_.ForceAlignment(); }
        public override bool IsAligned { get { return currAlignment_.IsAligned; } }
        public override AlignStar[] Stars { get { return currAlignment_.Stars; } }
        public override Vect3 EquAxis { get { return currAlignment_.EquAxis; } }
        public override double EquAngleFactor { get { return currAlignment_.EquAngleFactor; } }
        public override Alignment CorrectEquAxis(Vect3 newEquAxis)
        {
            currAlignment_ = currAlignment_.CorrectEquAxis(newEquAxis);
            return this;
        }
        
        public override string ToString() { return currAlignment_.ToString(); }
        public override string ToString(bool verbose) { return currAlignment_.ToString(verbose); }

        // deep cloning
        private DSCAlignment(Vect3 equAxis, Alignment currAlignment, Precisions precesions)
        {
            equAxis_ = equAxis;
            currAlignment_ = (Alignment)currAlignment.Clone();
            precesions_ = precesions;
        }
        public override Object Clone() { return new DSCAlignment(equAxis_, currAlignment_, precesions_); }
    }
}