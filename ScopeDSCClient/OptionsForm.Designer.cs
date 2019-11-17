namespace ScopeDSCClient
{
    partial class OptionsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupBoxLocUnits = new System.Windows.Forms.GroupBox();
            this.radioButtonDegree = new System.Windows.Forms.RadioButton();
            this.radioButtonDMS = new System.Windows.Forms.RadioButton();
            this.textBoxLatSec = new System.Windows.Forms.TextBox();
            this.textBoxLatMin = new System.Windows.Forms.TextBox();
            this.labelLat = new System.Windows.Forms.Label();
            this.textBoxLatDeg = new System.Windows.Forms.TextBox();
            this.textBoxLonSec = new System.Windows.Forms.TextBox();
            this.textBoxLonMin = new System.Windows.Forms.TextBox();
            this.labelLon = new System.Windows.Forms.Label();
            this.textBoxLonDeg = new System.Windows.Forms.TextBox();
            this.comboBoxLocation = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.checkBoxShowNearestAzmRotation = new System.Windows.Forms.CheckBox();
            this.checkBoxConnectToStellarium = new System.Windows.Forms.CheckBox();
            this.labelStellariumTcpPort = new System.Windows.Forms.Label();
            this.textBoxStellariumTCPPort = new System.Windows.Forms.TextBox();
            this.checkBoxOppHorzPositionDirection = new System.Windows.Forms.CheckBox();
            this.checkBoxAutoTrack = new System.Windows.Forms.CheckBox();
            this.buttonScreenKbd = new ScopeDSCClientHelper.NoSelectButton();
            this.checkBoxLogging = new System.Windows.Forms.CheckBox();
            this.buttonSaveLog = new System.Windows.Forms.Button();
            this.checkBoxLoggingAZM = new System.Windows.Forms.CheckBox();
            this.comboBoxLoggingType0 = new System.Windows.Forms.ComboBox();
            this.comboBoxLoggingType1 = new System.Windows.Forms.ComboBox();
            this.groupBoxLocUnits.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(312, 547);
            this.buttonOK.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(201, 92);
            this.buttonOK.TabIndex = 17;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(583, 547);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(201, 92);
            this.buttonCancel.TabIndex = 18;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // groupBoxLocUnits
            // 
            this.groupBoxLocUnits.Controls.Add(this.radioButtonDegree);
            this.groupBoxLocUnits.Controls.Add(this.radioButtonDMS);
            this.groupBoxLocUnits.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.groupBoxLocUnits.Location = new System.Drawing.Point(292, 117);
            this.groupBoxLocUnits.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBoxLocUnits.Name = "groupBoxLocUnits";
            this.groupBoxLocUnits.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBoxLocUnits.Size = new System.Drawing.Size(492, 83);
            this.groupBoxLocUnits.TabIndex = 10;
            this.groupBoxLocUnits.TabStop = false;
            this.groupBoxLocUnits.Text = "Units";
            // 
            // radioButtonDegree
            // 
            this.radioButtonDegree.AutoSize = true;
            this.radioButtonDegree.Location = new System.Drawing.Point(290, 34);
            this.radioButtonDegree.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.radioButtonDegree.Name = "radioButtonDegree";
            this.radioButtonDegree.Size = new System.Drawing.Size(115, 33);
            this.radioButtonDegree.TabIndex = 1;
            this.radioButtonDegree.TabStop = true;
            this.radioButtonDegree.Text = "degree";
            this.radioButtonDegree.UseVisualStyleBackColor = true;
            this.radioButtonDegree.CheckedChanged += new System.EventHandler(this.radioButtonDegree_CheckedChanged);
            // 
            // radioButtonDMS
            // 
            this.radioButtonDMS.AutoSize = true;
            this.radioButtonDMS.Location = new System.Drawing.Point(34, 34);
            this.radioButtonDMS.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.radioButtonDMS.Name = "radioButtonDMS";
            this.radioButtonDMS.Size = new System.Drawing.Size(173, 33);
            this.radioButtonDMS.TabIndex = 0;
            this.radioButtonDMS.TabStop = true;
            this.radioButtonDMS.Text = "deg,min,sec";
            this.radioButtonDMS.UseVisualStyleBackColor = true;
            this.radioButtonDMS.CheckedChanged += new System.EventHandler(this.radioButtonDMS_CheckedChanged);
            // 
            // textBoxLatSec
            // 
            this.textBoxLatSec.Location = new System.Drawing.Point(184, 151);
            this.textBoxLatSec.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBoxLatSec.Name = "textBoxLatSec";
            this.textBoxLatSec.Size = new System.Drawing.Size(64, 35);
            this.textBoxLatSec.TabIndex = 5;
            this.textBoxLatSec.Text = "0";
            this.textBoxLatSec.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxLatSec.TextChanged += new System.EventHandler(this.textBoxLatSec_TextChanged);
            // 
            // textBoxLatMin
            // 
            this.textBoxLatMin.Location = new System.Drawing.Point(119, 151);
            this.textBoxLatMin.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBoxLatMin.Name = "textBoxLatMin";
            this.textBoxLatMin.Size = new System.Drawing.Size(54, 35);
            this.textBoxLatMin.TabIndex = 4;
            this.textBoxLatMin.Text = "0";
            this.textBoxLatMin.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxLatMin.TextChanged += new System.EventHandler(this.textBoxLatMin_TextChanged);
            // 
            // labelLat
            // 
            this.labelLat.AutoSize = true;
            this.labelLat.Location = new System.Drawing.Point(35, 117);
            this.labelLat.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelLat.Name = "labelLat";
            this.labelLat.Size = new System.Drawing.Size(106, 29);
            this.labelLat.TabIndex = 2;
            this.labelLat.Text = "Latitude";
            // 
            // textBoxLatDeg
            // 
            this.textBoxLatDeg.Location = new System.Drawing.Point(39, 151);
            this.textBoxLatDeg.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBoxLatDeg.Name = "textBoxLatDeg";
            this.textBoxLatDeg.Size = new System.Drawing.Size(70, 35);
            this.textBoxLatDeg.TabIndex = 3;
            this.textBoxLatDeg.Text = "0";
            this.textBoxLatDeg.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxLatDeg.TextChanged += new System.EventHandler(this.textBoxLatDeg_TextChanged);
            // 
            // textBoxLonSec
            // 
            this.textBoxLonSec.Location = new System.Drawing.Point(185, 237);
            this.textBoxLonSec.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBoxLonSec.Name = "textBoxLonSec";
            this.textBoxLonSec.Size = new System.Drawing.Size(62, 35);
            this.textBoxLonSec.TabIndex = 9;
            this.textBoxLonSec.Text = "0";
            this.textBoxLonSec.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxLonSec.TextChanged += new System.EventHandler(this.textBoxLonSec_TextChanged);
            // 
            // textBoxLonMin
            // 
            this.textBoxLonMin.Location = new System.Drawing.Point(121, 237);
            this.textBoxLonMin.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBoxLonMin.Name = "textBoxLonMin";
            this.textBoxLonMin.Size = new System.Drawing.Size(54, 35);
            this.textBoxLonMin.TabIndex = 8;
            this.textBoxLonMin.Text = "0";
            this.textBoxLonMin.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxLonMin.TextChanged += new System.EventHandler(this.textBoxLonMin_TextChanged);
            // 
            // labelLon
            // 
            this.labelLon.AutoSize = true;
            this.labelLon.Location = new System.Drawing.Point(35, 203);
            this.labelLon.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelLon.Name = "labelLon";
            this.labelLon.Size = new System.Drawing.Size(129, 29);
            this.labelLon.TabIndex = 6;
            this.labelLon.Text = "Longitude";
            // 
            // textBoxLonDeg
            // 
            this.textBoxLonDeg.Location = new System.Drawing.Point(40, 237);
            this.textBoxLonDeg.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBoxLonDeg.Name = "textBoxLonDeg";
            this.textBoxLonDeg.Size = new System.Drawing.Size(70, 35);
            this.textBoxLonDeg.TabIndex = 7;
            this.textBoxLonDeg.Text = "0";
            this.textBoxLonDeg.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxLonDeg.TextChanged += new System.EventHandler(this.textBoxLonDeg_TextChanged);
            // 
            // comboBoxLocation
            // 
            this.comboBoxLocation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLocation.FormattingEnabled = true;
            this.comboBoxLocation.IntegralHeight = false;
            this.comboBoxLocation.Location = new System.Drawing.Point(39, 55);
            this.comboBoxLocation.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.comboBoxLocation.MaxDropDownItems = 12;
            this.comboBoxLocation.Name = "comboBoxLocation";
            this.comboBoxLocation.Size = new System.Drawing.Size(745, 37);
            this.comboBoxLocation.TabIndex = 1;
            this.comboBoxLocation.SelectedIndexChanged += new System.EventHandler(this.comboBoxLocation_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(33, 21);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(112, 29);
            this.label5.TabIndex = 0;
            this.label5.Text = "Location";
            // 
            // checkBoxShowNearestAzmRotation
            // 
            this.checkBoxShowNearestAzmRotation.AutoSize = true;
            this.checkBoxShowNearestAzmRotation.Location = new System.Drawing.Point(292, 220);
            this.checkBoxShowNearestAzmRotation.Name = "checkBoxShowNearestAzmRotation";
            this.checkBoxShowNearestAzmRotation.Size = new System.Drawing.Size(284, 33);
            this.checkBoxShowNearestAzmRotation.TabIndex = 11;
            this.checkBoxShowNearestAzmRotation.Text = "Nearest Azm Rotation";
            this.checkBoxShowNearestAzmRotation.UseVisualStyleBackColor = true;
            this.checkBoxShowNearestAzmRotation.CheckedChanged += new System.EventHandler(this.checkBoxShowNearestAzmRotation_CheckedChanged);
            // 
            // checkBoxConnectToStellarium
            // 
            this.checkBoxConnectToStellarium.AutoSize = true;
            this.checkBoxConnectToStellarium.Location = new System.Drawing.Point(292, 259);
            this.checkBoxConnectToStellarium.Name = "checkBoxConnectToStellarium";
            this.checkBoxConnectToStellarium.Size = new System.Drawing.Size(282, 33);
            this.checkBoxConnectToStellarium.TabIndex = 12;
            this.checkBoxConnectToStellarium.Text = "Connect to Stellarium";
            this.checkBoxConnectToStellarium.UseVisualStyleBackColor = true;
            this.checkBoxConnectToStellarium.CheckedChanged += new System.EventHandler(this.checkBoxConnectToStellarium_CheckedChanged);
            // 
            // labelStellariumTcpPort
            // 
            this.labelStellariumTcpPort.AutoSize = true;
            this.labelStellariumTcpPort.Location = new System.Drawing.Point(346, 300);
            this.labelStellariumTcpPort.Name = "labelStellariumTcpPort";
            this.labelStellariumTcpPort.Size = new System.Drawing.Size(127, 29);
            this.labelStellariumTcpPort.TabIndex = 13;
            this.labelStellariumTcpPort.Text = "TCP Port:";
            // 
            // textBoxStellariumTCPPort
            // 
            this.textBoxStellariumTCPPort.Location = new System.Drawing.Point(516, 297);
            this.textBoxStellariumTCPPort.Name = "textBoxStellariumTCPPort";
            this.textBoxStellariumTCPPort.Size = new System.Drawing.Size(100, 35);
            this.textBoxStellariumTCPPort.TabIndex = 14;
            this.textBoxStellariumTCPPort.TextChanged += new System.EventHandler(this.textBoxStellariumTCPPort_TextChanged);
            // 
            // checkBoxOppHorzPositionDirection
            // 
            this.checkBoxOppHorzPositionDirection.AutoSize = true;
            this.checkBoxOppHorzPositionDirection.Location = new System.Drawing.Point(292, 349);
            this.checkBoxOppHorzPositionDirection.Name = "checkBoxOppHorzPositionDirection";
            this.checkBoxOppHorzPositionDirection.Size = new System.Drawing.Size(402, 33);
            this.checkBoxOppHorzPositionDirection.TabIndex = 15;
            this.checkBoxOppHorzPositionDirection.Text = "Opposite Horizontal Positioning";
            this.checkBoxOppHorzPositionDirection.UseVisualStyleBackColor = true;
            this.checkBoxOppHorzPositionDirection.CheckedChanged += new System.EventHandler(this.checkBoxOppHorzPositionDirection_CheckedChanged);
            // 
            // checkBoxAutoTrack
            // 
            this.checkBoxAutoTrack.AutoSize = true;
            this.checkBoxAutoTrack.Location = new System.Drawing.Point(292, 388);
            this.checkBoxAutoTrack.Name = "checkBoxAutoTrack";
            this.checkBoxAutoTrack.Size = new System.Drawing.Size(157, 33);
            this.checkBoxAutoTrack.TabIndex = 19;
            this.checkBoxAutoTrack.Text = "Auto Track";
            this.checkBoxAutoTrack.UseVisualStyleBackColor = true;
            this.checkBoxAutoTrack.CheckedChanged += new System.EventHandler(this.checkBoxAutoTrack_CheckedChanged);
            // 
            // buttonScreenKbd
            // 
            this.buttonScreenKbd.Font = new System.Drawing.Font("Wingdings", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.buttonScreenKbd.Location = new System.Drawing.Point(40, 547);
            this.buttonScreenKbd.Name = "buttonScreenKbd";
            this.buttonScreenKbd.Size = new System.Drawing.Size(114, 92);
            this.buttonScreenKbd.TabIndex = 16;
            this.buttonScreenKbd.Text = "7";
            this.buttonScreenKbd.UseVisualStyleBackColor = true;
            this.buttonScreenKbd.Click += new System.EventHandler(this.buttonScreenKbd_Click);
            // 
            // checkBoxLogging
            // 
            this.checkBoxLogging.AutoSize = true;
            this.checkBoxLogging.Location = new System.Drawing.Point(135, 456);
            this.checkBoxLogging.Name = "checkBoxLogging";
            this.checkBoxLogging.Size = new System.Drawing.Size(76, 33);
            this.checkBoxLogging.TabIndex = 20;
            this.checkBoxLogging.Text = "Log";
            this.checkBoxLogging.UseVisualStyleBackColor = true;
            this.checkBoxLogging.CheckedChanged += new System.EventHandler(this.checkBoxLogging_CheckedChanged);
            // 
            // buttonSaveLog
            // 
            this.buttonSaveLog.Location = new System.Drawing.Point(621, 449);
            this.buttonSaveLog.Name = "buttonSaveLog";
            this.buttonSaveLog.Size = new System.Drawing.Size(163, 46);
            this.buttonSaveLog.TabIndex = 21;
            this.buttonSaveLog.Text = "Save Log";
            this.buttonSaveLog.UseVisualStyleBackColor = true;
            this.buttonSaveLog.Click += new System.EventHandler(this.buttonSaveLog_Click);
            // 
            // checkBoxLoggingAZM
            // 
            this.checkBoxLoggingAZM.AutoSize = true;
            this.checkBoxLoggingAZM.Location = new System.Drawing.Point(227, 457);
            this.checkBoxLoggingAZM.Name = "checkBoxLoggingAZM";
            this.checkBoxLoggingAZM.Size = new System.Drawing.Size(84, 33);
            this.checkBoxLoggingAZM.TabIndex = 22;
            this.checkBoxLoggingAZM.Text = "AZM";
            this.checkBoxLoggingAZM.UseVisualStyleBackColor = true;
            this.checkBoxLoggingAZM.CheckedChanged += new System.EventHandler(this.checkBoxLoggingAZM_CheckedChanged);
            // 
            // comboBoxLoggingType0
            // 
            this.comboBoxLoggingType0.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLoggingType0.FormattingEnabled = true;
            this.comboBoxLoggingType0.Location = new System.Drawing.Point(324, 454);
            this.comboBoxLoggingType0.Name = "comboBoxLoggingType0";
            this.comboBoxLoggingType0.Size = new System.Drawing.Size(121, 37);
            this.comboBoxLoggingType0.TabIndex = 23;
            this.comboBoxLoggingType0.SelectedIndexChanged += new System.EventHandler(this.comboBoxLoggingType0_SelectedIndexChanged);
            // 
            // comboBoxLoggingType1
            // 
            this.comboBoxLoggingType1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLoggingType1.FormattingEnabled = true;
            this.comboBoxLoggingType1.Location = new System.Drawing.Point(467, 454);
            this.comboBoxLoggingType1.Name = "comboBoxLoggingType1";
            this.comboBoxLoggingType1.Size = new System.Drawing.Size(121, 37);
            this.comboBoxLoggingType1.TabIndex = 24;
            this.comboBoxLoggingType1.SelectedIndexChanged += new System.EventHandler(this.comboBoxLoggingType1_SelectedIndexChanged);
            // 
            // OptionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(15F, 29F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(828, 683);
            this.Controls.Add(this.comboBoxLoggingType1);
            this.Controls.Add(this.comboBoxLoggingType0);
            this.Controls.Add(this.checkBoxLoggingAZM);
            this.Controls.Add(this.buttonSaveLog);
            this.Controls.Add(this.checkBoxLogging);
            this.Controls.Add(this.checkBoxAutoTrack);
            this.Controls.Add(this.checkBoxOppHorzPositionDirection);
            this.Controls.Add(this.textBoxStellariumTCPPort);
            this.Controls.Add(this.labelStellariumTcpPort);
            this.Controls.Add(this.checkBoxConnectToStellarium);
            this.Controls.Add(this.checkBoxShowNearestAzmRotation);
            this.Controls.Add(this.buttonScreenKbd);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.comboBoxLocation);
            this.Controls.Add(this.groupBoxLocUnits);
            this.Controls.Add(this.textBoxLonSec);
            this.Controls.Add(this.textBoxLatSec);
            this.Controls.Add(this.textBoxLonMin);
            this.Controls.Add(this.textBoxLatMin);
            this.Controls.Add(this.labelLon);
            this.Controls.Add(this.textBoxLonDeg);
            this.Controls.Add(this.labelLat);
            this.Controls.Add(this.textBoxLatDeg);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Margin = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this.Name = "OptionsForm";
            this.Text = "Options";
            this.Load += new System.EventHandler(this.OptionsForm_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OptionsForm_FormClosing);
            this.groupBoxLocUnits.ResumeLayout(false);
            this.groupBoxLocUnits.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.GroupBox groupBoxLocUnits;
        private System.Windows.Forms.RadioButton radioButtonDegree;
        private System.Windows.Forms.RadioButton radioButtonDMS;
        private System.Windows.Forms.TextBox textBoxLatSec;
        private System.Windows.Forms.TextBox textBoxLatMin;
        private System.Windows.Forms.Label labelLat;
        private System.Windows.Forms.TextBox textBoxLatDeg;
        private System.Windows.Forms.TextBox textBoxLonSec;
        private System.Windows.Forms.TextBox textBoxLonMin;
        private System.Windows.Forms.Label labelLon;
        private System.Windows.Forms.TextBox textBoxLonDeg;
        private System.Windows.Forms.ComboBox comboBoxLocation;
        private System.Windows.Forms.Label label5;
        private ScopeDSCClientHelper.NoSelectButton buttonScreenKbd;
        private System.Windows.Forms.CheckBox checkBoxShowNearestAzmRotation;
        private System.Windows.Forms.CheckBox checkBoxConnectToStellarium;
        private System.Windows.Forms.Label labelStellariumTcpPort;
        private System.Windows.Forms.TextBox textBoxStellariumTCPPort;
        private System.Windows.Forms.CheckBox checkBoxOppHorzPositionDirection;
        private System.Windows.Forms.CheckBox checkBoxAutoTrack;
        private System.Windows.Forms.CheckBox checkBoxLogging;
        private System.Windows.Forms.Button buttonSaveLog;
        private System.Windows.Forms.CheckBox checkBoxLoggingAZM;
        private System.Windows.Forms.ComboBox comboBoxLoggingType0;
        private System.Windows.Forms.ComboBox comboBoxLoggingType1;
    }
}