using BK;

namespace BK
{
	[Event(EventIdType.BehaviorTreeCreateNode)]
	public class BehaviorTreeCreateNodeEvent_SelectNode: AEvent<NodeDesigner>
	{
		public override void Run(NodeDesigner dstNode)
		{
			BTEditorWindow.Instance.OnSelectNode(dstNode.NodeData, dstNode);
		}
	}
}