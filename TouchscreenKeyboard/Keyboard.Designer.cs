namespace KeyboardClassLibrary
{
    partial class Keyboardcontrol
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pictureBoxCtrlDown = new System.Windows.Forms.PictureBox();
            this.pictureBoxLeftShiftDown = new System.Windows.Forms.PictureBox();
            this.pictureBoxCapsLockDown = new System.Windows.Forms.PictureBox();
            this.pictureBoxKeyboard = new System.Windows.Forms.PictureBox();
            this.pictureBoxAltDown = new System.Windows.Forms.PictureBox();
            this.pictureBoxFnDown = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCtrlDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLeftShiftDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCapsLockDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxKeyboard)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxAltDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxFnDown)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBoxCtrlDown
            // 
            this.pictureBoxCtrlDown.Image = global::KeyboardClassLibrary.Properties.Resources.ctrl_down_white;
            this.pictureBoxCtrlDown.Location = new System.Drawing.Point(1, 241);
            this.pictureBoxCtrlDown.Name = "pictureBoxCtrlDown";
            this.pictureBoxCtrlDown.Size = new System.Drawing.Size(60, 60);
            this.pictureBoxCtrlDown.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxCtrlDown.TabIndex = 4;
            this.pictureBoxCtrlDown.TabStop = false;
            this.pictureBoxCtrlDown.Visible = false;
            this.pictureBoxCtrlDown.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pictureBoxCtrlDown_MouseClick);
            // 
            // pictureBoxLeftShiftDown
            // 
            this.pictureBoxLeftShiftDown.Image = global::KeyboardClassLibrary.Properties.Resources.shift_down_white;
            this.pictureBoxLeftShiftDown.Location = new System.Drawing.Point(1, 181);
            this.pictureBoxLeftShiftDown.Name = "pictureBoxLeftShiftDown";
            this.pictureBoxLeftShiftDown.Size = new System.Drawing.Size(150, 60);
            this.pictureBoxLeftShiftDown.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxLeftShiftDown.TabIndex = 3;
            this.pictureBoxLeftShiftDown.TabStop = false;
            this.pictureBoxLeftShiftDown.Visible = false;
            this.pictureBoxLeftShiftDown.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pictureBoxLeftShiftState_MouseClick);
            // 
            // pictureBoxCapsLockDown
            // 
            this.pictureBoxCapsLockDown.Image = global::KeyboardClassLibrary.Properties.Resources.caps_down_white;
            this.pictureBoxCapsLockDown.Location = new System.Drawing.Point(1, 121);
            this.pictureBoxCapsLockDown.Name = "pictureBoxCapsLockDown";
            this.pictureBoxCapsLockDown.Size = new System.Drawing.Size(120, 60);
            this.pictureBoxCapsLockDown.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxCapsLockDown.TabIndex = 1;
            this.pictureBoxCapsLockDown.TabStop = false;
            this.pictureBoxCapsLockDown.Visible = false;
            this.pictureBoxCapsLockDown.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pictureBoxCapsLockState_MouseClick);
            // 
            // pictureBoxKeyboard
            // 
            this.pictureBoxKeyboard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxKeyboard.Image = global::KeyboardClassLibrary.Properties.Resources.keyboard_white;
            this.pictureBoxKeyboard.Location = new System.Drawing.Point(0, 0);
            this.pictureBoxKeyboard.Name = "pictureBoxKeyboard";
            this.pictureBoxKeyboard.Size = new System.Drawing.Size(902, 302);
            this.pictureBoxKeyboard.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxKeyboard.TabIndex = 0;
            this.pictureBoxKeyboard.TabStop = false;
            this.pictureBoxKeyboard.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pictureBoxKeyboard_MouseClick);
            this.pictureBoxKeyboard.SizeChanged += new System.EventHandler(this.pictureBoxKeyboard_SizeChanged);
            // 
            // pictureBoxAltDown
            // 
            this.pictureBoxAltDown.Image = global::KeyboardClassLibrary.Properties.Resources.alt_down_white;
            this.pictureBoxAltDown.Location = new System.Drawing.Point(61, 241);
            this.pictureBoxAltDown.Name = "pictureBoxAltDown";
            this.pictureBoxAltDown.Size = new System.Drawing.Size(60, 60);
            this.pictureBoxAltDown.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxAltDown.TabIndex = 5;
            this.pictureBoxAltDown.TabStop = false;
            this.pictureBoxAltDown.Visible = false;
            this.pictureBoxAltDown.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pictureBoxAltDown_MouseClick);
            // 
            // pictureBoxFnDown
            // 
            this.pictureBoxFnDown.Image = global::KeyboardClassLibrary.Properties.Resources.fn_down_white;
            this.pictureBoxFnDown.Location = new System.Drawing.Point(841, 181);
            this.pictureBoxFnDown.Name = "pictureBoxFnDown";
            this.pictureBoxFnDown.Size = new System.Drawing.Size(60, 60);
            this.pictureBoxFnDown.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxFnDown.TabIndex = 6;
            this.pictureBoxFnDown.TabStop = false;
            this.pictureBoxFnDown.Visible = false;
            this.pictureBoxFnDown.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pictureBoxFnDown_MouseClick);
            // 
            // Keyboardcontrol
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pictureBoxFnDown);
            this.Controls.Add(this.pictureBoxAltDown);
            this.Controls.Add(this.pictureBoxCtrlDown);
            this.Controls.Add(this.pictureBoxLeftShiftDown);
            this.Controls.Add(this.pictureBoxCapsLockDown);
            this.Controls.Add(this.pictureBoxKeyboard);
            this.Name = "Keyboardcontrol";
            this.Size = new System.Drawing.Size(902, 302);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCtrlDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLeftShiftDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCapsLockDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxKeyboard)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxAltDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxFnDown)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBoxCapsLockDown;
        private System.Windows.Forms.PictureBox pictureBoxLeftShiftDown;
        private System.Windows.Forms.PictureBox pictureBoxKeyboard;
        private System.Windows.Forms.PictureBox pictureBoxCtrlDown;
        private System.Windows.Forms.PictureBox pictureBoxAltDown;
        private System.Windows.Forms.PictureBox pictureBoxFnDown;
    }
}
