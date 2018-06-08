namespace DSCTest1
{
    partial class Form1
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
            this.textBoxAzmOff = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxAltOff = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxRotationAngle = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxAxisAzm = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.textBoxStar1Alt = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textBoxStar1Azm = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.textBoxStar2Alt = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.textBoxStar2Azm = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.textBoxObjectAlt = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.textBoxObjectAzm = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.textBoxResults = new System.Windows.Forms.TextBox();
            this.buttonUpdate = new System.Windows.Forms.Button();
            this.textBoxPlatformA = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.textBoxObjectEquA = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.textBoxLatitude = new System.Windows.Forms.TextBox();
            this.label18 = new System.Windows.Forms.Label();
            this.textBoxPlatformEquAxisAzm = new System.Windows.Forms.TextBox();
            this.textBoxPlatformEquAxisAlt = new System.Windows.Forms.TextBox();
            this.label19 = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label26 = new System.Windows.Forms.Label();
            this.textBoxEquAngleFactor = new System.Windows.Forms.TextBox();
            this.label22 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label24 = new System.Windows.Forms.Label();
            this.textBoxPlatformDA2 = new System.Windows.Forms.TextBox();
            this.label25 = new System.Windows.Forms.Label();
            this.textBoxPlatformDA = new System.Windows.Forms.TextBox();
            this.label23 = new System.Windows.Forms.Label();
            this.textBoxPlatformA2 = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.checkBox4StarAlignment = new System.Windows.Forms.CheckBox();
            this.label27 = new System.Windows.Forms.Label();
            this.textBoxRandomError = new System.Windows.Forms.TextBox();
            this.buttonSave = new System.Windows.Forms.Button();
            this.buttonLoad = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBoxAzmOff
            // 
            this.textBoxAzmOff.Location = new System.Drawing.Point(121, 26);
            this.textBoxAzmOff.Name = "textBoxAzmOff";
            this.textBoxAzmOff.Size = new System.Drawing.Size(120, 20);
            this.textBoxAzmOff.TabIndex = 2;
            this.textBoxAzmOff.TextChanged += new System.EventHandler(this.textBoxAzmOff_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(118, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(99, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Azimuth Offset, deg";
            // 
            // textBoxAltOff
            // 
            this.textBoxAltOff.Location = new System.Drawing.Point(284, 26);
            this.textBoxAltOff.Name = "textBoxAltOff";
            this.textBoxAltOff.Size = new System.Drawing.Size(120, 20);
            this.textBoxAltOff.TabIndex = 4;
            this.textBoxAltOff.TextChanged += new System.EventHandler(this.textBoxAltOff_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(281, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(97, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Altitude Offset, deg";
            // 
            // textBoxRotationAngle
            // 
            this.textBoxRotationAngle.Location = new System.Drawing.Point(284, 73);
            this.textBoxRotationAngle.Name = "textBoxRotationAngle";
            this.textBoxRotationAngle.Size = new System.Drawing.Size(120, 20);
            this.textBoxRotationAngle.TabIndex = 9;
            this.textBoxRotationAngle.TextChanged += new System.EventHandler(this.textBoxRotationAngle_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(281, 56);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(125, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Rotation Angle, 0-90 deg";
            // 
            // textBoxAxisAzm
            // 
            this.textBoxAxisAzm.Location = new System.Drawing.Point(121, 73);
            this.textBoxAxisAzm.Name = "textBoxAxisAzm";
            this.textBoxAxisAzm.Size = new System.Drawing.Size(120, 20);
            this.textBoxAxisAzm.TabIndex = 7;
            this.textBoxAxisAzm.TextChanged += new System.EventHandler(this.textBoxAxisAzm_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(118, 56);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(163, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Rotation Axis Azimuth, 0-360 deg";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 76);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(81, 13);
            this.label5.TabIndex = 5;
            this.label5.Text = "Scope Rotation";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 34);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(84, 13);
            this.label6.TabIndex = 0;
            this.label6.Text = "Alignment Star 1";
            // 
            // textBoxStar1Alt
            // 
            this.textBoxStar1Alt.Location = new System.Drawing.Point(284, 33);
            this.textBoxStar1Alt.Name = "textBoxStar1Alt";
            this.textBoxStar1Alt.Size = new System.Drawing.Size(120, 20);
            this.textBoxStar1Alt.TabIndex = 4;
            this.textBoxStar1Alt.TextChanged += new System.EventHandler(this.textBoxStar1Alt_TextChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(281, 17);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(105, 13);
            this.label7.TabIndex = 3;
            this.label7.Text = "Altitude, (-90)-90 deg";
            // 
            // textBoxStar1Azm
            // 
            this.textBoxStar1Azm.Location = new System.Drawing.Point(121, 34);
            this.textBoxStar1Azm.Name = "textBoxStar1Azm";
            this.textBoxStar1Azm.Size = new System.Drawing.Size(120, 20);
            this.textBoxStar1Azm.TabIndex = 2;
            this.textBoxStar1Azm.TextChanged += new System.EventHandler(this.textBoxStar1Azm_TextChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(118, 17);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(98, 13);
            this.label8.TabIndex = 1;
            this.label8.Text = "Azimuth, 0-360 deg";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(9, 78);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(84, 13);
            this.label9.TabIndex = 5;
            this.label9.Text = "Alignment Star 2";
            // 
            // textBoxStar2Alt
            // 
            this.textBoxStar2Alt.Location = new System.Drawing.Point(284, 77);
            this.textBoxStar2Alt.Name = "textBoxStar2Alt";
            this.textBoxStar2Alt.Size = new System.Drawing.Size(120, 20);
            this.textBoxStar2Alt.TabIndex = 9;
            this.textBoxStar2Alt.TextChanged += new System.EventHandler(this.textBoxStar2Alt_TextChanged);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(281, 61);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(105, 13);
            this.label10.TabIndex = 8;
            this.label10.Text = "Altitude, (-90)-90 deg";
            // 
            // textBoxStar2Azm
            // 
            this.textBoxStar2Azm.Location = new System.Drawing.Point(121, 78);
            this.textBoxStar2Azm.Name = "textBoxStar2Azm";
            this.textBoxStar2Azm.Size = new System.Drawing.Size(120, 20);
            this.textBoxStar2Azm.TabIndex = 7;
            this.textBoxStar2Azm.TextChanged += new System.EventHandler(this.textBoxStar2Azm_TextChanged);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(118, 61);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(98, 13);
            this.label11.TabIndex = 6;
            this.label11.Text = "Azimuth, 0-360 deg";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(9, 33);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(78, 13);
            this.label12.TabIndex = 0;
            this.label12.Text = "Object Position";
            // 
            // textBoxObjectAlt
            // 
            this.textBoxObjectAlt.Location = new System.Drawing.Point(284, 33);
            this.textBoxObjectAlt.Name = "textBoxObjectAlt";
            this.textBoxObjectAlt.Size = new System.Drawing.Size(120, 20);
            this.textBoxObjectAlt.TabIndex = 4;
            this.textBoxObjectAlt.TextChanged += new System.EventHandler(this.textBoxObjectAlt_TextChanged);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(281, 16);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(105, 13);
            this.label13.TabIndex = 3;
            this.label13.Text = "Altitude, (-90)-90 deg";
            // 
            // textBoxObjectAzm
            // 
            this.textBoxObjectAzm.Location = new System.Drawing.Point(121, 33);
            this.textBoxObjectAzm.Name = "textBoxObjectAzm";
            this.textBoxObjectAzm.Size = new System.Drawing.Size(120, 20);
            this.textBoxObjectAzm.TabIndex = 2;
            this.textBoxObjectAzm.TextChanged += new System.EventHandler(this.textBoxObjectAzm_TextChanged);
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(118, 16);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(98, 13);
            this.label14.TabIndex = 1;
            this.label14.Text = "Azimuth, 0-360 deg";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(26, 414);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(45, 13);
            this.label15.TabIndex = 8;
            this.label15.Text = "Results:";
            // 
            // textBoxResults
            // 
            this.textBoxResults.Location = new System.Drawing.Point(17, 430);
            this.textBoxResults.Multiline = true;
            this.textBoxResults.Name = "textBoxResults";
            this.textBoxResults.ReadOnly = true;
            this.textBoxResults.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxResults.Size = new System.Drawing.Size(718, 378);
            this.textBoxResults.TabIndex = 9;
            // 
            // buttonUpdate
            // 
            this.buttonUpdate.Location = new System.Drawing.Point(346, 814);
            this.buttonUpdate.Name = "buttonUpdate";
            this.buttonUpdate.Size = new System.Drawing.Size(75, 23);
            this.buttonUpdate.TabIndex = 12;
            this.buttonUpdate.Text = "Update";
            this.buttonUpdate.UseVisualStyleBackColor = true;
            this.buttonUpdate.Click += new System.EventHandler(this.buttonUpdate_Click);
            // 
            // textBoxPlatformA
            // 
            this.textBoxPlatformA.Location = new System.Drawing.Point(446, 33);
            this.textBoxPlatformA.Name = "textBoxPlatformA";
            this.textBoxPlatformA.Size = new System.Drawing.Size(120, 20);
            this.textBoxPlatformA.TabIndex = 11;
            this.textBoxPlatformA.TextChanged += new System.EventHandler(this.textBoxPlatformA_TextChanged);
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(445, 17);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(140, 13);
            this.label16.TabIndex = 10;
            this.label16.Text = "Equ Angle 1, (-180)-180 deg";
            // 
            // textBoxObjectEquA
            // 
            this.textBoxObjectEquA.Location = new System.Drawing.Point(446, 33);
            this.textBoxObjectEquA.Name = "textBoxObjectEquA";
            this.textBoxObjectEquA.Size = new System.Drawing.Size(120, 20);
            this.textBoxObjectEquA.TabIndex = 6;
            this.textBoxObjectEquA.TextChanged += new System.EventHandler(this.textBoxObjectEquA_TextChanged);
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(445, 16);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(165, 13);
            this.label17.TabIndex = 5;
            this.label17.Text = "Object Equ Angle, (-180)-180 deg";
            // 
            // textBoxLatitude
            // 
            this.textBoxLatitude.Location = new System.Drawing.Point(138, 23);
            this.textBoxLatitude.Name = "textBoxLatitude";
            this.textBoxLatitude.Size = new System.Drawing.Size(120, 20);
            this.textBoxLatitude.TabIndex = 1;
            this.textBoxLatitude.TextChanged += new System.EventHandler(this.textBoxLatitude_TextChanged);
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(138, 6);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(108, 13);
            this.label18.TabIndex = 0;
            this.label18.Text = "Latitude, (-90)-90 deg";
            // 
            // textBoxPlatformEquAxisAzm
            // 
            this.textBoxPlatformEquAxisAzm.Location = new System.Drawing.Point(121, 119);
            this.textBoxPlatformEquAxisAzm.Name = "textBoxPlatformEquAxisAzm";
            this.textBoxPlatformEquAxisAzm.Size = new System.Drawing.Size(120, 20);
            this.textBoxPlatformEquAxisAzm.TabIndex = 12;
            this.textBoxPlatformEquAxisAzm.TextChanged += new System.EventHandler(this.textBoxPlatformEquAxisAzm_TextChanged);
            // 
            // textBoxPlatformEquAxisAlt
            // 
            this.textBoxPlatformEquAxisAlt.Location = new System.Drawing.Point(284, 119);
            this.textBoxPlatformEquAxisAlt.Name = "textBoxPlatformEquAxisAlt";
            this.textBoxPlatformEquAxisAlt.Size = new System.Drawing.Size(120, 20);
            this.textBoxPlatformEquAxisAlt.TabIndex = 14;
            this.textBoxPlatformEquAxisAlt.TextChanged += new System.EventHandler(this.textBoxPlatformEquAxisAlt_TextChanged);
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(9, 119);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(82, 13);
            this.label19.TabIndex = 10;
            this.label19.Text = "Scope Equ Axis";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(281, 102);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(66, 13);
            this.label20.TabIndex = 13;
            this.label20.Text = "Altitude, deg";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(118, 102);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(68, 13);
            this.label21.TabIndex = 11;
            this.label21.Text = "Azimuth, deg";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label26);
            this.groupBox1.Controls.Add(this.textBoxEquAngleFactor);
            this.groupBox1.Controls.Add(this.label22);
            this.groupBox1.Controls.Add(this.textBoxPlatformEquAxisAzm);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.textBoxPlatformEquAxisAlt);
            this.groupBox1.Controls.Add(this.textBoxAltOff);
            this.groupBox1.Controls.Add(this.label19);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label20);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label21);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.textBoxRotationAngle);
            this.groupBox1.Controls.Add(this.textBoxAxisAzm);
            this.groupBox1.Controls.Add(this.textBoxAzmOff);
            this.groupBox1.Location = new System.Drawing.Point(17, 52);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(585, 156);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Scope";
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(443, 102);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(67, 13);
            this.label26.TabIndex = 15;
            this.label26.Text = "Angle Factor";
            // 
            // textBoxEquAngleFactor
            // 
            this.textBoxEquAngleFactor.Location = new System.Drawing.Point(446, 119);
            this.textBoxEquAngleFactor.Name = "textBoxEquAngleFactor";
            this.textBoxEquAngleFactor.Size = new System.Drawing.Size(120, 20);
            this.textBoxEquAngleFactor.TabIndex = 16;
            this.textBoxEquAngleFactor.TextChanged += new System.EventHandler(this.textBoxEquAngleFactor_TextChanged);
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(9, 29);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(83, 13);
            this.label22.TabIndex = 0;
            this.label22.Text = "Encoder Offsets";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label24);
            this.groupBox2.Controls.Add(this.textBoxPlatformDA2);
            this.groupBox2.Controls.Add(this.label25);
            this.groupBox2.Controls.Add(this.textBoxPlatformDA);
            this.groupBox2.Controls.Add(this.label23);
            this.groupBox2.Controls.Add(this.textBoxPlatformA2);
            this.groupBox2.Controls.Add(this.textBoxStar1Azm);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.textBoxStar1Alt);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.label11);
            this.groupBox2.Controls.Add(this.textBoxStar2Azm);
            this.groupBox2.Controls.Add(this.label10);
            this.groupBox2.Controls.Add(this.textBoxStar2Alt);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.label16);
            this.groupBox2.Controls.Add(this.textBoxPlatformA);
            this.groupBox2.Location = new System.Drawing.Point(17, 214);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(718, 114);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Alignment";
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(591, 61);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(32, 13);
            this.label24.TabIndex = 16;
            this.label24.Text = "Delta";
            // 
            // textBoxPlatformDA2
            // 
            this.textBoxPlatformDA2.Enabled = false;
            this.textBoxPlatformDA2.Location = new System.Drawing.Point(592, 77);
            this.textBoxPlatformDA2.Name = "textBoxPlatformDA2";
            this.textBoxPlatformDA2.Size = new System.Drawing.Size(120, 20);
            this.textBoxPlatformDA2.TabIndex = 17;
            this.textBoxPlatformDA2.TextChanged += new System.EventHandler(this.textBoxPlatformDA2_TextChanged);
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(591, 17);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(32, 13);
            this.label25.TabIndex = 12;
            this.label25.Text = "Delta";
            // 
            // textBoxPlatformDA
            // 
            this.textBoxPlatformDA.Location = new System.Drawing.Point(592, 33);
            this.textBoxPlatformDA.Name = "textBoxPlatformDA";
            this.textBoxPlatformDA.Size = new System.Drawing.Size(120, 20);
            this.textBoxPlatformDA.TabIndex = 13;
            this.textBoxPlatformDA.TextChanged += new System.EventHandler(this.textBoxPlatformDA_TextChanged);
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(445, 61);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(140, 13);
            this.label23.TabIndex = 14;
            this.label23.Text = "Equ Angle 2, (-180)-180 deg";
            // 
            // textBoxPlatformA2
            // 
            this.textBoxPlatformA2.Enabled = false;
            this.textBoxPlatformA2.Location = new System.Drawing.Point(446, 77);
            this.textBoxPlatformA2.Name = "textBoxPlatformA2";
            this.textBoxPlatformA2.Size = new System.Drawing.Size(120, 20);
            this.textBoxPlatformA2.TabIndex = 15;
            this.textBoxPlatformA2.TextChanged += new System.EventHandler(this.textBoxPlatformA2_TextChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.textBoxObjectAlt);
            this.groupBox3.Controls.Add(this.label14);
            this.groupBox3.Controls.Add(this.textBoxObjectAzm);
            this.groupBox3.Controls.Add(this.textBoxObjectEquA);
            this.groupBox3.Controls.Add(this.label13);
            this.groupBox3.Controls.Add(this.label17);
            this.groupBox3.Controls.Add(this.label12);
            this.groupBox3.Location = new System.Drawing.Point(17, 334);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(718, 70);
            this.groupBox3.TabIndex = 7;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Object";
            // 
            // checkBox4StarAlignment
            // 
            this.checkBox4StarAlignment.AutoSize = true;
            this.checkBox4StarAlignment.Location = new System.Drawing.Point(301, 25);
            this.checkBox4StarAlignment.Name = "checkBox4StarAlignment";
            this.checkBox4StarAlignment.Size = new System.Drawing.Size(100, 17);
            this.checkBox4StarAlignment.TabIndex = 2;
            this.checkBox4StarAlignment.Text = "4-star alignment";
            this.checkBox4StarAlignment.UseVisualStyleBackColor = true;
            this.checkBox4StarAlignment.CheckedChanged += new System.EventHandler(this.checkBox4StarAlignment_CheckedChanged);
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(460, 6);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(112, 13);
            this.label27.TabIndex = 3;
            this.label27.Text = "Random Error, arc min";
            // 
            // textBoxRandomError
            // 
            this.textBoxRandomError.Location = new System.Drawing.Point(463, 23);
            this.textBoxRandomError.Name = "textBoxRandomError";
            this.textBoxRandomError.Size = new System.Drawing.Size(120, 20);
            this.textBoxRandomError.TabIndex = 4;
            this.textBoxRandomError.TextChanged += new System.EventHandler(this.textBoxRandomError_TextChanged);
            // 
            // buttonSave
            // 
            this.buttonSave.Location = new System.Drawing.Point(33, 814);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(75, 23);
            this.buttonSave.TabIndex = 10;
            this.buttonSave.Text = "Save";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // buttonLoad
            // 
            this.buttonLoad.Location = new System.Drawing.Point(114, 814);
            this.buttonLoad.Name = "buttonLoad";
            this.buttonLoad.Size = new System.Drawing.Size(75, 23);
            this.buttonLoad.TabIndex = 11;
            this.buttonLoad.Text = "Load";
            this.buttonLoad.UseVisualStyleBackColor = true;
            this.buttonLoad.Click += new System.EventHandler(this.buttonLoad_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(751, 849);
            this.Controls.Add(this.buttonLoad);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.label27);
            this.Controls.Add(this.textBoxRandomError);
            this.Controls.Add(this.checkBox4StarAlignment);
            this.Controls.Add(this.textBoxLatitude);
            this.Controls.Add(this.label18);
            this.Controls.Add(this.buttonUpdate);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.textBoxResults);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox3);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxAzmOff;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxAltOff;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxRotationAngle;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxAxisAzm;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBoxStar1Alt;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBoxStar1Azm;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox textBoxStar2Alt;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox textBoxStar2Azm;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox textBoxObjectAlt;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox textBoxObjectAzm;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox textBoxResults;
        private System.Windows.Forms.Button buttonUpdate;
        private System.Windows.Forms.TextBox textBoxPlatformA;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TextBox textBoxObjectEquA;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox textBoxLatitude;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.TextBox textBoxPlatformEquAxisAzm;
        private System.Windows.Forms.TextBox textBoxPlatformEquAxisAlt;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.TextBox textBoxPlatformA2;
        private System.Windows.Forms.CheckBox checkBox4StarAlignment;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.TextBox textBoxPlatformDA2;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.TextBox textBoxPlatformDA;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.TextBox textBoxEquAngleFactor;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.TextBox textBoxRandomError;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonLoad;
    }
}

