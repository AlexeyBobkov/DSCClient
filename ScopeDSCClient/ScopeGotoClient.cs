//#define TESTING

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

        // alignment data
        private DSCAlignment alignment_;
        private ClientCommonAPI.AlignmentConnectionData alignmentConnectionGoTo_;

        // state
        private const int LAST_OBJ_COUNT = 20;
        private SkyObjectPosCalc.SkyPosition selectedObject_;
        private SkyObjectPosCalc.SkyPosition trackedObject_;
        private double trackedOffsetRa_, trackedOffsetDec_;
        private bool switchOn_ = false;
        private bool autoTrack_ = true;
        private bool allowAutoTrack_ = false;
        private SkyObjectPosCalc.SkyPosition[] lastSelectedObjects_ = new SkyObjectPosCalc.SkyPosition[LAST_OBJ_COUNT];

        // time sync
        private Int32 controllerTs_;
        private DateTime thisTs_;       // UTC time
        private ClientCommonAPI.Timeout tmoSendPos_ = new ClientCommonAPI.Timeout(3800);
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

        public const byte A_ALT = 0;    // command for alt adapter
        public const byte A_AZM = 1;    // command for azm adapter
        public const byte M_ALT = 2;    // command for alt motor
        public const byte M_AZM = 3;    // command for azm motor

#if LOGGING_ON
        private const UInt16 LMODE_ALT = 0;
        private const UInt16 LMODE_AZM = 0x8000;

        private const UInt16 LMODE_FIRST = 1;
        private const UInt16 LMODE_MPOS = 1;
        private const UInt16 LMODE_MLOG = 2;
        private const UInt16 LMODE_MSPD = 4;
        private const UInt16 LMODE_MERR = 8;
        private const UInt16 LMODE_APOS = 0x10;
        private const UInt16 LMODE_ALOG = 0x20;
        private const UInt16 LMODE_ASPD = 0x40;
        private const UInt16 LMODE_AERR = 0x80;
        private const UInt16 LMODE_LAST = 0x100;

        private const UInt16 LMODE_OFF = 0;

        private ClientCommonAPI.LoggingState loggingState_      = ClientCommonAPI.LoggingState.OFF;
        private ClientCommonAPI.LoggingChannel loggingChannel_  = ClientCommonAPI.LoggingChannel.AZM;
        private ClientCommonAPI.LoggingType loggingType0_       = ClientCommonAPI.LoggingType.M_POS;
        private ClientCommonAPI.LoggingType loggingType1_       = ClientCommonAPI.LoggingType.M_POS;
        private List<int> logData_ = new List<int>();
