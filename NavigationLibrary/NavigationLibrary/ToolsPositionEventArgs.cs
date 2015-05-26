using System;
using System.Collections.Generic;
namespace NavigationLibrary
{
	public class ToolsPositionEventArgs : EventArgs
	{
		public List<IToolInfo> ToolInfoList
		{
			get;
			private set;
		}
		public ToolsPositionEventArgs(List<IToolInfo> list)
		{
			this.ToolInfoList = list;
		}
	}
}
