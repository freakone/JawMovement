using System;
namespace NavigationLibrary
{
	public class NavigationHelper
	{
		public static string Uint2StrHex(uint value, int length)
		{
			string text = value.ToString("X");
			string str = "";
			for (int i = 0; i < length - text.Length; i++)
			{
				str += "0";
			}
			return str + text;
		}
		public static string ToolInfoPositionToStr(IToolInfo info, int precision)
		{
			string result;
			if (info == null || info.Status != DeviceMarkerStatus.Ok)
			{
				result = "---";
			}
			else
			{
				result = string.Concat(new string[]
				{
					" X: ",
					info.TransformationMatrix.X.ToString(),
					"\n Y: ",
					info.TransformationMatrix.Y.ToString(),
					"\n Z: ",
					info.TransformationMatrix.Z.ToString(),
					"\n RX: ",
					info.TransformationMatrix.OX_Deg.ToString(),
					"\n RY: ",
					info.TransformationMatrix.OY_Deg.ToString(),
					"\n RZ: ",
					info.TransformationMatrix.OZ_Deg.ToString()
				});
			}
			return result;
		}
	}
}
