using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BK.Config.Export
{
    public interface IData
    {
        IColumnType ColumnType { get; }
        object Object { get; }
    }

    public interface IArrayData : IData
    {
        List<IData> Data { get; }
        int Count { get; }
        void Add(IData data);
    }

    public interface IPooledData : IData
    {
        IPooledColumnType PooledColumnType { get; }
    }

    public static class DataExtension
    {
        public static void WriteToBinary(this IData self, IChunkBinary binary)
        {
            self.ColumnType.WriteToBinary(binary, self);
        }
        public static void WriteToPool(this IPooledData self, IChunkBinary binary)
        {
            self.PooledColumnType.WriteToPool(binary, self);
        }
    }

    public class Data : IData
    {
        public IColumnType ColumnType { get; }
        public object Object { get; private set; }
        public Data(IColumnType columnType, object obj)
        {
            ColumnType = columnType;
            Object = obj;
        }
        public override int GetHashCode()
        {
            return Object.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return ((Data)obj).Object.Equals(Object);
        }
    }

    public class PooledData : Data, IPooledData
    {
        public IPooledColumnType PooledColumnType { get; }
        public PooledData(IPooledColumnType columnType, object obj)
            : base(columnType, obj)
        {
            PooledColumnType = columnType;
        }

    }

    public class ArrayData : IArrayData
    {
        public IColumnType ColumnType { get; }
        public List<IData> Data { get; } = new List<IData>();
        public int Count => Data.Count;
        public object Object => Data;
        public ArrayData(IColumnType columnType)
        {
            ColumnType = columnType;
        }
        public void Add(IData data)
        {
            Data.Add(data);
        }
        public override int GetHashCode()
        {
            int hashcode = 0;
            foreach(var obj in Data)
            {
                hashcode ^= obj.GetHashCode();
            }
            return hashcode;
        }
        public override bool Equals(object obj)
        {
            ArrayData r = (ArrayData)obj;
            if (r.Data.Count != Data.Count)
                return false;
            for(int i = 0; i < Data.Count; i++)
            {
                if (!Data[i].Equals(r.Data[i]))
                    return false;
            }
            return true;
        }
    }

    public class PooledArrayData : ArrayData, IPooledData
    {
        public IPooledColumnType PooledColumnType { get; }
        public PooledArrayData(IPooledColumnType columnType)
            : base(columnType)
        {
            PooledColumnType = columnType;
        }
    }
}
