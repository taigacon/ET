﻿using System;

namespace BK
{
	[AttributeUsage(AttributeTargets.Class)]
	public class NodeDeprecatedAttribute: Attribute
	{
		public string Desc { get; set; }

		public NodeDeprecatedAttribute(string desc = "")
		{
			Desc = desc;
		}
	}
}