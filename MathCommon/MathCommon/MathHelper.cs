using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;

namespace MathCommon
{
	public class MathHelper
	{
		public static string CalcCrc16(string command)
		{
			char[] array = command.ToCharArray();
			uint num = 0u;
			uint[] array2 = new uint[256];
			for (int i = 0; i < 256; i++)
			{
				long num2 = (long)i;
				for (int j = 0; j < 8; j++)
				{
					num2 = (num2 >> 1 ^ (((num2 & 1L) > 0L) ? 40961L : 0L));
				}
				array2[i] = ((uint)num2 & 65535u);
			}
			for (int k = 0; k < command.Length; k++)
			{
				num = ((array2[(int)((UIntPtr)((num ^ (uint)array[k]) & 255u))] ^ num >> 8) & 65535u);
			}
			string text = num.ToString("X");
			string result;
			switch (text.Length)
			{
			case 1:
				result = "000" + text;
				break;
			case 2:
				result = "00" + text;
				break;
			case 3:
				result = "0" + text;
				break;
			default:
				result = text;
				break;
			}
			return result;
		}
		public static void QuaternionToEuler(double q0, double qx, double qy, double qz, out double x, out double y, out double z)
		{
			double[,] dtRotMatrix = new double[3, 3];
			double[] pdtQuatRot = new double[]
			{
				q0,
				qx,
				qy,
				qz
			};
			MathHelper.QuaternionToRotationMatrix(pdtQuatRot, ref dtRotMatrix);
			MathHelper.DetermineEuler(dtRotMatrix, out x, out y, out z);
			x *= 57.295779513082323;
			y *= 57.295779513082323;
			z *= 57.295779513082323;
		}
		public static void QuaternionToRotationMatrix(double[] pdtQuatRot, ref double[,] dtRotMatrix)
		{
			double num = pdtQuatRot[0] * pdtQuatRot[0];
			double num2 = pdtQuatRot[1] * pdtQuatRot[1];
			double num3 = pdtQuatRot[2] * pdtQuatRot[2];
			double num4 = pdtQuatRot[3] * pdtQuatRot[3];
			double num5 = pdtQuatRot[0] * pdtQuatRot[1];
			double num6 = pdtQuatRot[0] * pdtQuatRot[2];
			double num7 = pdtQuatRot[0] * pdtQuatRot[3];
			double num8 = pdtQuatRot[1] * pdtQuatRot[2];
			double num9 = pdtQuatRot[1] * pdtQuatRot[3];
			double num10 = pdtQuatRot[2] * pdtQuatRot[3];
			dtRotMatrix[0, 0] = num + num2 - num3 - num4;
			dtRotMatrix[0, 1] = 2.0 * (-num7 + num8);
			dtRotMatrix[0, 2] = 2.0 * (num6 + num9);
			dtRotMatrix[1, 0] = 2.0 * (num7 + num8);
			dtRotMatrix[1, 1] = num - num2 + num3 - num4;
			dtRotMatrix[1, 2] = 2.0 * (-num5 + num10);
			dtRotMatrix[2, 0] = 2.0 * (-num6 + num9);
			dtRotMatrix[2, 1] = 2.0 * (num5 + num10);
			dtRotMatrix[2, 2] = num - num2 - num3 + num4;
		}
		public static void DetermineEuler(double[,] dtRotMatrix, out double x, out double y, out double z)
		{
			double num = Math.Atan2(dtRotMatrix[1, 0], dtRotMatrix[0, 0]);
			double num2 = Math.Cos(num);
			double num3 = Math.Sin(num);
			z = num;
			y = Math.Atan2(-dtRotMatrix[2, 0], num2 * dtRotMatrix[0, 0] + num3 * dtRotMatrix[1, 0]);
			x = Math.Atan2(num3 * dtRotMatrix[0, 2] - num2 * dtRotMatrix[1, 2], -num3 * dtRotMatrix[0, 1] + num2 * dtRotMatrix[1, 1]);
		}
		public static void QuatRotatePoint(double[] RotationQuaternionPtr, double[] OriginalPositionPtr, ref double[] RotatedPositionPtr)
		{
			double[] array = new double[]
			{
				RotationQuaternionPtr[2] * OriginalPositionPtr[2] - RotationQuaternionPtr[3] * OriginalPositionPtr[1],
				RotationQuaternionPtr[3] * OriginalPositionPtr[0] - RotationQuaternionPtr[1] * OriginalPositionPtr[2],
				RotationQuaternionPtr[1] * OriginalPositionPtr[1] - RotationQuaternionPtr[2] * OriginalPositionPtr[0]
			};
			RotatedPositionPtr[0] = OriginalPositionPtr[0] + 2.0 * (RotationQuaternionPtr[0] * array[0] + RotationQuaternionPtr[2] * array[2] - RotationQuaternionPtr[3] * array[1]);
			RotatedPositionPtr[1] = OriginalPositionPtr[1] + 2.0 * (RotationQuaternionPtr[0] * array[1] + RotationQuaternionPtr[3] * array[0] - RotationQuaternionPtr[1] * array[2]);
			RotatedPositionPtr[2] = OriginalPositionPtr[2] + 2.0 * (RotationQuaternionPtr[0] * array[2] + RotationQuaternionPtr[1] * array[1] - RotationQuaternionPtr[2] * array[0]);
		}
		public static TMatrix GenerateTransforMatrix(double tX, double tY, double tZ, double q0, double qX, double qY, double qZ)
		{
			double angle;
			double angle2;
			double angle3;
			MathHelper.QuaternionToEuler(q0, qX, qY, qZ, out angle, out angle2, out angle3);
			TMatrix tMatrix = new TMatrix();
			tMatrix.Rotate(angle, 1.0, 0.0, 0.0);
			tMatrix.Rotate(angle2, 0.0, 1.0, 0.0);
			tMatrix.Rotate(angle3, 0.0, 0.0, 1.0);
			tMatrix.Translate(tX, tY, tZ);
			return tMatrix;
		}
		public static double Rad2Deg(double rad)
		{
			return rad * 180.0 / 3.1415926535897931;
		}
		public static double Deg2Rad(double deg)
		{
			return deg * 3.1415926535897931 / 180.0;
		}
		public static void Radial2Cartesian(double radius, double angle, out double x, out double y)
		{
			x = radius * Math.Cos(MathHelper.Deg2Rad(angle));
			y = radius * Math.Sin(MathHelper.Deg2Rad(angle));
		}
		public static void Cartesian2Radial(double x, double y, out double radius, out double angle)
		{
			radius = Math.Sqrt(x * x + y * y);
			if (Math.Abs(x) <= 4.94065645841247E-324)
			{
				if (y >= 0.0)
				{
					angle = 90.0;
				}
				else
				{
					angle = 270.0;
				}
			}
			else
			{
				angle = MathHelper.Rad2Deg(Math.Atan2(y, x));
			}
		}
		public static bool CalculateMatrix(double[][] p1, double[][] p2, double[] t, ref TMatrix matrix)
		{
			bool result;
			try
			{
				if (matrix == null)
				{
					matrix = new TMatrix();
				}
				double[] array = new double[3];
				MathHelper.CalculateNormal(p1, ref array);
				double[] array2 = new double[3];
				MathHelper.CalculateNormal(p2, ref array2);
				double[] array3 = new double[3];
				MathHelper.CrossProduct(array, array2, ref array3);
				double[] array4 = new double[16];
				array4[0] = array2[0];
				array4[4] = array2[1];
				array4[8] = array2[2];
				array4[1] = array3[0];
				array4[5] = array3[1];
				array4[9] = array3[2];
				array4[2] = array[0];
				array4[6] = array[1];
				array4[10] = array[2];
				double[] array5 = new double[3];
				double[] array6 = array5;
				for (int i = 0; i < 3; i++)
				{
					array6[0] += t[i] * array2[i];
					array6[1] += t[i] * array3[i];
					array6[2] += t[i] * array[i];
				}
				array4[12] = -array6[0];
				array4[13] = -array6[1];
				array4[14] = -array6[2];
				array4[15] = 1.0;
				matrix.SetMatrix(array4);
				matrix.Invert();
				result = true;
			}
			catch
			{
				result = false;
			}
			return result;
		}
		public static void Normalize(ref double[] v)
		{
			double num = 0.0;
			for (int i = 0; i < 3; i++)
			{
				num += v[i] * v[i];
			}
			num = Math.Sqrt(num);
			if (num < 4.94065645841247E-324)
			{
				num = 1.0;
			}
			for (int i = 0; i < 3; i++)
			{
				v[i] /= num;
			}
		}
		public static double DotProduct(double[] v1, double[] v2)
		{
			double num = 0.0;
			double num2 = 0.0;
			double num3 = 0.0;
			for (int i = 0; i < 3; i++)
			{
				num += v1[i] * v2[i];
				num2 += v1[i] * v1[i];
				num3 += v2[i] * v2[i];
			}
			num2 = Math.Sqrt(num2);
			num3 = Math.Sqrt(num3);
			double result;
			if (Math.Abs(num2 - 0.0) < 1.4012984643248171E-45 || Math.Abs(num3) < 1.4012984643248171E-45)
			{
				result = 0.0;
			}
			else
			{
				result = Math.Acos(num / (num2 * num3));
			}
			return result;
		}
		public static void CrossProduct(double[] v1, double[] v2, ref double[] result)
		{
			result[0] = v1[1] * v2[2] - v1[2] * v2[1];
			result[1] = v1[2] * v2[0] - v1[0] * v2[2];
			result[2] = v1[0] * v2[1] - v1[1] * v2[0];
			MathHelper.Normalize(ref result);
		}
		public static void CalculateNormal(double[][] p, ref double[] res)
		{
			if (res == null)
			{
				res = new double[3];
			}
			double[] array = new double[3];
			double[] array2 = new double[3];
			for (int i = 0; i < 3; i++)
			{
				array[i] = p[0][i] - p[1][i];
				array2[i] = p[2][i] - p[1][i];
			}
			MathHelper.CrossProduct(array2, array, ref res);
		}
		public static void ApplyMatchingMatrix(TMatrix inMatrix, TMatrix matchingMatrix, out TMatrix outMatrix)
		{
			if (inMatrix == null || matchingMatrix == null)
			{
				outMatrix = null;
			}
			else
			{
				try
				{
					outMatrix = new TMatrix(inMatrix);
					outMatrix.Multiply(matchingMatrix);
				}
				catch
				{
					outMatrix = null;
				}
			}
		}

