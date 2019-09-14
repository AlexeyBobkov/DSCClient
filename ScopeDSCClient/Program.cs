using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ScopeDSCClient
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (args.Length > 0 && args[0] == "/goto")
                Application.Run(new ScopeGotoClient());
            else
                Application.Run(new ScopeDSCClient());
        }
    }
}
