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
    public partial class ScopeDSCClient : Form
    {
        private ScopeDSCClientSettings settings_ = new ScopeDSCClientSettings();

        private bool fullScreen_ = false;
        private bool nightMode_ = false;
        private UInt16 timerCnt_;
        private bool swapAzmAltEncoders_ = false;
        private int azmRes_, altRes_, equRes_;
        private int azmPos_, altPos_, equPos_;
        private int errorCnt_ = 0;
        private bool positionsValid_ = false;
        private int azmOffset_ = 0, altOffset_ = 0;
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
        private AlignmentConnectionData alignmentConnectionAltAzm_;
        private AlignmentConnectionData alignmentConnectionEqu_;

        private const int LAST_OBJ_COUNT = 10;
        private SkyObjectPosCalc.SkyPosition object_;
        private SkyObjectPosCalc.SkyPosition[] lastObjects_ = new SkyObjectPosCalc.SkyPosition[LAST_OBJ_COUNT];

        private bool posTextChanged_ = true;
        private bool connectionAndAlignTextChanged_ = true;
        private bool scopePosAndObjTextChanged_ = true;
        private bool objectNameChanged_ = true;

        // connection capabilities flags
        private const byte CONNCAPS_ALTAZM = 1;
        private const byte CONNCAPS_EQU = 2;
        private const byte CONNCAPS_GPS = 4;

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
        private ConnectionData connectionAltAzm_;
        private ConnectionData connectionEqu_;
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
            public InitConnectionHandler(ScopeDSCClient parent, ConnectionData connectionData)
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

                if ((data[0] & CONNCAPS_ALTAZM) != 0)
                    parent_.BeginInvoke(new SetConnectionDelegate(parent_.SetAltAzmConnection), new object[] { connectionData_ });

                if ((data[0] & CONNCAPS_EQU) != 0)
                    parent_.BeginInvoke(new SetConnectionDelegate(parent_.SetEquConnection), new object[] { connectionData_ });

                if ((data[0] & CONNCAPS_GPS) != 0)
                    parent_.BeginInvoke(new SetConnectionDelegate(parent_.SetGPSConnection), new object[] { connectionData_ });
            }

            private ScopeDSCClient parent_;
            private ConnectionData connectionData_;
        }

        private class BaseConnectionHandler : SerialConnection.IReceiveHandler
        {
            public BaseConnectionHandler(ScopeDSCClient parent, ReceiveDelegate receiveDelegate, SerialConnection connection)
            {
                parent_ = parent;
                receiveDelegate_ = receiveDelegate;
                connection_ = connection;
            }

            public void Error()
            {
                TimeoutDelegate d = new TimeoutDelegate(parent_.SerialError);
                parent_.BeginInvoke(d, new object[] {connection_});
            }

            public void Received(byte[] data)
            {
                parent_.BeginInvoke(receiveDelegate_, new object[] {data});
            }

            private ScopeDSCClient parent_;
            private ReceiveDelegate receiveDelegate_;
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
            get { return (double)(azmPos_ + azmOffset_) * 2 * Math.PI / (azmRes_ != 0 ? (double)azmRes_ : 1); }
        }

        public double AltAngle
        {
            get { return (double)(altPos_ + altOffset_) * 2 * Math.PI / (altRes_ != 0 ? (double)altRes_ : 1); }
        }

        public double EquAngle
        {
            get { return (double)equPos_ * 2 * Math.PI / (equRes_ != 0 ? (double)equRes_ : 1) - Math.PI; }
        }

        private class ScopePositions : ClientCommonAPI.IScopePositions
        {
            public ScopePositions(ScopeDSCClient parent) { parent_ = parent; }
            public double AzmAngle { get { return parent_.AzmAngle; } }
            public double AltAngle { get { return parent_.AltAngle; } }
            public double EquAngle { get { return parent_.EquAngle; } }

            private ScopeDSCClient parent_;
        }
        
        private void SetPositionText()
        {
            if (!posTextChanged_)
                return;
            posTextChanged_ = false;
            
            string s = "";
            if (connectionAltAzm_ == null || connectionEqu_ == null || alignment_ == null || !alignment_.IsAligned || object_ == null)
            {
                s += "--, --";
            }
            else
            {
                double d = ClientCommonAPI.CalcTime();
                double azm, alt;
                object_.CalcAzimuthal(d, latitude_, longitude_, out azm, out alt);
                PairA objScope = alignment_.Horz2Scope(new PairA(azm * Const.toRad, alt * Const.toRad), EquAngle);

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

            if (connectionAltAzm_ == null)
                s += "Alt-Azm not Connected";
            else
                s += "Alt-Azm Connected to " + connectionAltAzm_.connection_.PortName + " at " + connectionAltAzm_.connection_.BaudRate;
            s += Environment.NewLine;

            if (connectionEqu_ == null)
                s += "Equ not Connected";
            else
                s += "Equ Connected to " + connectionEqu_.connection_.PortName + " at " + connectionEqu_.connection_.BaudRate;
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
            if (connectionAltAzm_ == null)
                s += "Encoder Abs Positions Unknown" + Environment.NewLine;
            else
            {
                s += "Encoder Abs Positions: Azm = " + ClientCommonAPI.PrintAngle(AzmAngle * Const.toDeg, false, false) + ", Alt = " + ClientCommonAPI.PrintAngle(AltAngle * Const.toDeg, false, false) + Environment.NewLine;
                s += "ErrCnt = " + errorCnt_ + Environment.NewLine;
            }

            if (connectionEqu_ == null)
                s += "Equ Angle Unknown" + Environment.NewLine;
            else
                s += "Equ Angle = " + ClientCommonAPI.PrintAngle(EquAngle * Const.toDeg) + Environment.NewLine;

            s += Environment.NewLine;
            if (connectionAltAzm_ == null || connectionEqu_ == null)
                s += "Scope Position Unknown";
            else if (alignment_ != null && alignment_.IsAligned)
            {
                PairA horz = alignment_.Scope2Horz(new PairA(AzmAngle, AltAngle), EquAngle);

                s += "Scope Position: Azm = " + ClientCommonAPI.PrintAngle(SkyObjectPosCalc.Rev(horz.Azm * Const.toDeg), false, false);
                s += ", Alt = " + ClientCommonAPI.PrintAngle(SkyObjectPosCalc.Rev(horz.Alt * Const.toDeg), false, false) + Environment.NewLine;

                double dec, ra;
                SkyObjectPosCalc.AzAlt2Equ(d, latitude_, longitude_, SkyObjectPosCalc.Rev(horz.Azm * Const.toDeg), SkyObjectPosCalc.Rev(horz.Alt * Const.toDeg), out dec, out ra);
                s += "R.A.\t= " + ClientCommonAPI.PrintTime(ra) + " (" + ra.ToString("F5") + "\x00B0)" + Environment.NewLine;
                s += "Dec.\t= " + ClientCommonAPI.PrintAngle(dec, true) + " (" + ClientCommonAPI.PrintDec(dec, "F5") + "\x00B0)" + Environment.NewLine;

                if (sendPositionToStellarium && stellariumConnection_ != null && stellariumConnection_.IsConnected)
                    stellariumConnection_.SendPosition(dec, ra);
            }

            s += Environment.NewLine;
            if (object_ == null)
                s += "No Object Selected";
            else
            {
                double azm, alt;
                object_.CalcAzimuthal(d, latitude_, longitude_, out azm, out alt);
                s += object_.Name + ": Azm = " + ClientCommonAPI.PrintAngle(azm, false, false);
                s += ", Alt = " + ClientCommonAPI.PrintAngle(alt, false, false) + Environment.NewLine;

                double dec, ra;
                object_.CalcTopoRaDec(d, latitude_, longitude_, out dec, out ra);
                s += "R.A.\t= " + ClientCommonAPI.PrintTime(ra) + " (" + ra.ToString("F5") + "\x00B0)" + Environment.NewLine;
                s += "Dec.\t= " + ClientCommonAPI.PrintAngle(dec, true) + " (" + ClientCommonAPI.PrintDec(dec, "F5") + "\x00B0)" + Environment.NewLine;

                /*
                double azm, alt;
                object_.CalcAzimuthal(d, latitude_, longitude_, out azm, out alt);
                s += "Azm.\t= " + ScopeDSCClient.PrintAngle(azm) + " (" + azm.ToString("F5") + "\x00B0)" + Environment.NewLine;
                s += "Alt.\t= " + ScopeDSCClient.PrintAngle(alt) + " (" + alt.ToString("F5") + "\x00B0)";
                 * */
            }

            textBoxObject.Text = s;
        }

        private void SetObjectNameLabelText()
        {
            if (!objectNameChanged_)
                return;
            objectNameChanged_ = false;
            objectNameLabel.Text = (object_ != null) ? object_.Name : "";
        }

        private void UpdateUI()
        {
            UpdateUI(false);
        }
        private void UpdateUI(bool sendPositionToStellarium)
        {
            if (connectionAltAzm_ == null || connectionEqu_ == null)
            {
                buttonAlign.Text = "Alignment (Connect to Scope First)";
                buttonAlign.Enabled = false;
            }
            else
            {
                buttonAlign.Text = "Alignment";
                buttonAlign.Enabled = true;
            }

            if (connectionEqu_ == null)
            {
                buttonTrackStars.Enabled = false;
                buttonTrackMoon.Enabled = false;
                buttonTrackSun.Enabled = false;
                buttonRewind.Enabled = false;
                buttonGoToCenter.Enabled = false;
                buttonFastFwd.Enabled = false;
                buttonStop.Enabled = false;
            }
            else
            {
                buttonTrackStars.Enabled = true;
                buttonTrackMoon.Enabled = true;
                buttonTrackSun.Enabled = true;
                buttonRewind.Enabled = true;
                buttonGoToCenter.Enabled = true;
                buttonFastFwd.Enabled = true;
                buttonStop.Enabled = true;
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

        private void ConnectionChangedAltAzm()
        {
            posTextChanged_ = true;
            connectionAndAlignTextChanged_ = true;
            scopePosAndObjTextChanged_ = true;

            if (alignment_ != null &&
                connectionAltAzm_ != null &&
                connectionAltAzm_.connection_ != null &&
                (connectionAltAzm_.connection_.PortName != alignmentConnectionAltAzm_.PortName ||
                 connectionAltAzm_.sessionId_ != alignmentConnectionAltAzm_.SessionId))
            {
                alignment_ = null;
                SaveAlignment();
                AlignmentChanged();
            }
        }

        private void ConnectionChangedEqu()
        {
            posTextChanged_ = true;
            connectionAndAlignTextChanged_ = true;
            scopePosAndObjTextChanged_ = true;

            // Not necessary, because Equ is persistent and always reports correct angle.
            /*
            if (alignment_ != null && 
                connectionEqu_ != null &&
                connectionEqu_.connection_ != null &&
                (connectionEqu_.connection_.PortName != alignmentConnectionEqu_.PortName ||
                 connectionEqu_.sessionId_ != alignmentConnectionEqu_.SessionId))
            {
                alignment_ = null;
                SaveAlignment();
                AlignmentChanged();
            }
            */
        }

        private void ConnectionChangedGPS()
        {
            connectionAndAlignTextChanged_ = true;
        }

        private void AlignmentChanged()
        {
            posTextChanged_ = true;
            connectionAndAlignTextChanged_ = true;
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

        private void ErrorCntChanged()
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
                    if (connectionAltAzm_ != null && connectionAltAzm_.connection_ != null)
                        settings_.AlignmentConnectionAltAzm = alignmentConnectionAltAzm_ = new AlignmentConnectionData(connectionAltAzm_.connection_.PortName, connectionAltAzm_.sessionId_);
                    if (connectionEqu_ != null && connectionEqu_.connection_ != null)
                        settings_.AlignmentConnectionEqu = alignmentConnectionEqu_ = new AlignmentConnectionData(connectionEqu_.connection_.PortName, connectionEqu_.sessionId_);
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

                    alignmentConnectionAltAzm_ = settings_.AlignmentConnectionAltAzm;
                    alignmentConnectionEqu_ = settings_.AlignmentConnectionEqu;
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
        public ScopeDSCClient()
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
            for (int i = lastObjects_.Length; --i >= 0; )
            {
                if (lastObjects_[i] != null && obj.Name == lastObjects_[i].Name)
                {
                    // move to first position
                    lastObjects_[i] = lastObjects_[0];
                    lastObjects_[0] = obj;
                    return;
                }
            }
                
            for (int i = lastObjects_.Length; --i >= 1; )
                lastObjects_[i] = lastObjects_[i - 1];
            lastObjects_[0] = obj;
        }

        private void ScopeDSCClient_Load(object sender, EventArgs e)
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

            // Everything is changed! (Yes, it's redundant.)
            //OptionsOrTimeChanged();
            ConnectionChangedAltAzm();
            ConnectionChangedEqu();
            ConnectionChangedGPS();
            ObjectChanged();
            ScopePosChanged();
            ErrorCntChanged();
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
            SkyObjectForm form = new SkyObjectForm(nightMode_, latitude_, longitude_, database_, stellariumConnection_, lastObjects_, lastObjSettings_);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            object_ = form.UseStellarium ? stellariumObj_ : form.Object;
            if (object_ != null)
                AddLastObject(object_);
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
                SendCommand(connectionEqu_, 'e', 2, this.ReceiveEquPosition);
                break;
            case 2:
            case 6:
                SendCommand(connectionAltAzm_, 'p', 1, this.ReceiveErrorCnt);
                TimeChanged();
                break;
            case 1:
            case 3:
            case 5:
            case 7:
                SendCommand(connectionAltAzm_, 'y', 4, this.ReceiveAltAzmPosition);
                sendPositionToStellarium = true;
                break;
            }
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

        private void ScopeDSCClient_Layout(object sender, LayoutEventArgs e)
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

        private void SetAltAzmConnection(ConnectionData data)
        {
            if (connectionAltAzm_ != null)
            {
                if (connectionAltAzm_.connection_ == data.connection_)
                    return;

                if (connectionAltAzm_ != connectionEqu_ && connectionAltAzm_ != connectionGPS_)
                    CloseConnection(connectionAltAzm_.connection_);
            }

            connectionAltAzm_ = data;
            if (swapAzmAltEncoders_ != data.swapAzmAltEncoders_)
            {
                int tmp = azmOffset_;
                azmOffset_ = altOffset_;
                altOffset_ = tmp;
            }
            swapAzmAltEncoders_ = data.swapAzmAltEncoders_;

            //SendCommand(connectionAltAzm_, new byte[] { (byte)'z', (byte)(10000 & 0xff), (byte)(10000 / 256), (byte)(10000 & 0xff), (byte)(10000 / 256) }, 1, this.ReceiveAcknowlage);
            SendCommand(connectionAltAzm_, 'h', 4, this.ReceiveAltAzmResolution);

            ConnectionChangedAltAzm();
            UpdateUI();
        }
        private void SetEquConnection(ConnectionData data)
        {
            if (connectionEqu_ != null)
            {
                if (connectionEqu_.connection_ == data.connection_)
                    return;

                if (connectionEqu_ != connectionAltAzm_ && connectionEqu_ != connectionGPS_)
                    CloseConnection(connectionEqu_.connection_);
            }

            connectionEqu_ = data;
            errorCnt_ = 0;
            SendCommand(connectionEqu_, 'q', 2, this.ReceiveEquResolution);

            ConnectionChangedEqu();
            UpdateUI();
        }
        private void SetGPSConnection(ConnectionData data)
        {
            if (connectionGPS_ != null)
            {
                if (connectionGPS_.connection_ == data.connection_)
                    return;

                if (connectionGPS_ != connectionAltAzm_ && connectionGPS_ != connectionEqu_)
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

        private void SendScopeMotionCommand(char cmd) { SendCommand(connectionEqu_, cmd, 1, ReceiveDummy); }

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

        private void ReceiveEquResolution(byte[] data)
        {
            equRes_ = (UInt16)((((UInt16)data[1]) << 8) + data[0]);
        }

        private void ReceiveAltAzmPosition(byte[] data)
        {
            int azmPos, altPos;
            if (swapAzmAltEncoders_)
            {
                azmPos = (UInt16)((((UInt16)data[1]) << 8) + data[0]);
                altPos = (UInt16)((((UInt16)data[3]) << 8) + data[2]);
            }
            else
            {
                altPos = (UInt16)((((UInt16)data[1]) << 8) + data[0]);
                azmPos = (UInt16)((((UInt16)data[3]) << 8) + data[2]);
            }
            /*
            if (!positionsValid_)
            {
                //azmOffset_ = azmRes_ / 2 - azmPos;
                //altOffset_ = altRes_ / 2 - altPos;
            }
            else
            {
                if (azmRes_ > 0)
                {
                    int diff = azmPos - azmPos_;
                    if (diff > azmRes_ / 2)
                        azmOffset_ -= azmRes_;
                    else if (diff < -azmRes_ / 2)
                        azmOffset_ += azmRes_;
                }
                if (altRes_ > 0)
                {
                    int diff = altPos - altPos_;
                    if (diff > altRes_ / 2)
                        altOffset_ -= altRes_;
                    else if (diff < -altRes_ / 2)
                        altOffset_ += altRes_;
                }
            }
            */
            if (azmPos_ != azmPos || altPos_ != altPos || !positionsValid_)
                ScopePosChanged();
            azmPos_ = azmPos;
            altPos_ = altPos;
            positionsValid_ = true;
        }

        private void ReceiveEquPosition(byte[] data)
        {
            int equPos = (UInt16)((((UInt16)data[1]) << 8) + data[0]);
            if(equPos_ != equPos)
                ScopePosChanged();
            equPos_ = equPos;
        }

        private void ReceiveErrorCnt(byte[] data)
        {
            if (data[0] > 0)
                ErrorCntChanged();
            errorCnt_ += data[0];
        }

        private void ReceiveDummy(byte[] data)
        {
        }

        private void CloseConnection(SerialConnection connection)
        {
            if (connection == null)
                return;

            if (connectionAltAzm_ != null && connectionAltAzm_.connection_ == connection)
            {
                connectionAltAzm_ = null;
                ConnectionChangedAltAzm();
            }

            if (connectionEqu_ != null && connectionEqu_.connection_ == connection)
            {
                connectionEqu_ = null;
                ConnectionChangedEqu();
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
            if (connectionAltAzm_ != null)
            {
                connectionAltAzm_ = null;
                ConnectionChangedAltAzm();
            }

            if (connectionEqu_ != null)
            {
                connectionEqu_ = null;
                ConnectionChangedEqu();
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

        private void ScopeDSCClient_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseAllConnections();
            CloseStellariumConnection();
            //settings_.Save();
        }

        private void buttonTrackStars_Click(object sender, EventArgs e)
        {
            SendScopeMotionCommand('1');
        }

        private void buttonTrackMoon_Click(object sender, EventArgs e)
        {
            SendScopeMotionCommand('2');
        }

        private void buttonTrackSun_Click(object sender, EventArgs e)
        {
            SendScopeMotionCommand('3');
        }

        private void buttonRewind_Click(object sender, EventArgs e)
        {
            SendScopeMotionCommand('*');
        }

        private void buttonGoToCenter_Click(object sender, EventArgs e)
        {
            SendScopeMotionCommand('8');
        }

        private void buttonFastFwd_Click(object sender, EventArgs e)
        {
            SendScopeMotionCommand('#');
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            SendScopeMotionCommand('0');
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
    sealed class ScopeDSCClientSettings
    {
        public ScopeDSCClientSettings()
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

        public ScopeDSCClient.AlignmentConnectionData AlignmentConnectionAltAzm
        {
            get { return (ScopeDSCClient.AlignmentConnectionData)profile_.GetValue(section_, "AlignmentConnectionAltAzm", new ScopeDSCClient.AlignmentConnectionData()); }
            set { profile_.SetValue(section_, "AlignmentConnectionAltAzm", value); }
        }

        public ScopeDSCClient.AlignmentConnectionData AlignmentConnectionEqu
        {
            get { return (ScopeDSCClient.AlignmentConnectionData)profile_.GetValue(section_, "AlignmentConnectionEqu", new ScopeDSCClient.AlignmentConnectionData()); }
            set { profile_.SetValue(section_, "AlignmentConnectionEqu", value); }
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

        private const string section_ = "entries";
        private XmlProfile profile_ = new XmlProfile();
    }
}

namespace ScopeDSCClientHelper
{
    public class NoSelectButton : Button
    {
        public NoSelectButton()
        {
            SetStyle(ControlStyles.Selectable, false);
        }
    }
}
