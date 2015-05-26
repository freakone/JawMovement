using MathCommon;
using System;
namespace NavigationLibrary
{
	public class MarkerInfo
	{
		public uint PortNumber
		{
			get;
			private set;
		}
		public TMatrix TransformationMatrix
		{
			get;
			private set;
		}
		public DeviceMarkerStatus Status
		{
			get;
			private set;
		}
		public double RmsErrorValue
		{
			get;
			private set;
		}
		public QuaternionContainer Quat
		{
			get;
			private set;
		}
		public string Name
		{
			get;
			private set;
		}

        public bool Clicked
        {
            get;
            private set;
        }

		public MarkerInfo()
		{
			this.TransformationMatrix = null;
			this.Quat = null;
			this.PortNumber = 0u;
			this.Status = DeviceMarkerStatus.Missing;
			this.RmsErrorValue = 0.0;
			this.Name = "";
		}
		public MarkerInfo(MarkerInfo mInfo)
		{
			if (mInfo == null || mInfo.Quat == null || mInfo.TransformationMatrix == null)
			{
				this.TransformationMatrix = null;
				this.PortNumber = 0u;
				this.Status = DeviceMarkerStatus.Missing;
				this.Quat = null;
				this.Name = "";
			}
			else
			{
				this.Name = mInfo.Name;
				double[] matrix = null;
				mInfo.TransformationMatrix.GetMatrix(ref matrix);
				this.TransformationMatrix = new TMatrix();
				this.TransformationMatrix.SetMatrix(matrix);
				this.PortNumber = mInfo.PortNumber;
				this.Status = mInfo.Status;
				this.RmsErrorValue = mInfo.RmsErrorValue;
				this.Quat = new QuaternionContainer
				{
					Q0 = mInfo.Quat.Q0,
					QX = mInfo.Quat.QX,
					QY = mInfo.Quat.QY,
					QZ = mInfo.Quat.QZ
				};
			}
		}
		public void Update(uint portNr, double transX, double transY, double transZ, double rotX, double rotY, double rotZ, double q0, double qx, double qy, double qz, bool clicked, DeviceMarkerStatus status, double rmsErrorValue, string name)
		{
			this.Name = name;
			this.PortNumber = portNr;
			this.Status = status;
			this.RmsErrorValue = rmsErrorValue;
            this.Clicked = clicked;
			if (this.TransformationMatrix == null)
			{
				this.TransformationMatrix = new TMatrix();
			}
			this.TransformationMatrix.LoadIdentity();
			if (!double.IsNaN(transX) && !double.IsNaN(transY) && !double.IsNaN(transZ) && !double.IsNaN(rotX) && !double.IsNaN(rotY) && !double.IsNaN(rotZ) && !double.IsNaN(q0) && !double.IsNaN(qx) && !double.IsNaN(qy) && !double.IsNaN(qz))
			{
				this.Quat = new QuaternionContainer
				{
					Q0 = q0,
					QX = qx,
					QY = qy,
					QZ = qz
				};
				this.TransformationMatrix.LoadIdentity();
				this.TransformationMatrix.Rotate(rotX, 1.0, 0.0, 0.0);
				this.TransformationMatrix.Rotate(rotY, 0.0, 1.0, 0.0);
				this.TransformationMatrix.Rotate(rotZ, 0.0, 0.0, 1.0);
				this.TransformationMatrix.Translate(transX, transY, transZ);
			}
		}
		public void Update(uint portNr, double transX, double transY, double transZ, double[,] rotMatrix, double q0, double qx, double qy, double qz, DeviceMarkerStatus status, double rmsErrorValue, string name)
		{
			this.Name = name;
			this.PortNumber = portNr;
			this.Status = status;
			this.RmsErrorValue = rmsErrorValue;
			if (this.TransformationMatrix == null)
			{
				this.TransformationMatrix = new TMatrix();
			}
			this.TransformationMatrix.LoadIdentity();
			if (!double.IsNaN(transX) && !double.IsNaN(transY) && !double.IsNaN(transZ))
			{
				this.Quat = new QuaternionContainer
				{
					Q0 = q0,
					QX = qx,
					QY = qy,
					QZ = qz
				};
				this.TransformationMatrix.Load(transX, transY, transZ, rotMatrix);
			}
		}
		public void Update(MarkerInfo markerInfo)
		{
			if (markerInfo == null)
			{
				throw new ArgumentNullException("markerInfo");
			}
			this.Name = markerInfo.Name;
			this.PortNumber = markerInfo.PortNumber;
			this.Status = markerInfo.Status;
			this.RmsErrorValue = markerInfo.RmsErrorValue;
			if (this.TransformationMatrix == null)
			{
				this.TransformationMatrix = new TMatrix(markerInfo.TransformationMatrix);
			}
			else
			{
				this.TransformationMatrix.SetMatrix(markerInfo.TransformationMatrix);
			}
			this.Quat = new QuaternionContainer
			{
				Q0 = markerInfo.Quat.Q0,
				QX = markerInfo.Quat.QX,
				QY = markerInfo.Quat.QY,
				QZ = markerInfo.Quat.QZ
			};
		}
	}
}
