using System;
using System.Collections.Generic;
using OfficeOpenXml;
using System.IO;
using System.Text;
using System.Linq;
using System.Security.Permissions;
using BK;
using BKEditor.Base;

namespace BKEditor.Config.Export
{
    public class Exporter
    {
		public string Name { get; }
        public ColumnTypeParser ColumnTypeParser { get; } = new ColumnTypeParser();
        private string FilePath { get; }
        private string CsOutputPath { get; }
	    private string BinaryOutputPath { get; }
		private ExcelPackage ExcelPackage { get; }
		private DataView DataView { get; }
		private ConfigBinary Binary { get; }

		public Exporter(string name, string filePath, string csOutputPath, string binaryOutputPath)
        {
            FilePath = filePath;
            CsOutputPath = csOutputPath;
	        BinaryOutputPath = binaryOutputPath;
	        Name = name;

			ExcelPackage = new ExcelPackage(new FileInfo(filePath));
	        DataView = new DataView(name, filePath, ExcelPackage);
	        Binary = new ConfigBinary(binaryOutputPath);
		}

	    public void Export()
	    {
			//读取表格头
			DataView.ReadHeader();
		    List<IData[]> data = DataView.ReadData();
		    WriteCsTemplate(data);
			Binary.WriteBinary(data);
	    }

	    private abstract class AConfigBinary : IConfigBinary
		{
			public AlignedUnsafeMemoryStreamWritter Stream { get; } = new AlignedUnsafeMemoryStreamWritter();
			public void Write(int i)
			{
				Stream.Write(i);
			}

			public void Write(uint i)
			{
				Stream.Write(i);
			}

			public void Write(long l)
			{
				Stream.Write(l);
			}

			public void Write(ulong l)
			{
				Stream.Write(l);
			}

			public void Write(bool b)
			{
				Stream.Write(b);
			}

			public void Write(float f)
			{
				Stream.Write(f);
			}

			public void Write(string s)
			{
				Stream.Write(s);
			}

			public abstract Pool GetPool(IPooledColumnType columnType);
		}

	    private class SubConfigBinary : AConfigBinary
	    {
		    private AConfigBinary Parent { get; }

		    public SubConfigBinary(AConfigBinary parent)
		    {
			    Parent = parent;
		    }

		    public override Pool GetPool(IPooledColumnType columnType)
		    {
			    return Parent.GetPool(columnType);
		    }
	    }

	    private static bool IsPrimitivePooledColumnType(IPooledColumnType columnType)
	    {
		    return columnType is IPrimitiveColomnType ||
		            (columnType as IArrayColumnType)?.BaseType is IPrimitiveColomnType;
	    }

	    private class ConfigBinary : AConfigBinary
	    {
		    private const int MAGIC = (byte)'B' | ((byte)'K' << 8) | ((byte)'C' << 16) | (1 << 24);
		    private readonly string outFile;
			private readonly List<KeyValuePair<IPooledColumnType, Pool>> pools = new List<KeyValuePair<IPooledColumnType, Pool>>();
			private readonly Dictionary<string, Pool> poolsDic = new Dictionary<string, Pool>();

		    public ConfigBinary(string outFile)
		    {
			    this.outFile = outFile;
		    }

		    public override Pool GetPool(IPooledColumnType columnType)
		    {
			    Pool pool;
			    if (!poolsDic.TryGetValue(columnType.TypeName, out pool))
			    {
					pool = new Pool();
					pools.Add(new KeyValuePair<IPooledColumnType, Pool>(columnType, pool));
					poolsDic.Add(columnType.TypeName, pool);
			    }

			    return pool;
		    }

		    public List<KeyValuePair<IPooledColumnType, Pool>> Pools => pools;

