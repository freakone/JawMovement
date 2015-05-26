using System;
using System.Collections.Generic;
namespace NavigationLibrary
{
	public class MarkersPositionEventArgs : EventArgs
	{
		public List<MarkerInfo> MarkerInfoList
		{
			get;
			private set;
		}
		public MarkersPositionEventArgs(List<MarkerInfo> list)
		{
			this.MarkerInfoList = list;
		}
	}
}
