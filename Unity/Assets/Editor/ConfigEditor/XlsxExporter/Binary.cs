namespace BK.Config.Export
{
    public interface IChunkBinary
    {
        void Write(int i);
        void Write(uint i);
        void Write(long l);
        void Write(ulong l);
        void Write(bool b);
        void Write(float f);
        void Write(string s);
        IConfigBinary GetConfigBinary();
    }

    public interface IConfigBinary
    {
        IChunkBinary NewChunk(string chunkName);
        Pool GetPool(string typeName);
    }
}
