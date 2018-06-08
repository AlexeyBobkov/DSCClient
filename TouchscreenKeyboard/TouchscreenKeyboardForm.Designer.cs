namespace KeyboardClassLibrary
{
    partial class TouchscreenKeyboardForm
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
            this.keyboardcontrol = new KeyboardClassLibrary.Keyboardcontrol();
            this.SuspendLayout();
            // 
            // keyboardcontrol
            // 
            this.keyboardcontrol.KeyboardType = KeyboardClassLibrary.BoW.StandardDay;
            this.keyboardcontrol.Location = new System.Drawing.Point(0, 0);
            this.keyboardcontrol.Margin = new System.Windows.Forms.Padding(7);
            this.keyboardcontrol.Name = "keyboardcontrol";
            this.keyboardcontrol.Size = new System.Drawing.Size(902, 302);
            this.keyboardcontrol.TabIndex = 0;
            this.keyboardcontrol.UserKeyPressed += new KeyboardClassLibrary.KeyboardDelegate(this.keyboardcontrol_UserKeyPressed);
            this.keyboardcontrol.ExitKeyPressed += new KeyboardClassLibrary.ExitDelegate(this.keyboardcontrol_ExitKeyPressed);
            // 
            // TouchscreenKeyboardForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(901, 301);
            this.Controls.Add(this.keyboardcontrol);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TouchscreenKeyboardForm";
            this.Text = "TouchscreenKeyboardForm";
            this.Load += new System.EventHandler(this.TouchscreenKeyboardForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private Keyboardcontrol keyboardcontrol;
    }
}