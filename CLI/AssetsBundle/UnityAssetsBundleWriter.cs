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


        public static (byte[] info, int infoLen, byte[] blocks) BuildFormat6(UnityAssetsBundlePayload bundlePayload)
        {

            if (bundlePayload.Files.Length != bundlePayload.Blocks.Length)
                throw new Exception($"Expected equal count of blocks(get {bundlePayload.Blocks.Length}) and files(get {bundlePayload.Files.Length}).");
            if (bundlePayload.ReadAtEnd)
                throw new NotImplementedException("Not support bundles with ReadAtEnd flag");

            //build compressed data (Payload.Blocks)
            var blocksCompressedStream = new MemoryStream();
            var blocksWriter = new EndianBinaryWriter(blocksCompressedStream);
            blocksWriter.Write(bundlePayload.Files.Length);
            var blockStart = (long)0;
            var blocksInfos = new UnityAssetsBundleReader.BlockInfo[bundlePayload.Files.Length];
            for (int i = 0; i < bundlePayload.Files.Length; i++)
            {
                using var blockStream = new MemoryStream();
                using var blockWriter = new EndianBinaryWriter(blockStream);

                var file = bundlePayload.Files[i];
                var nameBytes = Encoding.UTF8.GetBytes(file.Name);
                //offset = pos + offset(8) + size(8) + flags(4) + name(X)
                var offset = (long)(blockStart + 20 + nameBytes.Length + 1);
                blockWriter.Write(offset);
                blockWriter.Write(file.DataBytes.LongLength);   //decompressed size
                blockWriter.Write(file.Flags);      //flags
                blockWriter.Write(file.Name);       //name
                blockWriter.Write(file.DataBytes);  //data

                blockStart += offset + file.DataBytes.Length;

                var compressedBlock = Compress(blockStream.ToArray(), bundlePayload.Blocks[i].CompressionType);

                //var dec = DecompressToMemoryStream(compressedBlock, ECompressionType.LzMa, (int) blockStream.Length);
                blocksCompressedStream.Write(compressedBlock);

                blocksInfos[i] = new UnityAssetsBundleReader.BlockInfo()
                {
                    DecompressedSize = (uint)file.DataBytes.LongLength,
                    CompressedSize = (uint)compressedBlock.LongLength,
                    Flags = bundlePayload.Blocks[i].Flags
                };
            }

            var blocksInfosStream = new MemoryStream();
            var blocksInfosWriter = new EndianBinaryWriter(blocksInfosStream);
            blocksInfosWriter.Write(bundlePayload.UnknownBytes);
            blocksInfosWriter.Write(bundlePayload.Blocks.Length);
            foreach (var blockInfo in blocksInfos)
            {
                blocksInfosWriter.Write(blockInfo.DecompressedSize);
                blocksInfosWriter.Write(blockInfo.CompressedSize);
                blocksInfosWriter.Write(blockInfo.Flags);
            }


            var blocksInfosBytes = blocksInfosStream.ToArray();

            var compressedBlocksInfosBytes = Compress(blocksInfosBytes, bundlePayload.PayloadCompressionType);


            return (compressedBlocksInfosBytes, blocksInfosBytes.Length, blocksCompressedStream.ToArray());
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
