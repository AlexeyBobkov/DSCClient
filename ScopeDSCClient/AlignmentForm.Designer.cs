﻿namespace ScopeDSCClient
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
            this.label3 = new System.Windows.Forms.Label();
            this.comboBoxType = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonAddObject = new System.Windows.Forms.Button();
            this.textBoxResults = new System.Windows.Forms.TextBox();
            this.buttonReset = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.buttonCorrectPolarAxis = new System.Windows.Forms.Button();
            this.buttonSaveAlignment = new System.Windows.Forms.Button();
            this.buttonLoadAlignment = new System.Windows.Forms.Button();
            this.buttonRevalidate = new System.Windows.Forms.Button();
            this.listBoxObj = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(25, 123);
            this.label3.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(97, 29);
            this.label3.TabIndex = 3;
            this.label3.Text = "Object:";
            // 
            // comboBoxType
            // 
            this.comboBoxType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxType.Font = new System.Drawing.Font("Microsoft Sans Serif", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.comboBoxType.FormattingEnabled = true;
            this.comboBoxType.IntegralHeight = false;
            this.comboBoxType.Location = new System.Drawing.Point(30, 47);
            this.comboBoxType.Margin = new System.Windows.Forms.Padding(5);
            this.comboBoxType.MaxDropDownItems = 12;
            this.comboBoxType.Name = "comboBoxType";
            this.comboBoxType.Size = new System.Drawing.Size(429, 50);
            this.comboBoxType.TabIndex = 2;
            this.comboBoxType.SelectedIndexChanged += new System.EventHandler(this.comboBoxType_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(25, 15);
            this.label2.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(79, 29);
            this.label2.TabIndex = 1;
            this.label2.Text = "Type:";
            // 
            // buttonAddObject
            // 
            this.buttonAddObject.Location = new System.Drawing.Point(478, 47);
            this.buttonAddObject.Margin = new System.Windows.Forms.Padding(4);
            this.buttonAddObject.Name = "buttonAddObject";
            this.buttonAddObject.Size = new System.Drawing.Size(205, 151);
            this.buttonAddObject.TabIndex = 5;
            this.buttonAddObject.Text = "Add";
            this.buttonAddObject.UseVisualStyleBackColor = true;
            this.buttonAddObject.Click += new System.EventHandler(this.buttonAddObject_Click);
            // 
            // textBoxResults
            // 
            this.textBoxResults.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxResults.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.textBoxResults.Location = new System.Drawing.Point(693, 49);
            this.textBoxResults.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxResults.Multiline = true;
            this.textBoxResults.Name = "textBoxResults";
            this.textBoxResults.ReadOnly = true;
            this.textBoxResults.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxResults.Size = new System.Drawing.Size(418, 318);
            this.textBoxResults.TabIndex = 10;
            // 
            // buttonReset
            // 
            this.buttonReset.Location = new System.Drawing.Point(478, 216);
            this.buttonReset.Margin = new System.Windows.Forms.Padding(4);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Size = new System.Drawing.Size(205, 151);
            this.buttonReset.TabIndex = 6;
            this.buttonReset.Text = "Reset";
            this.buttonReset.UseVisualStyleBackColor = true;
            this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(693, 554);
            this.buttonOK.Margin = new System.Windows.Forms.Padding(4);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(205, 151);
            this.buttonOK.TabIndex = 12;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(906, 554);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(4);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(205, 151);
            this.buttonCancel.TabIndex = 14;
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
            this.buttonCorrectPolarAxis.Location = new System.Drawing.Point(478, 554);
            this.buttonCorrectPolarAxis.Margin = new System.Windows.Forms.Padding(4);
            this.buttonCorrectPolarAxis.Name = "buttonCorrectPolarAxis";
            this.buttonCorrectPolarAxis.Size = new System.Drawing.Size(205, 151);
            this.buttonCorrectPolarAxis.TabIndex = 8;
            this.buttonCorrectPolarAxis.Text = "Correct Polar Axis";
            this.buttonCorrectPolarAxis.UseVisualStyleBackColor = true;
            this.buttonCorrectPolarAxis.Click += new System.EventHandler(this.buttonCorrectPolarAxis_Click);
            // 
            // buttonSaveAlignment
            // 
            this.buttonSaveAlignment.Location = new System.Drawing.Point(693, 385);
            this.buttonSaveAlignment.Margin = new System.Windows.Forms.Padding(4);
            this.buttonSaveAlignment.Name = "buttonSaveAlignment";
            this.buttonSaveAlignment.Size = new System.Drawing.Size(205, 151);
            this.buttonSaveAlignment.TabIndex = 11;
            this.buttonSaveAlignment.Text = "Save";
            this.buttonSaveAlignment.UseVisualStyleBackColor = true;
            this.buttonSaveAlignment.Click += new System.EventHandler(this.buttonSaveAlignment_Click);
            // 
            // buttonLoadAlignment
            // 
            this.buttonLoadAlignment.Location = new System.Drawing.Point(906, 385);
            this.buttonLoadAlignment.Margin = new System.Windows.Forms.Padding(4);
            this.buttonLoadAlignment.Name = "buttonLoadAlignment";
            this.buttonLoadAlignment.Size = new System.Drawing.Size(205, 151);
            this.buttonLoadAlignment.TabIndex = 13;
            this.buttonLoadAlignment.Text = "Load";
            this.buttonLoadAlignment.UseVisualStyleBackColor = true;
            this.buttonLoadAlignment.Click += new System.EventHandler(this.buttonLoadAlignment_Click);
            // 
            // buttonRevalidate
            // 
            this.buttonRevalidate.Location = new System.Drawing.Point(478, 385);
            this.buttonRevalidate.Margin = new System.Windows.Forms.Padding(4);
            this.buttonRevalidate.Name = "buttonRevalidate";
            this.buttonRevalidate.Size = new System.Drawing.Size(205, 151);
            this.buttonRevalidate.TabIndex = 7;
            this.buttonRevalidate.Text = "Revalidate";
            this.buttonRevalidate.UseVisualStyleBackColor = true;
            this.buttonRevalidate.Click += new System.EventHandler(this.buttonRevalidate_Click);
            // 
            // listBoxObj
            // 
            this.listBoxObj.Font = new System.Drawing.Font("Microsoft Sans Serif", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.listBoxObj.FormattingEnabled = true;
            this.listBoxObj.ItemHeight = 42;
            this.listBoxObj.Location = new System.Drawing.Point(30, 155);
            this.listBoxObj.Name = "listBoxObj";
            this.listBoxObj.Size = new System.Drawing.Size(429, 550);
            this.listBoxObj.TabIndex = 4;
            this.listBoxObj.SelectedIndexChanged += new System.EventHandler(this.listBoxObj_SelectedIndexChanged);
            // 
            // AlignmentForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(15F, 29F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1136, 734);
            this.Controls.Add(this.listBoxObj);
            this.Controls.Add(this.buttonRevalidate);
            this.Controls.Add(this.buttonSaveAlignment);
            this.Controls.Add(this.buttonLoadAlignment);
            this.Controls.Add(this.buttonCorrectPolarAxis);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonReset);
            this.Controls.Add(this.textBoxResults);
            this.Controls.Add(this.buttonAddObject);
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

        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBoxType;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonAddObject;
        private System.Windows.Forms.TextBox textBoxResults;
        private System.Windows.Forms.Button buttonReset;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button buttonCorrectPolarAxis;
        private System.Windows.Forms.Button buttonSaveAlignment;
        private System.Windows.Forms.Button buttonLoadAlignment;
        private System.Windows.Forms.Button buttonRevalidate;
        private System.Windows.Forms.ListBox listBoxObj;
    }
}