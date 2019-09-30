using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using SkyObjectPosition;
using DSCCalculations;
using AAB.UtilityLibrary;

namespace ScopeDSCClient
{
    public partial class ScopeGotoClient : Form
    {
        private ScopeGotoClientSettings settings_ = new ScopeGotoClientSettings();

        private bool fullScreen_ = false;
        private bool nightMode_ = false;
        private UInt16 timerCnt_;
        private bool swapAzmAltEncoders_ = false;
        private int azmRes_, altRes_;
        private int azmPos_, altPos_;
        private bool positionsValid_ = false;
        private int iInitWidth_ = 1200, iInitHeight_ = 675;
        private float positionFontSize_ = (float)96;
        private float objectFontSize_ = (float)36;
        private string portName_;
        private int baudRate_ = 115200;
        private double latitude_, longitude_;
        private bool showNearestAzmRotation_ = false;
        private bool connectToStellarium_ = false;
        private int stellariumTcpPort_ = 8001;
        private bool oppositeHorzPositioningDir_ = false;
        private SkyObjectForm.LastSettings lastObjSettings_ = SkyObjectForm.LastSettings.Default;
        private AlignmentForm.LastSettings lastAlignmentObjSettings_ = AlignmentForm.LastSettings.Default;

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

        // alignment data
        private DSCAlignment alignment_;
        private AlignmentConnectionData alignmentConnectionGoTo_;

        // state
        private const int LAST_OBJ_COUNT = 20;
        private SkyObjectPosCalc.SkyPosition selectedObject_;
        private SkyObjectPosCalc.SkyPosition trackedObject_;
        private double trackedOffsetRA_, trackedOffsetDec_;
        private bool switchOn_ = false;
        private bool allowAutoTrack_ = false;
        private SkyObjectPosCalc.SkyPosition[] lastSelectedObjects_ = new SkyObjectPosCalc.SkyPosition[LAST_OBJ_COUNT];

        // time sync
        private Int32 controllerTs_;
        private DateTime thisTs_;       // UTC time
        private ClientCommonAPI.Timeout tmoSendPos_ = new ClientCommonAPI.Timeout(3500);
        private int nextPosTimeSec_ = 4;
        private double arrowMoveSpeed_ = 1 / 30.0;  // degree

        private bool posTextChanged_ = true;
        private bool connectionAndAlignTextChanged_ = true;
        private bool scopePosAndObjTextChanged_ = true;
        private bool objectNameChanged_ = true;

        // connection capabilities flags
        private const byte CONNCAPS_GPS = 4;
        private const byte CONNCAPS_GOTO = 8;

        // state bits
        private const byte STATE_ALT_RUNNING = 1;
        private const byte STATE_AZM_RUNNING = 2;
        private const byte STATE_SWITCH_ON = 4;

        private const byte A_ALT = 0;   // command for alt adapter
        private const byte A_AZM = 1;   // command for azm adapter

        private class ConnectionData
        {
            public SerialConnection connection_;
            public bool swapAzmAltEncoders_;
            public uint sessionId_;

            public ConnectionData(SerialConnection connection, bool swapAzmAltEncoders) : this(connection, swapAzmAltEncoders, 0) {}

            public ConnectionData(SerialConnection connection, bool swapAzmAltEncoders, uint sessionId)
            {
                connection_ = connection;
                swapAzmAltEncoders_ = swapAzmAltEncoders;
                sessionId_ = sessionId;
            }
        }

        // dynamic array of all open connections
        private List<ConnectionData> connectionList_ = new List<ConnectionData>();

        // specific connections (everyone points to an object in the connection list)
        private ConnectionData connectionGoTo_;
        private ConnectionData connectionGPS_;

        // stellarium
        private StellariumServer.Connection stellariumConnection_;
        private class StellariumObject : SkyObjectPosCalc.SkyPosition
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
        private StellariumObject stellariumObj_ = new StellariumObject();

        // object databases
        List<ClientCommonAPI.ObjDatabaseEntry> database_ = new List<ClientCommonAPI.ObjDatabaseEntry>();

        private delegate void TimeoutDelegate(SerialConnection connection);
        private delegate void SetConnectionDelegate(ConnectionData data);
        private delegate void ReceiveDelegate(byte[] data);

        private class InitConnectionHandler : SerialConnection.IReceiveHandler
        {
            public InitConnectionHandler(ScopeGotoClient parent, ConnectionData connectionData)
            {
                parent_ = parent;
                connectionData_ = connectionData;
            }

            public void Error()
            {
                TimeoutDelegate d = new TimeoutDelegate(parent_.SerialError);
                parent_.BeginInvoke(d, new object[] { connectionData_.connection_ });
            }

            public void Received(byte[] data)
            {
                connectionData_.sessionId_ = data[1];

                if ((data[0] & CONNCAPS_GOTO) != 0)
                    parent_.BeginInvoke(new SetConnectionDelegate(parent_.SetGoToConnection), new object[] { connectionData_ });

                if ((data[0] & CONNCAPS_GPS) != 0)
                    parent_.BeginInvoke(new SetConnectionDelegate(parent_.SetGPSConnection), new object[] { connectionData_ });
            }

            private ScopeGotoClient parent_;
            private ConnectionData connectionData_;
        }

        private class BaseConnectionHandler : SerialConnection.IReceiveHandler
        {
            public BaseConnectionHandler(ScopeGotoClient parent, ReceiveDelegate receiveDelegate, TimeoutDelegate timeoutDelegate, SerialConnection connection)
            {
                parent_ = parent;
                receiveDelegate_ = receiveDelegate;
                timeoutDelegate_ = timeoutDelegate;
                connection_ = connection;
            }

            public BaseConnectionHandler(ScopeGotoClient parent, ReceiveDelegate receiveDelegate, SerialConnection connection)
                : this(parent, receiveDelegate, parent.SerialError, connection) {}

            public void Error()
            {
                parent_.BeginInvoke(timeoutDelegate_, new object[] { connection_ });
            }

            public void Received(byte[] data)
            {
                parent_.BeginInvoke(receiveDelegate_, new object[] {data});
            }

            private ScopeGotoClient parent_;
            private ReceiveDelegate receiveDelegate_;
            private TimeoutDelegate timeoutDelegate_;
            private SerialConnection connection_;
        }

