namespace CLI
{
    public class UnityAssetsBundleHeader
    {
        //HEADER
        public EBundleType Type; // { get; private set; }
        public string RawStrType; // { get; private set; }

        public int Format; // { get; private set; }

        public string EngineVer; // { get; private set; }
        public string PlayerVer; // { get; private set; }
    }
}