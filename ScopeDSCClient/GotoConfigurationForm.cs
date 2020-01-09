using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AAB.UtilityLibrary;

namespace ScopeDSCClient
{
    public partial class GotoConfigurationForm : Form
    {
        public GotoConfigurationForm(ScopeGotoClient parent, ScopeGotoClientSettings settings)
        {
            parent_ = parent;
            settings_ = settings;
            InitializeComponent();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (settings_ == null)
                return;

            SaveFileDialog savefile = new SaveFileDialog();
            savefile.InitialDirectory = Application.StartupPath + @"\";
            savefile.FileName = MakeProfileName();
            savefile.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";

            try
            {
                DialogResult res = savefile.ShowDialog();
                if (res != DialogResult.OK)
                    return;

                XmlProfile profile = new XmlProfile(savefile.FileName);
                profile.AddTypes = AddType.Short;
                using (profile.Buffer())
                {
                    {
                        ScopeGotoClient.MotorOptions opt = settings_.AltMotorOptions;
                        if (opt.Valid)
                            profile.SetValue("entriesGoTo", "AltMotorOptions", opt);

                        opt = settings_.AzmMotorOptions;
                        if (opt.Valid)
                            profile.SetValue("entriesGoTo", "AzmMotorOptions", opt);
                    }

                    {
                        ScopeGotoClient.AdapterOptions opt = settings_.AltAdapterOptions;
                        if (opt.Valid)
                            profile.SetValue("entriesGoTo", "AltAdapterOptions", opt);

                        opt = settings_.AzmAdapterOptions;
                        if (opt.Valid)
                            profile.SetValue("entriesGoTo", "AzmAdapterOptions", opt);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            if (!parent_.CanConfigureMotorsAndAdapters() || settings_ == null)
                return;

            OpenFileDialog openfile = new OpenFileDialog();
            //openfile.InitialDirectory = Application.StartupPath + @"\";
            openfile.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            openfile.FilterIndex = 1;
            openfile.RestoreDirectory = true;

            try
            {
                DialogResult res = openfile.ShowDialog();
                if (res != DialogResult.OK)
                    return;

                XmlProfile profile = new XmlProfile(openfile.FileName);
                parent_.ConfigureMotor(ScopeGotoClient.M_ALT,
                    (ScopeGotoClient.MotorOptions)profile.GetValue("entriesGoTo", "AltMotorOptions", new ScopeGotoClient.MotorOptions()));
                parent_.ConfigureMotor(ScopeGotoClient.M_AZM,
                    (ScopeGotoClient.MotorOptions)profile.GetValue("entriesGoTo", "AzmMotorOptions", new ScopeGotoClient.MotorOptions()));
                parent_.ConfigureAdapter(ScopeGotoClient.A_ALT,
                    (ScopeGotoClient.AdapterOptions)profile.GetValue("entriesGoTo", "AltAdapterOptions", new ScopeGotoClient.AdapterOptions()));
                parent_.ConfigureAdapter(ScopeGotoClient.A_AZM,
                    (ScopeGotoClient.AdapterOptions)profile.GetValue("entriesGoTo", "AzmAdapterOptions", new ScopeGotoClient.AdapterOptions()));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        private string MakeProfileName()
        {
            DateTime dt = DateTime.Now;
            return String.Format("GoToConfiguration{0}-{1}-{2}_{3}-{4}-{5}.xml",
                dt.Year.ToString("D4"), dt.Month.ToString("D2"), dt.Day.ToString("D2"), dt.Hour.ToString("D2"), dt.Minute.ToString("D2"), dt.Second.ToString("D2"));
        }

        private ScopeGotoClient parent_;
        private ScopeGotoClientSettings settings_;

        private void GotoConfigurationForm_Load(object sender, EventArgs e)
        {
            if (!parent_.CanConfigureMotorsAndAdapters())
                buttonApply.Enabled = false;
        }
    }
}
