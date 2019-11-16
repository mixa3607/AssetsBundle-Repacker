using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AssetStudio;
using CLI.AssetsBundle;

// ReSharper disable MemberCanBePrivate.Global

namespace CLI
{
    class Program
    {
        static void Main(string[] args)
        {

            var bundleOrigBytes = File.ReadAllBytes("scripts32.dec.bundle"); 
            var bundleOrig = UnityAssetsBundleReader.ReadBundle(bundleOrigBytes);

            var bundleRepackBytes = UnityAssetsBundleWriter.WriteBundle(bundleOrig);
            var bundleRepack = UnityAssetsBundleReader.ReadBundle(bundleRepackBytes);

            //comparison test
            //for (int i = 0; i < bundleRepack.Payload.Files[0].DataBytes.Length; i++)
            //{
            //    if (bundleRepack.Payload.Files[0].DataBytes[i] != bundleOrig.Payload.Files[0].DataBytes[i])
            //    {
            //        throw new Exception();
            //    }
            //}
        }
    }

}