        // enter/exit full screen
        private void EnterFullScreen()
        {
            this.WindowState = FormWindowState.Normal;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            buttonFullScreen.Text = "Exit &Full Screen";
            AdjustFontSize();
            fullScreen_ = true;
        }
        private void ExitFullScreen()
        {
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.WindowState = FormWindowState.Normal;
            buttonFullScreen.Text = "&Full Screen";
            AdjustFontSize();
            fullScreen_ = false;
        }

        public double AzmAngle
        {
            get { return (double)(azmPos_) * 2 * Math.PI / (azmRes_ != 0 ? (double)azmRes_ : 1); }
        }

        public double AzmAngleLimited
        {
            get
            {
                int res = azmRes_ > 0 ? azmRes_ : 1;
                return (double)(azmPos_ >= 0 ? (azmPos_ % res) : res - 1 - ((-azmPos_ - 1) % res)) * 2 * Math.PI / res;
            }
        }

        public double AltAngle
        {
            get { return (double)(altPos_) * 2 * Math.PI / (altRes_ != 0 ? (double)altRes_ : 1); }
        }

        private class ScopePositions : ClientCommonAPI.IScopePositions
        {
            public ScopePositions(ScopeGotoClient parent) { parent_ = parent; }
            public double AzmAngle { get { return parent_.AzmAngleLimited; } }
            public double AltAngle { get { return parent_.AltAngle; } }
            public double EquAngle { get { return 0; } }

            private ScopeGotoClient parent_;
        }
        
        private void SetPositionText()
        {
            if (!posTextChanged_)
                return;
            posTextChanged_ = false;
            
            string s = "";
            if (connectionGoTo_ == null || alignment_ == null || !alignment_.IsAligned || selectedObject_ == null)
            {
                s += "--, --";
            }
            else
            {
                double d = ClientCommonAPI.CalcTime();
                double azm, alt;
                selectedObject_.CalcAzimuthal(d, latitude_, longitude_, out azm, out alt);
                PairA objScope = alignment_.Horz2Scope(new PairA(azm * Const.toRad, alt * Const.toRad), 0);

                if (!showNearestAzmRotation_)
                    s += ClientCommonAPI.PrintAzmAltDifference(SkyObjectPosCalc.Rev(objScope.Azm * Const.toDeg) - AzmAngle * Const.toDeg, (objScope.Alt - AltAngle) * Const.toDeg, oppositeHorzPositioningDir_);
                else
                {
                    double azmd = SkyObjectPosCalc.Rev(objScope.Azm * Const.toDeg - AzmAngle * Const.toDeg);
                    if (azmd > 180)
                        azmd -= 360;
                    s += ClientCommonAPI.PrintAzmAltDifference(azmd, (objScope.Alt - AltAngle) * Const.toDeg, oppositeHorzPositioningDir_);
                }
            }
            
            textBoxPosition.Text = s;
        }

        private void SetConnectionAndAlignmentText()
        {
            if (!connectionAndAlignTextChanged_)
                return;
            connectionAndAlignTextChanged_ = false;

            string s = "";

            if (connectionGoTo_ == null)
                s += "GoTo not Connected";
            else
                s += "GoTo Connected to " + connectionGoTo_.connection_.PortName + " at " + connectionGoTo_.connection_.BaudRate;
            s += Environment.NewLine;

            if (connectionGPS_ == null)
                s += "GPS not Connected";
            else
                s += "GPS Connected to " + connectionGPS_.connection_.PortName + " at " + connectionGPS_.connection_.BaudRate;
            s += Environment.NewLine;

            if (alignment_ == null)
                s += "Not Aligned";
            else
            {
                if (alignment_.IsAligned)
                    s += "Alignment valid";
                else
                    s += "Alignment not valid";
                s += Environment.NewLine;
                s += alignment_.ToString();
                if (ClientCommonAPI.IsEquAxisCorrectionNeeded(latitude_, alignment_))
                {
                    s += Environment.NewLine;
                    s += ClientCommonAPI.AddEquAxisCorrectionText(latitude_, alignment_);
                }
            }
            
            textBoxAlignment.Text = s;
        }

