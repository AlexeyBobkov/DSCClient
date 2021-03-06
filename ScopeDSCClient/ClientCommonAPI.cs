﻿#define ADD_MY_FAVORITE_LOCATIONS

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Data;
using SkyObjectPosition;
using DSCCalculations;

namespace ScopeDSCClient
{
    public struct ClientCommonAPI
    {
        public enum AutoTrack
        {
            ON,
            OFF,
            DISABLED
        }

        public enum LoggingState
        {
            ON,
            OFF,
            DISABLED
        }
        public enum LoggingChannel
        {
            ALT,
            AZM,
            UNUSED
        }
        public enum LoggingType
        {
            M_POS = 0,  // motor physical position
            M_LOG = 1,  // motor logical position
            M_SPD = 2,  // motor speed
            M_ERR = 3,  // motor error
            A_POS = 4,  // adapter physical position
            A_LOG = 5,  // adapter logical position
            A_SPD = 6,  // adapter speed
            A_ERR = 7,  // adapter error
            UNUSED
        }

        public interface IClientHost
        {
            // positions
            double AzmAngle { get; }
            double AltAngle { get; }
            double EquAngle { get; }

            // most common settings and options
            bool NightMode { get; }
            double Latitude { get; }
            double Longitude { get; }

            // host-specific configuration
            string GetConfigurationName { get; }
            void CallConfiguration();
        }
        
        // object databases
        public struct ObjDatabaseEntry
        {
            public string name_;
            public SkyObjectPosCalc.SkyPosition[] objects_;
        };

        // physical location
        public struct PhysicalLocation
        {
            public string name_;
            public double latitude_, longitude_;
            public PhysicalLocation(string name, double latitude, double longitude)
            {
                name_ = name;
                latitude_ = latitude;
                longitude_ = longitude;
            }
        }

        // timeout class
        public class Timeout
        {
            private DateTime start_;
            private int timeoutInMS_;
            private int timeoutOnceInMS_ = 0;

            public Timeout(int timeoutInMS)
            {
                timeoutInMS_ = timeoutInMS;
                Restart();
            }

            public void Restart() { timeoutOnceInMS_ = 0; start_ = DateTime.Now; }
            public void Restart(int timeoutInMS) { timeoutInMS_ = timeoutInMS; Restart(); }
            public void RestartOnce(int timeoutInMS) { timeoutOnceInMS_ = timeoutInMS; start_ = DateTime.Now; }

            public bool CheckExpired() { return CheckExpired(true); }
            public bool CheckExpired(bool restart)
            {
                DateTime now = DateTime.Now;
                if (timeoutOnceInMS_ > 0)
                {
                    if ((now - start_).TotalMilliseconds < timeoutOnceInMS_)
                        return false;
                    timeoutOnceInMS_ = 0;
                }
                else
                {
                    if ((now - start_).TotalMilliseconds < timeoutInMS_)
                        return false;
                }
                if (restart)
                    start_ = now;
                return true;
            }
        }

        public struct AlignmentConnectionData
        {
            private string portName_;
            private uint sessionId_;

            public string PortName { get { return portName_; } set { portName_ = value; } }
            public uint SessionId { get { return sessionId_; } set { sessionId_ = value; } }

