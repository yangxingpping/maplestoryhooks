using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;

namespace MapleStoryHooks
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        public MaplePacket CurrentPacket { get; set; }

        public DateTime Time { get; set; }

        
        public delegate void PacketFinished(MaplePacket packet);

        public void OnSendPacketFinished(MaplePacket packet)
        {
            string data = "";

            foreach (PacketSegment segment in packet.Segments)
            {
                data += segment.ToString() + " ";
            }

            data.TrimEnd(' ');

            int opcode = (int)packet.Segments[0].Value;

            listView1.Items.Add(new ListViewItem(new string[] { packet.Direction, packet.ToArray().Length.ToString(), BitConverter.ToString(packet.ToArray()) }));
            listView2.Items.Add(new ListViewItem(new string[] { packet.Direction, opcode.ToString(), data }));
        }

        public void OnRecvPacketFinished(MaplePacket packet)
        {
            string data = "";

            foreach (PacketSegment segment in packet.Segments)
            {
                data += segment.ToString() + " ";
            }

            data.TrimEnd(' ');

            int opcode = (int)packet.Segments[0].Value;

            listView1.Items.Add(new ListViewItem(new string[] { packet.Direction, packet.ToArray().Length.ToString(), BitConverter.ToString(packet.ToArray()) }));
            listView2.Items.Add(new ListViewItem(new string[] { packet.Direction, opcode.ToString(), data }));
        }

        public void AddSegment(int id, PacketSegment segment)
        {
            try
            {
                if (CurrentPacket == null)
                {
                    CurrentPacket = new MaplePacket(id, segment.Direction);
                    CurrentPacket.Segments.Add(segment);
                }
                else
                {
                    if (CurrentPacket.Id == id)
                    {
                        CurrentPacket.Segments.Add(segment);
                    }
                    else
                    {
                        MaplePacket oldPacket = CurrentPacket;

                        if (segment.Direction == "SEND")
                        {
                            this.Invoke(new PacketFinished(OnSendPacketFinished), oldPacket);
                        }
                        else
                        {
                            this.Invoke(new PacketFinished(OnRecvPacketFinished), oldPacket);
                        }

                        CurrentPacket = new MaplePacket(id, segment.Direction);
                        CurrentPacket.Segments.Add(segment);
                    }
                }
                Time = DateTime.Now;
            }
            catch (Exception e)
            {
                Main.Interface.WriteConsole("ADDSEGMENT: " + e.StackTrace + "\r\n" + e.Message);
            }
        }


        #region Hooked Methods
        public int OutPacketInitHooked(IntPtr @this, int nType, int bLoopback)
        {

            return Main.OutPacketInitOriginal(@this, nType, bLoopback);
        }

     

        public void EncodeByteHooked(IntPtr @this, byte n)
        {
            PacketSegment segment = new PacketSegment(@this.ToInt32(), PacketSegmentType.BYTE, n, "SEND");

            AddSegment(@this.ToInt32(), segment);

            Main.EncodeByteOriginal(@this, n);
        }

        public void EncodeShortHooked(IntPtr @this, UInt16 n)
        {
            PacketSegment segment = new PacketSegment(@this.ToInt32(), PacketSegmentType.SHORT, n, "SEND");

            AddSegment(@this.ToInt32(), segment);

            Main.EncodeShortOriginal(@this, n);
        }

        public void EncodeIntHooked(IntPtr @this, UInt32 n)
        {
            PacketSegment segment = new PacketSegment(@this.ToInt32(), PacketSegmentType.INT, n, "SEND");

            AddSegment(@this.ToInt32(), segment);

            Main.EncodeIntOriginal(@this, n);
        }

        public void EncodeBufferHooked(IntPtr @this, IntPtr bufferPointer, UInt32 uSize)
        {
            byte[] data = new byte[uSize];
            Marshal.Copy(bufferPointer, data, 0, (int)uSize);

            PacketSegment segment = new PacketSegment(@this.ToInt32(), PacketSegmentType.BUFFER, data, "SEND");

            AddSegment(@this.ToInt32(), segment);

            Main.EncodeBufferOriginal(@this, bufferPointer, uSize);
        }

        public void EncodeStringHooked(IntPtr @this, IntPtr stringPointer)
        {

            string s = Marshal.PtrToStringAnsi(stringPointer);

            PacketSegment segment = new PacketSegment(@this.ToInt32(), PacketSegmentType.STRING, s, "SEND");

            AddSegment(@this.ToInt32(), segment);

            Main.EncodeStringOriginal(@this, stringPointer);
        }


        public int SendPacketHooked(IntPtr @this, IntPtr packetPointer)
        {
            COutPacket packet = (COutPacket)Marshal.PtrToStructure(@this, typeof(COutPacket));
            byte[] data = packet.ToArray();

            PacketSegment segment = new PacketSegment(@this.ToInt32(), PacketSegmentType.BUFFER, data, "SEND WHOLE");

            AddSegment(@this.ToInt32(), segment);

            return Main.SendPacketOriginal(@this, packetPointer);
        }


        public byte DecodeByteHooked(IntPtr @this)
        {
            CInPacket packet = (CInPacket)Marshal.PtrToStructure(@this, typeof(CInPacket));
            byte result = packet.ToReader().ReadByte();

            PacketSegment segment = new PacketSegment(@this.ToInt32(), PacketSegmentType.BYTE, result, "RECV");
            AddSegment(@this.ToInt32(), segment);

            return Main.DecodeByteOriginal(@this);
        }

        public UInt16 DecodeShortHooked(IntPtr @this)
        {
            CInPacket packet = (CInPacket)Marshal.PtrToStructure(@this, typeof(CInPacket));
            short result = packet.ToReader().ReadInt16();

            PacketSegment segment = new PacketSegment(@this.ToInt32(), PacketSegmentType.SHORT, result, "RECV");
            AddSegment(@this.ToInt32(), segment);

            return Main.DecodeShortOriginal(@this);
        }

        public UInt32 DecodeIntHooked(IntPtr @this)
        {
            CInPacket packet = (CInPacket)Marshal.PtrToStructure(@this, typeof(CInPacket));
            int result = packet.ToReader().ReadInt32();

            PacketSegment segment = new PacketSegment(@this.ToInt32(), PacketSegmentType.INT, result, "RECV");
            AddSegment(@this.ToInt32(), segment);

            return Main.DecodeIntOriginal(@this);
        }

        public void DecodeBufferHooked(IntPtr @this, IntPtr bufferPointer, UInt32 uSize)
        {
            CInPacket packet = (CInPacket)Marshal.PtrToStructure(@this, typeof(CInPacket));
            byte[] result = packet.ToReader().ReadBytes((int)uSize);

            PacketSegment segment = new PacketSegment(@this.ToInt32(), PacketSegmentType.BUFFER, result, "RECV");
            AddSegment(@this.ToInt32(), segment);

            Main.DecodeBufferOriginal(@this, bufferPointer, uSize);
        }

        public IntPtr DecodeStringHooked(IntPtr @this, IntPtr resultPointer)
        {
            CInPacket packet = (CInPacket)Marshal.PtrToStructure(@this, typeof(CInPacket));
            BinaryReader reader = packet.ToReader();
            int length = reader.ReadInt16();
            string result = Encoding.ASCII.GetString(reader.ReadBytes(length));

            reader.Close(); // Should close all other readers????

            PacketSegment segment = new PacketSegment(@this.ToInt32(), PacketSegmentType.STRING, result, "RECV");
            AddSegment(@this.ToInt32(), segment);

            return Main.DecodeStringOriginal(@this, resultPointer);
        }
        #endregion

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (CurrentPacket != null && Time != null && Time.Ticks > 0 && DateTime.Now.Ticks - Time.Ticks > 500)
            {
                MaplePacket oldPacket = CurrentPacket;
                if (oldPacket.Direction == "SEND")
                {
                    this.Invoke(new PacketFinished(OnSendPacketFinished), oldPacket);
                }
                else
                {
                    this.Invoke(new PacketFinished(OnRecvPacketFinished), oldPacket);
                }
                CurrentPacket = null;
            }
        }
    } 
}
