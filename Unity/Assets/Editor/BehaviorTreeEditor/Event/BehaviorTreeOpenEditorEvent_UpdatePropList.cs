using BK;

namespace BKEditor
{
	[Event(EventIdType.BehaviorTreeOpenEditor)]
	public class BehaviorTreeOpenEditorEvent_UpdatePropList: AEvent
	{
		public override void Run()
		{
			BTEditorWindow.Instance.onUpdatePropList();
		}
	}
}