        private void SetScopePositionAndObjectText(bool sendPositionToStellarium)
        {
            if (!scopePosAndObjTextChanged_ && !sendPositionToStellarium)
                return;
            scopePosAndObjTextChanged_ = false;

            double d = ClientCommonAPI.CalcTime();

            string s = "";
            if (connectionGoTo_ == null)
                s += "Encoder Abs Positions Unknown" + Environment.NewLine;
            else
            {
                s += "Encoder Abs Positions: Azm = " + ClientCommonAPI.PrintAngle(AzmAngle * Const.toDeg, false, false) + ", Alt = " + ClientCommonAPI.PrintAngle(AltAngle * Const.toDeg, false, false) + Environment.NewLine;
            }
            if (!switchOn_)
                s += "TELESCOPE SWITCH OFF" + Environment.NewLine;

            s += Environment.NewLine;
            if (selectedObject_ == null)
                s += "No Object Selected" + Environment.NewLine;
            else
            {
                double azm, alt;
                selectedObject_.CalcAzimuthal(d, latitude_, longitude_, out azm, out alt);
                s += selectedObject_.Name + ": Azm = " + ClientCommonAPI.PrintAngle(azm, false, false);
                s += ", Alt = " + ClientCommonAPI.PrintAngle(alt, false, false) + Environment.NewLine;

                double dec, ra;
                selectedObject_.CalcTopoRaDec(d, latitude_, longitude_, out dec, out ra);
                s += "R.A.\t= " + ClientCommonAPI.PrintTime(ra) + " (" + ra.ToString("F5") + "\x00B0)" + Environment.NewLine;
                s += "Dec.\t= " + ClientCommonAPI.PrintAngle(dec, true) + " (" + ClientCommonAPI.PrintDec(dec, "F5") + "\x00B0)" + Environment.NewLine;
            }

            s += Environment.NewLine;
            if (trackedObject_ == null)
            {
                s += "No Object Tracked" + Environment.NewLine;
                s += Environment.NewLine;
                if (connectionGoTo_ == null)
                    s += "Scope Position Unknown" + Environment.NewLine;
                else if (alignment_ != null && alignment_.IsAligned)
                {
                    PairA horz = alignment_.Scope2Horz(new PairA(AzmAngle, AltAngle), 0);

                    s += "Scope Position: Azm = " + ClientCommonAPI.PrintAngle(SkyObjectPosCalc.Rev(horz.Azm * Const.toDeg), false, false);
                    s += ", Alt = " + ClientCommonAPI.PrintAngle(SkyObjectPosCalc.Rev(horz.Alt * Const.toDeg), false, false) + Environment.NewLine;

                    double dec, ra;
                    SkyObjectPosCalc.AzAlt2Equ(d, latitude_, longitude_, SkyObjectPosCalc.Rev(horz.Azm * Const.toDeg), SkyObjectPosCalc.Rev(horz.Alt * Const.toDeg), out dec, out ra);
                    s += "R.A.\t= " + ClientCommonAPI.PrintTime(ra) + " (" + ra.ToString("F5") + "\x00B0)" + Environment.NewLine;
                    s += "Dec.\t= " + ClientCommonAPI.PrintAngle(dec, true) + " (" + ClientCommonAPI.PrintDec(dec, "F5") + "\x00B0)" + Environment.NewLine;

                    if (sendPositionToStellarium && stellariumConnection_ != null && stellariumConnection_.IsConnected)
                        stellariumConnection_.SendPosition(dec, ra);
                }
            }
            else if (trackedObject_ != selectedObject_)
            {
                double azm, alt;
                trackedObject_.CalcAzimuthal(d, latitude_, longitude_, out azm, out alt);
                s += trackedObject_.Name + ": Azm = " + ClientCommonAPI.PrintAngle(azm, false, false);
                s += ", Alt = " + ClientCommonAPI.PrintAngle(alt, false, false) + Environment.NewLine;

                double dec, ra;
                trackedObject_.CalcTopoRaDec(d, latitude_, longitude_, out dec, out ra);
                s += "R.A.\t= " + ClientCommonAPI.PrintTime(ra) + " (" + ra.ToString("F5") + "\x00B0)" + Environment.NewLine;
                s += "Dec.\t= " + ClientCommonAPI.PrintAngle(dec, true) + " (" + ClientCommonAPI.PrintDec(dec, "F5") + "\x00B0)" + Environment.NewLine;
            }
            else
            {
                s += "Tracking: " + trackedObject_.Name + Environment.NewLine;
            }

            textBoxObject.Text = s;
        }

        private void SetObjectNameLabelText()
        {
            if (!objectNameChanged_)
                return;
            objectNameChanged_ = false;
            objectNameLabel.Text = (selectedObject_ != null) ? selectedObject_.Name : "";
        }

        private void UpdateUI()
        {
            UpdateUI(false);
        }
        private void UpdateUI(bool sendPositionToStellarium)
        {
            checkBoxTrackAuto.Enabled = true;

            if (connectionGoTo_ == null)
            {
                buttonAlign.Text = "Alignment (Connect to Scope First)";
                buttonAlign.Enabled = false;
            }
            else
            {
                buttonAlign.Text = "Alignment";
                buttonAlign.Enabled = true;
            }

            if (connectionGoTo_ == null || alignment_ == null || !switchOn_)
            {
                buttonTrackGoTo.Enabled = false;
                buttonStop.Enabled = false;
            }
            else
            {
                buttonTrackGoTo.Enabled = (selectedObject_ != null);
                buttonStop.Enabled = true;
                if (trackedObject_ != null)
                    buttonStop.Text = "\u25a0"; //"\u23F9";
                else
                    buttonStop.Text = "\u25B6";
            }

            // arrows
            if (trackedObject_ != null)
            {
                buttonTrackUp.Enabled = true;
                buttonTrackLeft.Enabled = true;
                buttonTrackDown.Enabled = true;
                buttonTrackRight.Enabled = true;
            }
            else
            {
                buttonTrackUp.Enabled = false;
                buttonTrackLeft.Enabled = false;
                buttonTrackDown.Enabled = false;
                buttonTrackRight.Enabled = false;
            }

            SetPositionText();
            SetConnectionAndAlignmentText();
            SetScopePositionAndObjectText(sendPositionToStellarium);
            SetObjectNameLabelText();
        }

        private void OptionsOrTimeChanged()
        {
            posTextChanged_ = true;
            connectionAndAlignTextChanged_ = true;
            scopePosAndObjTextChanged_ = true;

            alignment_ = null;
            SaveAlignment();
            AlignmentChanged();
        }

        private void ConnectionChangedGoTo()
        {
            posTextChanged_ = true;
            connectionAndAlignTextChanged_ = true;
            scopePosAndObjTextChanged_ = true;

            if (alignment_ != null &&
                connectionGoTo_ != null &&
                connectionGoTo_.connection_ != null &&
                (connectionGoTo_.connection_.PortName != alignmentConnectionGoTo_.PortName ||
                 connectionGoTo_.sessionId_ != alignmentConnectionGoTo_.SessionId))
            {
                alignment_ = null;
                SaveAlignment();
                AlignmentChanged();
            }

            if (connectionGoTo_ == null)
            {
                if (switchOn_)
                {
                    switchOn_ = false;
                    SwitchChanged();
                }
                if (trackedObject_ != null)
                {
                    trackedObject_ = null;
                    TrackedObjectChanged();
                    // As there is no connection, it doesn't make sense to stop motors.
                }
            }
            allowAutoTrack_ = false;
        }

        private void ConnectionChangedGPS()
        {
            connectionAndAlignTextChanged_ = true;
        }

        private void AlignmentChanged()
        {
            posTextChanged_ = true;
            connectionAndAlignTextChanged_ = true;
            allowAutoTrack_ = false;
        }

        private void ObjectChanged()
        {
            posTextChanged_ = true;
            scopePosAndObjTextChanged_ = true;
            objectNameChanged_ = true;
        }

        private void ScopePosChanged()
        {
            posTextChanged_ = true;
            scopePosAndObjTextChanged_ = true;
        }

        private void SwitchChanged()
        {
            scopePosAndObjTextChanged_ = true;
        }

        private void TrackedObjectChanged()
        {
            scopePosAndObjTextChanged_ = true;
        }

        private void TimeChanged()
        {
            posTextChanged_ = true;
            scopePosAndObjTextChanged_ = true;
        }

