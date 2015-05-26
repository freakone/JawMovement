using System;
namespace NavigationLibrary
{
	public class NavigationInitializationCompleteEventArgs : EventArgs
	{
		public NavigationErrorCodes ErrorCode;
		public NavigationInitializationCompleteEventArgs(NavigationErrorCodes errorCode)
		{
			this.ErrorCode = errorCode;
		}
	}
}
