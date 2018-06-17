namespace BKHotfix
{
	public enum PanelLifespan
	{
		Dynamic = 1, //动态加载卸载
		Scene = 2, //生命周期根据场景
		Static = 3, //不会销毁
	}
}