        private void SaveAlignment()
        {
            using (settings_.Buffer())
            {
                if (alignment_ == null)
                    settings_.AlignmentStars = null;
                else
                {
                    settings_.AlignmentStars = alignment_.Stars;
                    settings_.AlignmentEquAxis = alignment_.EquAxis;
                    if (connectionGoTo_ != null && connectionGoTo_.connection_ != null)
                        settings_.AlignmentConnectionGoTo = alignmentConnectionGoTo_ = new AlignmentConnectionData(connectionGoTo_.connection_.PortName, connectionGoTo_.sessionId_);
                }
            }
        }

        private void LoadAlignment()
        {
            try
            {
                AlignStar[] stars = settings_.AlignmentStars;
                if (stars == null)
                    alignment_ = null;
                else
                {
                    alignment_ = new DSCAlignment(settings_.AlignmentEquAxis, Precisions.Default);
                    for (int i = 0; i < stars.Length; ++i)
                        alignment_.AddStar(stars[i]);

                    alignment_.ForceAlignment();

                    alignmentConnectionGoTo_ = settings_.AlignmentConnectionGoTo;
                }
            }
            catch (Exception)
            {
                alignment_ = null;
            }
            AlignmentChanged();
        }

        /// <summary>
        /// /////////////////////////////////////////////////
        /// </summary>
        public ScopeGotoClient()
        {
            InitializeComponent();
        }

        private static void LoadFromFile(string path, ref string name, ref SkyObjectPosCalc.SkyPosition[] obj)
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
                                    bool positive = ClientCommonAPI.ParseSignedValue(parts[1], out ra);
                                    if (parts[2].Length > 0)
                                    {
                                        if (positive)
                                            ra += Convert.ToDouble(parts[2]) / 60;
                                        else
                                            ra -= Convert.ToDouble(parts[2]) / 60;
                                    }

                                    double dec;
                                    positive = ClientCommonAPI.ParseSignedValue(parts[3], out dec);
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

        private void AddToDatabase(string path)
        {
            SkyObjectPosCalc.SkyPosition[] objects = null;
            string name = "Unknown";
            LoadFromFile(path, ref name, ref objects);
            if(objects != null && objects.Length > 0)
                database_.Add(new ClientCommonAPI.ObjDatabaseEntry() { name_ = name, objects_ = objects });
        }

        private void AddLastObject(SkyObjectPosCalc.SkyPosition obj)
        {
            for (int i = lastSelectedObjects_.Length; --i >= 0; )
            {
                if (lastSelectedObjects_[i] != null && obj.Name == lastSelectedObjects_[i].Name)
                {
                    // move to first position
                    lastSelectedObjects_[i] = lastSelectedObjects_[0];
                    lastSelectedObjects_[0] = obj;
                    return;
                }
            }
                
            for (int i = lastSelectedObjects_.Length; --i >= 1; )
                lastSelectedObjects_[i] = lastSelectedObjects_[i - 1];
            lastSelectedObjects_[0] = obj;
        }

        private void ScopeGotoClient_Load(object sender, EventArgs e)
        {
            iInitWidth_ = this.Width;
            iInitHeight_ = this.Height;
            positionFontSize_ = textBoxPosition.Font.Size;
            objectFontSize_ = objectNameLabel.Font.Size;

            latitude_ = settings_.Latitude;
            longitude_ = settings_.Longitude;
            showNearestAzmRotation_ = settings_.ShowNearestAzmRotation;
            connectToStellarium_ = settings_.ConnectToStellarium;
            stellariumTcpPort_ = settings_.TcpPort;
            oppositeHorzPositioningDir_ = settings_.OppositeHorzPositioningDir;

            if (connectToStellarium_)
                OpenStellariumConnection(stellariumTcpPort_);

            checkBoxTrackAuto.Checked = settings_.AutoTrack;

            // Everything is changed! (Yes, it's redundant.)
            //OptionsOrTimeChanged();
            ConnectionChangedGoTo();
            ConnectionChangedGPS();
            ObjectChanged();
            ScopePosChanged();
            TimeChanged();
            UpdateUI();

            LoadAlignment();

            // load standard objects
            database_.Add(new ClientCommonAPI.ObjDatabaseEntry() { name_ = "Solar System Object", objects_ = SkyObjectPosCalc.sunSystemObjects });
            database_.Add(new ClientCommonAPI.ObjDatabaseEntry() { name_ = "Star", objects_ = SkyObjectPosCalc.stars });
            database_.Add(new ClientCommonAPI.ObjDatabaseEntry() { name_ = "Messier Object", objects_ = SkyObjectPosCalc.messier });
            
            // load database files
            string startupPath = Application.StartupPath + @"\";
            AddToDatabase(startupPath + "Objects0.csv");
            AddToDatabase(startupPath + "Objects1.csv");
            AddToDatabase(startupPath + "Objects2.csv");
            AddToDatabase(startupPath + "Objects3.csv");
            AddToDatabase(startupPath + "Objects4.csv");
            AddToDatabase(startupPath + "Objects5.csv");
            AddToDatabase(startupPath + "Objects6.csv");
            AddToDatabase(startupPath + "Objects7.csv");
            AddToDatabase(startupPath + "Objects8.csv");
            AddToDatabase(startupPath + "Objects9.csv");
        }

        private void buttonNightMode_Click(object sender, EventArgs e)
        {
            if (nightMode_)
            {
                ClientCommonAPI.ExitNightMode(this);
                buttonNightMode.Text = "Night &Mode";
                nightMode_ = false;
            }
            else
            {
                ClientCommonAPI.EnterNightMode(this);
                buttonNightMode.Text = "Day &Mode";
                nightMode_ = true;
            }
        }

        private void buttonFullScreen_Click(object sender, EventArgs e)
        {
            if (fullScreen_)
                ExitFullScreen();
            else
                EnterFullScreen();
        }

        private void buttonAlign_Click(object sender, EventArgs e)
        {
            AlignmentForm form = new AlignmentForm(new ScopePositions(this), nightMode_, latitude_, longitude_, lastAlignmentObjSettings_, alignment_);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            alignment_ = form.Alignment;
            SaveAlignment();

            lastAlignmentObjSettings_ = form.Settings;
            AlignmentChanged();
            UpdateUI();
        }

