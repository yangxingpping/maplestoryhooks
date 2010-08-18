using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using EasyHook;
using System.Runtime.Remoting;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MapleStoryHooks
{
   



    public class Program
    {
        [STAThread]
        private static void Main(string[] pArgs)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

    }
}
