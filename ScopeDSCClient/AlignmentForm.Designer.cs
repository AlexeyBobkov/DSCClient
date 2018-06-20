namespace ScopeDSCClient
{
    partial class AlignmentForm
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
            this.comboBoxObj = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBoxType = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonAddObject = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBoxResults = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.buttonReset = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.buttonCorrectPolarAxis = new System.Windows.Forms.Button();
            this.buttonSaveAlignment = new System.Windows.Forms.Button();
            this.buttonLoadAlignment = new System.Windows.Forms.Button();
            this.buttonCorrectOffsets = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // comboBoxObj
            // 
            this.comboBoxObj.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxObj.FormattingEnabled = true;
            this.comboBoxObj.IntegralHeight = false;
            this.comboBoxObj.Location = new System.Drawing.Point(50, 221);
            this.comboBoxObj.Margin = new System.Windows.Forms.Padding(5);
            this.comboBoxObj.MaxDropDownItems = 12;
            this.comboBoxObj.Name = "comboBoxObj";
            this.comboBoxObj.Size = new System.Drawing.Size(340, 37);
            this.comboBoxObj.TabIndex = 4;
            this.comboBoxObj.SelectedIndexChanged += new System.EventHandler(this.comboBoxObj_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(45, 193);
            this.label3.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(90, 29);
            this.label3.TabIndex = 3;
            this.label3.Text = "Object";
            // 
            // comboBoxType
            // 
            this.comboBoxType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxType.FormattingEnabled = true;
            this.comboBoxType.IntegralHeight = false;
            this.comboBoxType.Location = new System.Drawing.Point(50, 121);
            this.comboBoxType.Margin = new System.Windows.Forms.Padding(5);
            this.comboBoxType.MaxDropDownItems = 12;
            this.comboBoxType.Name = "comboBoxType";
            this.comboBoxType.Size = new System.Drawing.Size(340, 37);
            this.comboBoxType.TabIndex = 2;
            this.comboBoxType.SelectedIndexChanged += new System.EventHandler(this.comboBoxType_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(45, 94);
            this.label2.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(156, 29);
            this.label2.TabIndex = 1;
            this.label2.Text = "Object Type";
            // 
            // buttonAddObject
            // 
            this.buttonAddObject.Location = new System.Drawing.Point(417, 121);
            this.buttonAddObject.Margin = new System.Windows.Forms.Padding(4);
            this.buttonAddObject.Name = "buttonAddObject";
            this.buttonAddObject.Size = new System.Drawing.Size(205, 137);
            this.buttonAddObject.TabIndex = 5;
            this.buttonAddObject.Text = "Add";
            this.buttonAddObject.UseVisualStyleBackColor = true;
            this.buttonAddObject.Click += new System.EventHandler(this.buttonAddObject_Click);
            // 
            // textBox1
            // 
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.Location = new System.Drawing.Point(50, 22);
            this.textBox1.Margin = new System.Windows.Forms.Padding(4);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(605, 65);
            this.textBox1.TabIndex = 0;
            this.textBox1.TabStop = false;
            this.textBox1.Text = "Select an object, point the scope to it, then press \"Add Object\"";
            // 
            // textBoxResults
            // 
            this.textBoxResults.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxResults.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.textBoxResults.Location = new System.Drawing.Point(50, 319);
            this.textBoxResults.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxResults.Multiline = true;
            this.textBoxResults.Name = "textBoxResults";
            this.textBoxResults.ReadOnly = true;
            this.textBoxResults.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxResults.Size = new System.Drawing.Size(572, 416);
            this.textBoxResults.TabIndex = 14;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(46, 286);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(465, 29);
            this.label5.TabIndex = 13;
            this.label5.Text = "Object Position and Current Alignment:";
            // 
            // buttonReset
            // 
            this.buttonReset.Location = new System.Drawing.Point(916, 121);
            this.buttonReset.Margin = new System.Windows.Forms.Padding(4);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Size = new System.Drawing.Size(205, 137);
            this.buttonReset.TabIndex = 7;
            this.buttonReset.Text = "Reset";
            this.buttonReset.UseVisualStyleBackColor = true;
            this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(692, 598);
            this.buttonOK.Margin = new System.Windows.Forms.Padding(4);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(205, 137);
            this.buttonOK.TabIndex = 11;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(916, 598);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(4);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(205, 137);
            this.buttonCancel.TabIndex = 12;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // buttonCorrectPolarAxis
            // 
            this.buttonCorrectPolarAxis.Location = new System.Drawing.Point(916, 430);
            this.buttonCorrectPolarAxis.Margin = new System.Windows.Forms.Padding(4);
            this.buttonCorrectPolarAxis.Name = "buttonCorrectPolarAxis";
            this.buttonCorrectPolarAxis.Size = new System.Drawing.Size(205, 137);
            this.buttonCorrectPolarAxis.TabIndex = 10;
            this.buttonCorrectPolarAxis.Text = "Correct Polar Axis";
            this.buttonCorrectPolarAxis.UseVisualStyleBackColor = true;
            this.buttonCorrectPolarAxis.Click += new System.EventHandler(this.buttonCorrectPolarAxis_Click);
            // 
            // buttonSaveAlignment
            // 
            this.buttonSaveAlignment.Location = new System.Drawing.Point(692, 275);
            this.buttonSaveAlignment.Margin = new System.Windows.Forms.Padding(4);
            this.buttonSaveAlignment.Name = "buttonSaveAlignment";
            this.buttonSaveAlignment.Size = new System.Drawing.Size(205, 137);
            this.buttonSaveAlignment.TabIndex = 8;
            this.buttonSaveAlignment.Text = "Save";
            this.buttonSaveAlignment.UseVisualStyleBackColor = true;
            this.buttonSaveAlignment.Click += new System.EventHandler(this.buttonSaveAlignment_Click);
            // 
            // buttonLoadAlignment
            // 
            this.buttonLoadAlignment.Location = new System.Drawing.Point(916, 275);
            this.buttonLoadAlignment.Margin = new System.Windows.Forms.Padding(4);
            this.buttonLoadAlignment.Name = "buttonLoadAlignment";
            this.buttonLoadAlignment.Size = new System.Drawing.Size(205, 137);
            this.buttonLoadAlignment.TabIndex = 9;
            this.buttonLoadAlignment.Text = "Load";
            this.buttonLoadAlignment.UseVisualStyleBackColor = true;
            this.buttonLoadAlignment.Click += new System.EventHandler(this.buttonLoadAlignment_Click);
            // 
            // buttonCorrectOffsets
            // 
            this.buttonCorrectOffsets.Location = new System.Drawing.Point(692, 121);
            this.buttonCorrectOffsets.Margin = new System.Windows.Forms.Padding(4);
            this.buttonCorrectOffsets.Name = "buttonCorrectOffsets";
            this.buttonCorrectOffsets.Size = new System.Drawing.Size(205, 137);
            this.buttonCorrectOffsets.TabIndex = 6;
            this.buttonCorrectOffsets.Text = "Correct Offsets";
            this.buttonCorrectOffsets.UseVisualStyleBackColor = true;
            this.buttonCorrectOffsets.Click += new System.EventHandler(this.buttonCorrectOffsets_Click);
            // 
            // AlignmentForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(15F, 29F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1167, 771);
            this.Controls.Add(this.buttonCorrectOffsets);
            this.Controls.Add(this.buttonSaveAlignment);
            this.Controls.Add(this.buttonLoadAlignment);
            this.Controls.Add(this.buttonCorrectPolarAxis);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonReset);
            this.Controls.Add(this.textBoxResults);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.buttonAddObject);
            this.Controls.Add(this.comboBoxObj);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.comboBoxType);
            this.Controls.Add(this.label2);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Margin = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this.Name = "AlignmentForm";
            this.Text = "Alignment";
            this.Load += new System.EventHandler(this.AlignmentForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxObj;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBoxType;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonAddObject;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBoxResults;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button buttonReset;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button buttonCorrectPolarAxis;
        private System.Windows.Forms.Button buttonSaveAlignment;
        private System.Windows.Forms.Button buttonLoadAlignment;
        private System.Windows.Forms.Button buttonCorrectOffsets;
    }
}