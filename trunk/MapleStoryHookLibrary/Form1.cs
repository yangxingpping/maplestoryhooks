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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        #region Hooked Methods
        public void OutPacketInitHooked(IntPtr @this, int nType, int bLoopback)
        {

            Main.original1(@this, nType, bLoopback);
        }

        public void EncodeByteHooked(IntPtr @this, byte n)
        {
            PacketSegment segment = new PacketSegment(PacketSegmentType.BYTE, n, "SEND");
            addToTable(segment);

            Main.original2(@this, n);
        }

        public void EncodeShortHooked(IntPtr @this, UInt16 n)
        {
            PacketSegment segment = new PacketSegment(PacketSegmentType.SHORT, n, "SEND");
            addToTable(segment);

            Main.original3(@this, n);
        }

        public void EncodeIntHooked(IntPtr @this, UInt32 n)
        {
            PacketSegment segment = new PacketSegment(PacketSegmentType.INT, n, "SEND");
            addToTable(segment);

            Main.original4(@this, n);
        }

        public void EncodeBufferHooked(IntPtr @this, IntPtr bufferPointer, UInt32 uSize)
        {
            byte[] data = new byte[uSize];
            Marshal.Copy(bufferPointer, data, 0, (int)uSize);

            PacketSegment segment = new PacketSegment(PacketSegmentType.BUFFER, data, "SEND");
            addToTable(segment);

            Main.original5(@this, bufferPointer, uSize);
        }

        public void EncodeStringHooked(IntPtr @this, IntPtr stringPointer)
        {

            string s = Marshal.PtrToStringAnsi(stringPointer);

            PacketSegment segment = new PacketSegment(PacketSegmentType.STRING, s, "SEND");
            addToTable(segment);

            Main.original6(@this, stringPointer);
        }


        public int SendPacketHooked(IntPtr @this, IntPtr packetPointer)
        {
            listBox1.Items.Add("SND POINTER: " + packetPointer.ToInt32());
            return Main.original12(@this, packetPointer);
        }


        public byte DecodeByteHooked(IntPtr @this)
        {
            MaplePacket packet = (MaplePacket)Marshal.PtrToStructure(@this, typeof(MaplePacket));
            byte result = packet.ToReader().ReadByte();

            PacketSegment segment = new PacketSegment(PacketSegmentType.BYTE, result, "RECV");
            addToTable(segment);

            return Main.original7(@this);
        }

        public UInt16 DecodeShortHooked(IntPtr @this)
        {
            MaplePacket packet = (MaplePacket)Marshal.PtrToStructure(@this, typeof(MaplePacket));
            short result = packet.ToReader().ReadInt16();

            PacketSegment segment = new PacketSegment(PacketSegmentType.SHORT, result, "RECV");
            addToTable(segment);

            return Main.original8(@this);
        }

        public UInt32 DecodeIntHooked(IntPtr @this)
        {
            MaplePacket packet = (MaplePacket)Marshal.PtrToStructure(@this, typeof(MaplePacket));
            int result = packet.ToReader().ReadInt32();

            PacketSegment segment = new PacketSegment(PacketSegmentType.INT, result, "RECV");
            addToTable(segment);

            return Main.original9(@this);
        }

        public void DecodeBufferHooked(IntPtr @this, IntPtr bufferPointer, UInt32 uSize)
        {
            MaplePacket packet = (MaplePacket)Marshal.PtrToStructure(@this, typeof(MaplePacket));
            byte[] result = packet.ToReader().ReadBytes((int)uSize);

            PacketSegment segment = new PacketSegment(PacketSegmentType.BUFFER, result, "RECV");
            addToTable(segment);

            Main.original10(@this, bufferPointer, uSize);
        }

        public IntPtr DecodeStringHooked(IntPtr @this, IntPtr resultPointer)
        {
            MaplePacket packet = (MaplePacket)Marshal.PtrToStructure(@this, typeof(MaplePacket));
            BinaryReader reader = packet.ToReader();
            int length = reader.ReadInt16();
            string result = Encoding.ASCII.GetString(reader.ReadBytes(length));

            reader.Close(); // Should close all other readers????

            PacketSegment segment = new PacketSegment(PacketSegmentType.STRING, result, "RECV");
            addToTable(segment);

            return Main.original11(@this, resultPointer);
        }
        #endregion

        public void addToTable(PacketSegment segment)
        {
            listBox1.Items.Add(segment.Direction + " : " + segment.Type.ToString() + " : " + segment.ToString());
            listBox1.SelectedIndex = listBox1.Items.Count - 1;
            listBox1.SelectedIndex = -1;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
        }
    }

    public class PacketSegment
    {
        public PacketSegmentType Type;
        public object Value;
        public string Direction;

        public PacketSegment(PacketSegmentType pType, object pValue, string pDirection)
        {
            this.Type = pType;
            this.Value = pValue;
            this.Direction = pDirection;
        }

        /// <summary>
        /// Converts the value into a readable string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Type == PacketSegmentType.BUFFER)
            {
                return BitConverter.ToString((byte[])Value);
            }

            return Value.ToString();
        }

    }

    public enum PacketSegmentType
    {
        BYTE,
        SHORT,
        INT,
        BUFFER,
        STRING
    }
}