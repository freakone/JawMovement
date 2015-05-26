using MathCommon;
using System;
namespace NavigationLibrary
{
	public interface IOptoToolBaseInfo
	{
		MarkerInfo OptoMarkerInfo
		{
			get;
		}
		void SetOptoToolOffsetMatrix(TMatrix mat);
	}
}