			public void WriteBinary(List<IData[]> data)
		    {
				//将数据写入mainchunk
			    foreach (var datase in data)
			    {
				    foreach (var data1 in datase)
				    {
					    data1.WriteToBinary(this);
				    }
			    }
			    byte[] align2 = new byte[2] { 0,0 };
				var tmpBinary = new SubConfigBinary(this);
				using (var fs = new FileStream(outFile, FileMode.Truncate))
			    {
					fs.Write(MAGIC);
					//先写入Primitive Pool数据
				    foreach (var keyValuePair in pools)
				    {
					    if (IsPrimitivePooledColumnType(keyValuePair.Key))
					    {
						    var pool = keyValuePair.Value;
						    fs.Write(pool.Count);
						    foreach (var data2 in pool)
						    {
							    data2.WriteToPool(tmpBinary);
								tmpBinary.Stream.WriteToStream(fs);
								tmpBinary.Stream.Clear();
						    }
						}
					}
				    //再先写入非Primitive Pool数据
				    foreach (var keyValuePair in pools)
				    {
					    if (!IsPrimitivePooledColumnType(keyValuePair.Key))
					    {
						    var pool = keyValuePair.Value;
						    fs.Write(pool.Count);
						    foreach (var data2 in pool)
						    {
							    data2.WriteToPool(tmpBinary);
							    tmpBinary.Stream.WriteToStream(fs);
							    tmpBinary.Stream.Clear();
						    }
					    }
				    }
					//再写入mainchunk
					Stream.WriteToStream(fs);
			    }
		    }
	    }

	    private void WriteCsTemplate(List<IData[]> data)
	    {
		    using (var file = new FileStream($"{CsOutputPath}/{Name}Config.cs", FileMode.Truncate))
		    {
			    using (var sb = new IndentedStreamWritter(file, Encoding.UTF8))
			    {
					WriteCsTemplate(sb, data);
			    }
		    }
	    }

