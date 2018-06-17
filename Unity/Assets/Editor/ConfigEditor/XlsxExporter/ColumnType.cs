using Microsoft.CSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using TypeName;

namespace BK.Config.Export
{
    static partial class Define
    {
        public static readonly char[] ARRAY_SPLITTER = new char[3] { ',', ';', '|' };
    }

    public interface IColumnType
    {
        string TypeName { get; }
        int ArrayLevel { get; }
        string DefaultValue { get; }
        IData Parse(string excelString);
        void WriteToBinary(IChunkBinary binary, IData obj);
    }

    public interface IPooledColumnType : IColumnType
    {
        int GetPoolIndex(Pool pool, IPooledData obj);
        void WriteToPool(IChunkBinary binary, IPooledData obj);
    }

    public interface IRefIdColumnType : IColumnType
    {
        string ConfigName { get; }
    }

    public interface ICustomColumnType : IColumnType
    {
        List<KeyValuePair<IColumnType, string>> Fields { get; }
    }

    public interface IEnumColumnType : IColumnType
    {
        List<KeyValuePair<string, int>> Fields { get; }
    }

    abstract class ColumnType : IColumnType
    {
        public virtual string TypeName { get; protected set; }
        public int ArrayLevel { get; protected set; } = -1;
        public string DefaultValue { get; protected set; }
        public abstract IData Parse(string excelString);
        public abstract void WriteToBinary(IChunkBinary binary, IData obj);
    }

    public class ColumnTypeParser
    {
        public List<IColumnType> CustomTypes { get; } = new List<IColumnType>();

        public IColumnType ParseColumnType(string excelString)
        {
            IColumnType type = null;
            foreach(var builtinType in BuiltinColumnTypes.BuiltinTypes)
            {
                if(excelString.StartsWith(builtinType.TypeName, StringComparison.OrdinalIgnoreCase))
                {
                    type = builtinType;
                    excelString = excelString.Substring(builtinType.TypeName.Length + 1);
                    break;
                }
            }
            if (type == null)
            {
                //先处理各种类型
                if (excelString.StartsWith("id", StringComparison.OrdinalIgnoreCase))
                {
                    type = ParseIdRef(ref excelString);
                }
                else if(excelString.StartsWith("struct", StringComparison.OrdinalIgnoreCase) || 
                    excelString.StartsWith("class", StringComparison.OrdinalIgnoreCase))
                {
                    type = ParseCustomType(ref excelString);
                }
                else if(excelString.StartsWith("enum", StringComparison.OrdinalIgnoreCase))
                {
                    type = ParseEnumType(ref excelString);
                }
                else
                {
                    throw new Exception("解析失败，不支持的类型");
                }
                CustomTypes.Add(type);
            }
            while(excelString != "")
            {
                if (excelString.StartsWith("[]"))
                {
                    excelString = excelString.Substring(2).TrimStart();
                    type = new BuiltinColumnTypes.ArrayColumn(type);
                    CustomTypes.Add(type);
                }
                else
                {
                    throw new Exception($"含有多余的末尾{excelString}");
                }
            }
            return type;
        }

        private IColumnType ParseEnumType(ref string excelString)
        {
            excelString = excelString.Substring(4).TrimStart();
            if (excelString.StartsWith("{"))
            {
                if (excelString.IndexOf('{', 1) >= 0)
                {
                    throw new Exception("Enum暂时不支持嵌套");
                }
                int end = excelString.IndexOf('}', 1);
                if (end >= 0)
                {
                    string inner = excelString.Substring(1, end - 1).Trim();
                    excelString = excelString.Substring(end + 1).TrimStart();
                    if (inner == "")
                    {
                        throw new Exception("Enum内容不能为空");
                    }
                    string[] fieldDefs = inner.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    List<KeyValuePair<string, int>> fields = new List<KeyValuePair<string, int>>();
                    foreach (var fieldDef in fieldDefs)
                    {
                        string[] fieldInfo = fieldDef.Trim().Split('=');
                        if (fieldInfo.Length > 2)
                        {
                            throw new Exception("Enum Field格式不正确，应为 字段名[=int值]");
                        }
                        string fieldName = fieldInfo[0];
                        int value;
                        if (fieldInfo.Length > 1)
                            value = int.Parse(fieldInfo[1]);
                        else
                            value = fields.Count != 0 ? fields.Last().Value + 1 : 0;
                        if (fields.FindIndex((pair) => pair.Key == fieldName) >= 0)
                        {
                            throw new Exception($"解析Enum失败，存在重复的键{fieldName}");
                        }
                        fields.Add(new KeyValuePair<string, int>(fieldName, value));
                    }
                    EnumColumnType enumColumnType = new EnumColumnType(fields);
                    return enumColumnType;
                }
            }
            throw new Exception("Enum语法不正确，应为enum{字段名[=值], ...}");
        }