#endif

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
        private ClientCommonAPI.StellariumObject stellariumObj_ = new ClientCommonAPI.StellariumObject();

        // object databases
        private List<ClientCommonAPI.ObjDatabaseEntry> database_ = new List<ClientCommonAPI.ObjDatabaseEntry>();

        // physical locations
        private List<ClientCommonAPI.PhysicalLocation> locations_ = new List<ClientCommonAPI.PhysicalLocation>();

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

        private class ClientHost : ClientCommonAPI.IClientHost
        {
            public ClientHost(ScopeGotoClient parent) { parent_ = parent; }
            public ClientHost(ScopeGotoClient parent, ScopeGotoClientSettings settings) { parent_ = parent; settings_ = settings; }

            public double AzmAngle { get { return parent_.AzmAngleLimited; } }
            public double AltAngle { get { return parent_.AltAngle; } }
            public double EquAngle { get { return -Math.PI; } }

            public bool NightMode { get { return parent_.nightMode_; } }
            public double Latitude { get { return parent_.latitude_; } }
            public double Longitude { get { return parent_.longitude_; } }

            public string GetConfigurationName { get { return "GoTo Controller Configuration"; } }
            public void CallConfiguration()
            {
                GotoConfigurationForm form = new GotoConfigurationForm(parent_, settings_);
                form.ShowDialog();
            }

            private ScopeGotoClient parent_;
            private ScopeGotoClientSettings settings_;
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
                PairA objScope = alignment_.Horz2Scope(new PairA(azm * Const.toRad, alt * Const.toRad), -Math.PI);

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
                    PairA horz = alignment_.Scope2Horz(new PairA(AzmAngle, AltAngle), -Math.PI);

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
                s += "Tracking " + trackedObject_.Name;
                s += " (dec += " + ClientCommonAPI.PrintAngle(trackedOffsetDec_, false, false);
                s += ", ra += " + ClientCommonAPI.PrintAngle(trackedOffsetRa_, false, false) + ")";
                s += Environment.NewLine;
                s += "Azm = " + ClientCommonAPI.PrintAngle(azm, false, false);
                s += ", Alt = " + ClientCommonAPI.PrintAngle(alt, false, false) + Environment.NewLine;

                double dec, ra;
                trackedObject_.CalcTopoRaDec(d, latitude_, longitude_, out dec, out ra);
                s += "R.A.\t= " + ClientCommonAPI.PrintTime(ra) + " (" + ra.ToString("F5") + "\x00B0)" + Environment.NewLine;
                s += "Dec.\t= " + ClientCommonAPI.PrintAngle(dec, true) + " (" + ClientCommonAPI.PrintDec(dec, "F5") + "\x00B0)" + Environment.NewLine;
            }
            else
            {
                s += "Tracking: " + trackedObject_.Name;
                s += " (dec += " + ClientCommonAPI.PrintAngle(trackedOffsetDec_, false, false);
                s += ", ra += " + ClientCommonAPI.PrintAngle(trackedOffsetRa_, false, false) + ")";
                s += Environment.NewLine;
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
            //checkBoxTrackAuto.Enabled = true;

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
                    TrackedObjectChanged(true);
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

            if (alignment_ == null)
            {
                if (switchOn_)
                {
                    switchOn_ = false;
                    SwitchChanged();
                }
                if (trackedObject_ != null && connectionGoTo_ != null)
                    StopTracking();
                allowAutoTrack_ = false;
            }
            else if (connectionGoTo_ != null && switchOn_ && trackedObject_ != null)
                SetTrackedObject();     // correct, according to new alignment
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

        private void TrackedObjectChanged(bool sendMode)
        {
            scopePosAndObjTextChanged_ = true;
#if LOGGING_ON
            if(sendMode)
                SendLoggingMode();
#endif
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
                    {
                        settings_.AlignmentConnectionGoTo = alignmentConnectionGoTo_ =
                            new ClientCommonAPI.AlignmentConnectionData(connectionGoTo_.connection_.PortName, connectionGoTo_.sessionId_);
                    }
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

            autoTrack_ = settings_.AutoTrack;

            // Everything is changed! (Yes, it's redundant.)
            //OptionsOrTimeChanged();
            ConnectionChangedGoTo();
            ConnectionChangedGPS();
            ObjectChanged();
            ScopePosChanged();
            TimeChanged();
            UpdateUI();

            LoadAlignment();
            ClientCommonAPI.BuildObjectDatabase(ref database_);
            ClientCommonAPI.BuildLocationDatabase(ref locations_);
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
            AlignmentForm form = new AlignmentForm(new ClientHost(this), lastAlignmentObjSettings_, alignment_);
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
            SkyObjectForm form = new SkyObjectForm(new ClientHost(this), database_, stellariumConnection_, lastSelectedObjects_, lastObjSettings_);
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
            OptionsForm form = new OptionsForm(new ClientHost(this, settings_),
                                               locations_,
                                               showNearestAzmRotation_,
                                               connectToStellarium_,
                                               stellariumTcpPort_,
                                               oppositeHorzPositioningDir_,
                                               autoTrack_ ? ClientCommonAPI.AutoTrack.ON : ClientCommonAPI.AutoTrack.OFF,
#if LOGGING_ON
                                               loggingState_,
                                               loggingChannel_,
                                               loggingType0_,
                                               loggingType1_,
                                               logData_);
#else
                                               ClientCommonAPI.LoggingState.DISABLED,
                                               ClientCommonAPI.LoggingChannel.UNUSED,
                                               ClientCommonAPI.LoggingType.UNUSED,
                                               ClientCommonAPI.LoggingType.UNUSED,
                                               null);
#endif
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
                    if (stellariumConnection_ != null && stellariumTcpPort_ != form.StellariumTcpPort)
                        CloseStellariumConnection();
                    if (stellariumConnection_ == null)
                        OpenStellariumConnection(form.StellariumTcpPort);
                }
                else if (stellariumConnection_ != null)
                    CloseStellariumConnection();

                if (form.OppositeHorzPositioningDir != oppositeHorzPositioningDir_)
                    settings_.OppositeHorzPositioningDir = oppositeHorzPositioningDir_ = form.OppositeHorzPositioningDir;

                settings_.ConnectToStellarium = connectToStellarium_ = form.ConnectToStellarium;
                settings_.TcpPort = stellariumTcpPort_ = form.StellariumTcpPort;
                settings_.AutoTrack = autoTrack_ = (form.AutoTrack == ClientCommonAPI.AutoTrack.ON);
            }

#if LOGGING_ON
            loggingState_ = form.LoggingState;
            loggingChannel_ = form.LoggingChannel;
            loggingType0_ = form.LoggingType0;
            loggingType1_ = form.LoggingType1;
            logData_ = form.LogData;
#endif
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

#if LOGGING_ON
            if (connectionGoTo_ != null && loggingState_ == ClientCommonAPI.LoggingState.ON && tmoAddLogData_.CheckExpired())
            {
                int logCnt = logNextBlockSize_ + 5;
                if (logCnt > 14)
                    logCnt = 14;
                SendCommand(connectionGoTo_, new byte[] { (byte)'L', (byte)'w', (byte)logCnt, 0 }, 4 + logCnt * 4, AddLogData);
            }
