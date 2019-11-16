using System;
using System.IO;
using SevenZip;
using SevenZip.Compression.LZMA;


namespace AssetStudio
{
    public static class SevenZipHelper
    {
        private const byte PropertiesLen = 5;

        #region CompressData
        public static byte[] CompressData(byte[] inBytes, bool writeCompressedSize = false, ICodeProgress progressCallback = null)
        {
            var inStream = new MemoryStream(inBytes);
            var outStream = new MemoryStream();
            CompressData(inStream, outStream, inBytes.LongLength, writeCompressedSize, progressCallback);
            return outStream.ToArray();
        }

        public static long CompressData(byte[] inBytes, Stream outStream, bool writeCompressedSize = false, ICodeProgress progressCallback = null)
        {
            var inStream = new MemoryStream(inBytes);
            var compressedSize = CompressData(inStream, outStream, inBytes.Length, writeCompressedSize, progressCallback);
            return compressedSize;
        }

        public static byte[] CompressData(Stream inStream, long dataSize, bool writeCompressedSize = false, ICodeProgress progressCallback = null)
        {
            var outStream = new MemoryStream();
            var compressedSize = CompressData(inStream, outStream, dataSize, writeCompressedSize, progressCallback);
            return outStream.ToArray();
        }

        public static long CompressData(Stream inStream, Stream outStream, long dataSize, bool writeCompressedSize = false, ICodeProgress progressCallback = null)
        {
            var origPos = outStream.Position;
            var propIDs = new CoderPropID[]
            {
                CoderPropID.DictionarySize,
                CoderPropID.PosStateBits,
                CoderPropID.LitContextBits,
                CoderPropID.LitPosBits,
                CoderPropID.EndMarker
            };
            var propValues = new System.Object[]
            {
                (int)2048,
                (int)2,
                (int)3,
                (int)0,
                true
            };
            var encoder = new Encoder();
            encoder.SetCoderProperties(propIDs, propValues);
            encoder.WriteCoderProperties(outStream);
            //if (writeCompressedSize)
            //{
            //    
            //}
            encoder.Code(inStream, outStream, dataSize, -1, progressCallback);
            return outStream.Position - origPos;
        }
        #endregion


        #region DecompressData
        public static byte[] DecompressData(byte[] inBytes, long decompressedSize = -1, byte[] properties = null, ICodeProgress progressCallback = null)
        {
            var inStream = new MemoryStream(inBytes);
            var outStream = new MemoryStream();
            DecompressData(inStream, outStream, inBytes.Length, decompressedSize, properties, progressCallback);
            return outStream.ToArray();
        }

        public static void DecompressData(byte[] inBytes, Stream outStream, long decompressedSize = -1, byte[] properties = null, ICodeProgress progressCallback = null)
        {
            var inStream = new MemoryStream(inBytes);
            DecompressData(inStream, outStream, inBytes.Length, decompressedSize, properties, progressCallback);
        }

        public static byte[] DecompressData(Stream inStream, long compressedSize, long decompressedSize = -1, byte[] properties = null, ICodeProgress progressCallback = null)
        {
            var outStream = new MemoryStream();
            DecompressData(inStream, outStream, compressedSize, decompressedSize, properties, progressCallback);
            return outStream.ToArray();
        }

        public static void DecompressData(Stream inStream, Stream outStream, long compressedSize, long decompressedSize = -1, byte[] properties = null, ICodeProgress progressCallback = null)
        {
            var decoder = new Decoder();
            if (properties == null)
            {
                properties = new byte[5];
                inStream.Read(properties, 0, 5);
                compressedSize -= 5;
            }
            decoder.SetDecoderProperties(properties);
            decoder.Code(inStream, outStream, compressedSize, decompressedSize, progressCallback);
        }
        #endregion

        //public static byte[] DecompressData(Stream inStream, long compressedSize, long decompressedSize = -1)
        //{
        //    var inBinaryReader = new EndianBinaryReader(inStream, EndianType.BigEndian);
        //    var properties = new byte[5];
        //    if (inBinaryReader.Read(properties, 0, 5) != 5)
        //        throw new Exception("input .lzma is too short");
        //
        //    inBinaryReader.BaseStream.Position -= 4;
        //    var dictSize = inBinaryReader.ReadInt32();
        //
        //    //var prop = properties[0];
        //    //var pb = (byte)(prop / (9 * 5));
        //    //prop -= (byte)(pb * 9 * 5);
        //    //var lp = prop / 9;
        //    //var lc = prop - lp * 9;
        //
        //    if (decompressedSize == -1)
        //        decompressedSize = inBinaryReader.ReadInt64();
        //    compressedSize -= inStream.Position; //warning start pos maybe not 0
        //    var result = DecompressData(inStream, compressedSize, decompressedSize, properties, null);
        //    return result;
        //}

        //public static byte[] Decompress(Stream inStream)
        //{
        //    return DecompressData(inStream, inStream.Length);
        //}

        //maybe need use ulong
        private static long ReadBigEndianLong(Stream stream)
        {
            long result = 0;
            for (var i = 0; i < 8; i++)
            {
                var v = stream.ReadByte();
                //if (v < 0)
                //    throw new Exception("Can't Read 1");
                result |= ((long)(byte)v) << (8 * i);
            }

            return result;
        }

        private static long ReadBigEndianInt(Stream stream)
        {
            int result = 0;
            for (var i = 0; i < 4; i++)
            {
                var v = stream.ReadByte();
                //if (v < 0)
                //    throw new Exception("Can't Read 1");
                result |= ((int)(byte)v) << (8 * i);
            }

            return result;
        }


        public static MemoryStream StreamDecompress(MemoryStream inStream)
        {
            var decoder = new Decoder();

            inStream.Seek(0, SeekOrigin.Begin);
            var newOutStream = new MemoryStream();

            var properties = new byte[5];
            if (inStream.Read(properties, 0, 5) != 5)
                throw new Exception("input .lzma is too short");
            long outSize = 0;
            for (var i = 0; i < 8; i++)
            {
                var v = inStream.ReadByte();
                if (v < 0)
                    throw new Exception("Can't Read 1");
                outSize |= ((long)(byte)v) << (8 * i);
            }
            decoder.SetDecoderProperties(properties);

            var compressedSize = inStream.Length - inStream.Position;
            decoder.Code(inStream, newOutStream, compressedSize, outSize, null);

            newOutStream.Position = 0;
            return newOutStream;
        }

        public static void StreamDecompress(Stream inStream, Stream outStream, long inSize, long outSize)
        {
            var decoder = new Decoder();
            var properties = new byte[5];
            if (inStream.Read(properties, 0, 5) != 5)
                throw new Exception("input .lzma is too short");
            decoder.SetDecoderProperties(properties);
            inSize -= 5L;
            decoder.Code(inStream, outStream, inSize, outSize, null);
        }
    }
}
