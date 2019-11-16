using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssetStudio;
using K4os.Compression.LZ4;

namespace CLI.AssetsBundle
{
    public class UnityAssetsBundleWriter
    {
        public static byte[] WriteBundle(UnityAssetsBundle assetsBundle)
        {
            var bundleStream = new MemoryStream();
            var bundleWriter = new EndianBinaryWriter(bundleStream);


            if (assetsBundle.Header.Type == EBundleType.UnityFs)
            {
                bundleWriter.Write(assetsBundle.Header.RawStrType, true);
                bundleWriter.Write(assetsBundle.Header.Format);
                bundleWriter.Write(assetsBundle.Header.EngineVer, true);
                bundleWriter.Write(assetsBundle.Header.PlayerVer, true);
                if (assetsBundle.Header.Format == 6)
                {
                    var (info, infoLen, blocks) = BuildFormat6(assetsBundle.Payload);
                    var bundleSize = (long)(bundleWriter.Position + 20 + info.Length + blocks.Length);
                    bundleWriter.Write(bundleSize);
                    bundleWriter.Write(info.Length);
                    bundleWriter.Write(infoLen);
                    bundleWriter.Write(assetsBundle.Payload.Flags);
                    bundleWriter.Write(info);
                    bundleWriter.Write(blocks);
                }
                else
                    throw new NotSupportedException($"Allow only 6 format, but read {assetsBundle.Header.Format}");
            }
            else
            {
                throw new NotSupportedException($"Support only UnityFS, but read {assetsBundle.Header.Type:G}");
            }

            return bundleStream.ToArray();
        }


        public static (byte[] info, int infoLen, byte[] data) BuildFormat6(UnityAssetsBundlePayload bundlePayload)
        {

            if (bundlePayload.Files.Length != bundlePayload.Blocks.Length)
                throw new Exception($"Expected equal count of blocks(get {bundlePayload.Blocks.Length}) and files(get {bundlePayload.Files.Length}).");
            if (bundlePayload.ReadAtEnd)
                throw new NotImplementedException("Not support bundles with ReadAtEnd flag");

            //build compressed data (Payload.Blocks)
            int infoLen;
            byte[] compressedInfoBytes;
            //byte[] entriesInfosBytes;
            byte[] compressedDataBytes;
            //var blocksInfos = new UnityAssetsBundleReader.BlockInfo[bundlePayload.Files.Length];

            using (var compressedDataStream = new MemoryStream())
            {
                var blocksInfosStream = new MemoryStream();
                var blocksInfosWriter = new EndianBinaryWriter(blocksInfosStream);

                var entriesStream = new MemoryStream();
                var entriesWriter = new EndianBinaryWriter(entriesStream);

                blocksInfosWriter.Write(bundlePayload.UnknownBytes);
                blocksInfosWriter.Write(bundlePayload.Blocks.Length);
                entriesWriter.Write(bundlePayload.Files.Length);

                var entryOffset = (long) 0;
                for (int i = 0; i < bundlePayload.Files.Length; i++)
                {
                    var file = bundlePayload.Files[i];

                    entriesWriter.Write(entryOffset);
                    entriesWriter.Write((long)file.DataBytes.Length);
                    entriesWriter.Write(file.Flags);
                    entriesWriter.Write(file.Name);
                    entryOffset += file.DataBytes.Length;

                    var compressedEntry = Compress(file.DataBytes, bundlePayload.Blocks[i].CompressionType);
                    compressedDataStream.Write(compressedEntry);

                    blocksInfosWriter.Write((uint)file.DataBytes.LongLength);
                    blocksInfosWriter.Write((uint)compressedEntry.LongLength);
                    blocksInfosWriter.Write(bundlePayload.Blocks[i].Flags);
                    
                    
                }

                blocksInfosStream.Write(entriesStream.ToArray());
                infoLen = (int) blocksInfosStream.Length;
                compressedInfoBytes = Compress(blocksInfosStream.ToArray(), bundlePayload.PayloadCompressionType);
                //entriesInfosBytes = entriesStream.ToArray();
                compressedDataBytes = compressedDataStream.ToArray();
            }

            return (compressedInfoBytes, infoLen, compressedDataBytes);
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
    }
}
