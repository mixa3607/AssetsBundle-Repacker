using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AssetStudio;

// ReSharper disable MemberCanBePrivate.Global

namespace CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            var data = new byte[] {100, 100, 20, 50, 44};
            var compressed = UnityAssetsBundleReader.Compress(data, ECompressionType.Lz4);

            var decompStream = UnityAssetsBundleReader.DecompressToMemoryStream(compressed, ECompressionType.Lz4, data.Length);
            var decData = decompStream.ToArray();

            var scriptsBytes = File.ReadAllBytes("scripts32.dec.bundle"); //Decryptor.DecryptScriptsBundle("scripts32");
            //var bundleFile = new BundleFile(new EndianBinaryReader(new MemoryStream(scriptsBytes)), "tmp");
            var bundle = UnityAssetsBundleReader.ReadBundle(scriptsBytes);
            //var assetsFile = new AssetsFile(inputPath, new EndianBinaryReader(bundleFile.fileList[0].stream));
        }
    }

}