        public static double CalcDistanceBetweenPoints(Point3D matrix1, Point3D matrix2)
        {
            double deltax = matrix2.X - matrix1.X;
            double deltay = matrix2.Y - matrix1.Y;
            double deltaz = matrix2.Z - matrix1.Z;
            double distance = (float)Math.Sqrt(
                (deltax * deltax) +
                (deltay * deltay) +
                (deltaz * deltaz));
            return distance;
        }

		public static double CalcDistanceBetweenPoints(TMatrix matrix1, TMatrix matrix2)
		{
            double deltax = matrix2.X - matrix1.X;
            double deltay = matrix2.Y - matrix1.Y;
            double deltaz = matrix2.Z - matrix1.Z;
            double distance = (float) Math.Sqrt(
                (deltax * deltax) +
                (deltay * deltay) +
                (deltaz * deltaz));
            return distance;
		}
		public static double CalcSqrDistanceOfPoints(double x1, double y1, double z1, double x2, double y2, double z2)
		{
			return (x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1) + (z2 - z1) * (z2 - z1);
		}
		public static TMatrix CalculateOutputMatrixFromList(List<TMatrix> list, double deviationThreshold, out bool showWarning)
		{
			showWarning = false;
			TMatrix result;
			if (list == null || list.Count == 0)
			{
				result = null;
			}
			else if (list.Count == 1)
			{
				result = list[0];
			}
			else
			{
				double sumX = 0.0;
				double sumY = 0.0;
				double sumZ = 0.0;
				foreach (TMatrix current in list)
				{
					sumX += current.X;
					sumY += current.Y;
					sumZ += current.Z;
				}
				sumX /= (double)list.Count;
				sumY /= (double)list.Count;
				sumZ /= (double)list.Count;
				List<double> list2 = (
					from t in list
					select MathHelper.CalcSqrDistanceOfPoints(sumX, sumY, sumZ, t.X, t.Y, t.Z)).ToList<double>();
				list2.Sort();
				double averageDistanceToCenterPoint = list2.Average();
				double num = list2.Sum((double d) => Math.Pow(d - averageDistanceToCenterPoint, 2.0));
				num = Math.Sqrt(num);
				Logger.Instance.log(string.Format("Variance for measured point matching is {0:0.0000}", num), string.Empty, "INFO");
				if (num > deviationThreshold)
				{
					showWarning = true;
				}
				int num2 = list2.Count / 4;
				int index = num2 * 3;
				double num3 = list2[num2];
				double num4 = list2[index];
				double num5 = 1.2 * (num4 - num3);
				double num6 = num3 - num5;
				double num7 = num4 + num5;
				double num8 = 0.0;
				double num9 = 0.0;
				double num10 = 0.0;
				int num11 = 0;
				for (int i = 0; i < list.Count; i++)
				{
					if (list2[i] >= num6 && list2[i] <= num7)
					{
						num8 += list[i].X;
						num9 += list[i].Y;
						num10 += list[i].Z;
						num11++;
					}
				}
				TMatrix tMatrix = new TMatrix();
				if (num11 > 0)
				{
					double[] array = new double[16];
					list[list.Count / 2].GetMatrix(ref array);
					num8 /= (double)num11;
					num9 /= (double)num11;
					num10 /= (double)num11;
					array[12] = num8;
					array[13] = num9;
					array[14] = num10;
					tMatrix.SetMatrix(array);
					result = tMatrix;
				}
				else
				{
					result = null;
				}
			}
			return result;
		}
	}
}
