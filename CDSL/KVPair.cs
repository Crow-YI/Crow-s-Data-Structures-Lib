namespace CDSL
{
    public struct KVPair<KType, Vtype>
    {
        public KType key;
        public Vtype value;
        public KVPair(KType key, Vtype value)
        {
            this.key = key;
            this.value = value;
        }
    }
}
