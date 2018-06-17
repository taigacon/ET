﻿using System;

namespace BK
{
	[AttributeUsage(AttributeTargets.Field)]
	public class NodeFieldAttribute: NodeFieldBaseAttribute
	{
		public NodeFieldAttribute(string desc = "", object value = null): base(desc, value)
		{
		}
	}
}