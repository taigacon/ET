using System;
using System.Collections.Generic;
using System.IO;
using CodeGenerationTools.Generator;
using ETModel;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEditor;
using UnityEngine;

namespace CodeGenerationTools
{
	public class ILRuntimeCodeGenerator
	{
		private readonly string _outputPath = Application.dataPath + "/ThirdParty/ILRuntime/Generated/Adapter/";
		private readonly string _tmpdPath = Application.dataPath + "/ThirdParty/ILRuntime/Editor/Template/";
		private readonly string _ilScriptPath = Application.dataPath + "/Res/Code/Hotfix.dll.bytes";
		private readonly string _scriptDllPath = "Library/ScriptAssemblies/Assembly-CSharp.dll";
		private readonly string _adaptorAttrName = "ILRuntime.Other.NeedAdaptorAttribute";
		private readonly string _delegateAttrName = "ILRuntime.Other.DelegateExportAttribute";

		private readonly Dictionary<string, TypeDefinition> _adaptorDic = new Dictionary<string, TypeDefinition>();
		private readonly Dictionary<string, TypeDefinition> _delegateCovDic = new Dictionary<string, TypeDefinition>();
		private readonly Dictionary<string, TypeReference> _delegateRegDic = new Dictionary<string, TypeReference>();

		private AdaptorGenerator _adGenerator;
		private HelperGenerator _helpGenerator;

		private void LoadTemplates()
		{
			_adGenerator = new AdaptorGenerator();
			_adGenerator.LoadTemplateFromFile(_tmpdPath + "adaptor.tmpd");

			_helpGenerator = new HelperGenerator();
			_helpGenerator.LoadTemplateFromFile(_tmpdPath + "helper.tmpd");
		}

		private void Print(string s)
		{
			Debug.Log(s);
		}


		private void CreateILRuntimeHelper()
		{
			Print($"==================Begin create helper:=====================");

			_helpGenerator.LoadData(new Tuple<Dictionary<string, TypeDefinition>, Dictionary<string, TypeDefinition>, Dictionary<string, TypeReference>>(_adaptorDic, _delegateCovDic, _delegateRegDic));
			var helperStr = _helpGenerator.Generate();

			using (var fs2 = File.Create(_outputPath + "helper.cs"))
			{
				var sw = new StreamWriter(fs2);
				sw.Write(helperStr);
				sw.Flush();
			}

			Print($"==============End create helper:===================");
		}

		private void CreateAdaptor(TypeDefinition type)
		{
			if (type.IsInterface)
				return;


			Print($"================begin create adaptor:{type.Name}=======================");

			var adaptorName = type.Name + "Adaptor";

			using (var fs = File.Create(_outputPath + adaptorName + ".cs"))
			{

				_adGenerator.LoadData(type);
				var classbody = _adGenerator.Generate();

				var sw = new StreamWriter(fs);
				sw.Write(classbody);
				sw.Flush();
			}

			Print($"================end create adaptor:{type.Name}=======================");

		}


		private void LoadDelegateRegister(string key, TypeReference type)
		{
			if (!_delegateRegDic.ContainsKey(key))
				_delegateRegDic.Add(key, type);
			else
				_delegateRegDic[key] = type;
		}

		private void LoadDelegateConvertor(TypeDefinition type)
		{
			var key = type.FullName.Replace("/", ".");
			if (!_delegateCovDic.ContainsKey(key))
				_delegateCovDic.Add(key, type);
			else
				_delegateCovDic[type.FullName] = type;
		}

		private void LoadAdaptor(TypeDefinition type)
		{
			//var key = type.FullName.Replace("/", ".");
			if (!_adaptorDic.ContainsKey(type.FullName))
				_adaptorDic.Add(type.FullName, type);
			else
				_adaptorDic[type.FullName] = type;
		}

		private void Load()
		{
			LoadTemplates();
			// Main Project
			{
				_adaptorDic.Clear();
				_delegateCovDic.Clear();

				var module = ModuleDefinition.ReadModule(_scriptDllPath);
				var typeList = module.GetTypes();
				foreach (var t in typeList)
				{
					foreach (var customAttribute in t.CustomAttributes)
					{
						if (customAttribute.AttributeType.FullName == _adaptorAttrName)
						{
							Print("[Need Adaptor]" + t.FullName);
							LoadAdaptor(t);
							continue;
						}

						if (customAttribute.AttributeType.FullName == _delegateAttrName)
						{
							//unity dll egg hurt name has '/'
							var typeName = t.FullName.Replace("/", ".");
							Print("[Delegate Export]" + typeName);
							LoadDelegateConvertor(t);
							continue;
						}
					}
				}
			}

			// Hotfix Project
			{
				_delegateRegDic.Clear();
				var module = ModuleDefinition.ReadModule(_ilScriptPath);
				foreach (var typeDefinition in module.Types)
				{
					foreach (var methodDefinition in typeDefinition.Methods)
					{
						if (methodDefinition?.Body?.Instructions == null)
							continue;
						foreach (var instruction in methodDefinition.Body.Instructions)
						{
							if (instruction.OpCode != OpCodes.Newobj || instruction.Previous == null ||
							    instruction.Previous.OpCode != OpCodes.Ldftn) continue;

							var type = instruction.Operand as MethodReference;
							if (type == null ||
							    (!type.DeclaringType.Name.Contains("Action") &&
							     !type.DeclaringType.Name.Contains("Func"))) continue;

							var typeName = type.DeclaringType.FullName;//.Replace("/", ".");
							Print("[delegate register]" + typeName);
							LoadDelegateRegister(typeName, type.DeclaringType);
						}
					}
				}
			}
		}

		public void Generate()
		{
			Load();
			if (_adaptorDic.Count <= 0 && _delegateCovDic.Count <= 0 && _delegateRegDic.Count <= 0)
			{
				Print("[Warnning] There is nothing to Generate");
				return;
			}

			Directory.CreateDirectory(_outputPath);

			Print("===============================Clear Old Files================================");
			var files = Directory.GetFiles(_outputPath);
			foreach (var file in files)
			{
				File.Delete(file);
			}

			Print("[=============================Generate Begin==============================]");

			foreach (var type in _adaptorDic.Values)
			{
				CreateAdaptor(type);
			}

			//CreateILRuntimeHelper();

			Print("[=============================Generate End=================================]");
		}

		[MenuItem("Tools/ILRuntime/Generate Code")]
		public static void EditorGenerate()
		{
			new ILRuntimeCodeGenerator().Generate();
		}
	}
}