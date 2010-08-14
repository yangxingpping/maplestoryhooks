using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EasyHook;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using System.Diagnostics;

namespace MapleStoryHooks
{
    public class Main : IEntryPoint
    {
        internal static MapleStoryHookInterface Interface;
        internal static List<LocalHook> hooks;

        #region Original Functions
        internal static DOutPacketInit OutPacketInitOriginal;
        internal static DEncodeByte EncodeByteOriginal;
        internal static DEncodeShort EncodeShortOriginal;
        internal static DEncodeInt EncodeIntOriginal;
        internal static DEncodeBuffer EncodeBufferOriginal;
        internal static DEncodeString EncodeStringOriginal;

        internal static DDecodeByte DecodeByteOriginal;
        internal static DDecodeShort DecodeShortOriginal;
        internal static DDecodeInt DecodeIntOriginal;
        internal static DDecodeBuffer DecodeBufferOriginal;
        internal static DDecodeString DecodeStringOriginal;

        internal static DSendPacket SendPacketOriginal;
        #endregion

        #region Addresses
        internal static IntPtr OutPacketInitAddress = (IntPtr)0x004241AD;
        internal static IntPtr EncodeByteAddress = (IntPtr)0x00403FEE;
        internal static IntPtr EncodeShortAddress = (IntPtr)0x0040D928;
        internal static IntPtr EncodeIntAddress = (IntPtr)0x00403E9E;
        internal static IntPtr EncodeBufferAddress = (IntPtr)0x00409449;
        internal static IntPtr EncodeStringAddress = (IntPtr)0x0040D949;

        internal static IntPtr SendPacketAddress = (IntPtr)0x0049637B; //GMS 83

        internal static IntPtr DecodeByteAddress = (IntPtr)0x00403D9C;
        internal static IntPtr DecodeShortAddress = (IntPtr)0x0040928A;
        internal static IntPtr DecodeIntAddress = (IntPtr)0x00403FB6;
        internal static IntPtr DecodeBufferAddress = (IntPtr)0x0040E50A;
        internal static IntPtr DecodeStringAddress = (IntPtr)0x00409520;
        #endregion

        internal Form1 form = new Form1();

        public Main(RemoteHooking.IContext InContext, String InChannelName)
        {
            Interface = RemoteHooking.IpcConnectClient<MapleStoryHookInterface>(InChannelName);
        }

        public void Run(RemoteHooking.IContext InContext, String InChannelName)
        {

            try
            {
                // Call Host
                Interface.IsInstalled(RemoteHooking.GetCurrentProcessId());

                LocalHook.EnableRIPRelocation();

                string OutPacketInitPattern = "B8??????00E8??????005151568BF183660400";
                string EncodeBytePattern = "568BF16A01E8????????8B4E088B4604";
                string EncodeShortPattern = "568BF16A02E8????????8B4E088B4604";
                string EncodeIntPattern = "568BF16A04E8????????8B4E088B4604";
                
                Scanner scanner = new Scanner(0xFFFFFF);
                EncodeByteAddress = scanner.FindPattern(EncodeBytePattern, 0);
                EncodeShortAddress = scanner.FindPattern(EncodeShortPattern, 0);
                EncodeIntAddress = scanner.FindPattern(EncodeIntPattern, 0);
                OutPacketInitAddress = scanner.FindPattern(OutPacketInitPattern, 0);


                OutPacketInitOriginal = (DOutPacketInit)Marshal.GetDelegateForFunctionPointer(OutPacketInitAddress, typeof(DOutPacketInit));
                EncodeByteOriginal = (DEncodeByte)Marshal.GetDelegateForFunctionPointer(EncodeByteAddress, typeof(DEncodeByte));
                EncodeShortOriginal = (DEncodeShort)Marshal.GetDelegateForFunctionPointer(EncodeShortAddress, typeof(DEncodeShort));
                EncodeIntOriginal = (DEncodeInt)Marshal.GetDelegateForFunctionPointer(EncodeIntAddress, typeof(DEncodeInt));
                EncodeBufferOriginal = (DEncodeBuffer)Marshal.GetDelegateForFunctionPointer(EncodeBufferAddress, typeof(DEncodeBuffer));
                EncodeStringOriginal = (DEncodeString)Marshal.GetDelegateForFunctionPointer(EncodeStringAddress, typeof(DEncodeString));

                DecodeByteOriginal = (DDecodeByte)Marshal.GetDelegateForFunctionPointer(DecodeByteAddress, typeof(DDecodeByte));
                DecodeShortOriginal = (DDecodeShort)Marshal.GetDelegateForFunctionPointer(DecodeShortAddress, typeof(DDecodeShort));
                DecodeIntOriginal = (DDecodeInt)Marshal.GetDelegateForFunctionPointer(DecodeIntAddress, typeof(DDecodeInt));
                DecodeBufferOriginal = (DDecodeBuffer)Marshal.GetDelegateForFunctionPointer(DecodeBufferAddress, typeof(DDecodeBuffer));
                DecodeStringOriginal = (DDecodeString)Marshal.GetDelegateForFunctionPointer(DecodeStringAddress, typeof(DDecodeString));

                SendPacketOriginal = (DSendPacket)Marshal.GetDelegateForFunctionPointer(SendPacketAddress, typeof(DSendPacket));

                hooks = new List<LocalHook>();

                hooks.Add(LocalHook.Create(OutPacketInitAddress, new DOutPacketInit(form.OutPacketInitHooked), this));
                hooks.Add(LocalHook.Create(EncodeByteAddress, new DEncodeByte(form.EncodeByteHooked), this));
                hooks.Add(LocalHook.Create(EncodeShortAddress, new DEncodeShort(form.EncodeShortHooked), this));
                hooks.Add(LocalHook.Create(EncodeIntAddress, new DEncodeInt(form.EncodeIntHooked), this));
                hooks.Add(LocalHook.Create(EncodeBufferAddress, new DEncodeBuffer(form.EncodeBufferHooked), this));
                hooks.Add(LocalHook.Create(EncodeStringAddress, new DEncodeString(form.EncodeStringHooked), this));

                hooks.Add(LocalHook.Create(DecodeByteAddress, new DDecodeByte(form.DecodeByteHooked), this));
                hooks.Add(LocalHook.Create(DecodeShortAddress, new DDecodeShort(form.DecodeShortHooked), this));
                hooks.Add(LocalHook.Create(DecodeIntAddress, new DDecodeInt(form.DecodeIntHooked), this));
                hooks.Add(LocalHook.Create(DecodeBufferAddress, new DDecodeBuffer(form.DecodeBufferHooked), this));
                hooks.Add(LocalHook.Create(DecodeStringAddress, new DDecodeString(form.DecodeStringHooked), this));


                //hooks.Add(LocalHook.Create(SendPacketAddress, new DSendPacket(form.SendPacketHooked), this));


                hooks.ForEach(hook => hook.ThreadACL.SetExclusiveACL(new Int32[] { 0 }));

                Interface.WriteConsole("Initialized Hooks: " + hooks.Count);
                
                form.ShowDialog();

            }
            catch (Exception e)
            {
                Interface.WriteConsole("ERROR: " + e);
            }

        }

