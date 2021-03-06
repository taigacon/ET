﻿using BK;

namespace BKEditor
{
	[Event(EventIdType.BehaviorTreeRightDesignerDrag)]
	public class BehaviorTreeRightDesignerDragEvent_ModifyRightBorder: AEvent<float>
	{
		public override void Run(float deltaX)
		{
			BTEditorWindow.Instance.onDraggingRightDesigner(deltaX);
		}
	}
}