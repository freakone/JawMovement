using System;
namespace NavigationLibrary
{
	internal class WirelessMarkerInfo
	{
		public uint PortNumber
		{
			get;
			private set;
		}
		public string Name
		{
			get;
			private set;
		}
		public WirelessMarkerInfo(uint portNr, string name)
		{
			this.PortNumber = portNr;
			this.Name = name;
		}
	}
}
