namespace ScopeDSCConfigureHW
{
    partial class ScopeDSCConfigureHW
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
            this.buttonConnection = new System.Windows.Forms.Button();
            this.textBoxConnection = new System.Windows.Forms.TextBox();
            this.checkBoxAltAzm = new System.Windows.Forms.CheckBox();
            this.checkBoxEqu = new System.Windows.Forms.CheckBox();
            this.checkBoxGPS = new System.Windows.Forms.CheckBox();
            this.buttonSave = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonConnection
            // 
            this.buttonConnection.Location = new System.Drawing.Point(37, 27);
            this.buttonConnection.Name = "buttonConnection";
            this.buttonConnection.Size = new System.Drawing.Size(218, 139);
            this.buttonConnection.TabIndex = 0;
            this.buttonConnection.Text = "Connection";
            this.buttonConnection.UseVisualStyleBackColor = true;
            this.buttonConnection.Click += new System.EventHandler(this.buttonConnection_Click);
            // 
            // textBoxConnection
            // 
            this.textBoxConnection.Location = new System.Drawing.Point(298, 48);
            this.textBoxConnection.Multiline = true;
            this.textBoxConnection.Name = "textBoxConnection";
            this.textBoxConnection.ReadOnly = true;
            this.textBoxConnection.Size = new System.Drawing.Size(251, 118);
            this.textBoxConnection.TabIndex = 2;
            // 
            // checkBoxAltAzm
            // 
            this.checkBoxAltAzm.AutoSize = true;
            this.checkBoxAltAzm.Location = new System.Drawing.Point(26, 25);
            this.checkBoxAltAzm.Name = "checkBoxAltAzm";
            this.checkBoxAltAzm.Size = new System.Drawing.Size(151, 22);
            this.checkBoxAltAzm.TabIndex = 0;
            this.checkBoxAltAzm.Text = "Alt-Azm sensors";
            this.checkBoxAltAzm.UseVisualStyleBackColor = true;
            // 
            // checkBoxEqu
            // 
            this.checkBoxEqu.AutoSize = true;
            this.checkBoxEqu.Location = new System.Drawing.Point(26, 53);
            this.checkBoxEqu.Name = "checkBoxEqu";
            this.checkBoxEqu.Size = new System.Drawing.Size(160, 22);
            this.checkBoxEqu.TabIndex = 1;
            this.checkBoxEqu.Text = "Equatorial Motion";
            this.checkBoxEqu.UseVisualStyleBackColor = true;
            // 
            // checkBoxGPS
            // 
            this.checkBoxGPS.AutoSize = true;
            this.checkBoxGPS.Location = new System.Drawing.Point(26, 81);
            this.checkBoxGPS.Name = "checkBoxGPS";
            this.checkBoxGPS.Size = new System.Drawing.Size(149, 22);
            this.checkBoxGPS.TabIndex = 2;
            this.checkBoxGPS.Text = "GPS positioning";
            this.checkBoxGPS.UseVisualStyleBackColor = true;
            // 
            // buttonSave
            // 
            this.buttonSave.Location = new System.Drawing.Point(195, 351);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(218, 108);
            this.buttonSave.TabIndex = 4;
            this.buttonSave.Text = "Save to Board";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBoxEqu);
            this.groupBox1.Controls.Add(this.checkBoxAltAzm);
            this.groupBox1.Controls.Add(this.checkBoxGPS);
            this.groupBox1.Location = new System.Drawing.Point(37, 195);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(512, 126);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Capabilities";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(295, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(139, 18);
            this.label1.TabIndex = 1;
            this.label1.Text = "Connected Board";
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // ScopeDSCConfigureHW
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(592, 488);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.textBoxConnection);
            this.Controls.Add(this.buttonConnection);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "ScopeDSCConfigureHW";
            this.Text = "Configure Board";
            this.Load += new System.EventHandler(this.ScopeDSCConfigureHW_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ScopeDSCConfigureHW_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonConnection;
        private System.Windows.Forms.TextBox textBoxConnection;
        private System.Windows.Forms.CheckBox checkBoxAltAzm;
        private System.Windows.Forms.CheckBox checkBoxEqu;
        private System.Windows.Forms.CheckBox checkBoxGPS;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Timer timer1;

    }
}

