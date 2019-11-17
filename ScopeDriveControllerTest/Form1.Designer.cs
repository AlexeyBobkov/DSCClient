namespace ScopeDriveControllerTest
{
    partial class TestForm
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
            this.components = new System.ComponentModel.Container();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.textBoxSpeed = new System.Windows.Forms.TextBox();
            this.labelSpeed = new System.Windows.Forms.Label();
            this.textBoxOutput = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.timerPoll = new System.Windows.Forms.Timer(this.components);
            this.radioButtonMAZM = new System.Windows.Forms.RadioButton();
            this.radioButtonAZM = new System.Windows.Forms.RadioButton();
            this.radioButtonMALT = new System.Windows.Forms.RadioButton();
            this.radioButtonALT = new System.Windows.Forms.RadioButton();
            this.buttonSetSpeed = new System.Windows.Forms.Button();
            this.buttonSetPos = new System.Windows.Forms.Button();
            this.labelPos = new System.Windows.Forms.Label();
            this.textBoxSetPos = new System.Windows.Forms.TextBox();
            this.checkBoxSetNextPos = new System.Windows.Forms.CheckBox();
            this.checkBoxLogging = new System.Windows.Forms.CheckBox();
            this.buttonSaveLog = new System.Windows.Forms.Button();
            this.comboBoxLoggingType0 = new System.Windows.Forms.ComboBox();
            this.comboBoxLoggingType1 = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // buttonConnect
            // 
            this.buttonConnect.Location = new System.Drawing.Point(248, 30);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(116, 52);
            this.buttonConnect.TabIndex = 0;
            this.buttonConnect.Text = "Connect";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            // 
            // textBoxSpeed
            // 
            this.textBoxSpeed.Location = new System.Drawing.Point(46, 168);
            this.textBoxSpeed.Name = "textBoxSpeed";
            this.textBoxSpeed.Size = new System.Drawing.Size(100, 20);
            this.textBoxSpeed.TabIndex = 6;
            // 
            // labelSpeed
            // 
            this.labelSpeed.AutoSize = true;
            this.labelSpeed.Location = new System.Drawing.Point(43, 152);
            this.labelSpeed.Name = "labelSpeed";
            this.labelSpeed.Size = new System.Drawing.Size(64, 13);
            this.labelSpeed.TabIndex = 5;
            this.labelSpeed.Text = "Speed (rpm)";
            // 
            // textBoxOutput
            // 
            this.textBoxOutput.Location = new System.Drawing.Point(24, 250);
            this.textBoxOutput.Multiline = true;
            this.textBoxOutput.Name = "textBoxOutput";
            this.textBoxOutput.ReadOnly = true;
            this.textBoxOutput.Size = new System.Drawing.Size(556, 354);
            this.textBoxOutput.TabIndex = 16;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(21, 234);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 13);
            this.label2.TabIndex = 15;
            this.label2.Text = "Result";
            // 
            // timerPoll
            // 
            this.timerPoll.Enabled = true;
            this.timerPoll.Interval = 250;
            this.timerPoll.Tick += new System.EventHandler(this.timerPoll_Tick);
            // 
            // radioButtonMAZM
            // 
            this.radioButtonMAZM.AutoSize = true;
            this.radioButtonMAZM.Location = new System.Drawing.Point(127, 117);
            this.radioButtonMAZM.Name = "radioButtonMAZM";
            this.radioButtonMAZM.Size = new System.Drawing.Size(73, 17);
            this.radioButtonMAZM.TabIndex = 1;
            this.radioButtonMAZM.TabStop = true;
            this.radioButtonMAZM.Text = "motor azm";
            this.radioButtonMAZM.UseVisualStyleBackColor = true;
            this.radioButtonMAZM.CheckedChanged += new System.EventHandler(this.radioButtonMAZM_CheckedChanged);
            // 
            // radioButtonAZM
            // 
            this.radioButtonAZM.AutoSize = true;
            this.radioButtonAZM.Location = new System.Drawing.Point(215, 117);
            this.radioButtonAZM.Name = "radioButtonAZM";
            this.radioButtonAZM.Size = new System.Drawing.Size(76, 17);
            this.radioButtonAZM.TabIndex = 2;
            this.radioButtonAZM.TabStop = true;
            this.radioButtonAZM.Text = "scope azm";
            this.radioButtonAZM.UseVisualStyleBackColor = true;
            this.radioButtonAZM.CheckedChanged += new System.EventHandler(this.radioButtonAZM_CheckedChanged);
            // 
            // radioButtonMALT
            // 
            this.radioButtonMALT.AutoSize = true;
            this.radioButtonMALT.Location = new System.Drawing.Point(303, 117);
            this.radioButtonMALT.Name = "radioButtonMALT";
            this.radioButtonMALT.Size = new System.Drawing.Size(65, 17);
            this.radioButtonMALT.TabIndex = 3;
            this.radioButtonMALT.TabStop = true;
            this.radioButtonMALT.Text = "motor alt";
            this.radioButtonMALT.UseVisualStyleBackColor = true;
            this.radioButtonMALT.CheckedChanged += new System.EventHandler(this.radioButtonMALT_CheckedChanged);
            // 
            // radioButtonALT
            // 
            this.radioButtonALT.AutoSize = true;
            this.radioButtonALT.Location = new System.Drawing.Point(391, 117);
            this.radioButtonALT.Name = "radioButtonALT";
            this.radioButtonALT.Size = new System.Drawing.Size(68, 17);
            this.radioButtonALT.TabIndex = 4;
            this.radioButtonALT.TabStop = true;
            this.radioButtonALT.Text = "scope alt";
            this.radioButtonALT.UseVisualStyleBackColor = true;
            this.radioButtonALT.CheckedChanged += new System.EventHandler(this.radioButtonALT_CheckedChanged);
            // 
            // buttonSetSpeed
            // 
            this.buttonSetSpeed.Location = new System.Drawing.Point(152, 168);
            this.buttonSetSpeed.Name = "buttonSetSpeed";
            this.buttonSetSpeed.Size = new System.Drawing.Size(59, 20);
            this.buttonSetSpeed.TabIndex = 7;
            this.buttonSetSpeed.Text = "Set";
            this.buttonSetSpeed.UseVisualStyleBackColor = true;
            this.buttonSetSpeed.Click += new System.EventHandler(this.buttonSetSpeed_Click);
            // 
            // buttonSetPos
            // 
            this.buttonSetPos.Location = new System.Drawing.Point(354, 168);
            this.buttonSetPos.Name = "buttonSetPos";
            this.buttonSetPos.Size = new System.Drawing.Size(59, 20);
            this.buttonSetPos.TabIndex = 10;
            this.buttonSetPos.Text = "Set";
            this.buttonSetPos.UseVisualStyleBackColor = true;
            this.buttonSetPos.Click += new System.EventHandler(this.buttonSetPos_Click);
            // 
            // labelPos
            // 
            this.labelPos.AutoSize = true;
            this.labelPos.Location = new System.Drawing.Point(245, 152);
            this.labelPos.Name = "labelPos";
            this.labelPos.Size = new System.Drawing.Size(97, 13);
            this.labelPos.TabIndex = 8;
            this.labelPos.Text = "Position shift (units)";
            // 
            // textBoxSetPos
            // 
            this.textBoxSetPos.Location = new System.Drawing.Point(248, 168);
            this.textBoxSetPos.Name = "textBoxSetPos";
            this.textBoxSetPos.Size = new System.Drawing.Size(100, 20);
            this.textBoxSetPos.TabIndex = 9;
            // 
            // checkBoxSetNextPos
            // 
            this.checkBoxSetNextPos.AutoSize = true;
            this.checkBoxSetNextPos.Location = new System.Drawing.Point(438, 170);
            this.checkBoxSetNextPos.Name = "checkBoxSetNextPos";
            this.checkBoxSetNextPos.Size = new System.Drawing.Size(121, 17);
            this.checkBoxSetNextPos.TabIndex = 11;
            this.checkBoxSetNextPos.Text = "SetNextPos by timer";
            this.checkBoxSetNextPos.UseVisualStyleBackColor = true;
            // 
            // checkBoxLogging
            // 
            this.checkBoxLogging.AutoSize = true;
            this.checkBoxLogging.Location = new System.Drawing.Point(46, 210);
            this.checkBoxLogging.Name = "checkBoxLogging";
            this.checkBoxLogging.Size = new System.Drawing.Size(64, 17);
            this.checkBoxLogging.TabIndex = 12;
            this.checkBoxLogging.Text = "Logging";
            this.checkBoxLogging.UseVisualStyleBackColor = true;
            this.checkBoxLogging.CheckedChanged += new System.EventHandler(this.checkBoxLogging_CheckedChanged);
            // 
            // buttonSaveLog
            // 
            this.buttonSaveLog.Location = new System.Drawing.Point(475, 207);
            this.buttonSaveLog.Name = "buttonSaveLog";
            this.buttonSaveLog.Size = new System.Drawing.Size(76, 23);
            this.buttonSaveLog.TabIndex = 14;
            this.buttonSaveLog.Text = "Save Log";
            this.buttonSaveLog.UseVisualStyleBackColor = true;
            this.buttonSaveLog.Click += new System.EventHandler(this.buttonSaveLog_Click);
            // 
            // comboBoxLoggingType0
            // 
            this.comboBoxLoggingType0.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLoggingType0.FormattingEnabled = true;
            this.comboBoxLoggingType0.Location = new System.Drawing.Point(182, 208);
            this.comboBoxLoggingType0.Name = "comboBoxLoggingType0";
            this.comboBoxLoggingType0.Size = new System.Drawing.Size(121, 21);
            this.comboBoxLoggingType0.TabIndex = 13;
            this.comboBoxLoggingType0.SelectedIndexChanged += new System.EventHandler(this.comboBoxLoggingType0_SelectedIndexChanged);
            // 
            // comboBoxLoggingType1
            // 
            this.comboBoxLoggingType1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLoggingType1.FormattingEnabled = true;
            this.comboBoxLoggingType1.Location = new System.Drawing.Point(329, 208);
            this.comboBoxLoggingType1.Name = "comboBoxLoggingType1";
            this.comboBoxLoggingType1.Size = new System.Drawing.Size(121, 21);
            this.comboBoxLoggingType1.TabIndex = 17;
            this.comboBoxLoggingType1.SelectedIndexChanged += new System.EventHandler(this.comboBoxLoggingType1_SelectedIndexChanged);
            // 
            // TestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(603, 629);
            this.Controls.Add(this.comboBoxLoggingType1);
            this.Controls.Add(this.comboBoxLoggingType0);
            this.Controls.Add(this.buttonSaveLog);
            this.Controls.Add(this.checkBoxLogging);
            this.Controls.Add(this.checkBoxSetNextPos);
            this.Controls.Add(this.buttonSetPos);
            this.Controls.Add(this.labelPos);
            this.Controls.Add(this.textBoxSetPos);
            this.Controls.Add(this.buttonSetSpeed);
            this.Controls.Add(this.radioButtonALT);
            this.Controls.Add(this.radioButtonMALT);
            this.Controls.Add(this.radioButtonAZM);
            this.Controls.Add(this.radioButtonMAZM);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxOutput);
            this.Controls.Add(this.labelSpeed);
            this.Controls.Add(this.textBoxSpeed);
            this.Controls.Add(this.buttonConnect);
            this.Name = "TestForm";
            this.Text = "Debug";
            this.Load += new System.EventHandler(this.TestForm_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.TextBox textBoxSpeed;
        private System.Windows.Forms.Label labelSpeed;
        private System.Windows.Forms.TextBox textBoxOutput;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Timer timerPoll;
        private System.Windows.Forms.RadioButton radioButtonMAZM;
        private System.Windows.Forms.RadioButton radioButtonAZM;
        private System.Windows.Forms.RadioButton radioButtonMALT;
        private System.Windows.Forms.RadioButton radioButtonALT;
        private System.Windows.Forms.Button buttonSetSpeed;
        private System.Windows.Forms.Button buttonSetPos;
        private System.Windows.Forms.Label labelPos;
        private System.Windows.Forms.TextBox textBoxSetPos;
        private System.Windows.Forms.CheckBox checkBoxSetNextPos;
        private System.Windows.Forms.CheckBox checkBoxLogging;
        private System.Windows.Forms.Button buttonSaveLog;
        private System.Windows.Forms.ComboBox comboBoxLoggingType0;
        private System.Windows.Forms.ComboBox comboBoxLoggingType1;
    }
}

