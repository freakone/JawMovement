using System;
using System.Windows.Media.Media3D;
namespace MathCommon
{
	public class TMatrix
	{        
		private Matrix3D matrix = default(Matrix3D);
		public double X
		{
			get
			{
				bool flag = 1 == 0;
				return this.matrix.OffsetX;
			}
		}
		public double Y
		{
			get
			{
				bool flag = 1 == 0;
				return this.matrix.OffsetY;
			}
		}
		public double Z
		{
			get
			{
				bool flag = 1 == 0;
				return this.matrix.OffsetZ;
			}
		}

		public double OX
		{
			get
			{
				bool flag = 1 == 0;
				double[] array = new double[16];
				this.GetMatrix(ref array);
				double num = Math.Atan2(array[1], array[0]);
				double num2 = Math.Sin(num);
				double num3 = Math.Cos(num);
				return Math.Atan2(num2 * array[8] - num3 * array[9], -num2 * array[4] + num3 * array[5]);
			}
		}
		public double OY
		{
			get
			{
				bool flag = 1 == 0;
				double[] array = new double[16];
				this.GetMatrix(ref array);
				double num = Math.Atan2(array[1], array[0]);
				double num2 = Math.Cos(num);
				double num3 = Math.Sin(num);
				return Math.Atan2(-array[2], num2 * array[0] + num3 * array[1]);
			}
		}
		public double OZ
		{
			get
			{
				bool flag = 1 == 0;
				double[] array = new double[16];
				this.GetMatrix(ref array);
				return Math.Atan2(array[1], array[0]);
			}
		}
		public double OX_Deg
		{
			get
			{
				return double.IsNaN(this.OX) ? double.NaN : (this.OX * 57.295779513082323);
			}
		}
		public double OY_Deg
		{
			get
			{
				return double.IsNaN(this.OY) ? double.NaN : (this.OY * 57.295779513082323);
			}
		}
		public double OZ_Deg
		{
			get
			{
				return double.IsNaN(this.OZ) ? double.NaN : (this.OZ * 57.295779513082323);
			}
		}
		public double SX
		{
			get
			{
				bool flag = 1 == 0;
				return Math.Sqrt(Math.Pow(this.matrix.M11, 2.0) + Math.Pow(this.matrix.M12, 2.0) + Math.Pow(this.matrix.M13, 2.0));
			}
		}
		public double SY
		{
			get
			{
				bool flag = 1 == 0;
				return Math.Sqrt(Math.Pow(this.matrix.M21, 2.0) + Math.Pow(this.matrix.M22, 2.0) + Math.Pow(this.matrix.M23, 2.0));
			}
		}
		public double SZ
		{
			get
			{
				bool flag = 1 == 0;
				return Math.Sqrt(Math.Pow(this.matrix.M31, 2.0) + Math.Pow(this.matrix.M32, 2.0) + Math.Pow(this.matrix.M33, 2.0));
			}
		}
		public void SetMatrix(double[] m)
		{
			this.matrix.M11 = m[0];
			this.matrix.M12 = m[1];
			this.matrix.M13 = m[2];
			this.matrix.M14 = m[3];
			this.matrix.M21 = m[4];
			this.matrix.M22 = m[5];
			this.matrix.M23 = m[6];
			this.matrix.M24 = m[7];
			this.matrix.M31 = m[8];
			this.matrix.M32 = m[9];
			this.matrix.M33 = m[10];
			this.matrix.M34 = m[11];
			this.matrix.OffsetX = m[12];
			this.matrix.OffsetY = m[13];
			this.matrix.OffsetZ = m[14];
			this.matrix.M44 = m[15];
		}
		public void SetMatrix(TMatrix m)
		{
			if (m == null)
			{
				this.LoadIdentity();
			}
			else
			{
				double[] array = new double[16];
				m.GetMatrix(ref array);
				this.SetMatrix(array);
			}
		}
		public void GetMatrix(ref double[] m)
		{
			if (m == null || m.Length != 16)
			{
				m = new double[16];
			}
			m[0] = this.matrix.M11;
			m[1] = this.matrix.M12;
			m[2] = this.matrix.M13;
			m[3] = this.matrix.M14;
			m[4] = this.matrix.M21;
			m[5] = this.matrix.M22;
			m[6] = this.matrix.M23;
			m[7] = this.matrix.M24;
			m[8] = this.matrix.M31;
			m[9] = this.matrix.M32;
			m[10] = this.matrix.M33;
			m[11] = this.matrix.M34;
			m[12] = this.matrix.OffsetX;
			m[13] = this.matrix.OffsetY;
			m[14] = this.matrix.OffsetZ;
			m[15] = this.matrix.M44;
		}
		public void Translate(double x, double y, double z)
		{
			this.matrix.Translate(new Vector3D(x, y, z));
		}
		public void TranslatePrepend(double x, double y, double z)
		{
			this.matrix.TranslatePrepend(new Vector3D(x, y, z));
		}
		public void Rotate(double angle, double x, double y, double z)
		{
			this.matrix.Rotate(new Quaternion(new Vector3D(x, y, z), angle));
		}
		public void RotateAtPrepend(double angle, double x, double y, double z)
		{
			this.matrix.Rotate(new Quaternion(new Vector3D(x, y, z), angle));
		}
		public void RotateAroundPointAtPrepend(double angle, double x, double y, double z, double px, double py, double pz)
		{
			this.matrix.RotateAtPrepend(new Quaternion(new Vector3D(x, y, z), angle), new Point3D(px, py, pz));
		}
		public void RotateAroundPoint(double angle, double x, double y, double z, double px, double py, double pz)
		{
			this.matrix.Translate(new Vector3D(-px, -py, -pz));
			this.matrix.Rotate(new Quaternion(new Vector3D(x, y, z), angle));
			this.matrix.Translate(new Vector3D(px, py, pz));
		}
		public void RotateAroundAxis(double angle, double p1x, double p1y, double p1z, double p2x, double p2y, double p2z)
		{
			this.matrix.Translate(new Vector3D(-p1x, -p1y, -p1z));
			this.matrix.Rotate(new Quaternion(new Vector3D(p1x - p2x, p1y - p2y, p1z - p2z), angle));
			this.matrix.Translate(new Vector3D(p1x, p1y, p1z));
		}
		public void RotateAroundAxisAtPrepend(double angle, double p1x, double p1y, double p1z, double p2x, double p2y, double p2z)
		{
			this.matrix.RotateAtPrepend(new Quaternion(new Vector3D(p1x - p2x, p1y - p2y, p1z - p2z), angle), new Point3D(-p1x, -p1y, -p1z));
		}
		public void Scale(double x, double y, double z)
		{
			this.matrix.Scale(new Vector3D(x, y, z));
		}
		public void Multiply(double[] m)
		{
			Matrix3D matrix = default(Matrix3D);
			matrix.M11 = m[0];
			matrix.M12 = m[1];
			matrix.M13 = m[2];
			matrix.M14 = m[3];
			matrix.M21 = m[4];
			matrix.M22 = m[5];
			matrix.M23 = m[6];
			matrix.M24 = m[7];
			matrix.M31 = m[8];
			matrix.M32 = m[9];
			matrix.M33 = m[10];
			matrix.M34 = m[11];
			matrix.OffsetX = m[12];
			matrix.OffsetY = m[13];
			matrix.OffsetZ = m[14];
			matrix.M44 = m[15];
			this.matrix *= matrix;
		}
		public void Multiply(TMatrix m)
		{
			if (m == null)
			{
				this.LoadIdentity();
			}
			else
			{
				double[] m2 = new double[16];
				m.GetMatrix(ref m2);
				this.Multiply(m2);
			}
		}
		public void LoadIdentity()
		{
			this.matrix.SetIdentity();
		}
		public void Invert()
		{
			this.matrix.Invert();
		}
		public void ApplyReference(TMatrix reference)
		{
			TMatrix tMatrix = new TMatrix();
			double[] m = new double[16];
			reference.GetMatrix(ref m);
			tMatrix.SetMatrix(m);
			tMatrix.Invert();
			tMatrix.GetMatrix(ref m);
			this.Multiply(m);
		}
		public void Load(double tx, double ty, double tz, double[,] rotMatrix)
		{
			double[] array = new double[16];
			array[0] = rotMatrix[0, 0];
			array[4] = rotMatrix[0, 1];
			array[8] = rotMatrix[0, 2];
			array[1] = rotMatrix[1, 0];
			array[5] = rotMatrix[1, 1];
			array[9] = rotMatrix[1, 2];
			array[2] = rotMatrix[2, 0];
			array[6] = rotMatrix[2, 1];
			array[10] = rotMatrix[2, 2];
			array[12] = tx;
			array[13] = ty;
			array[14] = tz;
			array[3] = 0.0;
			array[7] = 0.0;
			array[11] = 0.0;
			array[15] = 1.0;
			this.SetMatrix(array);
		}
        public Point3D point3D()
        {
            return new Point3D(this.X, this.Y, this.Z);
        }

