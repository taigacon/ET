using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using LitJson;
using UnityEngine.Networking;

namespace BK
{
	[ObjectSystem]
	public class UiBundleDownloaderComponentAwakeSystem : AwakeSystem<BundleDownloaderComponent>
	{
		public override void Awake(BundleDownloaderComponent self)
		{
			self.bundles = new Queue<string>();
			self.downloadedBundles = new HashSet<string>();
			self.downloadingBundle = "";
		}
	}

	/// <summary>
	/// 用来对比web端的资源，比较md5，对比下载资源
	/// </summary>
	public class BundleDownloaderComponent : Component
	{
		public VersionConfig VersionConfig { get; private set; }

		public Queue<string> bundles;

		public long TotalSize;

		public HashSet<string> downloadedBundles;

		public string downloadingBundle;

		public TaskCompletionSource<bool> Tcs;

		public async Task StartAsync()
		{
			{
				string versionUrl = ""/*FIXME*/ + "StreamingAssets/" + "Version.txt";
				var request = await UnityWebRequest.Get(versionUrl).SendWebRequest();
				this.VersionConfig = JsonMapper.ToObject<VersionConfig>(request.downloadHandler.text);
			}

			VersionConfig localVersionConfig;
			// 对比本地的Version.txt
			string versionPath = Path.Combine(PathHelper.AppHotfixResPath, "Version.txt");
			if (File.Exists(versionPath))
			{
				localVersionConfig = JsonMapper.ToObject<VersionConfig>(File.ReadAllText(versionPath));
			}
			else
			{
				versionPath = Path.Combine(PathHelper.AppResPath4Web, "Version.txt");
				{
					var request = await UnityWebRequest.Get(versionPath).SendWebRequest();
					localVersionConfig = JsonMapper.ToObject<VersionConfig>(request.downloadHandler.text);
				}
			}


			// 先删除服务器端没有的ab
			foreach (FileVersionInfo fileVersionInfo in localVersionConfig.FileInfoDict.Values)
			{
				if (this.VersionConfig.FileInfoDict.ContainsKey(fileVersionInfo.File))
				{
					continue;
				}
				string abPath = Path.Combine(PathHelper.AppHotfixResPath, fileVersionInfo.File);
				File.Delete(abPath);
			}

			// 再下载
			foreach (FileVersionInfo fileVersionInfo in this.VersionConfig.FileInfoDict.Values)
			{
				FileVersionInfo localVersionInfo;
				if (localVersionConfig.FileInfoDict.TryGetValue(fileVersionInfo.File, out localVersionInfo))
				{
					if (fileVersionInfo.MD5 == localVersionInfo.MD5)
					{
						continue;
					}
				}

				if (fileVersionInfo.File == "Version.txt")
				{
					continue;
				}

				this.bundles.Enqueue(fileVersionInfo.File);
				this.TotalSize += fileVersionInfo.Size;
			}

			if (this.bundles.Count == 0)
			{
				return;
			}

			//Log.Debug($"need download bundles: {this.bundles.ToList().ListToString()}");
			await this.WaitAsync();
		}

		private async void UpdateAsync()
		{
			try
			{
				while (true)
				{
					if (this.bundles.Count == 0)
					{
						break;
					}

					this.downloadingBundle = this.bundles.Dequeue();

					while (true)
					{
						try
						{
							var request = await UnityWebRequest.Get(""/*FIXME*/ + "StreamingAssets/" + this.downloadingBundle).SendWebRequest();
							byte[] data = request.downloadHandler.data;

							string path = Path.Combine(PathHelper.AppHotfixResPath, this.downloadingBundle);
							if (!Directory.Exists(Path.GetDirectoryName(path)))
							{
								Directory.CreateDirectory(Path.GetDirectoryName(path));
							}
							using (FileStream fs = new FileStream(path, FileMode.Create))
							{
								fs.Write(data, 0, data.Length);
							}
						}
						catch (Exception e)
						{
							Log.Error($"download bundle error: {this.downloadingBundle}\n{e}");
							continue;
						}

						break;
					}
					this.downloadedBundles.Add(this.downloadingBundle);
					this.downloadingBundle = "";
				}

				using (FileStream fs = new FileStream(Path.Combine(PathHelper.AppHotfixResPath, "Version.txt"), FileMode.Create))
				using (StreamWriter sw = new StreamWriter(fs))
				{
					sw.Write(JsonMapper.ToJson(this.VersionConfig));
				}

				this.Tcs?.SetResult(true);
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}

		public int Progress
		{
			get
			{
				if (this.VersionConfig == null)
				{
					return 0;
				}

				if (this.TotalSize == 0)
				{
					return 0;
				}

				long alreadyDownloadBytes = 0;
				foreach (string downloadedBundle in this.downloadedBundles)
				{
					long size = this.VersionConfig.FileInfoDict[downloadedBundle].Size;
					alreadyDownloadBytes += size;
				}
				/*FIXME*/
				return (int)(alreadyDownloadBytes * 100f / this.TotalSize);
			}
		}

		private Task<bool> WaitAsync()
		{
			if (this.bundles.Count == 0 && this.downloadingBundle == "")
			{
				return Task.FromResult(true);
			}

			this.Tcs = new TaskCompletionSource<bool>();

			UpdateAsync();

			return this.Tcs.Task;
		}
	}
}