        private IColumnType ParseCustomType(ref string excelString)
        {
            if(excelString.StartsWith("struct", StringComparison.OrdinalIgnoreCase))
            {
                excelString = excelString.Substring(6);
            }
            else
            {
                excelString = excelString.Substring(5);
            }
            excelString = excelString.TrimStart();
            if(excelString.StartsWith("{"))
            {
                if(excelString.IndexOf('{', 1) >= 0)
                {
                    throw new Exception("Struct暂时不支持嵌套");
                }
                int end = excelString.IndexOf('}', 1);
                if(end >= 0)
                {
                    string inner = excelString.Substring(1, end - 1).Trim();
                    excelString = excelString.Substring(end + 1).TrimStart();
                    if (inner == "")
                    {
                        throw new Exception("Struct内容不能为空");
                    }
                    string[] fieldDefs = inner.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    List<KeyValuePair<IColumnType, string>> fields = new List<KeyValuePair<IColumnType, string>>();
                    foreach (var fieldDef in fieldDefs)
                    {
                        string[] fieldInfo = fieldDef.Trim().Split(' ');
                        if(fieldInfo.Length != 2)
                        {
                            throw new Exception("Struct Field格式不正确，应为 字段类型 字段名;");
                        }
                        IColumnType columnType;
                        try
                        {
                            columnType = ParseColumnType(fieldInfo[0]);
                        }
                        catch(Exception e)
                        {
                            throw new Exception($"解析Struct字段{fieldInfo[0]}失败，{e.Message}", e);
                        }
                        if(fields.FindIndex((pair)=> pair.Value == fieldInfo[1]) >= 0)
                        {
                            throw new Exception($"解析Struct失败，存在重复的键{fieldInfo[1]}");
                        }
                        fields.Add(new KeyValuePair<IColumnType, string>(columnType, fieldInfo[1]));
                    }
                    CustomColumnType customColumnType = new CustomColumnType(fields);
                    return customColumnType;
                }
            }
            throw new Exception("Struct语法不正确，应为struct/class{字段类型 字段名;...}");
        }

        private IColumnType ParseIdRef(ref string excelString)
        {
            if (excelString[2] == '(')
            {
                int end = excelString.IndexOf(')', 2);
                if (end > 0)
                {
                    string configName = excelString.Substring(3, end - 3);
                    excelString = excelString.Substring(end + 1);
                    return new RefIdColumnType(configName);
                }
            }
            throw new Exception("Id语法不正确，应为id(Config名)");
        }
    }

    class RefIdColumnType : ColumnType, IRefIdColumnType
    {
        public string ConfigName { get; }
        public RefIdColumnType(string configName)
        {
            ConfigName = configName;
        }

        public override IData Parse(string excelString)
        {
            return new Data(this, int.Parse(excelString));
        }

        public override void WriteToBinary(IChunkBinary binary, IData obj)
        {
            binary.Write((int)obj.Object);
        }
    }

    class CustomColumnType : ColumnType, ICustomColumnType, IPooledColumnType
    {
        public List<KeyValuePair<IColumnType, string>> Fields { get; } = new List<KeyValuePair<IColumnType, string>>();
        private int PoolIndex { get; set; } = -1;

        public CustomColumnType(List<KeyValuePair<IColumnType, string>> fields)
        {
            Fields = fields;
            ArrayLevel = Fields.Max((pair) => pair.Key.ArrayLevel) + 1;
            if (ArrayLevel > 3)
            {
                throw new Exception("Array Define过深，不能超过三层");
            }
        }

