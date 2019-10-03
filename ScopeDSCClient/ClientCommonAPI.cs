﻿using System;
using System.Windows.Forms;
using System.Drawing;
using System.Data;
using SkyObjectPosition;
using DSCCalculations;

namespace ScopeDSCClient
{
    public struct ClientCommonAPI
    {
        public interface IScopePositions
        {
            double AzmAngle { get; }
            double AltAngle { get; }
            double EquAngle { get; }
        }
        
        // object databases
        public struct ObjDatabaseEntry
        {
            public string name_;
            public SkyObjectPosCalc.SkyPosition[] objects_;
        };

        // timeout class
        public class Timeout
        {
            private DateTime start_;
            private int timeoutInMS_;

            public Timeout(int timeoutInMS)
            {
                timeoutInMS_ = timeoutInMS;
                Restart();
            }

            public void Restart() { start_ = DateTime.Now; }

            public bool CheckExpired() { return CheckExpired(true); }
            public bool CheckExpired(bool restart)
            {
                DateTime now = DateTime.Now;
                if ((now - start_).TotalMilliseconds < timeoutInMS_)
                    return false;
                if (restart)
                    start_ = now;
                return true;
            }
        }

        public class StellariumObject : SkyObjectPosCalc.SkyPosition
        {
            public double Dec { get; set; }
            public double Ra { get; set; }
            public bool Connected { get; set; }

            public StellariumObject() { Dec = Ra = 0; Connected = false; }
            public override string Name { get { return Connected ? "Stellarium" : "Disconnected"; } }
            public override void CalcEquatorial(double d, out double rg, out double dec, out double ra)
            {
                rg = 1;
                dec = Dec;
                ra = Ra;
            }
        }

        public static void EnterNightMode(Control control)
        {
            EnumControls(control, SetNightModeOnFn, 0);
        }

        public static void ExitNightMode(Control control)
        {
            EnumControls(control, SetNightModeOffFn, 0);
        }

        public static double CalcTime()
        {
            return CalcTime(DateTime.UtcNow);
        }

        public static double CalcTime(DateTime timeUtc)
        {
            return SkyObjectPosCalc.CalcTime(timeUtc.Year, timeUtc.Month, timeUtc.Day, timeUtc.Hour, timeUtc.Minute, timeUtc.Second, timeUtc.Millisecond);
        }

        public static string PrintAngle(double a)
        {
            return PrintAngle(a, false);
        }
        public static string PrintAngle(double a, bool fSign)
        {
            return PrintAngle(a, fSign, true);
        }
        public static string PrintAngle(double a, bool fSign, bool fAddSeconds)
        {
            bool pos = (a >= 0);
            a = Math.Abs(a);
            double a_deg = Math.Floor(a);
            double min = (a - a_deg) * 60;
            if (!fAddSeconds)
            {
                min = Math.Round(min);
                if (min == 60)
                {
                    min = 0;
                    a_deg += 1;
                }
                return (pos ? (fSign ? "+" : "") : "-") + a_deg.ToString("F0") + "\x00B0" + min.ToString("F0") + "'";
            }
            else
            {
                double a_min = Math.Floor(min), sec = (min - a_min) * 60;
                sec = Math.Round(sec * 10) / 10;
                if (sec == 60)
                {
                    sec = 0;
                    a_min += 1;
                    if (a_min == 60)
                    {
                        a_min = 0;
                        a_deg += 1;
                    }
                }
                return (pos ? (fSign ? "+" : "") : "-") + a_deg.ToString("F0") + "\x00B0" + a_min.ToString("F0") + "'" + sec.ToString("F1") + "\"";
            }
        }
        public static string PrintTime(double a)
        {
            bool pos = (a >= 0);
            a = Math.Abs(a);

            a /= 15.0;

            double a_deg = Math.Floor(a);
            double min = (a - a_deg) * 60, a_min = Math.Floor(min);
            double sec = (min - a_min) * 60;
            return (pos ? "" : "-") + a_deg.ToString("F0") + "h " + a_min.ToString("F0") + "min " + sec.ToString("F1") + "sec";
        }
        public static string PrintDec(double a, string fmt)
        {
            bool pos = (a > 0);
            a = Math.Abs(a);
            return (pos ? "+" : (a == 0) ? "" : "-") + a.ToString(fmt);
        }
        public static string PrintDec(double a)
        {
            bool pos = (a > 0);
            a = Math.Abs(a);
            return (pos ? "+" : (a == 0) ? "" : "-") + a.ToString();
        }

