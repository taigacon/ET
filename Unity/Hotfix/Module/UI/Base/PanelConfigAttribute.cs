using System;

namespace ETHotfix
{
	public class PanelConfigAttribute : Attribute
	{
		public PanelId PanelId { get; }
		public PanelType PanelType { get; }
		public Type BindViewType { get; }
		public PanelLifespan PanelLifespan { get; }

		PanelConfigAttribute(PanelId panelId, PanelType panelType, Type bindViewType, PanelLifespan panelLifespan = PanelLifespan.Dynamic)
		{
			this.PanelId = panelId;
			this.PanelType = panelType;
			this.BindViewType = bindViewType;
			this.PanelLifespan = panelLifespan;
		}
	}
}