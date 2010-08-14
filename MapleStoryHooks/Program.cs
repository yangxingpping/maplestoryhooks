using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using EasyHook;
using System.Runtime.Remoting;
using System.Threading;

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
    }
    
    public class Program
    {
        static String ChannelName = null;

        static void Main(string[] args)
        {
            Console.WriteLine("MapleStory Hooksss");
            Console.WriteLine();

            bool lookingForProcess = true;
            while (lookingForProcess)
            {
                Process[] processes = Process.GetProcessesByName("WvsLogin");


                if (processes.Length > 0)
                {
                    Console.WriteLine("Found process.");
                    int pid = processes[0].Id;
                    string path = @"MapleStoryHooks.dll";
                    string path2 = @"MapleStoryHooks.exe";

                    Config.Register("Maplestory hook", path, path2);

                    RemoteHooking.IpcCreateServer<MapleStoryHookInterface>(ref ChannelName, WellKnownObjectMode.SingleCall);
                    RemoteHooking.Inject(pid, path, path, ChannelName);
                    lookingForProcess = false;
                }
                else
                {
                    Console.WriteLine("Cannot find process.");
                    Thread.Sleep(500);
                }
            }

            Console.ReadLine();
        }
    }
}
