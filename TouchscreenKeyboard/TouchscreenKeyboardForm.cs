using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace KeyboardClassLibrary
{
    public partial class TouchscreenKeyboardForm : Form
    {
        public TouchscreenKeyboardForm(bool nightMode)
        {
            nightMode_ = nightMode;
            InitializeComponent();
        }

        protected override bool ShowWithoutActivation { get { return true; } }
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams ret = base.CreateParams;
                ret.Style = WS_THICKFRAME | WS_CHILD;
                ret.ExStyle |= WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW;
                return ret;
            }
        }

        private const int WS_THICKFRAME = 0x00040000;
        private const int WS_CHILD = 0x40000000;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private bool nightMode_ = false;

        private void TouchscreenKeyboardForm_Load(object sender, EventArgs e)
        {
            if (nightMode_)
                EnterNightMode(this);

            ClientSize = new Size(keyboardcontrol.Size.Width, keyboardcontrol.Size.Height);
            Location = new Point(0, Screen.FromControl(this).Bounds.Bottom - Size.Height);
        }

        private void keyboardcontrol_UserKeyPressed(object sender, KeyboardEventArgs e)
        {
            SendKeys.Send(e.KeyboardKeyPressed);
        }

        private void keyboardcontrol_ExitKeyPressed(object sender)
        {
            Close();
        }

        // set night mode on
        private static void SetNightModeOn(Control control)
        {
            Button bt = control as Button;
            if (bt != null)
            {
                bt.ForeColor = Color.Red;
                bt.BackColor = Color.Black;
                bt.FlatStyle = FlatStyle.Flat;
                bt.FlatAppearance.BorderColor = Color.Red;
                bt.FlatAppearance.MouseDownBackColor = Color.Black;
                bt.FlatAppearance.MouseOverBackColor = Color.Black;
                return;
            }

            Keyboardcontrol kbd = control as Keyboardcontrol;
            if (kbd != null)
            {
                kbd.KeyboardType = BoW.StandardNight;
                return;
            }

            control.ForeColor = Color.Red;
            control.BackColor = Color.Black;
        }
        private static void EnterNightMode(Control control)
        {
            SetNightModeOn(control);
            for (int i = control.Controls.Count; --i >= 0; )
                EnterNightMode(control.Controls[i]);
        }
    }
}
