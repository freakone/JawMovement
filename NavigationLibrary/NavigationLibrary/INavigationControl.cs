using MathCommon;
using System;
namespace NavigationLibrary
{
	public interface INavigationControl
	{
		event Navigation.NavigationInitializationCompleteEventHandler NavigationInitializationComplete;
		event Navigation.ToolsPositionUpdateHandler ToolsPositionUpdate;
		NavigationSettings Settings
		{
			get;
		}
		NavigationErrorCodes Init(NavigationInitKinds initKind, NavigationInitDeviceConfigs navigationInitDeviceConfig);
		NavigationErrorCodes Play();
		NavigationErrorCodes Stop();
		NavigationErrorCodes Close();
		IToolInfo AddToolInfo(ToolInfoMarkerSelectioKinds selectionKind, int optoIndex, string optoWirelessName, TMatrix optoOffsetMatrix, int electroIndex, TMatrix electroOffsetMatrix);
		bool RemoveToolInfo(IToolInfo info);
		bool SetReferenceTool(IToolInfo info);
		bool UpdateToolInfoRomName(IToolInfo toolInfo, string romName);
	}
}
