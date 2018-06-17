namespace BK
{
	public static class Game
	{
		public static GameEntity Entity { get; private set; }

		public static void Init()
		{
			Entity = new GameEntity();
			NetOuterComponent = Entity.AddComponent<NetOuterComponent>();
			ResourcesComponent = Entity.AddComponent<ResourcesComponent>();
			BehaviorTreeComponent = Entity.AddComponent<BehaviorTreeComponent>();
			ClientFrameComponent = Entity.AddComponent<ClientFrameComponent>();
			PanelManager = Entity.AddComponent<PanelManager>();
			OpcodeTypeComponent = Entity.AddComponent<OpcodeTypeComponent>();
			MessageDispatherComponent = Entity.AddComponent<MessageDispatherComponent>();
		}

		public static EventSystem EventSystem { get; } = new EventSystem();

		public static ObjectPool ObjectPool { get; } = new ObjectPool();

		public static Hotfix Hotfix { get; } = new Hotfix();

		#region Components
		
		public static NetOuterComponent NetOuterComponent { get; private set; }
		public static ResourcesComponent ResourcesComponent { get; private set; }
		public static BehaviorTreeComponent BehaviorTreeComponent { get; private set; }
		public static ClientFrameComponent ClientFrameComponent { get; private set; }
		public static PanelManager PanelManager { get; private set; }
		public static OpcodeTypeComponent OpcodeTypeComponent { get; private set; }
		public static MessageDispatherComponent MessageDispatherComponent { get; private set; }

		#endregion

		public static void Close()
		{
			Entity?.Dispose();
			Entity = null;
			EventSystem.Close();
			ObjectPool.Close();
			Hotfix.Close();
		}
	}
}