        public static bool ParseSignedValue(string text, out double val)
        {
            val = Convert.ToDouble(text);
            if (val > 0)
                return true;
            else if (val < 0)
                return false;
            else // val == 0
                return (text[0] != '-');
        }

        public static string PrintAzmAltDifference(double diffAzmDeg, double diffAltDeg, bool oppositeAzmPositioningDir)
        {
            string s = "";

            if (oppositeAzmPositioningDir)
            {
                if (diffAzmDeg > 0)
                    s += "\u25c4";
                else if (diffAzmDeg < 0)
                    s += "\u25ba";
            }
            else
            {
                if (diffAzmDeg > 0)
                    s += "\u25ba";
                else if (diffAzmDeg < 0)
                    s += "\u25c4";
            }
            s += ClientCommonAPI.PrintAngle(diffAzmDeg, true, false) + ", ";

            if (diffAltDeg > 0)
                s += "\u25b2";
            else if (diffAltDeg < 0)
                s += "\u25bc";
            s += ClientCommonAPI.PrintAngle(diffAltDeg, true, false);

            return s;
        }

        public static bool IsEquAxisCorrectionNeeded(double latitude, Alignment alignment)
        {
            if (alignment == null)
                return false;
            Vect3 equAxis = alignment.EquAxis;
            return (Math.Abs(-equAxis.Azm) > 0.0003 || Math.Abs(latitude * Const.toRad - equAxis.Alt) > 0.0003);
        }

        public static string AddEquAxisCorrectionText(double latitude, Alignment alignment)
        {
            if (alignment == null)
                return "";
            Vect3 equAxis = alignment.EquAxis;
            return "Polar Axis Correction Needed: " + PrintAzmAltDifference(-equAxis.Azm * Const.toDeg, latitude - equAxis.Alt * Const.toDeg, false);
        }


        //////////////////////////////////////////////////////////
        // enumerate controls
        private delegate void ControlFn(Control control, int cnt);
        private static int EnumControls(Control control, ControlFn fn, int cnt)
        {
            if (fn != null)
                fn(control, cnt);
            cnt++;
            for (int i = control.Controls.Count; --i >= 0; )
                cnt = EnumControls(control.Controls[i], fn, cnt);
            return cnt;
        }

        // set night mode on
        private static void SetNightModeOnFn(Control control, int cnt)
        {
            Button bt = control as Button;
            if (bt != null)
            {
                bt.ForeColor = Color.Red;
                bt.BackColor = Color.Black;
                bt.FlatStyle = FlatStyle.Flat;
                bt.FlatAppearance.BorderColor = Color.Red;
                bt.FlatAppearance.MouseDownBackColor = Color.Black;
                bt.FlatAppearance.MouseOverBackColor = Color.Black;
                return;
            }

            CheckBox cb = control as CheckBox;
            if (cb != null)
            {
                cb.ForeColor = Color.Red;
                cb.BackColor = Color.Black;
                cb.FlatStyle = FlatStyle.Flat;
                cb.FlatAppearance.BorderColor = Color.Red;
                cb.FlatAppearance.CheckedBackColor = Color.FromArgb(80, 0, 0);
                cb.FlatAppearance.MouseDownBackColor = Color.Black;
                cb.FlatAppearance.MouseOverBackColor = Color.Black;
                return;
            }

            control.ForeColor = Color.Red;
            control.BackColor = Color.Black;
        }
        private static void SetNightModeOffFn(Control control, int cnt)
        {
            TextBox tb = control as TextBox;
            if (tb != null)
            {
                tb.ForeColor = Color.FromKnownColor(System.Drawing.KnownColor.WindowText);
                tb.BackColor = Color.FromKnownColor(System.Drawing.KnownColor.Control);
                return;
            }
            Button bt = control as Button;
            if (bt != null)
            {
                bt.ForeColor = Color.FromKnownColor(System.Drawing.KnownColor.ControlText);
                bt.BackColor = Color.FromKnownColor(System.Drawing.KnownColor.Control);
                bt.FlatStyle = FlatStyle.Standard;
                return;
            }
            CheckBox cb = control as CheckBox;
            if (cb != null)
            {
                //cb.ForeColor = Color.FromKnownColor(System.Drawing.KnownColor.ControlText);
                //cb.BackColor = Color.FromKnownColor(System.Drawing.KnownColor.Control);
                cb.FlatStyle = FlatStyle.System;
                return;
            }

            control.ForeColor = Color.FromKnownColor(System.Drawing.KnownColor.ControlText);
            control.BackColor = Color.FromKnownColor(System.Drawing.KnownColor.Control);
        }
    }
}
