using MathCommon;
using System;
namespace NavigationLibrary
{
	internal class ToolInfo : IToolInfo, IOptoToolBaseInfo
	{
		private MarkerInfo _optoMarkerInfo;
		private MarkerInfo _electroMarkerInfo;
		public int OptoIndex
		{
			get;
			private set;
		}
		public int ElectroIndex
		{
			get;
			private set;
		}
		public TMatrix OptoToolOffsetMatrix
		{
			get;
			private set;
		}
		public TMatrix ElectroToolOffsetMatrix
		{
			get;
			private set;
		}
		public ToolInfoMarkerSelectioKinds OptoSelectionKind
		{
			get;
			private set;
		}
		public string OptoWirelessMarkerName
		{
			get;
			set;
		}
		public TMatrix TransformationMatrix
		{
			get;
			private set;
		}
		public double Rms
		{
			get;
			set;
		}
		public DeviceMarkerStatus Status
		{
			get;
			set;
		}
        public bool Clicked
        {
            get;
            set;
        }
		public MarkerInfo OptoMarkerInfo
		{
			get
			{
				return this._optoMarkerInfo;
			}
			set
			{
				if (value == null)
				{
					this._optoMarkerInfo = null;
				}
				else if (this._optoMarkerInfo == null)
				{
					this._optoMarkerInfo = new MarkerInfo(value);
				}
				else
				{
					this._optoMarkerInfo.Update(value);
				}
			}
		}
		public MarkerInfo ElectroMarkerInfo
		{
			get
			{
				return this._electroMarkerInfo;
			}
		}
		public void ResetState()
		{
			this.TransformationMatrix.LoadIdentity();
			this.Status = DeviceMarkerStatus.Missing;
			this.Rms = double.NaN;
		}
		public ToolInfo(ToolInfoMarkerSelectioKinds selectionKind, int optoIndex, string optoWirelessName, TMatrix optoOffsetMatrix, int electroIndex, TMatrix electroOffsetMatrix)
		{
			this.OptoWirelessMarkerName = optoWirelessName;
            this.ElectroIndex = electroIndex;            
			this.OptoIndex = optoIndex;
			this.TransformationMatrix = new TMatrix();
			if (optoOffsetMatrix != null)
			{
				this.OptoToolOffsetMatrix = optoOffsetMatrix;
			}
			else
			{
				this.OptoToolOffsetMatrix = new TMatrix();
			}
			if (electroOffsetMatrix != null)
			{
				this.ElectroToolOffsetMatrix = electroOffsetMatrix;
			}
			else
			{
				this.ElectroToolOffsetMatrix = new TMatrix();
			}
			this.TransformationMatrix.LoadIdentity();
			this.Rms = double.NaN;
			this.Status = DeviceMarkerStatus.Missing;
		}
		public ToolInfo(ToolInfo tInfo)
		{
			if (tInfo == null)
			{
				throw new ArgumentNullException("tInfo");
			}
            this.Clicked = tInfo.Clicked;
			this.OptoWirelessMarkerName = tInfo.OptoWirelessMarkerName;
			this.ElectroIndex = tInfo.ElectroIndex;
			this.OptoIndex = tInfo.OptoIndex;
			this.TransformationMatrix = new TMatrix();
			this.TransformationMatrix.SetMatrix(tInfo.TransformationMatrix);
			this.OptoToolOffsetMatrix.SetMatrix(tInfo.OptoToolOffsetMatrix);
			this.ElectroToolOffsetMatrix.SetMatrix(tInfo.ElectroToolOffsetMatrix);
			this.Rms = tInfo.Rms;
			this.Status = tInfo.Status;
		}
		public void SetOptoToolOffsetMatrix(TMatrix mat)
		{
			this.OptoToolOffsetMatrix = mat;
		}
	}
}
