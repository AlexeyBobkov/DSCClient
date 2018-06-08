namespace ScopeDSCConfigureHW
{
    partial class ConnectionForm
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
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.comboBoxBaudRate = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBoxSerialPort = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(297, 181);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(5);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(228, 98);
            this.buttonCancel.TabIndex = 13;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonConnect
            // 
            this.buttonConnect.AutoSize = true;
            this.buttonConnect.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonConnect.Location = new System.Drawing.Point(40, 181);
            this.buttonConnect.Margin = new System.Windows.Forms.Padding(5);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(228, 98);
            this.buttonConnect.TabIndex = 12;
            this.buttonConnect.Text = "OK";
            this.buttonConnect.UseVisualStyleBackColor = true;
            // 
            // comboBoxBaudRate
            // 
            this.comboBoxBaudRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxBaudRate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBoxBaudRate.FormattingEnabled = true;
            this.comboBoxBaudRate.IntegralHeight = false;
            this.comboBoxBaudRate.Location = new System.Drawing.Point(40, 117);
            this.comboBoxBaudRate.Margin = new System.Windows.Forms.Padding(5);
            this.comboBoxBaudRate.MaxDropDownItems = 12;
            this.comboBoxBaudRate.Name = "comboBoxBaudRate";
            this.comboBoxBaudRate.Size = new System.Drawing.Size(485, 26);
            this.comboBoxBaudRate.TabIndex = 10;
            this.comboBoxBaudRate.SelectedIndexChanged += new System.EventHandler(this.comboBoxBaudRate_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(37, 94);
            this.label2.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(86, 18);
            this.label2.TabIndex = 9;
            this.label2.Text = "Baud Rate";
            // 
            // comboBoxSerialPort
            // 
            this.comboBoxSerialPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSerialPort.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBoxSerialPort.FormattingEnabled = true;
            this.comboBoxSerialPort.IntegralHeight = false;
            this.comboBoxSerialPort.Location = new System.Drawing.Point(40, 46);
            this.comboBoxSerialPort.Margin = new System.Windows.Forms.Padding(9, 6, 9, 6);
            this.comboBoxSerialPort.MaxDropDownItems = 12;
            this.comboBoxSerialPort.Name = "comboBoxSerialPort";
            this.comboBoxSerialPort.Size = new System.Drawing.Size(485, 26);
            this.comboBoxSerialPort.TabIndex = 8;
            this.comboBoxSerialPort.SelectedIndexChanged += new System.EventHandler(this.comboBoxSerialPort_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(37, 22);
            this.label1.Margin = new System.Windows.Forms.Padding(9, 0, 9, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 18);
            this.label1.TabIndex = 7;
            this.label1.Text = "Serial Port";
            // 
            // ConnectionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(574, 322);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonConnect);
            this.Controls.Add(this.comboBoxBaudRate);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboBoxSerialPort);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.Name = "ConnectionForm";
            this.Text = "Connection";
            this.Load += new System.EventHandler(this.ConnectionForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.ComboBox comboBoxBaudRate;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBoxSerialPort;
        private System.Windows.Forms.Label label1;

    }
}