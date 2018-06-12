using System;
using System.Threading.Tasks;
using UnityEngine;

namespace ETModel
{
	public static class BundleHelper
	{
		public static async Task DownloadBundle()
		{
			Game.EventSystem.Run(EventIdType.LoadingBegin);
			await StartDownLoadResources();
			Game.EventSystem.Run(EventIdType.LoadingFinish);
		}
		
		public static async Task StartDownLoadResources()
		{
			if (Define.IsAsync)
			{
				try
				{
					using (BundleDownloaderComponent bundleDownloaderComponent = Game.Entity.AddComponent<BundleDownloaderComponent>())
					{
						await bundleDownloaderComponent.StartAsync();
					}
					Game.ResourcesComponent.LoadOneBundle("StreamingAssets");
					ResourcesComponent.AssetBundleManifestObject = (AssetBundleManifest)Game.ResourcesComponent.GetAsset("StreamingAssets", "AssetBundleManifest");
				}
				catch (Exception e)
				{
					Log.Error(e);
				}

			}
		}
	}
}
