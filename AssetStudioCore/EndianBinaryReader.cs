using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace AssetStudio
{
    public enum EndianType
    {
        LittleEndian,
        BigEndian
    }

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

        public void Write(string str, bool nullTerminated = true, string encoding = "UTF8")
        {
            var buffer = Encoding.GetEncoding(encoding).GetBytes(str);
            if (nullTerminated)
            {
                Array.Resize(ref buffer, buffer.Length + 1);
            }
            base.Write(buffer);
        }
    }

    public class EndianBinaryReader : BinaryReader
    {
        public EndianType endian;

        public EndianBinaryReader(Stream stream, EndianType endian = EndianType.BigEndian) : base(stream)
        {
            this.endian = endian;
        }

        public long Position
        {
            get => BaseStream.Position;
            set => BaseStream.Position = value;
        }

        public override short ReadInt16()
        {
            if (endian == EndianType.BigEndian)
            {
                var buff = ReadBytes(2);
                Array.Reverse(buff);
                return BitConverter.ToInt16(buff, 0);
            }
            return base.ReadInt16();
        }

        public override int ReadInt32()
        {
            if (endian == EndianType.BigEndian)
            {
                var buff = ReadBytes(4);
                Array.Reverse(buff);
                return BitConverter.ToInt32(buff, 0);
            }
            return base.ReadInt32();
        }

        public override long ReadInt64()
        {
            if (endian == EndianType.BigEndian)
            {
                var buff = ReadBytes(8);
                Array.Reverse(buff);
                return BitConverter.ToInt64(buff, 0);
            }
            return base.ReadInt64();
        }

        public override ushort ReadUInt16()
        {
            if (endian == EndianType.BigEndian)
            {
                var buff = ReadBytes(2);
                Array.Reverse(buff);
                return BitConverter.ToUInt16(buff, 0);
            }
            return base.ReadUInt16();
        }

        public override uint ReadUInt32()
        {
            if (endian == EndianType.BigEndian)
            {
                var buff = ReadBytes(4);
                Array.Reverse(buff);
                return BitConverter.ToUInt32(buff, 0);
            }
            return base.ReadUInt32();
        }

        public override ulong ReadUInt64()
        {
            if (endian == EndianType.BigEndian)
            {
                var buff = ReadBytes(8);
                Array.Reverse(buff);
                return BitConverter.ToUInt64(buff, 0);
            }
            return base.ReadUInt64();
        }

        public override float ReadSingle()
        {
            if (endian == EndianType.BigEndian)
            {
                var buff = ReadBytes(4);
                Array.Reverse(buff);
                return BitConverter.ToSingle(buff, 0);
            }
            return base.ReadSingle();
        }


        public override double ReadDouble()
        {
            if (endian == EndianType.BigEndian)
            {
                var buff = ReadBytes(8);
                Array.Reverse(buff);
                return BitConverter.ToUInt64(buff, 0);
            }
            return base.ReadDouble();
        }
    }
}
