using System;
using System.IO;
using System.Text;

namespace AssetStudio
{
    public class EndianBinaryWriter : BinaryWriter
    {
        public EndianType endian;

        public EndianBinaryWriter(Stream stream, EndianType endian = EndianType.BigEndian) : base(stream)
        {
            this.endian = endian;
        }

        public long Position
        {
            get => BaseStream.Position;
            set => BaseStream.Position = value;
        }

        public override void Write(int num)
        {
            var buffer = BitConverter.GetBytes(num);
            if (endian == EndianType.BigEndian)
            {
                Array.Reverse(buffer);
            }
            base.Write(buffer);
        }

        public override void Write(uint num)
        {
            var buffer = BitConverter.GetBytes(num);
            if (endian == EndianType.BigEndian)
            {
                Array.Reverse(buffer);
            }
            base.Write(buffer);
        }

        public override void Write(long num)
        {
            var buffer = BitConverter.GetBytes(num);
            if (endian == EndianType.BigEndian)
            {
                Array.Reverse(buffer);
            }
            base.Write(buffer);
        }

        public override void Write(ulong num)
        {
            var buffer = BitConverter.GetBytes(num);
            if (endian == EndianType.BigEndian)
            {
                Array.Reverse(buffer);
            }
            base.Write(buffer);
        }

        public override void Write(short num)
        {
            var buffer = BitConverter.GetBytes(num);
            if (endian == EndianType.BigEndian)
            {
                Array.Reverse(buffer);
            }
            base.Write(buffer);
        }

        public override void Write(ushort num)
        {
            var buffer = BitConverter.GetBytes(num);
            if (endian == EndianType.BigEndian)
            {
                Array.Reverse(buffer);
            }
            base.Write(buffer);
        }

        public override void Write(float num)
        {
            var buffer = BitConverter.GetBytes(num);
            if (endian == EndianType.BigEndian)
            {
                Array.Reverse(buffer);
            }
            base.Write(buffer);
        }

        public override void Write(double num)
        {
            var buffer = BitConverter.GetBytes(num);
            if (endian == EndianType.BigEndian)
            {
                Array.Reverse(buffer);
            }
            base.Write(buffer);
        }

        public int Write(string str, bool nullTerminated = true, string encoding = "UTF-8")
        {
            var buffer = Encoding.GetEncoding(encoding).GetBytes(str);
            if (nullTerminated)
            {
                Array.Resize(ref buffer, buffer.Length + 1);
            }
            base.Write(buffer);
            return buffer.Length;
        }
    }
}