        private void buttonSelectObject_Click(object sender, EventArgs e)
        {
            SkyObjectForm form = new SkyObjectForm(nightMode_, latitude_, longitude_, database_, stellariumConnection_, lastSelectedObjects_, lastObjSettings_);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            selectedObject_ = form.UseStellarium ? stellariumObj_ : form.Object;
            if (selectedObject_ != null)
                AddLastObject(selectedObject_);
            lastObjSettings_ = form.Settings;
            ObjectChanged();
            UpdateUI();
        }

        private void CloseReconnected(string portName)
        {
            for (int i = connectionList_.Count; --i >= 0;)
                if (connectionList_[i].connection_.PortName == portName)
                {
                    CloseConnection(connectionList_[i].connection_);
                    break;
                }
        }

        private void buttonConnection_Click(object sender, EventArgs e)
        {
            ConnectionForm form = new ConnectionForm(nightMode_, portName_, baudRate_, swapAzmAltEncoders_);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            if (form.DisconnectAll)
            {
                CloseAllConnections();
                UpdateUI();
                return;
            }

            CloseReconnected(form.PortName);

            portName_ = form.PortName;
            baudRate_ = form.BaudRate;
            positionsValid_ = false;

            if (portName_ != null)
            {
                SerialConnection connection = null;
                try
                {
                    connection = new SerialConnection(portName_, baudRate_);
                }
                catch (Exception)
                {
                }

                if (connection != null)
                {
                    ConnectionData data = new ConnectionData(connection, form.SwapAzmAltEncoders);
                    connectionList_.Add(data);
                    connection.SendReceiveRequest(new byte[] { (byte)'s' }, 2, new InitConnectionHandler(this, data));
                }
                UpdateUI();
            }
        }

        private void buttonOptions_Click(object sender, EventArgs e)
        {
            OptionsForm form = new OptionsForm(nightMode_, showNearestAzmRotation_, connectToStellarium_, stellariumTcpPort_, oppositeHorzPositioningDir_);
            form.Latitude = latitude_;
            form.Longitude = longitude_;
            if (form.ShowDialog() != DialogResult.OK)
                return;

            using (settings_.Buffer())
            {
                if (latitude_ != form.Latitude)
                {
                    settings_.Latitude = latitude_ = form.Latitude;
                    OptionsOrTimeChanged();
                }
                if (longitude_ != form.Longitude)
                {
                    settings_.Longitude = longitude_ = form.Longitude;
                    OptionsOrTimeChanged();
                }
                if (showNearestAzmRotation_ != form.ShowNearestAzmRotation)
                    settings_.ShowNearestAzmRotation = showNearestAzmRotation_ = form.ShowNearestAzmRotation;

                if (form.ConnectToStellarium)
                {
                    if (stellariumConnection_ != null && stellariumTcpPort_ != form.TcpPort)
                        CloseStellariumConnection();
                    if (stellariumConnection_ == null)
                        OpenStellariumConnection(form.TcpPort);
                }
                else if (stellariumConnection_ != null)
                    CloseStellariumConnection();

                if (form.OppositeHorzPositioningDir != oppositeHorzPositioningDir_)
                    settings_.OppositeHorzPositioningDir = oppositeHorzPositioningDir_ = form.OppositeHorzPositioningDir;

                settings_.ConnectToStellarium = connectToStellarium_ = form.ConnectToStellarium;
                settings_.TcpPort = stellariumTcpPort_ = form.TcpPort;
            }
            UpdateUI();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            ++timerCnt_;
            bool sendPositionToStellarium = false;
            switch (timerCnt_ & 0x07)
            {
            case 0:
            case 4:
                SendCommand(connectionGoTo_, 'R', 13, this.ReceiveStatus);
                break;
            case 2:
            case 6:
                SendCommand(connectionGoTo_, 'R', 13, this.ReceiveStatus);
                TimeChanged();
                break;
            case 1:
            case 3:
            case 5:
            case 7:
                //SendCommand(connectionGoTo_, 'y', 4, this.ReceiveAltAzmPosition);
                sendPositionToStellarium = true;
                break;
            }

            if (trackedObject_ != null && tmoSendPos_.CheckExpired())
                SendNextPositions();
            
            UpdateUI(sendPositionToStellarium);
        }

        private void AdjustFontSize()
        {
            float w, h;
            w = (float)this.Width / iInitWidth_;
            h = (float)this.Height / iInitHeight_;
            textBoxPosition.Font = new Font("Arial", positionFontSize_ * (w > h ? h : w));
            objectNameLabel.Font = new Font("Arial", objectFontSize_ * (w > h ? h : w), FontStyle.Bold);
        }

