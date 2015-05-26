using MathCommon;
using System;
using System.Collections.Generic;
using System.Threading;
namespace NavigationLibrary
{
	public class Navigation : INavigationControl
	{
		private enum NavigationState
		{
			NotInitialized,
			Initialized,
			Started
		}
		public delegate void ToolsPositionUpdateHandler(object sender, ToolsPositionEventArgs e);
		public delegate void NavigationInitializationCompleteEventHandler(object sender, NavigationInitializationCompleteEventArgs args);
		private UpdateBackgroudWorker _deviceBackgroundWorker;
		private Device _optoDevice;
		private Device _electroDevice;
		private Navigation.NavigationState _deviceState;
		private Navigation.NavigationState _optoState;
		private Navigation.NavigationState _electroState;
		private NavigationSettings _settings;
		private List<IToolInfo> _toolList;
		private Thread _initializationThread;
		private int _referenceIndex;
		public event Navigation.ToolsPositionUpdateHandler ToolsPositionUpdate;
		public event Navigation.NavigationInitializationCompleteEventHandler NavigationInitializationComplete;
		public NavigationSettings Settings
		{
			get
			{
				return this._settings;
			}
		}
		private void OptoMarkersPositionChanged(object sender, MarkersPositionEventArgs e)
		{
			this.ProcessMarkers(e.MarkerInfoList, null);
			if (this.ToolsPositionUpdate != null)
			{
				this.ToolsPositionUpdate(this, new ToolsPositionEventArgs(this._toolList));
			}
		}
		private void ElectroMarkersPositionChanged(object sender, MarkersPositionEventArgs e)
		{
			this.ProcessMarkers(null, e.MarkerInfoList);
			if (this.ToolsPositionUpdate != null)
			{
				this.ToolsPositionUpdate(this, new ToolsPositionEventArgs(this._toolList));
			}
		}
		private int GetOptoMarkerIndex(IToolInfo info, List<MarkerInfo> optoMarkerList)
		{
			int result;
			switch (((ToolInfo)info).OptoSelectionKind)
			{
			case ToolInfoMarkerSelectioKinds.Name:
				for (int i = 0; i < optoMarkerList.Count; i++)
				{
					if (!string.IsNullOrEmpty(((ToolInfo)info).OptoWirelessMarkerName) && !string.IsNullOrEmpty(optoMarkerList[i].Name))
					{
						string text = ((ToolInfo)info).OptoWirelessMarkerName.ToUpper();
						string value = optoMarkerList[i].Name.ToUpper();
						if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(value) && text.Equals(value))
						{
							result = i;
							return result;
						}
					}
				}
				break;
			case ToolInfoMarkerSelectioKinds.Index:
				result = ((ToolInfo)info).OptoIndex;
				return result;
			}
			result = -1;
			return result;
		}
		private void ProcessMarkers(List<MarkerInfo> optoMarkerList, List<MarkerInfo> electroMarkerList)
		{
			if (this._referenceIndex >= 0 && this._referenceIndex < this._toolList.Count)
			{
                int optoMarkerIndex = 0;// this.GetOptoMarkerIndex(this._toolList[this._referenceIndex], optoMarkerList);
                bool flag = false;// optoMarkerIndex >= 0 && optoMarkerIndex < optoMarkerList.Count;
				bool flag2 = ((ToolInfo)this._toolList[this._referenceIndex]).ElectroIndex >= 0 && ((ToolInfo)this._toolList[this._referenceIndex]).ElectroIndex < electroMarkerList.Count;
				if (flag && flag2)
				{
					for (int i = 0; i < this._toolList.Count; i++)
					{
						((ToolInfo)this._toolList[i]).Rms = double.NaN;
						((ToolInfo)this._toolList[i]).TransformationMatrix.LoadIdentity();
						((ToolInfo)this._toolList[i]).Status = DeviceMarkerStatus.Missing;
					}
				}
				else if (flag)
				{
					if (optoMarkerList[optoMarkerIndex].Status == DeviceMarkerStatus.Ok)
					{
						TMatrix tMatrix = new TMatrix();
						double[] matrix = new double[16];
						optoMarkerList[optoMarkerIndex].TransformationMatrix.GetMatrix(ref matrix);
						tMatrix.SetMatrix(matrix);
						using (List<IToolInfo>.Enumerator enumerator = this._toolList.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								ToolInfo toolInfo = (ToolInfo)enumerator.Current;
								int optoMarkerIndex2 = this.GetOptoMarkerIndex(toolInfo, optoMarkerList);
								if (optoMarkerIndex2 < 0 || optoMarkerIndex2 >= optoMarkerList.Count || optoMarkerList[optoMarkerIndex2].Status != DeviceMarkerStatus.Ok)
								{
									toolInfo.ResetState();
								}
								else
								{
									MarkerInfo markerInfo = optoMarkerList[optoMarkerIndex2];
									toolInfo.OptoMarkerInfo = markerInfo;
									toolInfo.Status = markerInfo.Status;
                                    toolInfo.Clicked = markerInfo.Clicked;
									toolInfo.Rms = markerInfo.RmsErrorValue;
									toolInfo.TransformationMatrix.LoadIdentity();
									toolInfo.TransformationMatrix.Multiply(toolInfo.OptoToolOffsetMatrix);
									toolInfo.TransformationMatrix.Rotate(markerInfo.TransformationMatrix.OX_Deg, 1.0, 0.0, 0.0);
									toolInfo.TransformationMatrix.Rotate(markerInfo.TransformationMatrix.OY_Deg, 0.0, 1.0, 0.0);
									toolInfo.TransformationMatrix.Rotate(markerInfo.TransformationMatrix.OZ_Deg, 0.0, 0.0, 1.0);
									toolInfo.TransformationMatrix.Translate(markerInfo.TransformationMatrix.X, markerInfo.TransformationMatrix.Y, markerInfo.TransformationMatrix.Z);
									toolInfo.TransformationMatrix.ApplyReference(tMatrix);
								}
							}
						}
					}
					else
					{
						using (List<IToolInfo>.Enumerator enumerator = this._toolList.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								ToolInfo toolInfo = (ToolInfo)enumerator.Current;
								toolInfo.ResetState();
							}
						}
					}
				}
				else if (flag2)
				{
					if (electroMarkerList != null && electroMarkerList[((ToolInfo)this._toolList[this._referenceIndex]).ElectroIndex].Status == DeviceMarkerStatus.Ok)
					{
						TMatrix tMatrix = new TMatrix();
						double[] matrix = new double[16];
						electroMarkerList[((ToolInfo)this._toolList[this._referenceIndex]).ElectroIndex].TransformationMatrix.GetMatrix(ref matrix);
						tMatrix.SetMatrix(matrix);
						using (List<IToolInfo>.Enumerator enumerator = this._toolList.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
                                
								ToolInfo toolInfo = (ToolInfo)enumerator.Current;

                                if (toolInfo.ElectroIndex >= electroMarkerList.Count)
                                    continue;

                                toolInfo.Clicked = electroMarkerList[toolInfo.ElectroIndex].Clicked;
								toolInfo.Status = electroMarkerList[toolInfo.ElectroIndex].Status;
								toolInfo.Rms = electroMarkerList[toolInfo.ElectroIndex].RmsErrorValue;
								toolInfo.TransformationMatrix.SetMatrix(electroMarkerList[toolInfo.ElectroIndex].TransformationMatrix);
								toolInfo.TransformationMatrix.Multiply(toolInfo.ElectroToolOffsetMatrix);
								toolInfo.TransformationMatrix.ApplyReference(tMatrix);
							}
						}
					}
					else
					{
						using (List<IToolInfo>.Enumerator enumerator = this._toolList.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								ToolInfo toolInfo = (ToolInfo)enumerator.Current;
								toolInfo.ResetState();
							}
						}
					}
				}
				else
				{
					using (List<IToolInfo>.Enumerator enumerator = this._toolList.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							ToolInfo toolInfo = (ToolInfo)enumerator.Current;
							toolInfo.ResetState();
						}
					}
				}
			}
			else if (optoMarkerList != null && electroMarkerList != null)
			{
				int i = 0;
				while (i < this._toolList.Count)
				{
					ToolInfo toolInfo2 = (ToolInfo)this._toolList[i];
					if (toolInfo2.ElectroIndex >= 0 && toolInfo2.OptoIndex < 0)
					{
						if (toolInfo2.ElectroIndex < 0 || toolInfo2.ElectroIndex >= electroMarkerList.Count)
						{
							goto IL_6EF;
						}
						toolInfo2.Rms = electroMarkerList[toolInfo2.ElectroIndex].RmsErrorValue;
						toolInfo2.TransformationMatrix.SetMatrix(electroMarkerList[toolInfo2.ElectroIndex].TransformationMatrix);
						toolInfo2.Status = electroMarkerList[toolInfo2.ElectroIndex].Status;
                        toolInfo2.Clicked = electroMarkerList[toolInfo2.ElectroIndex].Clicked;
					}
					else
					{
						if (toolInfo2.OptoIndex < 0 || toolInfo2.ElectroIndex >= 0)
						{
							goto IL_6EF;
						}
						if (toolInfo2.OptoIndex < 0 || toolInfo2.OptoIndex >= optoMarkerList.Count)
						{
							goto IL_6EF;
						}
						toolInfo2.Rms = optoMarkerList[toolInfo2.OptoIndex].RmsErrorValue;
						toolInfo2.TransformationMatrix.SetMatrix(optoMarkerList[toolInfo2.OptoIndex].TransformationMatrix);
						toolInfo2.Status = optoMarkerList[toolInfo2.OptoIndex].Status;
					}
					IL_744:
					i++;
					continue;
					IL_6EF:
					((ToolInfo)this._toolList[i]).Rms = double.NaN;
					((ToolInfo)this._toolList[i]).TransformationMatrix.LoadIdentity();
					((ToolInfo)this._toolList[i]).Status = DeviceMarkerStatus.Missing;
					goto IL_744;
				}
			}
			else if (optoMarkerList != null)
			{
				for (int i = 0; i < this._toolList.Count; i++)
				{
					ToolInfo toolInfo3 = (ToolInfo)this._toolList[i];
					int optoMarkerIndex2 = this.GetOptoMarkerIndex(toolInfo3, optoMarkerList);
					if (optoMarkerIndex2 >= 0 && optoMarkerIndex2 < optoMarkerList.Count && optoMarkerList[optoMarkerIndex2].Status == DeviceMarkerStatus.Ok)
					{
						MarkerInfo markerInfo = optoMarkerList[optoMarkerIndex2];
						toolInfo3.Rms = markerInfo.RmsErrorValue;
						toolInfo3.Status = markerInfo.Status;
                        toolInfo3.Clicked = markerInfo.Clicked;
						toolInfo3.OptoMarkerInfo = markerInfo;
						TMatrix tMatrix2 = new TMatrix();
						tMatrix2.Translate(toolInfo3.OptoToolOffsetMatrix.X, toolInfo3.OptoToolOffsetMatrix.Y, toolInfo3.OptoToolOffsetMatrix.Z);
						tMatrix2.Rotate(markerInfo.TransformationMatrix.OX_Deg, 1.0, 0.0, 0.0);
						tMatrix2.Rotate(markerInfo.TransformationMatrix.OY_Deg, 0.0, 1.0, 0.0);
						tMatrix2.Rotate(markerInfo.TransformationMatrix.OZ_Deg, 0.0, 0.0, 1.0);
						tMatrix2.Translate(markerInfo.TransformationMatrix.X, markerInfo.TransformationMatrix.Y, markerInfo.TransformationMatrix.Z);
						toolInfo3.TransformationMatrix.SetMatrix(tMatrix2);
					}
					else
					{
						toolInfo3.ResetState();
					}
				}
			}
			else if (electroMarkerList != null)
			{
				for (int i = 0; i < this._toolList.Count; i++)
				{
					ToolInfo toolInfo3 = (ToolInfo)this._toolList[i];
					if (toolInfo3.ElectroIndex >= 0 && toolInfo3.ElectroIndex < electroMarkerList.Count)
					{
						toolInfo3.Rms = electroMarkerList[toolInfo3.ElectroIndex].RmsErrorValue;
						toolInfo3.TransformationMatrix.SetMatrix(electroMarkerList[toolInfo3.ElectroIndex].TransformationMatrix);
						toolInfo3.Status = electroMarkerList[toolInfo3.ElectroIndex].Status;
                        toolInfo3.Clicked = electroMarkerList[toolInfo3.ElectroIndex].Clicked;
					}
					else
					{
						toolInfo3.Rms = double.NaN;
						toolInfo3.TransformationMatrix.LoadIdentity();
						toolInfo3.Status = DeviceMarkerStatus.Missing;
					}
				}
			}
		}
		private void OnBackgroundWorkerUpdatePosition(object sender)
		{
			MarkersPositionEventArgs markersPositionEventArgs = null;
			MarkersPositionEventArgs markersPositionEventArgs2 = null;
			if (this._optoState == Navigation.NavigationState.Started && this._electroState == Navigation.NavigationState.Started)
			{
				this._optoDevice.DeviceUpdatePositions(out markersPositionEventArgs);
				this._electroDevice.DeviceUpdatePositions(out markersPositionEventArgs2);
				if (markersPositionEventArgs != null && markersPositionEventArgs2 != null)
				{
					this.ProcessMarkers(markersPositionEventArgs.MarkerInfoList, markersPositionEventArgs2.MarkerInfoList);
					if (this.ToolsPositionUpdate != null)
					{
						this.ToolsPositionUpdate(this, new ToolsPositionEventArgs(this._toolList));
					}
				}
				else if (markersPositionEventArgs != null)
				{
					this.ProcessMarkers(markersPositionEventArgs.MarkerInfoList, null);
					if (this.ToolsPositionUpdate != null)
					{
						this.ToolsPositionUpdate(this, new ToolsPositionEventArgs(this._toolList));
					}
				}
				else if (markersPositionEventArgs2 != null)
				{
					this.ProcessMarkers(null, markersPositionEventArgs2.MarkerInfoList);
					if (this.ToolsPositionUpdate != null)
					{
						this.ToolsPositionUpdate(this, new ToolsPositionEventArgs(this._toolList));
					}
				}
			}
		}
		private bool CheckNewToolAvailable(int optoIndex, int electroIndex)
		{
			bool result;
			if (optoIndex < 0 && electroIndex < 0)
			{
				result = false;
			}
			else if (optoIndex < 0)
			{
				for (int i = 0; i < this._toolList.Count; i++)
				{
					if (((ToolInfo)this._toolList[i]).ElectroIndex == electroIndex)
					{
						result = false;
						return result;
					}
				}
				result = true;
			}
			else if (electroIndex < 0)
			{
				for (int i = 0; i < this._toolList.Count; i++)
				{
					if (((ToolInfo)this._toolList[i]).OptoIndex == optoIndex)
					{
						result = false;
						return result;
					}
				}
				result = true;
			}
			else
			{
				for (int i = 0; i < this._toolList.Count; i++)
				{
					if (((ToolInfo)this._toolList[i]).OptoIndex == optoIndex || ((ToolInfo)this._toolList[i]).ElectroIndex == electroIndex)
					{
						result = false;
						return result;
					}
				}
				result = true;
			}
			return result;
		}
		private int IndexOfTool(int optoIndex, int electroIndex)
		{
			int result;
			for (int i = 0; i < this._toolList.Count; i++)
			{
				if (((ToolInfo)this._toolList[i]).OptoIndex == optoIndex && ((ToolInfo)this._toolList[i]).ElectroIndex == electroIndex)
				{
					result = i;
					return result;
				}
			}
			result = -1;
			return result;
		}
		private NavigationErrorCodes InnerInit(NavigationInitDeviceConfigs navigationInitDeviceConfig)
		{
			NavigationErrorCodes result;
			if (this._deviceState != Navigation.NavigationState.NotInitialized)
			{
				result = NavigationErrorCodes.Ok;
			}
			else
			{
				bool flag = false;
				bool flag2 = false;
				switch (navigationInitDeviceConfig)
				{
				case NavigationInitDeviceConfigs.Opto:
					flag = this._optoDevice.DeviceOpenConnection();
					break;
				case NavigationInitDeviceConfigs.Electro:
					flag2 = this._electroDevice.DeviceOpenConnection();
					break;
				case NavigationInitDeviceConfigs.Both:
					flag = this._optoDevice.DeviceOpenConnection();
					flag2 = this._electroDevice.DeviceOpenConnection();
					break;
				}
				if (!flag && !flag2)
				{
					result = NavigationErrorCodes.OpenCommunicationFailed;
				}
				else
				{
					if (flag)
					{
						flag = this._optoDevice.DeviceInit();
					}
					if (flag2)
					{
						flag2 = this._electroDevice.DeviceInit();
                        this._electroDevice.DeviceActivateHandles();
					}
					if (!flag && !flag2)
					{
						result = NavigationErrorCodes.DeviceInitFailed;
					}
					else
					{
						if (flag)
						{
							flag = this._optoDevice.DeviceActivateHandles();
						}
						if (flag2)
						{
							flag2 = this._electroDevice.DeviceActivateHandles();
						}
						if (!flag && !flag2)
						{
							result = NavigationErrorCodes.DeviceActivateHandlesFailed;
						}
						else
						{
							if (flag)
							{
								this._optoState = Navigation.NavigationState.Initialized;
							}
							if (flag2)
							{
								this._electroState = Navigation.NavigationState.Initialized;
							}
							this._deviceState = Navigation.NavigationState.Initialized;
							result = NavigationErrorCodes.Ok;
						}
					}
				}
			}
			return result;
		}
		private void DoAsynchronousInit(object navigationInitDeviceConfig)
		{
			if (navigationInitDeviceConfig is NavigationInitDeviceConfigs)
			{
				NavigationErrorCodes errorCode = this.InnerInit((NavigationInitDeviceConfigs)navigationInitDeviceConfig);
				if (this.NavigationInitializationComplete != null)
				{
					this.NavigationInitializationComplete(this, new NavigationInitializationCompleteEventArgs(errorCode));
				}
			}
		}
		public Navigation()
		{
			this._deviceState = Navigation.NavigationState.NotInitialized;
			this._optoState = Navigation.NavigationState.NotInitialized;
			this._electroState = Navigation.NavigationState.NotInitialized;
			this._settings = NavigationSettings.DeserializeFromXML();
			this._optoDevice = new Device(DeviceKinds.Polaris, this._settings.RomFilesDirPath);
			this._optoDevice.MarkersPositionUpdate += new Device.MarkersPositionUpdateHandler(this.OptoMarkersPositionChanged);
			this._electroDevice = new Device(DeviceKinds.Aurora, "");
			this._electroDevice.MarkersPositionUpdate += new Device.MarkersPositionUpdateHandler(this.ElectroMarkersPositionChanged);
			this._deviceBackgroundWorker = new UpdateBackgroudWorker();
			this._deviceBackgroundWorker.ThreadUpdated += new UpdateBackgroudWorker.ThreadUpdateHandler(this.OnBackgroundWorkerUpdatePosition);
			this._toolList = new List<IToolInfo>();
			this._referenceIndex = -1;
		}
		public NavigationErrorCodes Init(NavigationInitKinds initKind, NavigationInitDeviceConfigs navigationInitDeviceConfig)
		{
			NavigationErrorCodes result;
			switch (initKind)
			{
			case NavigationInitKinds.Synchronous:
				result = this.InnerInit(navigationInitDeviceConfig);
				break;
			case NavigationInitKinds.Asynchronous:
				this._initializationThread = new Thread(new ParameterizedThreadStart(this.DoAsynchronousInit));
				this._initializationThread.Start(navigationInitDeviceConfig);
				result = NavigationErrorCodes.Ok;
				break;
			default:
				result = NavigationErrorCodes.DeviceInitFailed;
				break;
			}
			return result;
		}
		public NavigationErrorCodes Play()
		{
			NavigationErrorCodes result;
			if (this._deviceState == Navigation.NavigationState.Started)
			{
				result = NavigationErrorCodes.Ok;
			}
			else if (this._deviceState == Navigation.NavigationState.NotInitialized)
			{
				result = NavigationErrorCodes.DeviceNotInitialized;
			}
			else
			{
				if (this._optoState == Navigation.NavigationState.Initialized && this._electroState == Navigation.NavigationState.Initialized)
				{
					this._optoDevice.DeviceUpdateWorkingMode = DeviceUpdateWorkingModes.ManualUpdate;
					this._electroDevice.DeviceUpdateWorkingMode = DeviceUpdateWorkingModes.ManualUpdate;
					this._deviceBackgroundWorker.Start();
				}
				else if (this._optoState == Navigation.NavigationState.Initialized)
				{
					this._optoDevice.DeviceUpdateWorkingMode = DeviceUpdateWorkingModes.AutoUpdate;
				}
				else if (this._electroState == Navigation.NavigationState.Initialized)
				{
					this._electroDevice.DeviceUpdateWorkingMode = DeviceUpdateWorkingModes.AutoUpdate;
				}
				bool flag = false;
				bool flag2 = false;
				if (this._optoState == Navigation.NavigationState.Initialized)
				{
					flag = this._optoDevice.DeviceStartTracking();
				}
				if (this._electroState == Navigation.NavigationState.Initialized)
				{
					flag2 = this._electroDevice.DeviceStartTracking();
				}
				if (!flag && !flag2)
				{
					result = NavigationErrorCodes.DeviceStartTrackingFailed;
				}
				else
				{
					if (flag)
					{
						this._optoState = Navigation.NavigationState.Started;
					}
					if (flag2)
					{
						this._electroState = Navigation.NavigationState.Started;
					}
					this._deviceState = Navigation.NavigationState.Started;
					result = NavigationErrorCodes.Ok;
				}
			}
			return result;
		}
		public NavigationErrorCodes Stop()
		{
			bool flag = this._deviceBackgroundWorker.Stop();
			NavigationErrorCodes result;
			if (this._deviceState == Navigation.NavigationState.NotInitialized || this._deviceState == Navigation.NavigationState.Initialized)
			{
				result = NavigationErrorCodes.Ok;
			}
			else
			{
				bool flag2 = false;
				bool flag3 = false;
				if (this._optoState == Navigation.NavigationState.Started)
				{
					flag2 = this._optoDevice.DeviceStopTracking();
				}
				if (this._electroState == Navigation.NavigationState.Started)
				{
					flag3 = this._electroDevice.DeviceStopTracking();
				}
				if (!flag2 && !flag3)
				{
					result = NavigationErrorCodes.DeviceStopTrackingFailed;
				}
				else
				{
					if (flag2)
					{
						this._optoState = Navigation.NavigationState.Initialized;
					}
					if (flag3)
					{
						this._electroState = Navigation.NavigationState.Initialized;
					}
					this._deviceState = Navigation.NavigationState.Initialized;
					result = NavigationErrorCodes.Ok;
				}
			}
			return result;
		}
		public NavigationErrorCodes Close()
		{
			NavigationErrorCodes result;
			if (this._deviceState == Navigation.NavigationState.NotInitialized)
			{
				result = NavigationErrorCodes.Ok;
			}
			else
			{
				if (this._deviceState == Navigation.NavigationState.Started)
				{
					NavigationErrorCodes navigationErrorCodes = this.Stop();
					if (navigationErrorCodes != NavigationErrorCodes.Ok)
					{
						result = navigationErrorCodes;
						return result;
					}
				}
				bool flag = false;
				bool flag2 = false;
				if (this._optoState == Navigation.NavigationState.Initialized)
				{
					flag = this._optoDevice.DeviceCloseConnection();
				}
				if (this._electroState == Navigation.NavigationState.Initialized)
				{
					flag2 = this._electroDevice.DeviceCloseConnection();
				}
				if (!flag && !flag2)
				{
					result = NavigationErrorCodes.CloseCommunicationFailed;
				}
				else
				{
					if (flag)
					{
						this._optoState = Navigation.NavigationState.NotInitialized;
					}
					if (flag2)
					{
						this._electroState = Navigation.NavigationState.NotInitialized;
					}
					this._deviceState = Navigation.NavigationState.NotInitialized;
					result = NavigationErrorCodes.Ok;
				}
			}
			return result;
		}
		public IToolInfo AddToolInfo(ToolInfoMarkerSelectioKinds selectionKind, int optoIndex, string optoWirelessName, TMatrix optoOffsetMatrix, int electroIndex, TMatrix electroOffsetMatrix)
		{
			if (this._deviceState != Navigation.NavigationState.NotInitialized)
			{
				throw new InvalidOperationException("Device already initialized");
			}
			IToolInfo result;
			if (optoIndex < 0 && electroIndex < 0)
			{
				result = null;
			}
			else
			{
				this._toolList.Add(new ToolInfo(selectionKind, optoIndex, optoWirelessName, optoOffsetMatrix, electroIndex, electroOffsetMatrix));
				result = this._toolList[this._toolList.Count - 1];
			}
			return result;
		}
		public bool RemoveToolInfo(IToolInfo info)
		{
			if (this._deviceState != Navigation.NavigationState.NotInitialized)
			{
				throw new InvalidOperationException("Device already initialized");
			}
			bool result;
			if (info == null)
			{
				result = false;
			}
			else
			{
				int num = this._toolList.IndexOf(info);
				result = (num >= 0 && num < this._toolList.Count && this._toolList.Remove(info));
			}
			return result;
		}
		public bool SetReferenceTool(IToolInfo info)
		{
			bool result;
			if (info == null)
			{
				result = false;
			}
			else
			{
				int num = this._toolList.IndexOf(info);
				if (num < 0 || num >= this._toolList.Count)
				{
					result = false;
				}
				else
				{
					this._referenceIndex = num;
					result = true;
				}
			}
			return result;
		}
		public bool UpdateToolInfoRomName(IToolInfo toolInfo, string romName)
		{
			bool result;
			if (toolInfo == null || this._deviceState != Navigation.NavigationState.NotInitialized)
			{
				result = false;
			}
			else
			{
				int num = this._toolList.IndexOf(toolInfo);
				if (num < 0 || num >= this._toolList.Count)
				{
					result = false;
				}
				else
				{
					((ToolInfo)this._toolList[num]).OptoWirelessMarkerName = romName;
					result = true;
				}
			}
			return result;
		}
	}
}
