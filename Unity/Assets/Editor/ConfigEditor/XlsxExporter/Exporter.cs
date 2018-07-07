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
    public abstract class Exporter
    {
		public string Name { get; }
        protected string FilePath { get; }
	    protected string CsOutputPath { get; }
	    protected string BinaryOutputPath { get; }
	    protected ExcelPackage ExcelPackage { get; }
	    protected DataView DataView { get; }
	    protected ConfigBinary Binary { get; }

	    protected Exporter(string name, string filePath, string csOutputPath, string binaryOutputPath)
        {
            FilePath = filePath;
            CsOutputPath = csOutputPath;
	        BinaryOutputPath = binaryOutputPath;
	        Name = name;

			ExcelPackage = new ExcelPackage(new FileInfo(filePath));
	        DataView = new DataView(name, filePath, ExcelPackage);
	        Binary = new ConfigBinary($"{binaryOutputPath}/{Name}.bytes");
		}

	    public void Export()
	    {
			//读取表格头
			DataView.ReadHeader();
		    List<IData[]> data = DataView.ReadData();
		    Binary.WriteBinary(data);
			WriteTemplate(data);
	    }

	    protected abstract class AConfigBinary : IConfigBinary
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

	    protected class SubConfigBinary : AConfigBinary
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

	    protected static bool IsPrimitivePooledColumnType(IPooledColumnType columnType)
	    {
		    return columnType is IPrimitiveColomnType ||
		            (columnType as IArrayColumnType)?.BaseType is IPrimitiveColomnType;
	    }

	    protected class ConfigBinary : AConfigBinary
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
			    this.Write(data.Count);
				foreach (var datase in data)
			    {
				    foreach (var data1 in datase)
				    {
					    data1.WriteToBinary(this);
				    }
			    }
				var tmpBinary = new SubConfigBinary(this);
				using (var fs = new FileStream(outFile, FileMode.Create))
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

	    protected abstract void WriteTemplate(List<IData[]> data);
	}
}