        public override IData Parse(string excelString)
        {
            var splitter = Define.ARRAY_SPLITTER[ArrayLevel];
            string[] result = excelString.Split(splitter);
            if(result.Length != Fields.Count)
            {
                throw new Exception("参数数量和类型不符");
            }
            PooledArrayData arrayData = new PooledArrayData(this);
            for(int i = 0; i < result.Length; i++)
            {
                var data = Fields[i].Key.Parse(result[i]);
                arrayData.Add(data);
            }
            return arrayData;
        }

        public int GetPoolIndex(Pool pool, IPooledData obj)
        {
            if (PoolIndex < 0)
                PoolIndex = pool.GetIndex(obj);
            IArrayData arrayData = obj as IArrayData;
            if (arrayData != null)
            {
                foreach (var data in arrayData.Data)
                {
                    var pooledData = data as IPooledData;
                    if (pooledData != null)
                    {
                        pooledData.PooledColumnType.GetPoolIndex(pool, pooledData);
                    }
                }
            }
            return PoolIndex;
        }

        public override void WriteToBinary(IChunkBinary binary, IData obj)
        {
            Pool pool = binary.GetConfigBinary().GetPool(TypeName);
            binary.Write(GetPoolIndex(pool, (IPooledData)obj));
        }

        public void WriteToPool(IChunkBinary binary, IPooledData obj)
        {
            PooledArrayData arrayData = (PooledArrayData)obj;
            binary.Write(arrayData.Count);
            foreach (var data in arrayData.Data)
            {
                data.WriteToBinary(binary);
            }
        }
    }

    class EnumColumnType : ColumnType, IEnumColumnType
    {
        public List<KeyValuePair<string, int>> Fields { get; } = new List<KeyValuePair<string, int>>();
        private readonly Dictionary<string, int> fieldsDic = new Dictionary<string, int>();

        public EnumColumnType(List<KeyValuePair<string, int>> fields)
        {
            Fields = fields;
            foreach(var p in fields)
            {
                fieldsDic.Add(p.Key, p.Value);
            }
        }

        public override IData Parse(string excelString)
        {
            int value;
            if(fieldsDic.TryGetValue(excelString, out value))
            {
                return new Data(this, value);
            }
            throw new Exception("Enum键不存在");
        }

        public override void WriteToBinary(IChunkBinary binary, IData obj)
        {
            binary.Write((int)obj.Object);
        }
    }


    static class BuiltinColumnTypes
    {
        public readonly static ColumnType[] BuiltinTypes = new ColumnType[]
        {
            new PrimitiveColumn<int>(),
            new PrimitiveColumn<uint>(),
            new PrimitiveColumn<long>(),
            new PrimitiveColumn<ulong>(),
            new PrimitiveColumn<bool>(),
            new PrimitiveColumn<float>(),
            new PrimitiveColumn<string>(),
            new DateTimeColumnType(),
            new Vector3ColumnType(),
            new Vector2ColumnType(),
            new ColorColumnType(),
        };

        public abstract class GenericArrayColumn : ColumnType, IPooledColumnType
        {
            public IColumnType BaseType { get; }
            public int FixedLength { get; protected set; } = 0;
            private int PoolIndex { get; set; } = -1;
            public GenericArrayColumn(IColumnType baseType)
            {
                BaseType = baseType;
                ArrayLevel = baseType.ArrayLevel + 1;
                if (ArrayLevel > 3)
                {
                    throw new Exception("Array Define过深，不能超过三层");
                }
            }

            public override IData Parse(string excelString)
            {
                var splitter = Define.ARRAY_SPLITTER[ArrayLevel];
                string[] result = excelString.Split(splitter);
                if (FixedLength > 0)
                {
                    if (result.Length != FixedLength)
                    {
                        throw new Exception($"数组长度不符合定义的长度：{FixedLength}");
                    }
                }
                PooledArrayData arrayData = new PooledArrayData(this);
                foreach (var r in result)
                {
                    arrayData.Add(BaseType.Parse(r));
                }
                return arrayData;
            }

            public override void WriteToBinary(IChunkBinary binary, IData obj)
            {
                Pool pool = binary.GetConfigBinary().GetPool(TypeName);
                binary.Write(GetPoolIndex(pool, (IPooledData)obj));
            }

            public int GetPoolIndex(Pool pool, IPooledData obj)
            {
                if(PoolIndex < 0)
                    PoolIndex = pool.GetIndex(obj);
                IArrayData arrayData = obj as IArrayData;
                if(arrayData != null)
                {
                    foreach(var data in arrayData.Data)
                    {
                        var pooledData = data as IPooledData;
                        if(pooledData != null)
                        {
                            pooledData.PooledColumnType.GetPoolIndex(pool, pooledData);
                        }
                    }
                }
                return PoolIndex;
            }

