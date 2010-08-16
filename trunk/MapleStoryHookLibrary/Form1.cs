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
            //listBox2.DataSource = packetDict.Keys;
        }

        #region Hooked Methods
        public int OutPacketInitHooked(IntPtr @this, int nType, int bLoopback)
        {

            return Main.OutPacketInitOriginal(@this, nType, bLoopback);
        }

        public void EncodeByteHooked(IntPtr @this, byte n)
        {
            PacketSegment segment = new PacketSegment(@this.ToInt32(), PacketSegmentType.BYTE, n, "SEND");
            addToTable(segment);

            Main.EncodeByteOriginal(@this, n);
        }

        public void EncodeShortHooked(IntPtr @this, UInt16 n)
        {
            PacketSegment segment = new PacketSegment(@this.ToInt32(), PacketSegmentType.SHORT, n, "SEND");
            addToTable(segment);

            Main.EncodeShortOriginal(@this, n);
        }

        public void EncodeIntHooked(IntPtr @this, UInt32 n)
        {
            PacketSegment segment = new PacketSegment(@this.ToInt32(), PacketSegmentType.INT, n, "SEND");
            addToTable(segment);

            Main.EncodeIntOriginal(@this, n);
        }

        public void EncodeBufferHooked(IntPtr @this, IntPtr bufferPointer, UInt32 uSize)
        {
            byte[] data = new byte[uSize];
            Marshal.Copy(bufferPointer, data, 0, (int)uSize);

            PacketSegment segment = new PacketSegment(@this.ToInt32(), PacketSegmentType.BUFFER, data, "SEND");
            addToTable(segment);

            Main.EncodeBufferOriginal(@this, bufferPointer, uSize);
        }

        public void EncodeStringHooked(IntPtr @this, IntPtr stringPointer)
        {

            string s = Marshal.PtrToStringAnsi(stringPointer);

            PacketSegment segment = new PacketSegment(@this.ToInt32(), PacketSegmentType.STRING, s, "SEND");
            addToTable(segment);

            Main.EncodeStringOriginal(@this, stringPointer);
        }


        public int SendPacketHooked(IntPtr @this, IntPtr packetPointer)
        {
            COutPacket packet = (COutPacket)Marshal.PtrToStructure(@this, typeof(COutPacket));
            byte[] data = packet.ToArray();

            PacketSegment segment = new PacketSegment(@this.ToInt32(), PacketSegmentType.BUFFER, data, "SEND WHOLE");
            addToTable(segment);
   
            return Main.SendPacketOriginal(@this, packetPointer);
        }


        public byte DecodeByteHooked(IntPtr @this)
        {
            CInPacket packet = (CInPacket)Marshal.PtrToStructure(@this, typeof(CInPacket));
            byte result = packet.ToReader().ReadByte();

            PacketSegment segment = new PacketSegment(@this.ToInt32(), PacketSegmentType.BYTE, result, "RECV");
            addToTable(segment);

            return Main.DecodeByteOriginal(@this);
        }

        public UInt16 DecodeShortHooked(IntPtr @this)
        {
            CInPacket packet = (CInPacket)Marshal.PtrToStructure(@this, typeof(CInPacket));
            short result = packet.ToReader().ReadInt16();

            PacketSegment segment = new PacketSegment(@this.ToInt32(), PacketSegmentType.SHORT, result, "RECV");
            addToTable(segment);

            return Main.DecodeShortOriginal(@this);
        }

        public UInt32 DecodeIntHooked(IntPtr @this)
        {
            CInPacket packet = (CInPacket)Marshal.PtrToStructure(@this, typeof(CInPacket));
            int result = packet.ToReader().ReadInt32();

            PacketSegment segment = new PacketSegment(@this.ToInt32(), PacketSegmentType.INT, result, "RECV");
            addToTable(segment);

            return Main.DecodeIntOriginal(@this);
        }

        public void DecodeBufferHooked(IntPtr @this, IntPtr bufferPointer, UInt32 uSize)
        {
            CInPacket packet = (CInPacket)Marshal.PtrToStructure(@this, typeof(CInPacket));
            byte[] result = packet.ToReader().ReadBytes((int)uSize);

            PacketSegment segment = new PacketSegment(@this.ToInt32(), PacketSegmentType.BUFFER, result, "RECV");
            addToTable(segment);

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
            addToTable(segment);

            return Main.DecodeStringOriginal(@this, resultPointer);
        }
        #endregion


        public void addToTreeViewReal(PacketSegment segment)
        {
            if (!treeView1.Nodes.ContainsKey(segment.Id.ToString()))
            {
                treeView1.Nodes.Add(segment.Id.ToString(), segment.Direction + "(" + segment.Id + ")");

            }
            TreeNode node = treeView1.Nodes[segment.Id.ToString()];
            node.Nodes.Add(segment.Type + ": " + segment.ToString());
        }

        public delegate void addToTreeViewDelegate(PacketSegment segment);

        public void addToTable(PacketSegment segment)
        {
            try
            {
                this.Invoke(new addToTreeViewDelegate(addToTreeViewReal), segment);

            }
            catch (Exception e)
            {
                Main.Interface.WriteConsole(e.Message);
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            treeView1.Nodes.Clear();
        }
    }

    public class MaplePacket
    {
        public int Id;
        public List<PacketSegment> Segments = new List<PacketSegment>();
    }

    public class PacketSegment
    {
        public int Id;
        public PacketSegmentType Type;
        public object Value;
        public string Direction;

        public PacketSegment(int pId, PacketSegmentType pType, object pValue, string pDirection)
        {
            this.Id = pId;
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