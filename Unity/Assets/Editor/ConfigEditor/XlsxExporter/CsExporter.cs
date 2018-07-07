using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace BKEditor.Config.Export
{
	public class CsExporter : Exporter
	{
		public CsExporter(string name, string filePath, string csOutputPath, string binaryOutputPath)
			: base(name, filePath, csOutputPath, binaryOutputPath)
		{
		}

		protected override void WriteTemplate(List<IData[]> data)
		{
			WriteCsTemplate(data);
		}



		private void WriteCsTemplate(List<IData[]> data)
		{
			using (var sb = new IndentedTextWriter())
			{
				WriteCsTemplate(sb, data);
				File.WriteAllText($"{CsOutputPath}/{Name}Config.cs", sb.ToString(), Encoding.UTF8);
			}
		}

		private static readonly Dictionary<Type, string> primitiveTypeStrings = new Dictionary<Type, string>()
		{
			{ typeof(int), "binary.ReadInt()"},
			{ typeof(uint), "binary.ReadUInt()"},
			{ typeof(long), "binary.ReadLong()"},
			{ typeof(ulong), "binary.ReadULong()"},
			{ typeof(bool), "binary.ReadBool()"},
			{ typeof(float), "binary.ReadFloat()"},
			{ typeof(string), "binary.ReadString()"},
			{ typeof(Vector3), "new Vector3(binary.ReadFloat(),binary.ReadFloat(),binary.ReadFloat())"},
			{ typeof(Vector2), "new Vector2(binary.ReadFloat(),binary.ReadFloat())"},
			{ typeof(Color), "ColorUtils.IntToColor(binary.ReadInt())"},
			{ typeof(DateTime), "DateTimeOffset.FromUnixTimeSeconds(binary.ReadInt()).DateTime"},

		};

		private string GetReadFromPrimitiveTypeString(Type primitiveType)
		{
			string str;
			return primitiveTypeStrings.TryGetValue(primitiveType, out str) ? str : null;
		}

		private string GetReadFromPrimitiveTypeString(IPrimitiveColomnType type)
		{
			string str = GetReadFromPrimitiveTypeString(type.PrimitiveType);
			if (str != null)
				return str;
			if (type.PrimitiveType == typeof(Enum))
			{
				return $"({type.TypeName})binary.ReadInt()";
			}

			throw new Exception($"找不到Primitive {type.PrimitiveType.Name} 的Reader");
		}

		private string GetReadFromPoolString(IPooledColumnType type)
		{
			var primitiveColomnType = type as IPrimitiveColomnType;
			if (primitiveColomnType != null)
			{
				return GetReadFromPrimitiveTypeString(primitiveColomnType);
			}
			var arrayColumnType = type as IArrayColumnType;
			if (arrayColumnType != null)
			{
				return $"Load{NormalizeName(arrayColumnType.TypeName.Replace("[]", "Array"))}(binary)";
			}
			var customColumnType = type as ICustomColumnType;
			if (customColumnType != null)
			{
				return $"Load{NormalizeName(customColumnType.TypeName)}(binary)";
			}
			throw new Exception();
		}

		private string NormalizeName(string name)
		{
			char head = name[0];
			if (char.IsUpper(head))
			{
				return name;
			}

			return char.ToUpper(head) + name.Substring(1, name.Length - 1);
		}

		private string NewArray(string typeName)
		{
			var sig = typeName.IndexOf('[');
			if (sig >= 0)
			{
				return $"{typeName.Substring(0, sig)}[count]{typeName.Substring(sig)}";
			}
			else
			{
				return $"{typeName}[count]";
			}
		}

		private string GetPoolName(IPooledColumnType type)
		{
			var arrayColumnType = type as IArrayColumnType;
			if (arrayColumnType != null)
			{
				return $"pool{NormalizeName(arrayColumnType.TypeName.Replace("[]", "Array"))}";
			}

			return $"pool{NormalizeName(type.TypeName)}";
		}

		private string GetReadFromBinaryString(IColumnType type)
		{
			var pooledColumnType = type as IPooledColumnType;
			if (pooledColumnType != null)
			{
				return $"{GetPoolName(pooledColumnType)}[binary.ReadInt()]";
			}
			var primitiveColomnType = type as IPrimitiveColomnType;
			if (primitiveColomnType != null)
			{
				return GetReadFromPrimitiveTypeString(primitiveColomnType);
			}
			throw new Exception();
		}

		private void WriteCsTemplate(IndentedTextWriter sw, List<IData[]> dataList)
		{
			// 先找出所有自定义class定义
			var classes =
				from @class in
					from column in DataView.ColumnTypeParser.CustomTypes
					where column is ICustomColumnType
					select column as ICustomColumnType
				select new KeyValuePair<string, List<KeyValuePair<IColumnType, string>>>(@class.TypeName, @class.Fields);
			var enums =
				from @enum in
					from column in DataView.ColumnTypeParser.CustomTypes
					where column is IEnumColumnType
					select column as IEnumColumnType
				select new KeyValuePair<string, List<KeyValuePair<string, int>>>(@enum.TypeName, @enum.Fields);

			sw.WriteLine("using System;");
			sw.WriteLine("using System.Collections;");
			sw.WriteLine("using System.Collections.Generic;");
			sw.WriteLine("using UnityEngine;");
			sw.WriteLine("using BK;");
			foreach (var pair in classes)
			{
				sw.WriteLine($"using {pair.Key} = BK.Config.{Name}Config.{pair.Key};");
			}
			foreach (var pair in enums)
			{
				sw.WriteLine($"using {pair.Key} = BK.Config.{Name}Config.{pair.Key};");
			}
			sw.WriteLine("using BK.Config.Loader;");
			sw.WriteLine("namespace BK.Config");
			sw.WriteLine("{");
			sw.Indent++;

			sw.WriteLine($"public class {Name}Config");
			sw.WriteLine("{");
			sw.Indent++;

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
				sw.Indent++;
				var ctorInits =
					from p in DataView.ColumnHeaders
					select $"this.{p.Name} = {p.Name};";
				foreach (var ctorInit in ctorInits)
				{
					sw.WriteLine(ctorInit);
				}
				sw.Indent--;
				sw.WriteLine("}");
			}

			sw.WriteLine($"public static int Count => Internal.{Name}ConfigLoader.ConfigCount;");
			sw.WriteLine($"public static {Name}Config GetConfig(uint id) => Internal.{Name}ConfigLoader.ConfigDic[id];");
			sw.WriteLine($"public static IEnumerable<{Name}Config> All => Internal.{Name}ConfigLoader.ConfigList;");

			{
				// 写入静态atlas
				var atlases =
					from id in
						from data in dataList
						select data[0] as IIdData
					where id.Atlas != null
					select new KeyValuePair<uint, string>((uint)id.Object, id.Atlas);
				if (atlases.Any())
				{
					sw.WriteLine($"public static {Name}Config GetConfig(Atlas idAtlas) => GetConfig((uint)idAtlas);");
					sw.WriteLine();
					sw.WriteLine($"public enum Atlas : uint");
					sw.WriteLine("{");
					sw.Indent++;
					foreach (var atlas in atlases)
					{
						sw.WriteLine($"{atlas.Value} = {atlas.Key},");
					}
					sw.Indent--;
					sw.WriteLine("}");
					foreach (var atlas in atlases)
					{
						sw.WriteLine($"public const Atlas {atlas.Value} = Atlas.{atlas.Value};");
					}
				}
			}

			// 写入自定义class定义
			foreach (var keyValuePair in classes)
			{
				sw.WriteLine();
				var typeName = keyValuePair.Key;
				sw.WriteLine($"public class {typeName}");
				sw.WriteLine("{");
				sw.Indent++;

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
				sw.Indent++;
				var ctorInits =
					from p in keyValuePair.Value
					select $"this.{p.Value} = {p.Value};";
				foreach (var ctorInit in ctorInits)
				{
					sw.WriteLine(ctorInit);
				}
				sw.Indent--;
				sw.WriteLine("}");

				sw.Indent--;
				sw.WriteLine("}");
			}

			//写入自定义enum
			foreach (var keyValuePair in enums)
			{
				sw.WriteLine();
				var typeName = keyValuePair.Key;
				sw.WriteLine($"public enum {typeName}");
				sw.WriteLine("{");
				sw.Indent++;

				foreach (var field in keyValuePair.Value)
				{
					sw.WriteLine($"{field.Key} = {field.Value},");
				}
				sw.Indent--;
				sw.WriteLine("}");
			}

			sw.Indent--;
			sw.WriteLine("}");

			//内部实现
			sw.WriteLine($"static partial class Internal");
			sw.WriteLine("{");
			sw.Indent++;

			sw.WriteLine($"public static class {Name}ConfigLoader");
			sw.WriteLine("{");
			sw.Indent++;
			//内部结构变量
			sw.WriteLine($"public static {Name}Config[] ConfigList {{get; private set;}}");
			sw.WriteLine($"public static readonly Dictionary<uint, {Name}Config> ConfigDic = new Dictionary<uint, {Name}Config>();");
			sw.WriteLine($"public static int ConfigCount;");
			sw.WriteLine("");
			//写入pool变量
			foreach (var pair in Binary.Pools)
			{
				if (!IsPrimitivePooledColumnType(pair.Key))
					continue;
				var typeName = pair.Key.TypeName;
				var poolName = GetPoolName(pair.Key);
				sw.WriteLine($"private static {typeName}[] {poolName} = null;");
			}

			{
				// 写入自定义class定义
				foreach (var keyValuePair in classes)
				{
					sw.WriteLine();
					var typeName = keyValuePair.Key;
					sw.WriteLine($"private static {typeName} Load{NormalizeName(typeName)}(IConfigBinary binary)");
					sw.WriteLine("{");
					sw.Indent++;
					for (var index = 0; index < keyValuePair.Value.Count; index++)
					{
						var valuePair = keyValuePair.Value[index];
						sw.WriteLine($"var field{index+1} = {GetReadFromBinaryString(valuePair.Key)};");
					}

					var strings = Enumerable.Range(1, keyValuePair.Value.Count).Select(i => $"field{i}");
					sw.WriteLine($"return new {typeName}({string.Join(",", strings.ToArray())})");

					sw.Indent--;
					sw.WriteLine("}");
				}
				// 写入Array的定义
				var arrays =
					from p in Binary.Pools
					where p.Key is IArrayColumnType
					select p.Key as IArrayColumnType;
				foreach (var array in arrays)
				{
					sw.WriteLine();
					var typeName = array.TypeName;
					sw.WriteLine($"private static {typeName} Load{NormalizeName(typeName.Replace("[]", "Array"))}(IConfigBinary binary)");
					sw.WriteLine("{");
					sw.Indent++;
					sw.WriteLine($"var count = binary.ReadInt();");
					sw.WriteLine($"var arr = new {NewArray(array.BaseType.TypeName)};");
					sw.WriteLine($"for(int i = 0; i < count; i++)");
					sw.WriteLine("{");
					sw.Indent++;
					sw.WriteLine($"arr[i] = {GetReadFromBinaryString(array.BaseType)};");
					sw.Indent--;
					sw.WriteLine("}");
					sw.WriteLine($"return arr;");
					sw.Indent--;
					sw.WriteLine("}");
				}
			}
			{
				sw.WriteLine();
				//内部初始化函数
				sw.WriteLine($"public static void Init()");
				sw.WriteLine("{");
				sw.Indent++;

				sw.WriteLine($"var asset = (TextAsset)Game.ResourcesComponent.GetAsset(\"config\", \"{Name}Config.bytes\");");
				sw.WriteLine($"var bytes = asset.bytes;");
				sw.WriteLine($"var binary = new ConfigBinary(bytes);");
				sw.WriteLine($"const int MAGIC = (byte)'B' | ((byte)'K' << 8) | ((byte)'C' << 16) | (1 << 24);");
				sw.WriteLine($"if(binary.ReadInt() != MAGIC) throw new Exception(\"Wrong Magic\");");
				sw.WriteLine();

				// 先初始化Primitive类型的Pool
				foreach (var pair in Binary.Pools)
				{
					if (!IsPrimitivePooledColumnType(pair.Key))
						continue;
					var typeName = pair.Key.TypeName;
					var poolName = GetPoolName(pair.Key);
					sw.WriteLine("{");
					sw.Indent++;
					sw.WriteLine($"int count = binary.ReadInt();");
					sw.WriteLine($"{poolName} =  new {NewArray(typeName)};");
					sw.WriteLine($"for(int i = 0; i < count; i++)");
					sw.WriteLine("{");
					sw.Indent++;
					sw.WriteLine($"{poolName}[i] = {GetReadFromPoolString(pair.Key)};");
					sw.Indent--;
					sw.WriteLine("}");
					sw.Indent--;
					sw.WriteLine("}");
				}
				// 再初始化非Primitive类型的Pool
				foreach (var pair in Binary.Pools)
				{
					if (IsPrimitivePooledColumnType(pair.Key))
						continue;
					var typeName = pair.Key.TypeName;
					var poolName = GetPoolName(pair.Key);
					sw.WriteLine("{");
					sw.Indent++;
					sw.WriteLine($"int count = binary.ReadInt();");
					sw.WriteLine($"{poolName} =  new {NewArray(typeName)};");
					sw.WriteLine($"for(int i = 0; i < count; i++)");
					sw.WriteLine("{");
					sw.Indent++;
					sw.WriteLine($"{poolName}[i] = {GetReadFromPoolString(pair.Key)};");
					sw.Indent--;
					sw.WriteLine("}");
					sw.Indent--;
					sw.WriteLine("}");
				}

				// 初始化main chunk
				{
					sw.WriteLine("{");
					sw.Indent++;
					sw.WriteLine($"int count = binary.ReadInt();");
					sw.WriteLine($"ConfigCount = count;");
					sw.WriteLine($"ConfigList = new {Name}Config[count];");
					sw.WriteLine($"for(int i = 0; i < count; i++)");
					sw.WriteLine("{");
					sw.Indent++;

					foreach (var columnHeader in DataView.ColumnHeaders)
					{
						sw.WriteLine($"var {columnHeader.Name} = {GetReadFromBinaryString(columnHeader.ColumnType)};");
					}
					sw.WriteLine($"var cfg = new {Name}Config({string.Join(", ", DataView.ColumnHeaders.Select(c => c.Name).ToArray())});");
					sw.WriteLine($"ConfigList[i] = cfg;");
					sw.WriteLine($"ConfigDic.Add(cfg.Id, cfg);");

					sw.Indent--;
					sw.WriteLine("}");
					sw.Indent--;
					sw.WriteLine("}");
				}

				sw.Indent--;
				sw.WriteLine("}");
			}

			sw.Indent--;
			sw.WriteLine("}");
			
			sw.Indent--;
			sw.WriteLine("}");

			sw.Indent--;
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