        private void ScopeGotoClient_Layout(object sender, LayoutEventArgs e)
        {
            AdjustFontSize();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Escape) && fullScreen_)
            {
                ExitFullScreen();
                return true;
            }
            if (keyData == (Keys.Alt | Keys.Enter) && !fullScreen_)
            {
                EnterFullScreen();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void SerialError(SerialConnection connection)
        {
            CloseConnection(connection);
            UpdateUI();
        }

        private void SetGoToConnection(ConnectionData data)
        {
            if (connectionGoTo_ != null)
            {
                if (connectionGoTo_.connection_ == data.connection_)
                    return;

                if (connectionGoTo_ != connectionGPS_)
                    CloseConnection(connectionGoTo_.connection_);
            }

            connectionGoTo_ = data;
            swapAzmAltEncoders_ = data.swapAzmAltEncoders_;

            SendCommand(connectionGoTo_, 'h', 4, this.ReceiveAltAzmResolution);
            SendCommand(connectionGoTo_, 'R', 13, this.ReceiveStatus);

            ConnectionChangedGoTo();
            UpdateUI();
        }
        private void SetGPSConnection(ConnectionData data)
        {
            if (connectionGPS_ != null)
            {
                if (connectionGPS_.connection_ == data.connection_)
                    return;

                if (connectionGPS_ != connectionGoTo_)
                    CloseConnection(connectionGPS_.connection_);
            }

            connectionGPS_ = data;

            ConnectionChangedGPS();
            UpdateUI();
        }

        private void SendCommand(ConnectionData connectionData, char cmd, int receiveCnt, ReceiveDelegate receiveDelegate)
        {
            if (connectionData != null && connectionData.connection_ != null)
                connectionData.connection_.SendReceiveRequest(new byte[] { (byte)cmd }, receiveCnt, new BaseConnectionHandler(this, receiveDelegate, connectionData.connection_));
        }

        private void SendCommand(ConnectionData connectionData, byte[] cmd, int receiveCnt, ReceiveDelegate receiveDelegate)
        {
            if (connectionData != null && connectionData.connection_ != null)
                connectionData.connection_.SendReceiveRequest(cmd, receiveCnt, new BaseConnectionHandler(this, receiveDelegate, connectionData.connection_));
        }

        private void SendCommand(ConnectionData connectionData, byte[] cmd, int receiveCnt, ReceiveDelegate receiveDelegate, TimeoutDelegate timeoutDelegate)
        {
            if (connectionData != null && connectionData.connection_ != null)
                connectionData.connection_.SendReceiveRequest(cmd, receiveCnt, new BaseConnectionHandler(this, receiveDelegate, timeoutDelegate, connectionData.connection_));
        }

        private void SendScopeMotionCommand(char cmd) { SendCommand(connectionGoTo_, cmd, 1, ReceiveDummy); }

        private void SendStopMotorCommand(byte dst)
        {
            SendCommand(connectionGoTo_, new byte[] { (byte)'T', dst }, 1, ReceiveAcknowlage);
        }

        private void SendSetNextPosCommand(Int32 sp, Int32 ts, byte dst)
        {
            if (connectionGoTo_ != null)
                SendCommand(connectionGoTo_, new byte[] { (byte)'N',
                                                          dst,
                                                          (byte)sp,
                                                          (byte)(sp >> 8),
                                                          (byte)(sp >> 16),
                                                          (byte)(sp >> 24),
                                                          (byte)ts,
                                                          (byte)(ts >> 8),
                                                          (byte)(ts >> 16),
                                                          (byte)(ts >> 24)}, 8, ReceiveNextPosCommand);
        }
        
        private bool altStartSent_, azmStartSent_;
        private void StartMotors()
        {
            if (connectionGoTo_ != null && connectionGoTo_.connection_ != null)
            {
                Int32 speed = 0;
                SendCommand(connectionGoTo_, new byte[] { (byte)'S',
                                                          A_ALT,
                                                          (byte)speed,
                                                          (byte)(speed >> 8),
                                                          (byte)(speed >> 16),
                                                          (byte)(speed >> 24)}, 8, ReceiveAltStart, TimeoutAltStart);
                SendCommand(connectionGoTo_, new byte[] { (byte)'S',
                                                          A_AZM,
                                                          (byte)speed,
                                                          (byte)(speed >> 8),
                                                          (byte)(speed >> 16),
                                                          (byte)(speed >> 24)}, 8, ReceiveAzmStart, TimeoutAzmStart);
                altStartSent_ = azmStartSent_ = true;
            }
        }
        private void ReceiveAltStart(byte[] data)
        {
            altStartSent_ = false;
        }
        private void TimeoutAltStart(SerialConnection connection)
        {
            altStartSent_ = false;
            SerialError(connection);
        }
        private void ReceiveAzmStart(byte[] data)
        {
            azmStartSent_ = false;
            tmoSendPos_.Restart();
            SendNextPositions();
        }
        private void TimeoutAzmStart(SerialConnection connection)
        {
            azmStartSent_ = false;
            SerialError(connection);
        }

        private void SendNextPositions()
        {
            if (trackedObject_ != null && alignment_ != null)
            {
                // next timestamp
                DateTime nextThisTs = DateTime.UtcNow + new TimeSpan(0, 0, nextPosTimeSec_);

                // calculate next positions
                double d = ClientCommonAPI.CalcTime(nextThisTs);

                double azm, alt;
                //trackedObject_.CalcAzimuthal(d, latitude_, longitude_, out azm, out alt);
                {
                    double dec, ra;
                    trackedObject_.CalcTopoRaDec(d, latitude_, longitude_, out dec, out ra);
                    SkyObjectPosCalc.Equ2AzAlt(d, latitude_, longitude_, dec + trackedOffsetDec_, ra + trackedOffsetRA_, out azm, out alt);
                }
                PairA objScope = alignment_.Horz2Scope(new PairA(azm * Const.toRad, alt * Const.toRad), 0);

                // azimuth difference, in degree
                double azmd = SkyObjectPosCalc.Rev(objScope.Azm * Const.toDeg - AzmAngle * Const.toDeg);
                if (azmd > 180)
                    azmd -= 360;

                // altitude difference, in degree
                double altd = (objScope.Alt - AltAngle) * Const.toDeg;

                // next positions
                Int32 nextAzmPos = azmPos_ + (int)(azmd * azmRes_ / 360.0);
                Int32 nextAltPos = altPos_ + (int)(altd * altRes_ / 360.0);

                // next timestamp
                Int32 nextTs = controllerTs_ + (Int32)(nextThisTs - thisTs_).TotalMilliseconds;

                // send positions
                SendSetNextPosCommand(nextAltPos, nextTs, A_ALT);
                SendSetNextPosCommand(nextAzmPos, nextTs, A_AZM);
            }
        }
        private void ReceiveNextPosCommand(byte[] data)
        {
        }

        private void StartTracking()
        {
            if (trackedObject_ == null && alignment_ != null && connectionGoTo_ != null && switchOn_)
            {
                // get current telescope position
                PairA horz = alignment_.Scope2Horz(new PairA(AzmAngle, AltAngle), 0);
                double dec, ra;
                SkyObjectPosCalc.AzAlt2Equ(ClientCommonAPI.CalcTime(), latitude_, longitude_,
                                            SkyObjectPosCalc.Rev(horz.Azm * Const.toDeg), SkyObjectPosCalc.Rev(horz.Alt * Const.toDeg), out dec, out ra);
                
                // select object to track
                //if (selectedObject_ != null)
                //{
                //}
                //else
                {
                    trackedObject_ = new SkyObjectPosCalc.StarPosition("Tracking", ra / 15.0, dec);
                    trackedOffsetRA_ = trackedOffsetDec_ = 0;
                    StartMotors();
                }
                TrackedObjectChanged();
            }
        }

        private void StopTracking()
        {
            if (trackedObject_ != null)
            {
                SendStopMotorCommand(A_ALT);
                SendStopMotorCommand(A_AZM);
                trackedObject_ = null;
                TrackedObjectChanged();
            }
        }

        private void OffsetTrackingObject(double offsetAzm, double offsetAlt)
        {
            if (trackedObject_ != null)
            {
                // get current telescope position
                PairA horz = alignment_.Scope2Horz(new PairA(AzmAngle, AltAngle), 0);
                double dec, ra;
                SkyObjectPosCalc.AzAlt2Equ(ClientCommonAPI.CalcTime(), latitude_, longitude_,
                                            SkyObjectPosCalc.Rev(horz.Azm * Const.toDeg), SkyObjectPosCalc.Rev(horz.Alt * Const.toDeg), out dec, out ra);
                horz = alignment_.Scope2Horz(new PairA(AzmAngle + offsetAzm * Const.toRad, AltAngle + offsetAlt * Const.toRad), 0);
                double shiftedDec, shiftedRA;
                SkyObjectPosCalc.AzAlt2Equ(ClientCommonAPI.CalcTime(), latitude_, longitude_,
                                            SkyObjectPosCalc.Rev(horz.Azm * Const.toDeg), SkyObjectPosCalc.Rev(horz.Alt * Const.toDeg), out shiftedDec, out shiftedRA);

                trackedOffsetDec_ += shiftedDec - dec;
                trackedOffsetRA_ += shiftedRA - ra;

                tmoSendPos_.Restart();
                SendNextPositions();
                TrackedObjectChanged();
            }
        }
        
        private Int32 GetInt32(byte[] data, int offset)
        {
            return (Int32)((((UInt32)data[offset+3]) << 24) +
                           (((UInt32)data[offset+2]) << 16) +
                           (((UInt32)data[offset+1]) << 8) +
                             (UInt32)(data[offset]));
        }
        
        private void ReceiveAcknowlage(byte[] data)
        {
        }
        private void ReceiveAltAzmResolution(byte[] data)
        {
            if (swapAzmAltEncoders_)
            {
                azmRes_ = (UInt16)((((UInt16)data[1]) << 8) + data[0]);
                altRes_ = (UInt16)((((UInt16)data[3]) << 8) + data[2]);
            }
            else
            {
                altRes_ = (UInt16)((((UInt16)data[1]) << 8) + data[0]);
                azmRes_ = (UInt16)((((UInt16)data[3]) << 8) + data[2]);
            }
        }

        private void ReceiveStatus(byte[] data)
        {
            // sync time
            controllerTs_ = GetInt32(data, 0);
            thisTs_ = DateTime.UtcNow;

            // store telescope position
            Int32 altPos, azmPos;
            if (swapAzmAltEncoders_)
            {
                altPos = GetInt32(data, 8);
                azmPos = GetInt32(data, 4);
            }
            else
            {
                altPos = GetInt32(data, 4);
                azmPos = GetInt32(data, 8);
            }
            if (azmPos_ != azmPos || altPos_ != altPos || !positionsValid_)
                ScopePosChanged();
            azmPos_ = azmPos;
            altPos_ = altPos;
            positionsValid_ = true;

            byte state = data[12];

            bool oldSwitchOn = switchOn_;
            switchOn_ = (state & STATE_SWITCH_ON) != 0;
            if (switchOn_)
            {
                if (!altStartSent_ && !azmStartSent_)
                {
                    bool altOn = (state & STATE_ALT_RUNNING) != 0, azmOn = (state & STATE_AZM_RUNNING) != 0;
                    if (!altOn || !azmOn)
                    {
                        if (altOn)
                            SendStopMotorCommand(A_ALT);
                        if (azmOn)
                            SendStopMotorCommand(A_AZM);
                        if (trackedObject_ != null)
                        {
                            trackedObject_ = null;
                            TrackedObjectChanged();
                        }
                    }
                }
                if (!oldSwitchOn)
                {
                    if (checkBoxTrackAuto.Checked && allowAutoTrack_)
                        StartTracking();
                    SwitchChanged();
                }
            }
            else
            {
                if (oldSwitchOn)
                    SwitchChanged();
                if (trackedObject_ != null)
                {
                    trackedObject_ = null;
                    TrackedObjectChanged();
                    // As switch is off, motors are stopped by themselves
                }
            }
            UpdateUI();
        }

        private void ReceiveDummy(byte[] data)
        {
        }

        private void CloseConnection(SerialConnection connection)
        {
            if (connection == null)
                return;

            if (connectionGoTo_ != null && connectionGoTo_.connection_ == connection)
            {
                connectionGoTo_ = null;
                ConnectionChangedGoTo();
            }

            if (connectionGPS_ != null && connectionGPS_.connection_ == connection)
            {
                connectionGPS_ = null;
                ConnectionChangedGPS();
            }

            // find and close old
            for (int i = 0; i < connectionList_.Count; ++i)
                if (connectionList_[i].connection_ == connection)
                {
                    connection.Close();
                    connectionList_.RemoveAt(i);
                    break;
                }
        }

        private void CloseAllConnections()
        {
            if (connectionGoTo_ != null)
            {
                connectionGoTo_ = null;
                ConnectionChangedGoTo();
            }

            if (connectionGPS_ != null)
            {
                connectionGPS_ = null;
                ConnectionChangedGPS();
            }

            // close all connections (maybe redundant)
            foreach (ConnectionData data in connectionList_)
                data.connection_.Close();
            connectionList_.Clear();
        }

        private void OpenStellariumConnection(int port)
        {
            buttonStellariumConnect.Text = "Stellarium: Connecting..." + Environment.NewLine + "(Press to Diconnect)";
            stellariumConnection_ = new StellariumServer.Connection(System.Net.IPAddress.Any, port);
            stellariumConnection_.StatusChanged += StellariumStatusChangedHandlerAsync;
            stellariumConnection_.ReceivedGoto += StellariumReceivedGoto;
        }

        private void CloseStellariumConnection()
        {
            buttonStellariumConnect.Text = "Stellarium: Disconnected" + Environment.NewLine + "(Press to Connect)";
            if (stellariumConnection_ != null)
            {
                stellariumConnection_.StatusChanged -= StellariumStatusChangedHandlerAsync;
                stellariumConnection_.ReceivedGoto -= StellariumReceivedGoto;
                stellariumConnection_.Close();
                stellariumConnection_ = null;
            }
        }

        private void ScopeGotoClient_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseAllConnections();
            CloseStellariumConnection();
            //settings_.Save();
        }