#endif
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

            //SendCommand(connectionGoTo_, 'h', 4, this.ReceiveAltAzmResolution);
            SendCommand(connectionGoTo_, 'R', 13, this.ReceiveStatus);
            SetAndReciveMotorAndAdapterConfigOptions();

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

        private void SendSetNextPosCommand(float sp, Int32 ts, byte dst)
        {
            byte[] spBytes = BitConverter.GetBytes(sp);
            if (connectionGoTo_ != null)
                SendCommand(connectionGoTo_, new byte[] { (byte)'N',
                                                          dst,
                                                          spBytes[0],
                                                          spBytes[1],
                                                          spBytes[2],
                                                          spBytes[3],
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
                double altd1, azmd1, altd2, azmd2;
                CalcScopeShifts(DateTime.UtcNow, out altd1, out azmd1);
                CalcScopeShifts(DateTime.UtcNow + new TimeSpan(0, 0, nextPosTimeSec_), out altd2, out azmd2);

                Int32 altSpeed = (Int32)((altd2 - altd1) * altRes_ * 60.0 * 60.0 * 24.0 / (360.0 * nextPosTimeSec_));
                Int32 azmSpeed = (Int32)((azmd2 - azmd1) * azmRes_ * 60.0 * 60.0 * 24.0 / (360.0 * nextPosTimeSec_));
#if !TESTING
                SendCommand(connectionGoTo_, new byte[] { (byte)'S',
                                                          A_ALT,
                                                          (byte)altSpeed,
                                                          (byte)(altSpeed >> 8),
                                                          (byte)(altSpeed >> 16),
                                                          (byte)(altSpeed >> 24)}, 8, ReceiveAltStart, TimeoutAltStart);
#endif
                SendCommand(connectionGoTo_, new byte[] { (byte)'S',
                                                          A_AZM,
                                                          (byte)azmSpeed,
                                                          (byte)(azmSpeed >> 8),
                                                          (byte)(azmSpeed >> 16),
                                                          (byte)(azmSpeed >> 24)}, 8, ReceiveAzmStart, TimeoutAzmStart);
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

        private void CalcScopeShifts(DateTime nextTs, out double altdDeg, out double azmdDeg)
        {
            // next timestamp
            double d = ClientCommonAPI.CalcTime(nextTs);

            // calculate scope positions
            double azm, alt;    // in degree
            {
                double dec, ra;
                trackedObject_.CalcTopoRaDec(d, latitude_, longitude_, out dec, out ra);
                SkyObjectPosCalc.Equ2AzAlt(d, latitude_, longitude_, dec + trackedOffsetDec_, ra + trackedOffsetRa_, out azm, out alt);
            }
            PairA objScope = alignment_.Horz2Scope(new PairA(azm * Const.toRad, alt * Const.toRad), -Math.PI);

            // azimuth difference, in degree
            azmdDeg = SkyObjectPosCalc.Rev(objScope.Azm * Const.toDeg - AzmAngle * Const.toDeg);
            if (azmdDeg > 180)
                azmdDeg -= 360;

            // altitude difference, in degree
#if TESTING
            altdDeg = 0;
#else
            altdDeg = (objScope.Alt - AltAngle) * Const.toDeg;
#endif
        }

        private void SendNextPositions()
        {
            if (trackedObject_ != null && alignment_ != null)
            {
                // next timestamp
                DateTime nextThisTs = DateTime.UtcNow + new TimeSpan(0, 0, nextPosTimeSec_);
                double altd, azmd;
                CalcScopeShifts(nextThisTs, out altd, out azmd);

                // next positions
                double nextAzmPos = azmPos_ + azmd * azmRes_ / 360.0;
                double nextAltPos = altPos_ + altd * altRes_ / 360.0;

                // next timestamp
                Int32 nextTs = controllerTs_ + Convert.ToInt32((nextThisTs - thisTs_).TotalMilliseconds);

                // send positions
                SendSetNextPosCommand((float)nextAltPos, nextTs, A_ALT);
                SendSetNextPosCommand((float)nextAzmPos, nextTs, A_AZM);
            }
        }
        private void ReceiveNextPosCommand(byte[] data)
        {
        }

#if LOGGING_ON
        private const int LOG_PERIOD = 200; //msec
        private UInt32 logStart_ = 0;
        private Int32 logTs_ = Int32.MinValue;
        private Int32 logAbs1_ = Int32.MinValue, logAbs2_ = Int32.MinValue;
        private int logNextBlockSize_ = 0;
        private ClientCommonAPI.Timeout tmoAddLogData_ = new ClientCommonAPI.Timeout(1000);

        private void AddLogData(byte[] data)
        {
            int reported = (int)data[1];
            int stillInBuffer = (int)data[2];
            if (data[3] != 0)   // ring buffer was overflowed
            {
                // force restart and re-synchronization
                logStart_ = 0;
                logAbs1_ = logAbs2_ = logTs_ = Int32.MinValue;
            }

            logNextBlockSize_ = stillInBuffer;

            for (int i = 0; i < reported; ++i)
            {
                int start = i * 4 + 4;
                if (data[start + 3] == 0x80)
                {
                    // re-sync!
                    logStart_ = (((UInt32)data[start + 3]) << 24) + (((UInt32)data[start + 2]) << 16) + (((UInt32)data[start + 1]) << 8) + (UInt32)data[start];
                    logAbs1_ = logAbs2_ = logTs_ = Int32.MinValue;
                    continue;
                }
                if (logStart_ == 0)
                    continue;       // skip it: waiting for re-sync

                if (logTs_ == Int32.MinValue)
                {
                    logTs_ = (Int32)((((UInt32)(byte)(logStart_ & 1L)) << 31) + (((UInt32)data[start + 3]) << 24) +
                                     (((UInt32)data[start + 2]) << 16) + (((UInt32)data[start + 1]) << 8) + (UInt32)data[start]);
                    continue;
                }
                if (logAbs1_ == Int32.MinValue)
                {
                    logAbs1_ = (Int32)((((UInt32)(byte)(logStart_ & 2L)) << 30) + (((UInt32)data[start + 3]) << 24) +
                                       (((UInt32)data[start + 2]) << 16) + (((UInt32)data[start + 1]) << 8) + (UInt32)data[start]);
                    continue;
                }

                Int32 pos1, pos2;
                if (logAbs2_ == Int32.MinValue)
                {
                    pos1 = logAbs1_;
                    pos2 = logAbs2_ = (Int32)((((UInt32)(byte)(logStart_ & 4L)) << 29) + (((UInt32)data[start + 3]) << 24) +
                                              (((UInt32)data[start + 2]) << 16) + (((UInt32)data[start + 1]) << 8) + (UInt32)data[start]);
                }
                else
                {
                    pos1 = logAbs1_ + (Int16)((((UInt16)data[start + 1]) << 8) + (UInt16)data[start]);
                    pos2 = logAbs2_ + (Int16)((((UInt16)data[start + 3]) << 8) + (UInt16)data[start + 2]);
                }

                logData_.Add(logTs_);
                logData_.Add(pos1);
                logData_.Add(pos2);

                logTs_ += LOG_PERIOD;
            }
        }

        private void SendLoggingMode()
        {
            if (connectionGoTo_ != null)
            {
                UInt16 loggingMode = 0;
                if (trackedObject_ == null)
                    loggingMode = LMODE_OFF;
                else
                    switch (loggingState_)
                    {
                    default:
                        break;

                    case ClientCommonAPI.LoggingState.ON:
                        switch (loggingType0_)
                        {
                        default: break;
                        case ClientCommonAPI.LoggingType.M_POS: loggingMode = LMODE_MPOS; break;
                        case ClientCommonAPI.LoggingType.M_LOG: loggingMode = LMODE_MLOG; break;
                        case ClientCommonAPI.LoggingType.M_SPD: loggingMode = LMODE_MSPD; break;
                        case ClientCommonAPI.LoggingType.M_ERR: loggingMode = LMODE_MERR; break;
                        case ClientCommonAPI.LoggingType.A_POS: loggingMode = LMODE_APOS; break;
                        case ClientCommonAPI.LoggingType.A_LOG: loggingMode = LMODE_ALOG; break;
                        case ClientCommonAPI.LoggingType.A_SPD: loggingMode = LMODE_ASPD; break;
                        case ClientCommonAPI.LoggingType.A_ERR: loggingMode = LMODE_AERR; break;
                        }

                        switch (loggingType1_)
                        {
                        default: break;
                        case ClientCommonAPI.LoggingType.M_POS: loggingMode |= LMODE_MPOS; break;
                        case ClientCommonAPI.LoggingType.M_LOG: loggingMode |= LMODE_MLOG; break;
                        case ClientCommonAPI.LoggingType.M_SPD: loggingMode |= LMODE_MSPD; break;
                        case ClientCommonAPI.LoggingType.M_ERR: loggingMode |= LMODE_MERR; break;
                        case ClientCommonAPI.LoggingType.A_POS: loggingMode |= LMODE_APOS; break;
                        case ClientCommonAPI.LoggingType.A_LOG: loggingMode |= LMODE_ALOG; break;
                        case ClientCommonAPI.LoggingType.A_SPD: loggingMode |= LMODE_ASPD; break;
                        case ClientCommonAPI.LoggingType.A_ERR: loggingMode |= LMODE_AERR; break;
                        }

                        switch (loggingChannel_)
                        {
                        default:
                            loggingMode = LMODE_OFF;
                            break;

                        case ClientCommonAPI.LoggingChannel.ALT:
                            loggingMode |= LMODE_ALT;
                            break;

                        case ClientCommonAPI.LoggingChannel.AZM:
                            loggingMode |= LMODE_AZM;
                            break;
                        }
                        break;
                    }
                SendCommand(connectionGoTo_, new byte[] { (byte)'L', (byte)'m', (byte)loggingMode, (byte)(loggingMode >> 8) }, 1, ReceiveDummy);
            }
        }
#endif

        private const double maxSelDiff_ = 1.0;
        private void SetTrackedObject()
        {
            if (alignment_ != null && connectionGoTo_ != null && switchOn_)
            {
                // get current telescope position
                PairA horz = alignment_.Scope2Horz(new PairA(AzmAngle, AltAngle), -Math.PI);
                double scopeDec, scopeRa;
                SkyObjectPosCalc.AzAlt2Equ(ClientCommonAPI.CalcTime(), latitude_, longitude_,
                                            SkyObjectPosCalc.Rev(horz.Azm * Const.toDeg), SkyObjectPosCalc.Rev(horz.Alt * Const.toDeg), out scopeDec, out scopeRa);

                // select object to track
                if (selectedObject_ != null)
                {
                    double selDec, selRa;
                    selectedObject_.CalcTopoRaDec(ClientCommonAPI.CalcTime(), latitude_, longitude_, out selDec, out selRa);
                    double diffDec = SkyObjectPosCalc.Rev(scopeDec - selDec);
                    if (diffDec > 180)
                        diffDec -= 360;
                    double diffRa = SkyObjectPosCalc.Rev(scopeRa - selRa);
                    if (diffRa > 180)
                        diffRa -= 360;
                    if (diffDec < maxSelDiff_ && diffDec > -maxSelDiff_ && diffRa < maxSelDiff_ && diffRa > -maxSelDiff_)
                    {
                        trackedObject_ = selectedObject_;
                        trackedOffsetDec_ = diffDec;
                        trackedOffsetRa_ = diffRa;
                    }
                }
                if (trackedObject_ == null)
                {
                    trackedObject_ = new SkyObjectPosCalc.StarPosition("Unknown", scopeRa / 15.0, scopeDec, false);
                    trackedOffsetRa_ = trackedOffsetDec_ = 0;
                }
                TrackedObjectChanged(true);
            }
        }
        private void StartTracking()
        {
            if (trackedObject_ == null && alignment_ != null && connectionGoTo_ != null && switchOn_)
            {
                SetTrackedObject();
                StartMotors();
            }
        }

        private void StopTracking()
        {
            if (trackedObject_ != null)
            {
                SendStopMotorCommand(A_ALT);
                SendStopMotorCommand(A_AZM);
                trackedObject_ = null;
                TrackedObjectChanged(true);
            }
        }

        private void OffsetTrackingObject(double offsetAzm, double offsetAlt)
        {
            if (trackedObject_ != null)
            {
                // get current telescope position
                PairA horz = alignment_.Scope2Horz(new PairA(AzmAngle, AltAngle), -Math.PI);
                double dec, ra;
                SkyObjectPosCalc.AzAlt2Equ(ClientCommonAPI.CalcTime(), latitude_, longitude_,
                                            SkyObjectPosCalc.Rev(horz.Azm * Const.toDeg), SkyObjectPosCalc.Rev(horz.Alt * Const.toDeg), out dec, out ra);
                horz = alignment_.Scope2Horz(new PairA(AzmAngle + offsetAzm * Const.toRad, AltAngle + offsetAlt * Const.toRad), -Math.PI);
                double shiftedDec, shiftedRa;
                SkyObjectPosCalc.AzAlt2Equ(ClientCommonAPI.CalcTime(), latitude_, longitude_,
                                            SkyObjectPosCalc.Rev(horz.Azm * Const.toDeg), SkyObjectPosCalc.Rev(horz.Alt * Const.toDeg), out shiftedDec, out shiftedRa);

                trackedOffsetDec_ += shiftedDec - dec;
                trackedOffsetRa_ += shiftedRa - ra;

                tmoSendPos_.Restart();
                SendNextPositions();
                TrackedObjectChanged(false);
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
                if (!oldSwitchOn)
                    SwitchChanged();

                bool altOn = (state & STATE_ALT_RUNNING) != 0, azmOn = (state & STATE_AZM_RUNNING) != 0;
                if (trackedObject_ != null && !altStartSent_ && !azmStartSent_ && (!altOn || !azmOn))
                {
#if !TESTING
                    if (altOn)
                        SendStopMotorCommand(A_ALT);
                    if (azmOn)
                        SendStopMotorCommand(A_AZM);
                    trackedObject_ = null;
                    TrackedObjectChanged(true);
#endif
                }
                else if (!oldSwitchOn && autoTrack_ && allowAutoTrack_)
                    StartTracking();
            }
            else
            {
                if (oldSwitchOn)
                    SwitchChanged();
                if (trackedObject_ != null)
                {
                    trackedObject_ = null;
                    TrackedObjectChanged(true);
                    // As switch is off, motors are stopped by themselves
                }
            }
            UpdateUI();
        }

        private void ReceiveDummy(byte[] data)
        {
        }


        // MOTOR/ADAPTER CONFIGURATION OPTIONS BEGIN

        private void SetAndReciveMotorAndAdapterConfigOptions()
        {
            SendMotorConfigOptionsSizes();
        }
        private void SendMotorConfigOptionsSizes()
        {
            if (connectionGoTo_ != null)
                SendCommand(connectionGoTo_, new byte[] { (byte)'Z' }, 4, ReceiveMotorConfigOptionsSizes);
        }
        private void SendGetMotorConfigOptions(byte dst)
        {
            if (connectionGoTo_ != null)
                SendCommand(connectionGoTo_, new byte[] { (byte)'O', dst }, motorOptionSize_ + 1, ReceiveGetMotorConfigOptions);
        }
        private void SendSetMotorConfigOptions(byte dst, MotorOptions opt)
        {
            if (connectionGoTo_ != null)
            {
                List<byte> l = new List<byte>();
                l.Add((byte)'M');
                l.Add(dst);
                WriteMotorOptions(opt, l);
                SendCommand(connectionGoTo_, l.ToArray(), 1, ReceiveSetMotorConfigOptions);
            }
        }
        private void SendGetAdapterConfigOptions(byte dst)
        {
            if (connectionGoTo_ != null)
                SendCommand(connectionGoTo_, new byte[] { (byte)'O', dst }, adapterOptionSize_ + 1, ReceiveGetAdapterConfigOptions);
        }
        private void SendSetAdapterConfigOptions(byte dst, AdapterOptions opt)
        {
            if (connectionGoTo_ != null)
            {
                List<byte> l = new List<byte>();
                l.Add((byte)'E');
                l.Add(dst);
                WriteAdapterOptions(opt, l);
                SendCommand(connectionGoTo_, l.ToArray(), 1, ReceiveSetAdapterConfigOptions);
            }
        }

        private const int motorOptionSize_ = 29, adapterOptionSize_ = 48;
        private void ReceiveMotorConfigOptionsSizes(byte[] data)
        {
            if (motorOptionSize_ != BitConverter.ToInt16(data, 0) || adapterOptionSize_ != BitConverter.ToInt16(data, 2))
            {
                MessageBox.Show("Host and Controller Options Versions Mismatch", "Error", MessageBoxButtons.OK);
                SendCommand(connectionGoTo_, 'h', 4, this.ReceiveAltAzmResolution);
                return;
            }

            try
            {
                ConfigureMotor(M_ALT, settings_.AltMotorOptions);
                ConfigureMotor(M_AZM, settings_.AzmMotorOptions);
                ConfigureAdapter(A_ALT, settings_.AltAdapterOptions);
                ConfigureAdapter(A_AZM, settings_.AzmAdapterOptions);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void ReceiveSetMotorConfigOptions(byte[] data)
        {
            SendGetMotorConfigOptions(data[0]);
        }
        private void ReceiveSetAdapterConfigOptions(byte[] data)
        {
            SendGetAdapterConfigOptions(data[0]);
        }

        private void ReceiveGetMotorConfigOptions(byte[] data)
        {
            try
            {
                MotorOptions opt;
                switch (data[0])
                {
                default:
                case M_ALT: ReadMotorOptions(data, 1, out opt); settings_.AltMotorOptions = opt; break;
                case M_AZM: ReadMotorOptions(data, 1, out opt); settings_.AzmMotorOptions = opt; break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void ReceiveGetAdapterConfigOptions(byte[] data)
        {
            try
            {
                AdapterOptions opt;
                switch (data[0])
                {
                default:
                case A_ALT:
                    ReadAdapterOptions(data, 1, out opt);
                    settings_.AltAdapterOptions = opt;
                    if (swapAzmAltEncoders_)
                        azmRes_ = opt.encRes_;
                    else
                        altRes_ = opt.encRes_;
                    break;

                case A_AZM:
                    ReadAdapterOptions(data, 1, out opt);
                    settings_.AzmAdapterOptions = opt;
                    if (swapAzmAltEncoders_)
                        altRes_ = opt.encRes_;
                    else
                        azmRes_ = opt.encRes_;
                    break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public enum ApproximationType
        {
            LINEAR,
            EXPONENTIAL
        };
        public struct PWMProfile   // size = 4
        {
            public byte value_;        // required value
            public byte magnitude_;    // PWM magnitude
            public Int16 period_;      // PWM (and PID) period
        }
        public struct MotorOptions             // size = 4 + 4*4 + 1 + 4*2 = 29
        {
            public bool valid_;
            public Int32 encRes_;
            public float maxSpeedRPM_;
            public float Kp_, KiF_, Kd_;
            public ApproximationType approximationType_;
            public PWMProfile loProfile_, hiProfile_;
        }
        private void ReadMotorOptions(byte[] data, int offset, out MotorOptions opt)
        {
            opt = new MotorOptions();
            opt.valid_ = true;
            opt.encRes_ = BitConverter.ToInt32(data, offset);
            opt.maxSpeedRPM_ = BitConverter.ToSingle(data, offset + 4);
            opt.Kp_ = BitConverter.ToSingle(data, offset + 8);
            opt.KiF_ = BitConverter.ToSingle(data, offset + 12);
            opt.Kd_ = BitConverter.ToSingle(data, offset + 16);
            opt.approximationType_ = data[offset + 20] == 0 ? ApproximationType.LINEAR : ApproximationType.EXPONENTIAL;
            opt.loProfile_.value_ = data[offset + 21];
            opt.loProfile_.magnitude_ = data[offset + 22];
            opt.loProfile_.period_ = BitConverter.ToInt16(data, offset + 23);
            opt.hiProfile_.value_ = data[offset + 25];
            opt.hiProfile_.magnitude_ = data[offset + 26];
            opt.hiProfile_.period_ = BitConverter.ToInt16(data, offset + 27);
        }
        private void WriteMotorOptions(MotorOptions opt, List<byte> data)
        {
            data.AddRange(BitConverter.GetBytes(opt.encRes_));
            data.AddRange(BitConverter.GetBytes(opt.maxSpeedRPM_));
            data.AddRange(BitConverter.GetBytes(opt.Kp_));
            data.AddRange(BitConverter.GetBytes(opt.KiF_));
            data.AddRange(BitConverter.GetBytes(opt.Kd_));
            data.Add((byte)(opt.approximationType_ == ApproximationType.LINEAR ? 0 : 1));
            data.Add(opt.loProfile_.value_);
            data.Add(opt.loProfile_.magnitude_);
            data.AddRange(BitConverter.GetBytes(opt.loProfile_.period_));
            data.Add(opt.hiProfile_.value_);
            data.Add(opt.hiProfile_.magnitude_);
            data.AddRange(BitConverter.GetBytes(opt.hiProfile_.period_));
        }

        public struct AdapterOptions             // size = 4*12 = 48
        {
            public bool valid_;
            public Int32 encRes_;
            public float scopeToMotor_;
            public float deviationSpeedFactor_, KiF_, KdF_, KpFast2F_, KpFast3F_;
            public float diff2_, diff3_;
            public Int32 pidPollPeriod_;    // ms
            public Int32 adjustPidTmo_;     // ms
            public Int32 speedSmoothTime_;  // ms
        }
        private void ReadAdapterOptions(byte[] data, int offset, out AdapterOptions opt)
        {
            opt = new AdapterOptions();
            opt.valid_ = true;
            opt.encRes_ = BitConverter.ToInt32(data, offset);
            opt.scopeToMotor_ = BitConverter.ToSingle(data, offset + 4);
            opt.deviationSpeedFactor_ = BitConverter.ToSingle(data, offset + 8);
            opt.KiF_ = BitConverter.ToSingle(data, offset + 12);
            opt.KdF_ = BitConverter.ToSingle(data, offset + 16);
            opt.KpFast2F_ = BitConverter.ToSingle(data, offset + 20);
            opt.KpFast3F_ = BitConverter.ToSingle(data, offset + 24);
            opt.diff2_ = BitConverter.ToSingle(data, offset + 28);
            opt.diff3_ = BitConverter.ToSingle(data, offset + 32);
            opt.pidPollPeriod_ = BitConverter.ToInt32(data, offset + 36);
            opt.adjustPidTmo_ = BitConverter.ToInt32(data, offset + 40);
            opt.speedSmoothTime_ = BitConverter.ToInt32(data, offset + 44);
        }
        private void WriteAdapterOptions(AdapterOptions opt, List<byte> data)
        {
            data.AddRange(BitConverter.GetBytes(opt.encRes_));
            data.AddRange(BitConverter.GetBytes(opt.scopeToMotor_));
            data.AddRange(BitConverter.GetBytes(opt.deviationSpeedFactor_));
            data.AddRange(BitConverter.GetBytes(opt.KiF_));
            data.AddRange(BitConverter.GetBytes(opt.KdF_));
            data.AddRange(BitConverter.GetBytes(opt.KpFast2F_));
            data.AddRange(BitConverter.GetBytes(opt.KpFast3F_));
            data.AddRange(BitConverter.GetBytes(opt.diff2_));
            data.AddRange(BitConverter.GetBytes(opt.diff3_));
            data.AddRange(BitConverter.GetBytes(opt.pidPollPeriod_));
            data.AddRange(BitConverter.GetBytes(opt.adjustPidTmo_));
            data.AddRange(BitConverter.GetBytes(opt.speedSmoothTime_));
        }

        public bool CanConfigureMotorsAndAdapters()
        {
            return connectionGoTo_ != null;
        }
        public void ConfigureMotor(byte dst, MotorOptions opt)
        {
            if (opt.valid_)
                SendSetMotorConfigOptions(dst, opt);
            else
                SendGetMotorConfigOptions(dst);
        }
        public void ConfigureAdapter(byte dst, AdapterOptions opt)
        {
            if (opt.valid_)
                SendSetAdapterConfigOptions(dst, opt);
            else
                SendGetAdapterConfigOptions(dst);
        }

        // MOTOR/ADAPTER CONFIGURATION OPTIONS END

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
                {
                    StartTracking();
                }
                else
                {
                    SkyObjectPosCalc.SkyPosition prevTrackedObj = trackedObject_;
                    trackedObject_ = selectedObject_;
                    trackedOffsetRa_ = trackedOffsetDec_ = 0;
                    if (prevTrackedObj == null)
                    {
                        StartMotors();
                        TrackedObjectChanged(true);
                    }
                    else
                    {
                        tmoSendPos_.Restart();
                        SendNextPositions();
                        TrackedObjectChanged(false);
                    }
                }
                allowAutoTrack_ = true;
                UpdateUI();
            }
        }

        private void buttonTrackUp_Click(object sender, EventArgs e)
        {
            OffsetTrackingObject(0, arrowMoveSpeed_);
        }

        private void buttonTrackLeft_Click(object sender, EventArgs e)
        {
            if(oppositeHorzPositioningDir_)
                OffsetTrackingObject(arrowMoveSpeed_, 0);
            else
                OffsetTrackingObject(-arrowMoveSpeed_, 0);
        }

        private void buttonTrackDown_Click(object sender, EventArgs e)
        {
            OffsetTrackingObject(0, -arrowMoveSpeed_);
        }

        private void buttonTrackRight_Click(object sender, EventArgs e)
        {
            if (oppositeHorzPositioningDir_)
                OffsetTrackingObject(-arrowMoveSpeed_, 0);
            else
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
            {
                StopTracking();
                allowAutoTrack_ = false;
            }
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

        private void buttonArrowSpeed_Click(object sender, EventArgs e)
        {

        }
    }

    //Application settings wrapper class
    sealed public class ScopeGotoClientSettings
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

        public ClientCommonAPI.AlignmentConnectionData AlignmentConnectionGoTo
        {
            get { return (ClientCommonAPI.AlignmentConnectionData)profile_.GetValue(section_, "AlignmentConnectionGoTo", new ClientCommonAPI.AlignmentConnectionData()); }
            set { profile_.SetValue(section_, "AlignmentConnectionGoTo", value); }
        }

        public bool AutoTrack
        {
            get { return profile_.GetValue(sectionGoTo_, "AutoTrack", false); }
            set { profile_.SetValue(sectionGoTo_, "AutoTrack", value); }
        }

        public ScopeGotoClient.MotorOptions AltMotorOptions
        {
            get { return (ScopeGotoClient.MotorOptions)profile_.GetValue(sectionGoTo_, "AltMotorOptions", new ScopeGotoClient.MotorOptions()); }
            set
            {
                if (value.valid_)
                    profile_.SetValue(sectionGoTo_, "AltMotorOptions", value);
            }
        }

        public ScopeGotoClient.MotorOptions AzmMotorOptions
        {
            get { return (ScopeGotoClient.MotorOptions)profile_.GetValue(sectionGoTo_, "AzmMotorOptions", new ScopeGotoClient.MotorOptions()); }
            set
            {
                if (value.valid_)
                    profile_.SetValue(sectionGoTo_, "AzmMotorOptions", value);
            }
        }

        public ScopeGotoClient.AdapterOptions AltAdapterOptions
        {
            get { return (ScopeGotoClient.AdapterOptions)profile_.GetValue(sectionGoTo_, "AltAdapterOptions", new ScopeGotoClient.AdapterOptions()); }
            set
            {
                if (value.valid_)
                    profile_.SetValue(sectionGoTo_, "AltAdapterOptions", value);
            }
        }

        public ScopeGotoClient.AdapterOptions AzmAdapterOptions
        {
            get { return (ScopeGotoClient.AdapterOptions)profile_.GetValue(sectionGoTo_, "AzmAdapterOptions", new ScopeGotoClient.AdapterOptions()); }
            set
            {
                if (value.valid_)
                    profile_.SetValue(sectionGoTo_, "AzmAdapterOptions", value);
            }
        }

        public XmlBuffer Buffer()
        {
            return profile_.Buffer();
        }

        private const string section_ = "entries", sectionGoTo_ = "entriesGoTo";
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