		private void WriteCsTemplate(IndentedStreamWritter sw, List<IData[]> dataList)
	    {
		    sw.WriteLine("using System;");
		    sw.WriteLine("using System.Collections;");
		    sw.WriteLine("using System.Collections.Generic;");
			sw.WriteLine("using UnityEngine;");
		    sw.WriteLine("using BK;");
		    sw.WriteLine("using BK.Config.Loader;");
			sw.WriteLine("namespace BK.Config");
		    sw.WriteLine("{");
			sw.AddIndent();

			sw.WriteLine($"public class {Name}Config");
		    sw.WriteLine("{");
		    sw.AddIndent();

		    {
			    // 写入字段
			    foreach (var dataViewColumnHeader in DataView.ColumnHeaders)
			    {
				    sw.WriteLine($"public readonly {dataViewColumnHeader.ColumnType.TypeName} {dataViewColumnHeader.Name};");
			    }
			    sw.WriteLine();
			}

		    {
				// 写入构造函数
			    var ctorParams =
				    from p in DataView.ColumnHeaders
					select $"{p.ColumnType.TypeName} {p.Name}";
				sw.WriteLine($"public {Name}Config({string.Join(", ", ctorParams.ToArray())})");
			    sw.WriteLine("{");
			    sw.AddIndent();
			    var ctorInits =
				    from p in DataView.ColumnHeaders
					select $"this.{p.Name} = {p.Name};";
			    foreach (var ctorInit in ctorInits)
			    {
				    sw.WriteLine(ctorInit);
			    }
				sw.DecIndent();
			    sw.WriteLine("}");
			}

			sw.WriteLine($"public static int Count => Internal.{Name}ConfigCount");
		    sw.WriteLine();

		    {
			    // 写入静态atlas
			    var atlases =
				    from id in
					    from data in dataList
					    select data[0] as IIdData
				    where id.Atlas != null
				    select new KeyValuePair<uint, string>((uint)id.Object, id.Atlas);
			    foreach (var atlas in atlases)
			    {
				    sw.WriteLine($"public static const uint {atlas.Value} = {atlas.Key};");
			    }
			}

			// 写入自定义class定义
		    var classes =
			    from @class in
				    from column in ColumnTypeParser.CustomTypes
					where column is ICustomColumnType
					select column as ICustomColumnType
			    select new KeyValuePair<string, List<KeyValuePair<IColumnType, string>>>(@class.TypeName, @class.Fields);
		    foreach (var keyValuePair in classes)
		    {
			    sw.WriteLine();
				var typeName = keyValuePair.Key;
			    sw.WriteLine($"public class {typeName}");
			    sw.WriteLine("{");
			    sw.AddIndent();

			    foreach (var field in keyValuePair.Value)
			    {
				    sw.WriteLine($"public readonly {field.Key.TypeName} {field.Value};");
				}
			    sw.WriteLine();
			    var ctorParams =
				    from p in keyValuePair.Value
				    select $"{p.Key.TypeName} {p.Value}";
			    sw.WriteLine($"public {typeName}({string.Join(", ", ctorParams.ToArray())})");
			    sw.WriteLine("{");
			    sw.AddIndent();
			    var ctorInits =
				    from p in keyValuePair.Value
				    select $"this.{p.Value} = {p.Value};";
			    foreach (var ctorInit in ctorInits)
			    {
					sw.WriteLine(ctorInit);
				}
				sw.DecIndent();
			    sw.WriteLine("}");

				sw.DecIndent();
			    sw.WriteLine("}");
			}

			//写入自定义enum
		    var enums =
			    from @enum in
				    from column in ColumnTypeParser.CustomTypes
				    where column is IEnumColumnType
				    select column as IEnumColumnType
				select new KeyValuePair<string, List<KeyValuePair<string, int>>>(@enum.TypeName, @enum.Fields);
		    foreach (var keyValuePair in enums)
			{
				sw.WriteLine();
				var typeName = keyValuePair.Key;
			    sw.WriteLine($"public enum {typeName}");
			    sw.WriteLine("{");
			    sw.AddIndent();

			    foreach (var field in keyValuePair.Value)
			    {
				    sw.WriteLine($"public {field.Key} = {field.Value};");
			    }
			    sw.DecIndent();
			    sw.WriteLine("}");
		    }

			sw.DecIndent();
		    sw.WriteLine("}");

			//内部实现
		    sw.WriteLine($"static partial class Internal");
		    sw.WriteLine("{");
		    sw.AddIndent();
			//内部结构变量
		    sw.WriteLine($"public static readonly List<{Name}Config> {Name}ConfigList = new List<{Name}Config>();");
		    sw.WriteLine($"public static readonly Dictionary<uint, {Name}Config> {Name}ConfigDic = new Dictionary<uint, {Name}Config>();");
			sw.WriteLine($"public static int {Name}ConfigCount;");
		    {
			    //内部初始化函数
				sw.WriteLine($"public static void Init{Name}Config()");
			    sw.WriteLine("{");
			    sw.AddIndent();
				
				sw.WriteLine($"var asset = (TextAsset)Game.ResourcesComponent.GetAsset(\"config\", \"{Name}Config.bytes\");");
			    sw.WriteLine($"var bytes = asset.bytes;"); 
			    sw.WriteLine($"var binary = new ConfigBinary(bytes);");
			    sw.WriteLine($"const int MAGIC = (byte)'B' | ((byte)'K' << 8) | ((byte)'C' << 16) | (1 << 24);");
			    sw.WriteLine($"if(binary.ReadInt() != MAGIC) throw new Exception(\"Wrong Magic\");");
			    sw.WriteLine();

				var primitiveTypeNames =
				    from t in Binary.Pools
				    where t.Item1
				    select t.Item2;
			    foreach (var primitiveTypeName in primitiveTypeNames)
			    {
				    sw.WriteLine($"{primitiveTypeName}[] pool{primitiveTypeName};");
			    }

			    sw.DecIndent();
			    sw.WriteLine("}");
			}

			sw.DecIndent();
		    sw.WriteLine("}");

			sw.DecIndent();
		    sw.WriteLine("}");
		}
	}

	static class FileStreamEx
	{
		public static void Write(this FileStream fs, byte[] bytes)
		{
			fs.Write(bytes, 0, bytes.Length);
		}

		public static void Write(this FileStream fs, int i)
		{
			fs.Write(BitConverter.GetBytes(i));
		}
	}
}