            public AlignmentConnectionData(string portName, uint sessionId)
            {
                portName_ = portName;
                sessionId_ = sessionId;
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

        public static void LoadSkyObjectsFromFile(string path, ref string name, ref SkyObjectPosCalc.SkyPosition[] obj)
        {
            List<SkyObjectPosCalc.SkyPosition> objects = new List<SkyObjectPosCalc.SkyPosition>();
            try
            {
                using (System.IO.StreamReader sr = new System.IO.StreamReader(path))
                {
                    name = sr.ReadLine();
                    if (name == null)
                        return;

                    string type = sr.ReadLine();
                    if (type == null)
                        return;

                    switch (type.ToUpper())
                    {
                        case "N_RH_D":
                            {
                                string line;
                                while ((line = sr.ReadLine()) != null)
                                {
                                    string[] parts = line.Split(',');
                                    if (parts.Length < 3)
                                        throw new ApplicationException("Incorrect line: " + line);
                                    objects.Add(new SkyObjectPosCalc.StarPosition(parts[0], Convert.ToDouble(parts[1]), Convert.ToDouble(parts[2])));
                                }
                                break;
                            }

                        case "N_RH_RM_DD_DM":
                            {
                                string line;
                                while ((line = sr.ReadLine()) != null)
                                {
                                    string[] parts = line.Split(',');
                                    if (parts.Length < 5)
                                        throw new ApplicationException("Incorrect line: " + line);

                                    double ra;
                                    bool positive = ParseSignedValue(parts[1], out ra);
                                    if (parts[2].Length > 0)
                                    {
                                        if (positive)
                                            ra += Convert.ToDouble(parts[2]) / 60;
                                        else
                                            ra -= Convert.ToDouble(parts[2]) / 60;
                                    }

                                    double dec;
                                    positive = ParseSignedValue(parts[3], out dec);
                                    if (parts[4].Length > 0)
                                    {
                                        if (positive)
                                            dec += Convert.ToDouble(parts[4]) / 60;
                                        else
                                            dec -= Convert.ToDouble(parts[4]) / 60;
                                    }

                                    objects.Add(new SkyObjectPosCalc.StarPosition(parts[0], ra, dec));
                                }
                                break;
                            }

                        default:
                            throw new Exception("Unknown format: " + type);
                    }
                }
            }
            catch (Exception)
            {
            }
            obj = objects.Count > 0 ? objects.ToArray() : null;
        }

        public static void AddToObjDatabase(string path, ref List<ObjDatabaseEntry> database)
        {
            SkyObjectPosCalc.SkyPosition[] objects = null;
            string name = "Unknown";
            LoadSkyObjectsFromFile(path, ref name, ref objects);
            if (objects != null && objects.Length > 0)
                database.Add(new ClientCommonAPI.ObjDatabaseEntry() { name_ = name, objects_ = objects });
        }

        public static void BuildObjectDatabase(ref List<ObjDatabaseEntry> database)
        {
            database.Add(new ClientCommonAPI.ObjDatabaseEntry() { name_ = "Solar System Object", objects_ = SkyObjectPosCalc.sunSystemObjects });
            database.Add(new ClientCommonAPI.ObjDatabaseEntry() { name_ = "Star", objects_ = SkyObjectPosCalc.stars });
            database.Add(new ClientCommonAPI.ObjDatabaseEntry() { name_ = "Messier Object", objects_ = SkyObjectPosCalc.messier });

            string startupPath = Application.StartupPath + @"\";
            AddToObjDatabase(startupPath + "Objects0.csv", ref database);
            AddToObjDatabase(startupPath + "Objects1.csv", ref database);
            AddToObjDatabase(startupPath + "Objects2.csv", ref database);
            AddToObjDatabase(startupPath + "Objects3.csv", ref database);
            AddToObjDatabase(startupPath + "Objects4.csv", ref database);
            AddToObjDatabase(startupPath + "Objects5.csv", ref database);
            AddToObjDatabase(startupPath + "Objects6.csv", ref database);
            AddToObjDatabase(startupPath + "Objects7.csv", ref database);
            AddToObjDatabase(startupPath + "Objects8.csv", ref database);
            AddToObjDatabase(startupPath + "Objects9.csv", ref database);
        }

        public static void AddLocationsFromFile(string path, ref List<PhysicalLocation> locations)
        {
            try
            {
                using (System.IO.StreamReader sr = new System.IO.StreamReader(path))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] parts = line.Split(',');
                        if (parts.Length < 3)
                            throw new ApplicationException("Incorrect line: " + line);
                        locations.Add(new PhysicalLocation(parts[0], Convert.ToDouble(parts[1]), Convert.ToDouble(parts[2])));
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public static void BuildLocationDatabase(ref List<PhysicalLocation> locations)
        {
#if ADD_MY_FAVORITE_LOCATIONS
            locations.Add(new ClientCommonAPI.PhysicalLocation("San Jose Houge Park", 37.257521, -121.942354));
            locations.Add(new ClientCommonAPI.PhysicalLocation("Adin CA, Frosty Acres", 41.135511, -120.978589));
            locations.Add(new ClientCommonAPI.PhysicalLocation("Fremont Peak SP CA", 36.759363, -121.503721));
#endif

            string startupPath = Application.StartupPath + @"\";
            AddLocationsFromFile(startupPath + "Locations0.csv", ref locations);
            AddLocationsFromFile(startupPath + "Locations1.csv", ref locations);
            AddLocationsFromFile(startupPath + "Locations2.csv", ref locations);
            AddLocationsFromFile(startupPath + "Locations3.csv", ref locations);
            AddLocationsFromFile(startupPath + "Locations4.csv", ref locations);
            AddLocationsFromFile(startupPath + "Locations5.csv", ref locations);
            AddLocationsFromFile(startupPath + "Locations6.csv", ref locations);
            AddLocationsFromFile(startupPath + "Locations7.csv", ref locations);
            AddLocationsFromFile(startupPath + "Locations8.csv", ref locations);
            AddLocationsFromFile(startupPath + "Locations9.csv", ref locations);
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
            s += PrintAngle(diffAzmDeg, true, false) + ", ";

            if (diffAltDeg > 0)
                s += "\u25b2";
            else if (diffAltDeg < 0)
                s += "\u25bc";
            s += PrintAngle(diffAltDeg, true, false);

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
