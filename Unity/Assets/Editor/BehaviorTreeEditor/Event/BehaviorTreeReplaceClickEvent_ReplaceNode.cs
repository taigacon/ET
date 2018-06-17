﻿using BK;
using UnityEngine;

namespace BKEditor
{
	[Event(EventIdType.BehaviorTreeReplaceClick)]
	public class BehaviorTreeReplaceClickEvent_ReplaceNode: AEvent<string, Vector2>
	{
		public override void Run(string name, Vector2 pos)
		{
			BTEditorWindow.Instance.onChangeNodeType(name, pos);
		}
	}
}