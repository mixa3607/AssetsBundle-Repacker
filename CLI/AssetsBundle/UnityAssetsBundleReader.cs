using System;
using System.IO;
using AssetStudio;
using K4os.Compression.LZ4;
using Lz4;

namespace CLI
{
    public static class UnityAssetsBundleReader
    {

        public static byte[] WriteBundle(UnityAssetsBundle assetsBundle)
        {
            var bundleStream = new MemoryStream();
            var bundleWriter = new EndianBinaryWriter(bundleStream);

            bundleWriter.Write(assetsBundle.Header.RawStrType, true);
            if (assetsBundle.Header.Type == EBundleType.UnityFs)
            {
                bundleWriter.Write(assetsBundle.Header.Format);
                bundleWriter.Write(assetsBundle.Header.EngineVer, true);
                bundleWriter.Write(assetsBundle.Header.PlayerVer, true);
                if (assetsBundle.Header.Format == 6)
                    bundleWriter.Write(BuildFormat6(assetsBundle.Payload));
                else
                    throw new NotSupportedException($"Allow only 6 format, but read {assetsBundle.Header.Format}");
            }
            else
            {
                throw new NotSupportedException($"Support only UnityFS, but read {assetsBundle.Header.Type:G}");
            }

            return bundleStream.ToArray();
        }


        public static byte[] BuildFormat6(UnityAssetsBundlePayload bundlePayload)
        {
            return null;
        }

        public static UnityAssetsBundle ReadBundle(byte[] bundleFileBytes)
        {
            var bundle = new UnityAssetsBundle();
            var reader = new EndianBinaryReader(new MemoryStream(bundleFileBytes));

            //read bundle type
            bundle.Header.RawStrType = reader.ReadStringToNull();
            switch (bundle.Header.RawStrType)
            {
                case "UnityFS": bundle.Header.Type = EBundleType.UnityFs; break;
                case "UnityRaw": bundle.Header.Type = EBundleType.UnityRaw; break;
                case "\xFA\xFA\xFA\xFA\xFA\xFA\xFA\xFA": //.bytes
                case "UnityWeb": bundle.Header.Type = EBundleType.UnityWeb; break;
                default: throw new NotSupportedException($"Unknown bundle type {bundle.Header.RawStrType}!");
            }

            if (bundle.Header.Type == EBundleType.UnityFs)
            {
                bundle.Header.Format = reader.ReadInt32();
                bundle.Header.EngineVer = reader.ReadStringToNull();
                bundle.Header.PlayerVer = reader.ReadStringToNull();
                if (bundle.Header.Format == 6)
                    bundle.Payload = ReadFormat6(reader);
                else
                    throw new NotSupportedException($"Allow only 6 format, but read {bundle.Header.Format}");
            }
            else
            {
                throw new NotSupportedException($"Support only UnityFS, but read {bundle.Header.Type:G}");
            }

            return bundle;
        }

        public static byte[] Compress(byte[] data, ECompressionType compressType) //, int start = 0, int count = 0)
        {
            byte[] compressedData;
            switch (compressType)
            {
                case ECompressionType.LzMa: 
                    compressedData = SevenZipHelper.CompressData(data); 
                    break;
                case ECompressionType.Lz4:   //LZ4
                case ECompressionType.Lz4Hc: //LZ4HC
                    compressedData = new byte[LZ4Codec.MaximumOutputSize(data.Length)];
                    var compressedSize = LZ4Codec.Encode(data, 0, data.Length, 
                        compressedData, 0, compressedData.Length);
                    Array.Resize(ref compressedData, compressedSize);
                    break;
                //case CompressionType.LzHam:   //LZHAM
                case ECompressionType.None:      //None
                default:
                {
                    compressedData = data;
                    break;
                }
            }

            return compressedData;
        }

