using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace KeyboardClassLibrary
{
    public partial class Keyboardcontrol : UserControl
    {
        public Keyboardcontrol()
        {
            InitializeComponent();
        }

        private Boolean shiftindicator = false;
        private Boolean capslockindicator = false;
        private Boolean ctrlindicator = false;
        private Boolean altindicator = false;
        private Boolean fnindicator = false;
        private string pvtKeyboardKeyPressed = "";

        private BoW pvtKeyboardType = BoW.StandardDay;
        public BoW KeyboardType
        {
            get
            {
                return pvtKeyboardType;
            }
            set
            {
                pvtKeyboardType = value;
                if (shiftindicator) HandleShiftClick();
                if (capslockindicator) HandleCapsLock();

                if (pvtKeyboardType == BoW.StandardDay)
                {
                    pictureBoxKeyboard.Image = KeyboardClassLibrary.Properties.Resources.keyboard_white;
                    pictureBoxCapsLockDown.Image = KeyboardClassLibrary.Properties.Resources.caps_down_white;
                    pictureBoxLeftShiftDown.Image = KeyboardClassLibrary.Properties.Resources.shift_down_white;
                    pictureBoxCtrlDown.Image = KeyboardClassLibrary.Properties.Resources.ctrl_down_white;
                    pictureBoxAltDown.Image = KeyboardClassLibrary.Properties.Resources.alt_down_white;
                    pictureBoxFnDown.Image = KeyboardClassLibrary.Properties.Resources.fn_down_white;
                }
                else //pvtKeyboardType == BoW.StandardNight
                {
                    pictureBoxKeyboard.Image = KeyboardClassLibrary.Properties.Resources.keyboard_red;
                    pictureBoxCapsLockDown.Image = KeyboardClassLibrary.Properties.Resources.caps_down_red;
                    pictureBoxLeftShiftDown.Image = KeyboardClassLibrary.Properties.Resources.shift_down_red;
                    pictureBoxCtrlDown.Image = KeyboardClassLibrary.Properties.Resources.ctrl_down_red;
                    pictureBoxAltDown.Image = KeyboardClassLibrary.Properties.Resources.alt_down_red;
                    pictureBoxFnDown.Image = KeyboardClassLibrary.Properties.Resources.fn_down_red;
                }
            }
        }

        [Category("Mouse"), Description("Return value of mouseclicked key")]
        public event KeyboardDelegate UserKeyPressed;
        protected virtual void OnUserKeyPressed(KeyboardEventArgs e)
        {
            if (UserKeyPressed != null)
                UserKeyPressed(this, e);
        }

        [Category("Mouse"), Description("Exit key clicked")]
        public event ExitDelegate ExitKeyPressed;
        protected virtual void OnExitKeyPressed()
        {
            if (ExitKeyPressed != null)
                ExitKeyPressed(this);
        }

        private void pictureBoxKeyboard_MouseClick(object sender, MouseEventArgs e)
        {
            Single xpos = e.X;
            Single ypos = e.Y;

            xpos = 902 * (xpos / pictureBoxKeyboard.Width);
            ypos = 302 * (ypos / pictureBoxKeyboard.Height);

            pvtKeyboardKeyPressed = HandleTheMouseClick(xpos, ypos);

            KeyboardEventArgs dea = new KeyboardEventArgs(pvtKeyboardKeyPressed);

            OnUserKeyPressed(dea);
        }

        private void pictureBoxLeftShiftState_MouseClick(object sender, MouseEventArgs e)
        {
            HandleShiftClick();
        }

        private void pictureBoxCapsLockState_MouseClick(object sender, MouseEventArgs e)
        {
            HandleCapsLock();
        }

        private void pictureBoxCtrlDown_MouseClick(object sender, MouseEventArgs e)
        {
            HandleCtrlClick();
        }

        private void pictureBoxAltDown_MouseClick(object sender, MouseEventArgs e)
        {
            HandleAltClick();
        }

        private void pictureBoxFnDown_MouseClick(object sender, MouseEventArgs e)
        {
            HandleFnClick();
        }

        private string HandleTheMouseClick(Single x, Single y)
        {
            string Keypressed = null;
            if (x >= 1 && x < 901 && y >= 1 && y < 301)         //  keyboard section
            {
                if (y < 61)
                {
                    if (x >= 1 && x < 61) Keypressed = HandleShiftableKey("`");
                    else if (x >= 61 && x < 121) Keypressed = HandleShiftableKey("1");
                    else if (x >= 121 && x < 181) Keypressed = HandleShiftableKey("2");
                    else if (x >= 181 && x < 241) Keypressed = HandleShiftableKey("3");
                    else if (x >= 241 && x < 301) Keypressed = HandleShiftableKey("4");
                    else if (x >= 301 && x < 361) Keypressed = HandleShiftableKey("5");
                    else if (x >= 361 && x < 421) Keypressed = HandleShiftableKey("6");
                    else if (x >= 421 && x < 481) Keypressed = HandleShiftableKey("7");
                    else if (x >= 481 && x < 541) Keypressed = HandleShiftableKey("8");
                    else if (x >= 541 && x < 601) Keypressed = HandleShiftableKey("9");
                    else if (x >= 601 && x < 661) Keypressed = HandleShiftableKey("0");
                    else if (x >= 661 && x < 721) Keypressed = HandleShiftableKey("-");
                    else if (x >= 721 && x < 781) Keypressed = HandleShiftableKey("=");
                    else if (x >= 781 && x < 841) Keypressed = HandleShiftableKey("\\");
                    else if (x >= 841 && x < 901) Keypressed = HandleKey("{BACKSPACE}");
                    else Keypressed = null;
                }
                else if (y >= 61 && y < 121)
                {
                    if (x >= 1 && x < 91) Keypressed = HandleShiftableKey("{TAB}");
                    else if (x >= 91 && x < 151) Keypressed = HandleShiftableCaplockableKey("q");
                    else if (x >= 151 && x < 211) Keypressed = HandleShiftableCaplockableKey("w");
                    else if (x >= 211 && x < 271) Keypressed = HandleShiftableCaplockableKey("e");
                    else if (x >= 271 && x < 331) Keypressed = HandleShiftableCaplockableKey("r");
                    else if (x >= 331 && x < 391) Keypressed = HandleShiftableCaplockableKey("t");
                    else if (x >= 391 && x < 451) Keypressed = HandleShiftableCaplockableKey("y");
                    else if (x >= 451 && x < 511) Keypressed = HandleShiftableCaplockableKey("u");
                    else if (x >= 511 && x < 571) Keypressed = HandleShiftableCaplockableKey("i");
                    else if (x >= 571 && x < 631) Keypressed = HandleShiftableCaplockableKey("o");
                    else if (x >= 631 && x < 691) Keypressed = HandleShiftableCaplockableKey("p");
                    else if (x >= 691 && x < 751) Keypressed = HandleShiftableKey("{[}");
                    else if (x >= 751 && x < 811) Keypressed = HandleShiftableKey("{]}");
                    else if (x >= 811 && x < 901) HandleExitClick();
                    else Keypressed = null;
                }
                else if (y >= 121 && y < 181)
                {
                    if (x >= 1 && x < 121) HandleCapsLock();
                    else if (x >= 121 && x < 181) Keypressed = HandleShiftableCaplockableKey("a");
                    else if (x >= 181 && x < 241) Keypressed = HandleShiftableCaplockableKey("s");
                    else if (x >= 241 && x < 301) Keypressed = HandleShiftableCaplockableKey("d");
                    else if (x >= 301 && x < 361) Keypressed = HandleShiftableCaplockableKey("f");
                    else if (x >= 361 && x < 421) Keypressed = HandleShiftableCaplockableKey("g");
                    else if (x >= 421 && x < 481) Keypressed = HandleShiftableCaplockableKey("h");
                    else if (x >= 481 && x < 541) Keypressed = HandleShiftableCaplockableKey("j");
                    else if (x >= 541 && x < 601) Keypressed = HandleShiftableCaplockableKey("k");
                    else if (x >= 601 && x < 661) Keypressed = HandleShiftableCaplockableKey("l");
                    else if (x >= 661 && x < 721) Keypressed = HandleShiftableKey(";");
                    else if (x >= 721 && x < 781) Keypressed = HandleShiftableKey("'");
                    else if (x >= 781 && x < 901) Keypressed = HandleKey("{ENTER}");
                    else Keypressed = null;
                }
                else if (y >= 181 && y < 241)
                {
                    if (x >= 1 && x < 151) HandleShiftClick();
                    else if (x >= 151 && x < 211) Keypressed = HandleShiftableCaplockableKey("z");
                    else if (x >= 211 && x < 271) Keypressed = HandleShiftableCaplockableKey("x");
                    else if (x >= 271 && x < 331) Keypressed = HandleShiftableCaplockableKey("c");
                    else if (x >= 331 && x < 391) Keypressed = HandleShiftableCaplockableKey("v");
                    else if (x >= 391 && x < 451) Keypressed = HandleShiftableCaplockableKey("b");
                    else if (x >= 451 && x < 511) Keypressed = HandleShiftableCaplockableKey("n");
                    else if (x >= 511 && x < 571) Keypressed = HandleShiftableCaplockableKey("m");
                    else if (x >= 571 && x < 631) Keypressed = HandleShiftableKey(",");
                    else if (x >= 631 && x < 691) Keypressed = HandleShiftableKey(".");
                    else if (x >= 691 && x < 751) Keypressed = HandleShiftableKey("/");
                    else if (x >= 781 && x < 841) Keypressed = HandleFunctionableKey("{UP}");
                    else if (x >= 841 && x < 901) HandleFnClick();
                    else Keypressed = null;
                }
                else if (y >= 241 && y < 301)
                {
                    if (x >= 1 && x < 61) HandleCtrlClick();
                    else if (x >= 61 && x < 121) HandleAltClick();
                    else if (x >= 151 && x < 211) Keypressed = HandleKey("{ESC}");
                    else if (x >= 226 && x < 646) Keypressed = HandleKey(" ");
                    else if (x >= 661 && x < 721) Keypressed = HandleFunctionableKey("{DELETE}");
                    else if (x >= 721 && x < 781) Keypressed = HandleFunctionableKey("{LEFT}");
                    else if (x >= 781 && x < 841) Keypressed = HandleFunctionableKey("{DOWN}");
                    else if (x >= 841 && x < 901) Keypressed = HandleFunctionableKey("{RIGHT}");
                    else Keypressed = null;
                }
            }
            if (Keypressed != null)
            {
                if (shiftindicator) HandleShiftClick();
                if (ctrlindicator) HandleCtrlClick();
                if (altindicator) HandleAltClick();
                return Keypressed;
            }
            else
            {
                return null;
            }
        }

        private string HandleKey(string theKey)
        {
            if (ctrlindicator)
                theKey = "^" + theKey;

            if (altindicator)
                theKey = "%" + theKey;

            return theKey;
        }

        private string HandleShiftableKey(string theKey)
        {
            if (shiftindicator)
                theKey = "+" + theKey;

            return HandleKey(theKey);
        }

        private string HandleShiftableCaplockableKey(string theKey)
        {
            if (capslockindicator)
                theKey = "+" + theKey;
            else if (shiftindicator)
                theKey = "+" + theKey;

            return HandleKey(theKey);
        }

        private string HandleFunctionableKey(string theKey)
        {
            if (fnindicator)
            {
                switch (theKey)
                {
                    case "{UP}": return HandleShiftableKey("{PGUP}");
                    case "{DOWN}": return HandleShiftableKey("{PGDN}");
                    case "{LEFT}": return HandleShiftableKey("{HOME}");
                    case "{RIGHT}": return HandleShiftableKey("{END}");
                    case "{DELETE}": return HandleShiftableKey("{INSERT}");
                    default: return null;
                }
            }
            return HandleShiftableKey(theKey);
        }

        private void HandleExitClick()
        {
            OnExitKeyPressed();
        }

        private void HandleShiftClick()
        {
            if (shiftindicator)
            {
                shiftindicator = false;
                pictureBoxLeftShiftDown.Visible = false;
            }
            else
            {
                shiftindicator = true;
                pictureBoxLeftShiftDown.Visible = true;
            }
        }

        private void HandleCapsLock()
        {
            if (capslockindicator)
            {
                capslockindicator = false;
                pictureBoxCapsLockDown.Visible = false;
            }
            else
            {
                capslockindicator = true;
                pictureBoxCapsLockDown.Visible = true;
            }
        }

        private void HandleCtrlClick()
        {
            if (ctrlindicator)
            {
                ctrlindicator = false;
                pictureBoxCtrlDown.Visible = false;
            }
            else
            {
                ctrlindicator = true;
                pictureBoxCtrlDown.Visible = true;
            }
        }

        private void HandleAltClick()
        {
            if (altindicator)
            {
                altindicator = false;
                pictureBoxAltDown.Visible = false;
            }
            else
            {
                altindicator = true;
                pictureBoxAltDown.Visible = true;
            }
        }

        private void HandleFnClick()
        {
            if (fnindicator)
            {
                fnindicator = false;
                pictureBoxFnDown.Visible = false;
            }
            else
            {
                fnindicator = true;
                pictureBoxFnDown.Visible = true;
            }
        }

        private void pictureBoxKeyboard_SizeChanged(object sender, EventArgs e)
        {
            // position the capslock and shift down overlays
            pictureBoxCapsLockDown.Left = Convert.ToInt16(pictureBoxKeyboard.Width * 1 / 902);
            pictureBoxCapsLockDown.Top = Convert.ToInt16(pictureBoxKeyboard.Height * 121 / 302);
            pictureBoxLeftShiftDown.Left = Convert.ToInt16(pictureBoxKeyboard.Width * 1 / 902);
            pictureBoxLeftShiftDown.Top = Convert.ToInt16(pictureBoxKeyboard.Height * 181 / 302);
            pictureBoxCtrlDown.Left = Convert.ToInt16(pictureBoxKeyboard.Width * 1 / 902);
            pictureBoxCtrlDown.Top = Convert.ToInt16(pictureBoxKeyboard.Height * 241 / 302);
            pictureBoxAltDown.Left = Convert.ToInt16(pictureBoxKeyboard.Width * 61 / 902);
            pictureBoxAltDown.Top = Convert.ToInt16(pictureBoxKeyboard.Height * 241 / 302);
            pictureBoxFnDown.Left = Convert.ToInt16(pictureBoxKeyboard.Width * 841 / 902);
            pictureBoxFnDown.Top = Convert.ToInt16(pictureBoxKeyboard.Height * 181 / 302);


            // size the capslock and shift down overlays

            pictureBoxCapsLockDown.Width = Convert.ToInt16(pictureBoxKeyboard.Width * 120 / 902);
            pictureBoxCapsLockDown.Height = Convert.ToInt16(pictureBoxKeyboard.Height * 60 / 302);
            pictureBoxLeftShiftDown.Width = Convert.ToInt16(pictureBoxKeyboard.Width * 150 / 902);
            pictureBoxLeftShiftDown.Height = Convert.ToInt16(pictureBoxKeyboard.Height * 60 / 302);
            pictureBoxCtrlDown.Width = Convert.ToInt16(pictureBoxKeyboard.Width * 60 / 902);
            pictureBoxCtrlDown.Height = Convert.ToInt16(pictureBoxKeyboard.Height * 60 / 302);
            pictureBoxAltDown.Width = Convert.ToInt16(pictureBoxKeyboard.Width * 60 / 902);
            pictureBoxAltDown.Height = Convert.ToInt16(pictureBoxKeyboard.Height * 60 / 302);
            pictureBoxFnDown.Width = Convert.ToInt16(pictureBoxKeyboard.Width * 60 / 902);
            pictureBoxFnDown.Height = Convert.ToInt16(pictureBoxKeyboard.Height * 60 / 302);
        }
    }

    public delegate void KeyboardDelegate(object sender, KeyboardEventArgs e);
    public delegate void ExitDelegate(object sender);

    public class KeyboardEventArgs : EventArgs
    {
        private readonly string pvtKeyboardKeyPressed;

        public KeyboardEventArgs(string KeyboardKeyPressed)
        {
            this.pvtKeyboardKeyPressed = KeyboardKeyPressed;
        }

        public string KeyboardKeyPressed
        {
            get
            {
                return pvtKeyboardKeyPressed;
            }
        }
    }

    [Category("Keyboard Type"),Description("Type of keyboard to use")]
    public enum BoW { StandardDay, StandardNight};
}