		public TMatrix()
		{
			this.matrix.SetIdentity();
		}
		public TMatrix(TMatrix mat)
		{
			if (mat == null)
			{
				this.matrix.SetIdentity();
			}
			else
			{
				this.SetMatrix(mat);
			}
		}
		public void MultiplyMatrixByVector(double[] vec, double[] result)
		{
			double[] array = new double[]
			{
				this.matrix.M11,
				this.matrix.M12,
				this.matrix.M13,
				this.matrix.M14,
				this.matrix.M21,
				this.matrix.M22,
				this.matrix.M23,
				this.matrix.M24,
				this.matrix.M31,
				this.matrix.M32,
				this.matrix.M33,
				this.matrix.M34,
				this.matrix.OffsetX,
				this.matrix.OffsetY,
				this.matrix.OffsetZ,
				this.matrix.M44
			};
			double[] array2 = new double[]
			{
				vec[0],
				vec[1],
				vec[2],
				1.0
			};
			double[] array3 = new double[4];
			double[] array4 = array3;
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					array4[i] += array[j * 4 + i] * array2[j];
				}
			}
			result[0] = array4[0];
			result[1] = array4[1];
			result[2] = array4[2];
		}
		public void DetermineEuler(out double x, out double y, out double z)
		{
			double num = 57.295779513082323;
			double num2 = Math.Atan2(this.matrix.M21, this.matrix.M11);
			double num3 = Math.Cos(num2);
			double num4 = Math.Sin(num2);
			z = num2 * num;
			y = Math.Atan2(-this.matrix.M31, num3 * this.matrix.M11 + num4 * this.matrix.M21) * num;
			x = Math.Atan2(num4 * this.matrix.M13 - num3 * this.matrix.M23, -num4 * this.matrix.M12 + num3 * this.matrix.M22) * num;
		}
		public static void GenerateIdentityMatrix(out double[] matrix)
		{
			Matrix3D matrix3D = default(Matrix3D);
			matrix3D.SetIdentity();
			matrix = new double[16];
			matrix[0] = matrix3D.M11;
			matrix[1] = matrix3D.M12;
			matrix[2] = matrix3D.M13;
			matrix[3] = matrix3D.M14;
			matrix[4] = matrix3D.M21;
			matrix[5] = matrix3D.M22;
			matrix[6] = matrix3D.M23;
			matrix[7] = matrix3D.M24;
			matrix[8] = matrix3D.M31;
			matrix[9] = matrix3D.M32;
			matrix[10] = matrix3D.M33;
			matrix[11] = matrix3D.M34;
			matrix[12] = matrix3D.OffsetX;
			matrix[13] = matrix3D.OffsetY;
			matrix[14] = matrix3D.OffsetZ;
			matrix[15] = matrix3D.M44;
		}
	}
}