            public virtual void WriteToPool(IChunkBinary binary, IPooledData obj)
            {
                PooledArrayData arrayData = (PooledArrayData)obj;
                binary.Write(arrayData.Count);
                foreach(var data in arrayData.Data)
                {
                    data.WriteToBinary(binary);
                }
            }
        }

        public class ArrayColumn : GenericArrayColumn
        {
            public override string TypeName => base.TypeName + "[]";

            public ArrayColumn(IColumnType baseType)
                : base(baseType)
            {
                DefaultValue = "";
            }
        }

        public class PrimitiveColumn<T> : ColumnType
        {
            public PrimitiveColumn()
            {
                TypeName = typeof(T).GetTypeNameString();
                DefaultValue = typeof(T) == typeof(bool) ? "false" : "0";
            }

            public override IData Parse(string excelString)
            {
                return new Data(this, Convert.ChangeType(excelString, typeof(T)));
            }

            public override void WriteToBinary(IChunkBinary binary, IData data)
            {
                if(typeof(T) == typeof(int))
                {
                    binary.Write((int)data.Object);
                }
                else if (typeof(T) == typeof(uint))
                {
                    binary.Write((uint)data.Object);
                }
                else if (typeof(T) == typeof(float))
                {
                    binary.Write((float)data.Object);
                }
                else if (typeof(T) == typeof(long))
                {
                    binary.Write((long)data.Object);
                }
                else if (typeof(T) == typeof(ulong))
                {
                    binary.Write((ulong)data.Object);
                }
                else if (typeof(T) == typeof(bool))
                {
                    binary.Write((bool)data.Object);
                }
            }
        }

        public class DateTimeColumnType : ColumnType
        {
            public DateTimeColumnType()
            {
                TypeName = "DateTime";
            }

            public override IData Parse(string excelString)
            {
                if(string.IsNullOrWhiteSpace(excelString))
                {
                    return new Data(this, null);
                }
                return new Data(this, DateTime.ParseExact(excelString, "yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture));
            }

            public override void WriteToBinary(IChunkBinary binary, IData obj)
            {
                if(obj.Object == null)
                {
                    binary.Write(-1L);
                }
                else
                {
                    DateTime dt = (DateTime)obj.Object;
                    binary.Write(new DateTimeOffset(dt).ToUnixTimeSeconds());
                }
            }
        }

        public class Vector3ColumnType : GenericArrayColumn
        {
            public Vector3ColumnType() 
                : base(new PrimitiveColumn<float>())
            {
                FixedLength = 3;
                TypeName = "Vector3";
            }

            public override void WriteToBinary(IChunkBinary binary, IData obj)
            {
                List<IData> data = (List<IData>)obj.Object;
                binary.Write((float)data[0].Object);
                binary.Write((float)data[1].Object);
                binary.Write((float)data[2].Object);
            }
        }

        public class Vector2ColumnType : GenericArrayColumn
        {
            public Vector2ColumnType()
                : base(new PrimitiveColumn<float>())
            {
                FixedLength = 2;
                TypeName = "Vector2";
            }

            public override void WriteToBinary(IChunkBinary binary, IData obj)
            {
                List<IData> data = (List<IData>)obj.Object;
                binary.Write((float)data[0].Object);
                binary.Write((float)data[1].Object);
            }
        }

        public class ColorColumnType : ColumnType
        {
            public ColorColumnType()
            {
                TypeName = "Color";
            }

            public override IData Parse(string excelString)
            {
                int result;
                if(excelString.Length == 6)
                {
                    result = int.Parse(excelString, NumberStyles.HexNumber) << 8 | 0xFF;
                }
                else if(excelString.Length == 8)
                {
                    result = int.Parse(excelString, NumberStyles.HexNumber);
                }
                else
                {
                    throw new Exception($"错误的颜色格式，应为RRGGBB或者RRGGBBAA格式的Hex字符串");
                }
                return new Data(this, result);
            }

            public override void WriteToBinary(IChunkBinary binary, IData obj)
            {
                binary.Write((int)obj.Object);
            }
        }
    }
}
