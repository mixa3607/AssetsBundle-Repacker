using System.IO;

namespace CLI
{

    public class UnityAssetsBundle
    {
        public UnityAssetsBundleHeader Header = new UnityAssetsBundleHeader();
        public UnityAssetsBundlePayload Payload = new UnityAssetsBundlePayload();
    }

    public class UnityAssetsBundlePayload
    {
        //public UnityAssetsBundlePayloadHeader Header { get; set; } = new UnityAssetsBundlePayloadHeader();
        public int Flags { get; set; }
        public bool ReadAtEnd => (Flags & 0b0000_0000_0000_0000_0000_0000_1000_0000) != 0;
        public ECompressionType PayloadCompressionType => (ECompressionType)(Flags & 0b0000_0000_0000_0000_0000_0000_0011_1111);


        public byte[] UnknownBytes { get; set; }

        public UnityAssetsBundlePayloadBlock[] Blocks { get; set; }
        public UnityAssetsBundleStreamFile[] Files { get; set; }
    }

    public class UnityAssetsBundlePayloadHeader
    {
        //PAYLOAD
        //public long TmpBundleSize; // { get; private set; }
        //public int TmpCompressedSize; // { get; private set; }
        //public int TmpDecompressedSize; // { get; private set; }

        
    }

    public class UnityAssetsBundlePayloadBlock
    {
        //public uint TmpCompressedSize; // { get; private set; }
        //public uint TmpDecompressedSize; // { get; private set; }
        public short Flags { get; set; }
        public ECompressionType CompressionType => (ECompressionType)(Flags & 0b0000_0000_0000_0000_0000_0000_0011_1111);

        //public byte[] DataBytes { get; set; }
        //public Stream DataStream { get; set; }
    }

    public class UnityAssetsBundleStreamFile
    {
        //public long TmpOffset;
        //public long TmpSize;
        public int Flags { get; set; }
        public string Name { get; set; }
        public byte[] DataBytes { get; set; }
    }
}