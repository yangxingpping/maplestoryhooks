using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using EasyHook;
using System.Runtime.Remoting;

namespace MapleStoryHooks
{

    public class MapleStoryHookInterface : MarshalByRefObject
    {



        public void IsInstalled(Int32 InClientPID)
        {
            Console.WriteLine("Hook installed in target {0}.\r\n", InClientPID);
        }

        public void WriteConsole(string s)
        {
            Console.WriteLine("[DLL] {0}", s);
        }

        public void AddByte(int id, byte n, bool isSend)
        {
            //Console.WriteLine((isSend ? "SEND" : "RECV") + " " + n);
            Form1.Instance.AddItem(n.ToString(), id.ToString());
        }

        public void AddShort(int id, short n, bool isSend)
        {
            //Console.WriteLine((isSend ? "SEND" : "RECV") + " " + n);
        }

        public void AddInt(int id, int n, bool isSend)
        {
            //Console.WriteLine((isSend ? "SEND" : "RECV") + " " + n);
        }

        public void AddBuffer(int id, byte[] buffer, bool isSend)
        {
            //Console.WriteLine((isSend ? "SEND" : "RECV") + " " + BitConverter.ToString(buffer));
        }

        public void AddString(int id, string s, bool isSend)
        {
            //Console.WriteLine((isSend ? "SEND" : "RECV") + " " + s);
            //Form1.Instance;
        }
    }

    public partial class Form1 : Form
    {
        public static Form1 Instance { get; set; }
        public static string ChannelName;

        public Form1()
        {
            InitializeComponent();
            Form1.Instance = this;
            ProcessDialog dialog = new ProcessDialog();
            string processName = "";
            if (dialog.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                processName = dialog.ProcessName;
            }

            statusLabel.Text = "Status: Looking for process...";

            while (true)
            {
                Process[] processes = Process.GetProcessesByName(processName);

                if (processes.Length > 0)
                {
                    Process p = processes[0];
                    statusLabel.Text = "Status: Found process.";
                    int pid = p.Id;
                    string path = @"MapleStoryHooks.dll";
                    string path2 = @"MapleStoryHooks.exe";

                    Config.Register("Maplestory hook", path, path2);

                    RemoteHooking.IpcCreateServer<MapleStoryHookInterface>(ref ChannelName, WellKnownObjectMode.SingleCall);
                    RemoteHooking.Inject(pid, path, path, ChannelName);

                    break;
                }
                Thread.Sleep(500);
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        public void AddItem(string s1, string s2)
        {
            listView1.Items.Add(new ListViewItem(new string[] { "SEND", s1, s2 }));
        }
    }
}