        #region Delegates
        [UnmanagedFunctionPointer(CallingConvention.ThisCall, SetLastError = true)]
        public delegate void DOutPacketInit(IntPtr @this, int nType, int bLoopback);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall, SetLastError = true)]
        public delegate void DEncodeByte(IntPtr @this, byte n);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall, SetLastError = true)]
        public delegate void DEncodeShort(IntPtr @this, UInt16 n);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall, SetLastError = true)]
        public delegate void DEncodeInt(IntPtr @this, UInt32 n);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall, SetLastError = true)]
        public delegate void DEncodeBuffer(IntPtr @this, IntPtr bufferPointer, UInt32 uSize);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall, SetLastError = true)]
        public delegate void DEncodeString(IntPtr @this, IntPtr stringPointer);


        [UnmanagedFunctionPointer(CallingConvention.ThisCall, SetLastError = true)]
        public delegate int DSendPacket(IntPtr @this, IntPtr packetPointer);


        [UnmanagedFunctionPointer(CallingConvention.ThisCall, SetLastError = true)]
        public delegate byte DDecodeByte(IntPtr @this);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall, SetLastError = true)]
        public delegate UInt16 DDecodeShort(IntPtr @this);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall, SetLastError = true)]
        public delegate UInt32 DDecodeInt(IntPtr @this);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall, SetLastError = true)]
        public delegate void DDecodeBuffer(IntPtr @this, IntPtr bufferPointer, UInt32 uSize);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall, SetLastError = true)]
        public delegate IntPtr DDecodeString(IntPtr @this, IntPtr resultPointer);

        #endregion


    }

    [StructLayout(LayoutKind.Sequential)]
    public class MaplePacket
    {
        public int Loopback; //0x00
        public int State; //0x04
        public IntPtr BufferPointer; //0x08
        public UInt16 Length; //0x0C
        public UInt16 RawSeq; //0x0E
        public UInt16 DataLen; //0x10
        public UInt16 UNK_SHORT; //0x12
        public UInt32 Offset; //0x14


        public byte[] ToArray()
        {
            byte[] buffer = new byte[Length];
            Marshal.Copy(BufferPointer, buffer, 0, Length);
            return buffer;
        }

        public BinaryReader ToReader()
        {
            MemoryStream memStream = new MemoryStream(ToArray());
            memStream.Position = Offset;
            BinaryReader reader = new BinaryReader(memStream);
            return reader;
        }
 
    }

}