using CodeGenerationTools;
using UnityEditor;

namespace ILRuntime
{
	public class ILRuntime
	{
		[MenuItem("Tools/ILRuntime/Generate All", priority = 0)]
		static void GenerateAll()
		{
			ILRuntimeCodeGenerator.EditorGenerate();
			AssetDatabase.Refresh();
			ILRuntimeCLRBinding.GenerateCLRBindingByAnalysis();
			AssetDatabase.Refresh();
		}
	}
}