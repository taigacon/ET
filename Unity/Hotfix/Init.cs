using System;
using BK;

namespace BKHotfix
{
	public static class Init
	{
		public static void Start()
		{
			try
			{
				// 注册热更层回调
				BK.Game.Hotfix.Update = Update;
				BK.Game.Hotfix.LateUpdate = LateUpdate;
				BK.Game.Hotfix.OnApplicationQuit = OnApplicationQuit;
				
				Game.Init();
				Game.EventSystem.Run(EventIdType.InitSceneStart);
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}

		public static void Update()
		{
			try
			{
				Game.EventSystem.Update();
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}

		public static void LateUpdate()
		{
			try
			{
				Game.EventSystem.LateUpdate();
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}

		public static void OnApplicationQuit()
		{
			Game.Close();
		}
	}
}