using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace BK
{
	[ObjectSystem]
	public class PanelManagerAwakeSystem : AwakeSystem<PanelManager>
	{
		public override void Awake(PanelManager self)
		{
			self.Awake();
		}
	}

	[ObjectSystem]
	public class PanelManagerLoadSystem : LoadSystem<PanelManager>
	{
		public override void Load(PanelManager self)
		{
			self.Load();
		}
	}

	/// <summary>
	/// 管理所有UI
	/// </summary>
	public class PanelManager: Component
	{
		private const int DYNAMIC_LIMIT = 5;
		private GameObject Root;
		private readonly Dictionary<PanelId, Panel> panelCache = new Dictionary<PanelId, Panel>();
		private readonly Dictionary<PanelId, PanelConfig> panelConfigs = new Dictionary<PanelId, PanelConfig>();
		private readonly Dictionary<PanelId, Type> panelTypes = new Dictionary<PanelId, Type>();
		private readonly List<PanelId> isLoading = new List<PanelId>();
		private readonly List<PanelId> dynamicPanelIds = new List<PanelId>(); //LRU

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}
			
			foreach (var pair in this.panelCache)
			{
				pair.Value.Dispose();
			}
			this.panelCache.Clear();
			base.Dispose();
		}

		public void Awake()
		{
			this.Root = GameObject.Find("Global/UI/");
			this.Load();
		}

		public void Load()
		{
			List<Type> types = Game.EventSystem.GetTypes();

			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof (PanelConfigAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}

				PanelConfigAttribute attribute = attrs[0] as PanelConfigAttribute;
				var panelId = attribute.PanelConfig.PanelId;
				if (this.panelConfigs.ContainsKey(panelId))
				{
					throw new Exception($"已经存在同个PanelId: {panelId}");
				}
				this.panelConfigs.Add(panelId, attribute.PanelConfig);
				this.panelTypes.Add(panelId, type);
			}
		}

		public async Task Load(PanelId panelId, bool showWhenDone = true, object param = null)
		{
			try
			{
				Panel panel;
				if (this.panelCache.TryGetValue(panelId, out panel))
				{
					if (panel.PanelLifespan == PanelLifespan.Dynamic)
					{
						this.dynamicPanelIds.Remove(panelId);
					}
					if(!panel.IsShow && showWhenDone)
						panel.OnShow(param);
					return;
				}

				if (this.isLoading.Contains(panelId))
				{
					Debug.LogWarning($"Panel {panelId} is already loading.");
					return;
				}
				this.isLoading.Add(panelId);
				Type type = this.panelTypes[panelId];
				PanelConfig config = this.panelConfigs[panelId];
				await Game.ResourcesComponent.LoadBundleAsync($"{panelId}");
				GameObject go = GameObject.Instantiate((GameObject)Game.ResourcesComponent.GetAsset($"{panelId}", $"{panelId}.prefab"));
				panel = (Panel)ObjectFactory.CreateEntity(type);
				panel.Awake(go, config);
				this.isLoading.Remove(panelId);
				this.panelCache.Add(panelId, panel);
				if(showWhenDone)
					panel.OnShow(param);
				return;
			}
			catch (Exception e)
			{
				throw new Exception($"{panelId} UI 错误: {e}");
			}
		}

		private void Destroy(PanelId panelId)
		{
			Panel needDestroyPanel = this.panelCache[panelId];
			this.panelCache.Remove(panelId);
			needDestroyPanel.Dispose();
		}

		public void Open(PanelId panelId, object param = null)
		{
			this.Load(panelId, true, param).WrapErrors();
		}

		public void Close(PanelId panelId, object param = null)
		{
			Panel panel;
			if (this.panelCache.TryGetValue(panelId, out panel))
			{
				if (panel.IsShow)
				{
					panel.OnClose(param);
					if (panel.PanelLifespan == PanelLifespan.Dynamic)
					{
						this.dynamicPanelIds.Add(panelId);
						if (this.dynamicPanelIds.Count > DYNAMIC_LIMIT)
						{
							PanelId needDestroyId = this.dynamicPanelIds[0];
							this.dynamicPanelIds.RemoveAt(0);
							Destroy(needDestroyId);
						}
					}
				}
				return;
			}
		}
	}
}