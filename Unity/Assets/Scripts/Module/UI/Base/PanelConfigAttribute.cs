using System;

namespace BK
{
	public class PanelConfigAttribute : Attribute
	{
		public PanelConfig PanelConfig { get; }

		PanelConfigAttribute(PanelId panelId, PanelType panelType, Type bindViewType, PanelLifespan panelLifespan = PanelLifespan.Dynamic)
		{
			PanelConfig = new PanelConfig(panelId, panelType, bindViewType, panelLifespan);
		}
	}
}