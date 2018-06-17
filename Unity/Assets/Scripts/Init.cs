using System;
using System.Threading;
using UnityEngine;

namespace BK
{
	public class Init : MonoBehaviour
	{

		private async void Start()
		{
			try
			{
				DontDestroyOnLoad(gameObject);
				Game.EventSystem.Add(DLLType.Model, typeof(Init).Assembly);

				Game.Init();

				// 下载ab包
				await BundleHelper.DownloadBundle();

				Game.Hotfix.LoadHotfixAssembly();

				// 加载配置
				Game.ResourcesComponent.LoadBundle("config.unity3d");
				Game.Entity.AddComponent<ConfigComponent>();
				Game.ResourcesComponent.UnloadBundle("config.unity3d");

				Game.Hotfix.GotoHotfix();

				Game.EventSystem.Run(EventIdType.TestHotfixSubscribMonoEvent, "TestHotfixSubscribMonoEvent");
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}

		private void Update()
		{
			Game.Hotfix.Update?.Invoke();
			Game.EventSystem.Update();
		}

		private void LateUpdate()
		{
			Game.Hotfix.LateUpdate?.Invoke();
			Game.EventSystem.LateUpdate();
		}

		private void OnApplicationQuit()
		{
			Game.Hotfix.OnApplicationQuit?.Invoke();
			Game.Close();
		}
	}
}