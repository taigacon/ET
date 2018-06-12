using ETModel;

namespace ETHotfix
{
	public static class Game
	{
		public static GameEntity Entity { get; private set; }

		public static EventSystem EventSystem { get; } = new EventSystem();

		public static ObjectPool ObjectPool { get; } = new ObjectPool();

		public static Hotfix Hotfix { get; private set; }

		public static void Init()
		{
			Entity = new GameEntity();
			PanelManager = Entity.AddComponent<PanelManager>();
			OpcodeTypeComponent = Entity.AddComponent<OpcodeTypeComponent>();
			MessageDispatherComponent = Entity.AddComponent<MessageDispatherComponent>();
			Hotfix = ETModel.Game.Hotfix;
			ResourcesComponent = ETModel.Game.ResourcesComponent;
		}
		public static PanelManager PanelManager { get; private set; }
		public static OpcodeTypeComponent OpcodeTypeComponent { get; private set; }
		public static MessageDispatherComponent MessageDispatherComponent { get; private set; }
		public static ResourcesComponent ResourcesComponent { get; private set; }

		public static void Close()
		{
			Entity?.Dispose();
			Entity = null;
			EventSystem.Close();
			ObjectPool.Close();
		}
	}
}