using BKEditor.Config.Export;
using UnityEditor;
using UnityEngine;

namespace BKEditor
{
	static partial class Define
	{
		public static readonly string XlsxPath = Application.dataPath + "/Config/";
		public static readonly string ConfigOutputPath = Application.dataPath + "/Scripts/Config/";
		public static readonly string BinaryOututPath = Application.streamingAssetsPath + "/Config/";
	}

    class XlsxExporter
    {
	    [MenuItem("Tools/配表/导出")]
	    static void Export()
	    {
			new CsExporter("Test", Define.XlsxPath + "Test.xlsx", Define.ConfigOutputPath, Define.BinaryOututPath).Export();
	    }
    }
}
