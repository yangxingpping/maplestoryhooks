using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace MapleStoryHooks
{
    public class MaplePacket
    {
        public int Id;
        public string Direction;
        public List<PacketSegment> Segments = new List<PacketSegment>();
        private byte[] buffer;

        public MaplePacket(int pId, string pDirection)
        {
            this.Id = pId;
            this.Direction = pDirection;
        }

        public byte[] ToArray()
        {
            if (buffer != null)
            {
                return buffer;
            }

            MemoryStream ms = new MemoryStream();

            BinaryWriter writer = new BinaryWriter(ms);

            foreach (PacketSegment segment in Segments)
            {
                switch (segment.Type)
                {
                    case PacketSegmentType.BYTE:
                        writer.Write((byte)segment.Value);
                        break;
                    case PacketSegmentType.SHORT:
                        writer.Write((short)segment.Value);
                        break;
                    case PacketSegmentType.INT:
                        writer.Write((int)segment.Value);
                        break;
                    case PacketSegmentType.BUFFER:
                        writer.Write((byte[])segment.Value);
                        break;
                    case PacketSegmentType.STRING:
                        string value = (string)segment.Value;
                        writer.Write((short)value.Length);
                        writer.Write(Encoding.ASCII.GetBytes(value));
                        break;
                }
            }

            buffer = ms.ToArray();
            writer.Close();

            return buffer;
        }
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
