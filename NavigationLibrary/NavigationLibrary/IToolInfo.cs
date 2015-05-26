using MathCommon;
using System;
namespace NavigationLibrary
{
	public interface IToolInfo
	{
		TMatrix TransformationMatrix
		{
			get;
		}
		double Rms
		{
			get;
		}
		DeviceMarkerStatus Status
		{
			get;
		}
        int ElectroIndex
        {
            get;
        }
        bool Clicked
        {
            get;
        }
        
	}
}