        private void buttonTrackGoTo_Click(object sender, EventArgs e)
        {
            if (alignment_ != null && connectionGoTo_ != null && switchOn_)
            {
                if (selectedObject_ == null)
                    StartTracking();
                else
                {
                    SkyObjectPosCalc.SkyPosition prevTrackedObj = trackedObject_;
                    trackedObject_ = selectedObject_;
                    trackedOffsetRA_ = trackedOffsetDec_ = 0;
                    if (prevTrackedObj == null)
                        StartMotors();
                    else
                    {
                        tmoSendPos_.Restart();
                        SendNextPositions();
                    }
                    TrackedObjectChanged();
                }
            }
        }

        private void buttonTrackUp_Click(object sender, EventArgs e)
        {
            OffsetTrackingObject(0, -arrowMoveSpeed_);
        }

        private void checkBoxTrackAuto_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxTrackAuto.Checked)
            {
                StartTracking();
                allowAutoTrack_ = true;
                UpdateUI();
            }
            settings_.AutoTrack = checkBoxTrackAuto.Checked;
        }

        private void buttonTrackLeft_Click(object sender, EventArgs e)
        {
            OffsetTrackingObject(-arrowMoveSpeed_, 0);
        }

        private void buttonTrackDown_Click(object sender, EventArgs e)
        {
            OffsetTrackingObject(0, -arrowMoveSpeed_);
        }

        private void buttonTrackRight_Click(object sender, EventArgs e)
        {
            OffsetTrackingObject(arrowMoveSpeed_, 0);
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            if (trackedObject_ == null)
            {
                StartTracking();
                allowAutoTrack_ = true;
            }
            else
                StopTracking();
            UpdateUI();
        }

        public void StellariumStatusChangedHandlerAsync()
        {
            this.BeginInvoke(new StellariumServer.Connection.StatusChangedHandler(this.StellariumStatusChangedHandler));
        }

        public void StellariumStatusChangedHandler()
        {
            bool connected = stellariumConnection_.IsConnected;
            if(connected)
                buttonStellariumConnect.Text = "Stellarium: Connected" + Environment.NewLine + "(Press to Disconnect)";
            else
                buttonStellariumConnect.Text = "Stellarium: Connecting..." + Environment.NewLine + "(Press to Disconnect)";
            stellariumObj_.Connected = connected;
            ObjectChanged();
            UpdateUI(true);
        }

        public void StellariumReceivedGotoAsync(double dec, double ra)
        {
            this.BeginInvoke(new StellariumServer.Connection.ReceivedGotoHandler(this.StellariumReceivedGoto), new object[] { dec, ra });
        }

        public void StellariumReceivedGoto(double dec, double ra)
        {
            stellariumObj_.Dec = dec;
            stellariumObj_.Ra = ra;
        }

        private void buttonStellariumConnect_Click(object sender, EventArgs e)
        {
            if (stellariumConnection_ != null)
                CloseStellariumConnection();
            else
                OpenStellariumConnection(stellariumTcpPort_);
        }
    }

    //Application settings wrapper class
    sealed class ScopeGotoClientSettings
    {
        public ScopeGotoClientSettings()
        {
            profile_.AddTypes = AddType.Short;
        }

        public double Latitude
        {
            get { return profile_.GetValue(section_, "Latitude", 37.257471); }
            set { profile_.SetValue(section_, "Latitude", value); }
        }

        public double Longitude
        {
            get { return profile_.GetValue(section_, "Longitude", -121.942246); }
            set { profile_.SetValue(section_, "Longitude", value); }
        }

        public bool ShowNearestAzmRotation
        {
            get { return profile_.GetValue(section_, "ShowNearestAzmRotation", true); }
            set { profile_.SetValue(section_, "ShowNearestAzmRotation", value); }
        }

        public bool ConnectToStellarium
        {
            get { return profile_.GetValue(section_, "ConnectToStellarium", false); }
            set { profile_.SetValue(section_, "ConnectToStellarium", value); }
        }

        public int TcpPort
        {
            get { return profile_.GetValue(section_, "TcpPort", 8001); }
            set { profile_.SetValue(section_, "TcpPort", value); }
        }

        public bool OppositeHorzPositioningDir
        {
            get { return profile_.GetValue(section_, "OppositeHorzPositioningDir", false); }
            set { profile_.SetValue(section_, "OppositeHorzPositioningDir", value); }
        }

        public AlignStar[] AlignmentStars
        {
            get { return (AlignStar[])profile_.GetValue(section_, "AlignmentStars", null, typeof(AlignStar[])); }
            set { profile_.SetValue(section_, "AlignmentStars", value); }
        }

        public Vect3 AlignmentEquAxis
        {
            get { return (Vect3)profile_.GetValue(section_, "AlignmentEquAxis", new Vect3()); }
            set { profile_.SetValue(section_, "AlignmentEquAxis", value); }
        }

        public ScopeGotoClient.AlignmentConnectionData AlignmentConnectionGoTo
        {
            get { return (ScopeGotoClient.AlignmentConnectionData)profile_.GetValue(section_, "AlignmentConnectionGoTo", new ScopeGotoClient.AlignmentConnectionData()); }
            set { profile_.SetValue(section_, "AlignmentConnectionGoTo", value); }
        }

        public bool AutoTrack
        {
            get { return profile_.GetValue(section_, "AutoTrack", false); }
            set { profile_.SetValue(section_, "AutoTrack", value); }
        }

        /*
        public long AzmResolution
        {
            get { return profile_.GetValue(section_, "AzmResolution", 1800); }
            set { profile_.SetValue(section_, "AzmResolution", value); }
        }

        public long AltResolution
        {
            get { return profile_.GetValue(section_, "AltResolution", 1800); }
            set { profile_.SetValue(section_, "AltResolution", value); }
        }
        */

        public XmlBuffer Buffer()
        {
            return profile_.Buffer();
        }

        private const string section_ = "entriesGoTo";
        private XmlProfile profile_ = new XmlProfile();
    }
}

namespace ScopeGotoClientHelper
{
    public class NoSelectButton : Button
    {
        public NoSelectButton()
        {
            SetStyle(ControlStyles.Selectable, false);
        }
    }
}
