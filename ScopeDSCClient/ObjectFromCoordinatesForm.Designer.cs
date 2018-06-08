namespace ScopeDSCClient
{
    partial class ObjectFromCoordinatesForm
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
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxRADeg = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxRAMin = new System.Windows.Forms.TextBox();
            this.textBoxRASec = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioButtonRADegree = new System.Windows.Forms.RadioButton();
            this.radioButtonRADMS = new System.Windows.Forms.RadioButton();
            this.radioButtonRAHMS = new System.Windows.Forms.RadioButton();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxDecSec = new System.Windows.Forms.TextBox();
            this.textBoxDecMin = new System.Windows.Forms.TextBox();
            this.textBoxDecDeg = new System.Windows.Forms.TextBox();
            this.textBoxResults = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.buttonScreenKbd = new ScopeDSCClientHelper.NoSelectButton();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.radioButtonDecDegree = new System.Windows.Forms.RadioButton();
            this.radioButtonDecDMS = new System.Windows.Forms.RadioButton();
            this.checkBoxJ2000 = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(494, 617);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(4);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(202, 88);
            this.buttonCancel.TabIndex = 16;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(284, 617);
            this.buttonOK.Margin = new System.Windows.Forms.Padding(4);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(202, 88);
            this.buttonOK.TabIndex = 15;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(37, 22);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(342, 29);
            this.label1.TabIndex = 0;
            this.label1.Text = "Object Equatorial Coordinates:";
            // 
            // textBoxRADeg
            // 
            this.textBoxRADeg.Location = new System.Drawing.Point(42, 110);
            this.textBoxRADeg.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxRADeg.Name = "textBoxRADeg";
            this.textBoxRADeg.Size = new System.Drawing.Size(60, 35);
            this.textBoxRADeg.TabIndex = 2;
            this.textBoxRADeg.Text = "0";
            this.textBoxRADeg.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxRADeg.TextChanged += new System.EventHandler(this.textBoxRADeg_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(37, 77);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 29);
            this.label2.TabIndex = 1;
            this.label2.Text = "R.A.";
            // 
            // textBoxRAMin
            // 
            this.textBoxRAMin.Location = new System.Drawing.Point(111, 110);
            this.textBoxRAMin.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxRAMin.Name = "textBoxRAMin";
            this.textBoxRAMin.Size = new System.Drawing.Size(46, 35);
            this.textBoxRAMin.TabIndex = 3;
            this.textBoxRAMin.Text = "0";
            this.textBoxRAMin.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxRAMin.TextChanged += new System.EventHandler(this.textBoxRAMin_TextChanged);
            // 
            // textBoxRASec
            // 
            this.textBoxRASec.Location = new System.Drawing.Point(165, 110);
            this.textBoxRASec.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxRASec.Name = "textBoxRASec";
            this.textBoxRASec.Size = new System.Drawing.Size(45, 35);
            this.textBoxRASec.TabIndex = 4;
            this.textBoxRASec.Text = "0";
            this.textBoxRASec.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxRASec.TextChanged += new System.EventHandler(this.textBoxRASec_TextChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioButtonRADegree);
            this.groupBox1.Controls.Add(this.radioButtonRADMS);
            this.groupBox1.Controls.Add(this.radioButtonRAHMS);
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.groupBox1.Location = new System.Drawing.Point(230, 83);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(466, 68);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "R.A. Units";
            // 
            // radioButtonRADegree
            // 
            this.radioButtonRADegree.AutoSize = true;
            this.radioButtonRADegree.Location = new System.Drawing.Point(333, 27);
            this.radioButtonRADegree.Margin = new System.Windows.Forms.Padding(4);
            this.radioButtonRADegree.Name = "radioButtonRADegree";
            this.radioButtonRADegree.Size = new System.Drawing.Size(109, 33);
            this.radioButtonRADegree.TabIndex = 2;
            this.radioButtonRADegree.TabStop = true;
            this.radioButtonRADegree.Text = "degree";
            this.radioButtonRADegree.UseVisualStyleBackColor = true;
            this.radioButtonRADegree.CheckedChanged += new System.EventHandler(this.radioButtonRADegree_CheckedChanged);
            // 
            // radioButtonRADMS
            // 
            this.radioButtonRADMS.AutoSize = true;
            this.radioButtonRADMS.Location = new System.Drawing.Point(122, 27);
            this.radioButtonRADMS.Margin = new System.Windows.Forms.Padding(4);
            this.radioButtonRADMS.Name = "radioButtonRADMS";
            this.radioButtonRADMS.Size = new System.Drawing.Size(183, 33);
            this.radioButtonRADMS.TabIndex = 1;
            this.radioButtonRADMS.TabStop = true;
            this.radioButtonRADMS.Text = "deg,arcm,arcs";
            this.radioButtonRADMS.UseVisualStyleBackColor = true;
            this.radioButtonRADMS.CheckedChanged += new System.EventHandler(this.radioButtonRADMS_CheckedChanged);
            // 
            // radioButtonRAHMS
            // 
            this.radioButtonRAHMS.AutoSize = true;
            this.radioButtonRAHMS.Location = new System.Drawing.Point(8, 27);
            this.radioButtonRAHMS.Margin = new System.Windows.Forms.Padding(4);
            this.radioButtonRAHMS.Name = "radioButtonRAHMS";
            this.radioButtonRAHMS.Size = new System.Drawing.Size(88, 33);
            this.radioButtonRAHMS.TabIndex = 0;
            this.radioButtonRAHMS.TabStop = true;
            this.radioButtonRAHMS.Text = "h,m,s";
            this.radioButtonRAHMS.UseVisualStyleBackColor = true;
            this.radioButtonRAHMS.CheckedChanged += new System.EventHandler(this.radioButtonRAHMS_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(37, 179);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 29);
            this.label3.TabIndex = 5;
            this.label3.Text = "Dec.";
            // 
            // textBoxDecSec
            // 
            this.textBoxDecSec.Location = new System.Drawing.Point(165, 211);
            this.textBoxDecSec.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxDecSec.Name = "textBoxDecSec";
            this.textBoxDecSec.Size = new System.Drawing.Size(45, 35);
            this.textBoxDecSec.TabIndex = 8;
            this.textBoxDecSec.Text = "0";
            this.textBoxDecSec.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxDecSec.TextChanged += new System.EventHandler(this.textBoxDecSec_TextChanged);
            // 
            // textBoxDecMin
            // 
            this.textBoxDecMin.Location = new System.Drawing.Point(111, 211);
            this.textBoxDecMin.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxDecMin.Name = "textBoxDecMin";
            this.textBoxDecMin.Size = new System.Drawing.Size(46, 35);
            this.textBoxDecMin.TabIndex = 7;
            this.textBoxDecMin.Text = "0";
            this.textBoxDecMin.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxDecMin.TextChanged += new System.EventHandler(this.textBoxDecMin_TextChanged);
            // 
            // textBoxDecDeg
            // 
            this.textBoxDecDeg.Location = new System.Drawing.Point(43, 211);
            this.textBoxDecDeg.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxDecDeg.Name = "textBoxDecDeg";
            this.textBoxDecDeg.Size = new System.Drawing.Size(59, 35);
            this.textBoxDecDeg.TabIndex = 6;
            this.textBoxDecDeg.Text = "0";
            this.textBoxDecDeg.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxDecDeg.TextChanged += new System.EventHandler(this.textBoxDecDeg_TextChanged);
            // 
            // textBoxResults
            // 
            this.textBoxResults.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxResults.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.textBoxResults.Location = new System.Drawing.Point(42, 306);
            this.textBoxResults.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxResults.Multiline = true;
            this.textBoxResults.Name = "textBoxResults";
            this.textBoxResults.ReadOnly = true;
            this.textBoxResults.Size = new System.Drawing.Size(654, 279);
            this.textBoxResults.TabIndex = 13;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(38, 273);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(177, 29);
            this.label5.TabIndex = 12;
            this.label5.Text = "Object Position";
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // buttonScreenKbd
            // 
            this.buttonScreenKbd.Font = new System.Drawing.Font("Wingdings", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.buttonScreenKbd.Location = new System.Drawing.Point(43, 617);
            this.buttonScreenKbd.Name = "buttonScreenKbd";
            this.buttonScreenKbd.Size = new System.Drawing.Size(114, 88);
            this.buttonScreenKbd.TabIndex = 14;
            this.buttonScreenKbd.Text = "7";
            this.buttonScreenKbd.UseVisualStyleBackColor = true;
            this.buttonScreenKbd.Click += new System.EventHandler(this.buttonScreenKbd_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.radioButtonDecDegree);
            this.groupBox3.Controls.Add(this.radioButtonDecDMS);
            this.groupBox3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.groupBox3.Location = new System.Drawing.Point(230, 179);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox3.Size = new System.Drawing.Size(345, 68);
            this.groupBox3.TabIndex = 10;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Dec. Units";
            // 
            // radioButtonDecDegree
            // 
            this.radioButtonDecDegree.AutoSize = true;
            this.radioButtonDecDegree.Location = new System.Drawing.Point(230, 27);
            this.radioButtonDecDegree.Margin = new System.Windows.Forms.Padding(4);
            this.radioButtonDecDegree.Name = "radioButtonDecDegree";
            this.radioButtonDecDegree.Size = new System.Drawing.Size(109, 33);
            this.radioButtonDecDegree.TabIndex = 1;
            this.radioButtonDecDegree.TabStop = true;
            this.radioButtonDecDegree.Text = "degree";
            this.radioButtonDecDegree.UseVisualStyleBackColor = true;
            this.radioButtonDecDegree.CheckedChanged += new System.EventHandler(this.radioButtonDecDegree_CheckedChanged);
            // 
            // radioButtonDecDMS
            // 
            this.radioButtonDecDMS.AutoSize = true;
            this.radioButtonDecDMS.Location = new System.Drawing.Point(19, 27);
            this.radioButtonDecDMS.Margin = new System.Windows.Forms.Padding(4);
            this.radioButtonDecDMS.Name = "radioButtonDecDMS";
            this.radioButtonDecDMS.Size = new System.Drawing.Size(183, 33);
            this.radioButtonDecDMS.TabIndex = 0;
            this.radioButtonDecDMS.TabStop = true;
            this.radioButtonDecDMS.Text = "deg,arcm,arcs";
            this.radioButtonDecDMS.UseVisualStyleBackColor = true;
            this.radioButtonDecDMS.CheckedChanged += new System.EventHandler(this.radioButtonDecDMS_CheckedChanged);
            // 
            // checkBoxJ2000
            // 
            this.checkBoxJ2000.AutoSize = true;
            this.checkBoxJ2000.Location = new System.Drawing.Point(600, 206);
            this.checkBoxJ2000.Name = "checkBoxJ2000";
            this.checkBoxJ2000.Size = new System.Drawing.Size(96, 33);
            this.checkBoxJ2000.TabIndex = 11;
            this.checkBoxJ2000.Text = "J2000";
            this.checkBoxJ2000.UseVisualStyleBackColor = true;
            this.checkBoxJ2000.CheckedChanged += new System.EventHandler(this.checkBoxJ2000_CheckedChanged);
            // 
            // ObjectFromCoordinatesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(14F, 29F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(737, 742);
            this.Controls.Add(this.checkBoxJ2000);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.buttonScreenKbd);
            this.Controls.Add(this.textBoxResults);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBoxDecSec);
            this.Controls.Add(this.textBoxDecMin);
            this.Controls.Add(this.textBoxDecDeg);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.textBoxRASec);
            this.Controls.Add(this.textBoxRAMin);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxRADeg);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Margin = new System.Windows.Forms.Padding(8, 7, 8, 7);
            this.Name = "ObjectFromCoordinatesForm";
            this.Text = "Set Object Coordinates";
            this.Load += new System.EventHandler(this.ObjectFromCoordinatesForm_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ObjectFromCoordinatesForm_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxRADeg;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxRAMin;
        private System.Windows.Forms.TextBox textBoxRASec;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioButtonRADegree;
        private System.Windows.Forms.RadioButton radioButtonRADMS;
        private System.Windows.Forms.RadioButton radioButtonRAHMS;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxDecSec;
        private System.Windows.Forms.TextBox textBoxDecMin;
        private System.Windows.Forms.TextBox textBoxDecDeg;
        private System.Windows.Forms.TextBox textBoxResults;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Timer timer1;
        private ScopeDSCClientHelper.NoSelectButton buttonScreenKbd;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton radioButtonDecDegree;
        private System.Windows.Forms.RadioButton radioButtonDecDMS;
        private System.Windows.Forms.CheckBox checkBoxJ2000;
    }
}