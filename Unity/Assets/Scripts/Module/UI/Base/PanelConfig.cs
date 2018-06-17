using System;

namespace BK
{
	public class PanelConfig
	{
		public PanelId PanelId { get; }
		public PanelType PanelType { get; }
		public Type BindViewType { get; }
		public PanelLifespan PanelLifespan { get; }

		public PanelConfig(PanelId panelId, PanelType panelType, Type bindViewType, PanelLifespan panelLifespan)
		{
			this.PanelId = panelId;
			this.PanelType = panelType;
			this.BindViewType = bindViewType;
			this.PanelLifespan = panelLifespan;
		}
	}
}