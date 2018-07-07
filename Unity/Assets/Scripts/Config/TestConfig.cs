using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BK;
using EnumAType = BK.Config.TestConfig.EnumAType;
using BK.Config.Loader;
namespace BK.Config
{
	public class TestConfig
	{
		public readonly uint Id;
		public readonly string Str;
		public readonly int Int;
		public readonly float Flo;
		public readonly string[] StrA;
		public readonly int[] IntA;
		public readonly EnumAType EnumA;
		
		public TestConfig(uint Id, string Str, int Int, float Flo, string[] StrA, int[] IntA, EnumAType EnumA)
		{
			this.Id = Id;
			this.Str = Str;
			this.Int = Int;
			this.Flo = Flo;
			this.StrA = StrA;
			this.IntA = IntA;
			this.EnumA = EnumA;
		}
		public static int Count => Internal.TestConfigLoader.ConfigCount;
		public static TestConfig GetConfig(uint id) => Internal.TestConfigLoader.ConfigDic[id];
		public static IEnumerable<TestConfig> All => Internal.TestConfigLoader.ConfigList;
		public static TestConfig GetConfig(Atlas idAtlas) => GetConfig((uint)idAtlas);
		
		public enum Atlas : uint
		{
			Abc = 1,
			Def = 2,
		}
		public const Atlas Abc = Atlas.Abc;
		public const Atlas Def = Atlas.Def;
		
		public enum EnumAType
		{
			Test = 1,
			Test2 = 2,
		}
	}
	static partial class Internal
	{
		public static class TestConfigLoader
		{
			public static TestConfig[] ConfigList {get; private set;}
			public static readonly Dictionary<uint, TestConfig> ConfigDic = new Dictionary<uint, TestConfig>();
			public static int ConfigCount;
			
			private static string[] poolString = null;
			private static string[][] poolStringArray = null;
			private static int[][] poolIntArray = null;
			
			private static string[] LoadStringArray(IConfigBinary binary)
			{
				var count = binary.ReadInt();
				var arr = new string[count];
				for(int i = 0; i < count; i++)
				{
					arr[i] = poolString[binary.ReadInt()];
				}
				return arr;
			}
			
			private static int[] LoadIntArray(IConfigBinary binary)
			{
				var count = binary.ReadInt();
				var arr = new int[count];
				for(int i = 0; i < count; i++)
				{
					arr[i] = binary.ReadInt();
				}
				return arr;
			}
			
			public static void Init()
			{
				var asset = (TextAsset)Game.ResourcesComponent.GetAsset("config", "TestConfig.bytes");
				var bytes = asset.bytes;
				var binary = new ConfigBinary(bytes);
				const int MAGIC = (byte)'B' | ((byte)'K' << 8) | ((byte)'C' << 16) | (1 << 24);
				if(binary.ReadInt() != MAGIC) throw new Exception("Wrong Magic");
				
				{
					int count = binary.ReadInt();
					poolString =  new string[count];
					for(int i = 0; i < count; i++)
					{
						poolString[i] = binary.ReadString();
					}
				}
				{
					int count = binary.ReadInt();
					poolStringArray =  new string[count][];
					for(int i = 0; i < count; i++)
					{
						poolStringArray[i] = LoadStringArray(binary);
					}
				}
				{
					int count = binary.ReadInt();
					poolIntArray =  new int[count][];
					for(int i = 0; i < count; i++)
					{
						poolIntArray[i] = LoadIntArray(binary);
					}
				}
				{
					int count = binary.ReadInt();
					ConfigCount = count;
					ConfigList = new TestConfig[count];
					for(int i = 0; i < count; i++)
					{
						var Id = binary.ReadUInt();
						var Str = poolString[binary.ReadInt()];
						var Int = binary.ReadInt();
						var Flo = binary.ReadFloat();
						var StrA = poolStringArray[binary.ReadInt()];
						var IntA = poolIntArray[binary.ReadInt()];
						var EnumA = (EnumAType)binary.ReadInt();
						var cfg = new TestConfig(Id, Str, Int, Flo, StrA, IntA, EnumA);
						ConfigList[i] = cfg;
						ConfigDic.Add(cfg.Id, cfg);
					}
				}
			}
		}
	}
}
