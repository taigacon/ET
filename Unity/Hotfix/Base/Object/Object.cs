namespace ETHotfix
{
	public interface IDisposable
	{
		void Dispose();
	}

	public interface IComponentSerialize
	{
		// 序列化之前调用
		void BeginSerialize();
		// 反序列化之后调用
		void EndDeSerialize();
	}

	public abstract class Object: ISupportInitialize
	{

		public override string ToString()
		{
			return JsonHelper.ToJson(this);
		}
	}
}