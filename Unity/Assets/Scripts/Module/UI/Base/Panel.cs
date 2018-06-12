using System.Collections.Generic;
using ETModel.UIBind;
using UnityEngine;

namespace ETModel
{
	[ObjectSystem]
	public class PanelAwakeSystem : AwakeSystem<Panel, GameObject, PanelConfigAttribute>
	{
		public override void Awake(Panel self, GameObject gameObject, PanelConfigAttribute config)
		{
			self.Awake(gameObject, config);
		}
	}

	public abstract class Panel : Entity
	{
		public string Name => this.GameObject.name;

		public GameObject GameObject { get; private set; }
		public UIBindRoot BindRoot { get; private set; }
		public PanelId PanelId { get; private set; }
		public PanelType PanelType { get; private set; }
		public PanelLifespan PanelLifespan { get; private set; }

		public bool IsShow { get; private set; } = false;

		public void Awake(GameObject gameObject, PanelConfigAttribute config)
		{
			this.GameObject = gameObject;
			this.PanelId = config.PanelId;
			this.PanelType = config.PanelType;
			this.PanelLifespan = config.PanelLifespan;
			gameObject.SetActive(false);
		}

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			this.OnDestroy();
			this.BindRoot.Dispose();
			base.Dispose();
			
			UnityEngine.Object.Destroy(GameObject);
			this.Parent = null;
		}

		public virtual void OnShow(object param)
		{
			IsShow = true;
		}

		public virtual void OnClose(object param)
		{
			IsShow = false;
		}

		protected virtual void OnDestroy()
		{

		}
	}
}