        public static MemoryStream DecompressToMemoryStream(byte[] compressedBytes, ECompressionType compressType, int decompressedSize = -1)
        {
            MemoryStream decompressedStream;
            switch (compressType)
            {
                case ECompressionType.LzMa:  //LZMA
                {
                    decompressedStream = new MemoryStream();
                    SevenZipHelper.DecompressData(compressedBytes, decompressedStream, decompressedSize);
                    
                    //if (decompressedSize == -1)
                    //{
                    //    decompressedStream = SevenZipHelper.StreamDecompress(new MemoryStream(compressedBytes));
                    //}
                    //else
                    //{
                    //    decompressedStream = new MemoryStream();
                    //    SevenZipHelper.StreamDecompress(new MemoryStream(compressedBytes), decompressedStream,
                    //        compressedBytes.Length, decompressedSize);
                    //}
                    break;
                }
                case ECompressionType.Lz4:   //LZ4
                case ECompressionType.Lz4Hc: //LZ4HC
                {
                    var decompressedBytes = new byte[decompressedSize];
                    LZ4Codec.Decode(compressedBytes, 0, compressedBytes.Length,
                        decompressedBytes, 0, decompressedSize);
                    //using (var decoder = new Lz4DecoderStream(new MemoryStream(compressedBytes)))
                    //{
                    //    decoder.Read(decompressedBytes, 0, decompressedSize);
                    //}
                    decompressedStream = new MemoryStream(decompressedBytes);
                    break;
                }
                //case CompressionType.LzHam:   //LZHAM
                case ECompressionType.None:      //None
                default:
                {
                    decompressedStream = new MemoryStream(compressedBytes);
                    break;
                }
            }
            return decompressedStream;
        }

        private class BlockInfo : UnityAssetsBundlePayloadBlock
        {
            public uint CompressedSize; // { get; private set; }
            public uint DecompressedSize; // { get; private set; }
        }

        private static UnityAssetsBundlePayload ReadFormat6(EndianBinaryReader bundleReader, bool padding = false)
        {
            var payload = new UnityAssetsBundlePayload();
            var payloadBundleSize = bundleReader.ReadInt64();
            var payloadCompressedSize = bundleReader.ReadInt32();
            var payloadDecompressedSize = bundleReader.ReadInt32();
            payload.Flags = bundleReader.ReadInt32();

            //
            if (padding)
                bundleReader.ReadByte();

            byte[] compressedBlocksInfoBytes;
            if (payload.ReadAtEnd)//at end of file
            {
                var position = bundleReader.Position;
                bundleReader.Position = bundleReader.BaseStream.Length - payloadCompressedSize;
                compressedBlocksInfoBytes = bundleReader.ReadBytes(payloadCompressedSize);
                bundleReader.Position = position;
            }
            else
            {
                compressedBlocksInfoBytes = bundleReader.ReadBytes(payloadCompressedSize);
            }

            var blocksInfoStream = DecompressToMemoryStream(compressedBlocksInfoBytes, payload.PayloadCompressionType, payloadDecompressedSize);

            using var blocksInfoReader = new EndianBinaryReader(blocksInfoStream);
            payload.UnknownBytes = blocksInfoReader.ReadBytes(16);
            var payloadBlocksCount = blocksInfoReader.ReadInt32();

            payload.Blocks = new UnityAssetsBundlePayloadBlock[payloadBlocksCount];
            var blockInfos = new BlockInfo[payloadBlocksCount];

            for (var i = 0; i < payloadBlocksCount; i++)
            {
                blockInfos[i] = new BlockInfo()
                {
                    DecompressedSize = blocksInfoReader.ReadUInt32(),
                    CompressedSize = blocksInfoReader.ReadUInt32(),
                    Flags = blocksInfoReader.ReadInt16()
                };
                payload.Blocks[i] = new UnityAssetsBundlePayloadBlock()
                {
                    Flags = blockInfos[i].Flags
                };
            }

            //create stream with decompressed blocks
            var dataStream = new MemoryStream();
            foreach (var blockInfo in blockInfos)
            {
                var compressedBytes = bundleReader.ReadBytes((int) blockInfo.CompressedSize);
                var decompressedMemStream = DecompressToMemoryStream(
                    compressedBytes, 
                    blockInfo.CompressionType,
                    (int)blockInfo.DecompressedSize);
                decompressedMemStream.CopyTo(dataStream, blockInfo.DecompressedSize);
            }
            dataStream.Position = 0; //reset

            using (dataStream)
            {
                var entryInfoCount = blocksInfoReader.ReadInt32();
                payload.Files = new UnityAssetsBundleStreamFile[entryInfoCount];
                for (var i = 0; i < payload.Files.Length; i++)
                {
                    var offset = blocksInfoReader.ReadInt64();
                    var size = blocksInfoReader.ReadInt64();
                    var file = new UnityAssetsBundleStreamFile
                    {
                        Flags = blocksInfoReader.ReadInt32(),
                        Name = blocksInfoReader.ReadStringToNull(),
                        DataBytes = new byte[size]
                    };

                    dataStream.Position = offset;
                    dataStream.Read(file.DataBytes, 0, (int) size);

                    payload.Files[i] = file;
                }
            }

            return payload;
        }
    }
}