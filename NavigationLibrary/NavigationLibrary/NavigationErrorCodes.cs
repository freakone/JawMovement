using System;
namespace NavigationLibrary
{
	public enum NavigationErrorCodes
	{
		Ok,
		OpenCommunicationFailed,
		CloseCommunicationFailed,
		DeviceInitFailed,
		DeviceActivateHandlesFailed,
		DeviceNotInitialized,
		DeviceStartTrackingFailed,
		DeviceStopTrackingFailed
	}
}
