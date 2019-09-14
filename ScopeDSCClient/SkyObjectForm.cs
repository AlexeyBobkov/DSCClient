using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SkyObjectPosition;
using StellariumServer;

namespace ScopeDSCClient
{
    public partial class SkyObjectForm : Form
    {
        public struct LastSettings
        {
            public ObjectFromListForm.LastSettings objListLastSettings_;
            public ObjectFromCoordinatesForm.LastSettings objCoordSettings_;
            public static LastSettings Default
            {
                get
                {
                    return new LastSettings
                    {
                        objListLastSettings_ = ObjectFromListForm.LastSettings.Default,
                        objCoordSettings_ = ObjectFromCoordinatesForm.LastSettings.Default
                    };
                }
            }
        }

        public SkyObjectForm   (ScopeDSCClient parent,
                                bool nightMode,
                                double latitude,
                                double longitude,
                                List<ScopeDSCClient.ObjDatabaseEntry> database,
                                StellariumServer.Connection stellariumConnection,
                                SkyObjectPosCalc.SkyPosition[] lastObjects,
                                LastSettings settings)
        {
            nightMode_ = nightMode;
            parent_ = parent;
            latitude_ = latitude;
            longitude_ = longitude;
            database_ = database;
            stellariumConnection_ = stellariumConnection;
            lastObjects_ = lastObjects;
            settings_ = settings;
            InitializeComponent();
        }

        // implementation
        private bool init_ = false;
        private bool nightMode_ = false;
        private ScopeDSCClient parent_;
        private double latitude_, longitude_;
        private List<ScopeDSCClient.ObjDatabaseEntry> database_;
        private StellariumServer.Connection stellariumConnection_;
        private SkyObjectPosCalc.SkyPosition[] lastObjects_;
        private LastSettings settings_;
        private SkyObjectPosCalc.SkyPosition object_;
        private bool useStellarium_ = false;

        public SkyObjectPosCalc.SkyPosition Object { get { return object_; } }
        public bool UseStellarium { get { return useStellarium_; } }
        public LastSettings Settings { get { return settings_; } }

        private void buttonFromList_Click(object sender, EventArgs e)
        {
            ObjectFromListForm form = new ObjectFromListForm(parent_, nightMode_, latitude_, longitude_, database_, settings_.objListLastSettings_);
            if (form.ShowDialog() == DialogResult.OK)
            {
                object_ = form.Object;
                settings_.objListLastSettings_ = form.Settings;
                DialogResult = DialogResult.OK;
            }
        }

        private void buttonByCoordinates_Click(object sender, EventArgs e)
        {
            ObjectFromCoordinatesForm form = new ObjectFromCoordinatesForm(parent_, nightMode_, latitude_, longitude_, settings_.objCoordSettings_);
            if (form.ShowDialog() == DialogResult.OK)
            {
                object_ = form.Object;
                settings_.objCoordSettings_ = form.Settings;
                DialogResult = DialogResult.OK;
            }
        }

        private void buttonLastObj_Click(object sender, EventArgs e)
        {
            ObjectLastSelectionForm form = new ObjectLastSelectionForm(parent_, nightMode_, latitude_, longitude_, lastObjects_);
            if (form.ShowDialog() == DialogResult.OK)
            {
                object_ = form.Object;
                DialogResult = DialogResult.OK;
            }
        }

        private void buttonStellarium_Click(object sender, EventArgs e)
        {
            if (stellariumConnection_ != null && stellariumConnection_.IsConnected)
            {
                object_ = null;
                useStellarium_ = true;
                DialogResult = DialogResult.OK;
            }
        }

        private void SkyObjectForm_Load(object sender, EventArgs e)
        {
            if (nightMode_)
                ScopeDSCClient.EnterNightMode(this);

            buttonLastObj.Enabled = (lastObjects_.Length != 0 && lastObjects_[0] != null);
            if (stellariumConnection_ == null)
                buttonStellarium.Enabled = false;
            else
            {
                buttonStellarium.Enabled = stellariumConnection_.IsConnected;
                stellariumConnection_.StatusChanged += StellariumStatusChangedHandlerAsync;
            }

            init_ = true;
            CalcAndOutputResults();
        }

        private void SkyObjectForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (stellariumConnection_ != null)
                stellariumConnection_.StatusChanged -= StellariumStatusChangedHandlerAsync;
        }

        public void StellariumStatusChangedHandlerAsync()
        {
            this.BeginInvoke(new StellariumServer.Connection.StatusChangedHandler(this.StellariumStatusChangedHandler));
        }

        public void StellariumStatusChangedHandler()
        {
            buttonStellarium.Enabled = stellariumConnection_.IsConnected;
        }

        private void CalcAndOutputResults()
        {
            if (!init_)
                return;
        }
    }
}
