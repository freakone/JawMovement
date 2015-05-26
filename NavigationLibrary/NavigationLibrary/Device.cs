using MathCommon;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Threading;
namespace NavigationLibrary
{
	internal class Device
	{
		public delegate void MarkersPositionUpdateHandler(object sender, MarkersPositionEventArgs e);
		private readonly int _portTimeoutMs = 10000;
		private SerialPort _port;
		private DeviceKinds _deviceKind;
		private DeviceSettings _settings;
		private AutoResetEvent _commandCompleteEvent;
		private string _deviceResponse;
		private string _romFilesDirPath;
		private List<WirelessMarkerInfo> _wirelessToolsInfoList;
		private UpdateBackgroudWorker _deviceBackgroundWorker;
		private List<MarkerInfo> _markerList;
		private DeviceUpdateWorkingModes _deviceUpdateWorkingMode;
		private bool _trackingStarted;
		public event Device.MarkersPositionUpdateHandler MarkersPositionUpdate;
		public DeviceUpdateWorkingModes DeviceUpdateWorkingMode
		{
			get
			{
				return this._deviceUpdateWorkingMode;
			}
			set
			{
				if (this._trackingStarted)
				{
					throw new InvalidOperationException("Cannot set device update working mode when tracking is on");
				}
				this._deviceUpdateWorkingMode = value;
			}
		}
		private void SerialDataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			this._deviceResponse += this._port.ReadExisting();
			int num = this._deviceResponse.IndexOf('\r');
			if (num >= 0)
			{
				this._deviceResponse = this._deviceResponse.Substring(0, num + 1);
				this._commandCompleteEvent.Set();
			}
		}
		private void OnUpdatePosition(object sender)
		{
			if (this.SendCommand(DeviceCommands.Tx, DeviceResponces.Empty, DeviceCommandOptions.Option0001, DeviceResponseVerificationMode.NoVerification, true))
			{
				if (this.DeviceDecodeTxResponse())
				{
					if (this.MarkersPositionUpdate != null)
					{
						this.MarkersPositionUpdate(this, new MarkersPositionEventArgs(this._markerList));
					}
				}
			}
		}
		private bool DeviceDecodeTxResponse()
		{
			string s = this._deviceResponse.Substring(0, 2);
			uint num;
			bool result;
			if (!uint.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out num))
			{
				result = false;
			}
			else if (num == 0u)
			{
				result = true;
			}
			else
			{
				int num2 = 2;
				while ((long)this._markerList.Count > (long)((ulong)num))
				{
					this._markerList.RemoveAt(this._markerList.Count - 1);
				}
				int num3 = 0;
				while ((long)num3 < (long)((ulong)num))
				{
					string s2 = this._deviceResponse.Substring(num2, 2);
					uint num4;
					if (!uint.TryParse(s2, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out num4))
					{
						result = false;
						return result;
					}
					num2 += 2;
					if (this._deviceResponse.ToCharArray()[num2] == 'M')
					{
						DeviceMarkerStatus status = DeviceMarkerStatus.Missing;
						if (num3 >= this._markerList.Count)
						{
							this._markerList.Add(new MarkerInfo());
						}
						this._markerList[num3].Update(num4, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, false, status, double.NaN, this.GetNameForPortNumber(num4));
						num2 += 24;
					}
					else
					{
						DeviceMarkerStatus status = DeviceMarkerStatus.Ok;
						double q;
						if (!this.DeviceDecodeQ(num2, out q))
						{
							result = false;
							return result;
						}
						num2 += 6;
						double qx;
						if (!this.DeviceDecodeQ(num2, out qx))
						{
							result = false;
							return result;
						}
						num2 += 6;
						double qy;
						if (!this.DeviceDecodeQ(num2, out qy))
						{
							result = false;
							return result;
						}
						num2 += 6;
						double qz;
						if (!this.DeviceDecodeQ(num2, out qz))
						{
							result = false;
							return result;
						}
						num2 += 6;
						double rotX;
						double rotY;
						double rotZ;
						MathHelper.QuaternionToEuler(q, qx, qy, qz, out rotX, out rotY, out rotZ);
						double transX;
						if (!this.DeviceDecodeT(num2, out transX))
						{
							result = false;
							return result;
						}
						num2 += 7;
						double transY;
						if (!this.DeviceDecodeT(num2, out transY))
						{
							result = false;
							return result;
						}
						num2 += 7;
						double transZ;
						if (!this.DeviceDecodeT(num2, out transZ))
						{
							result = false;
							return result;
						}
						num2 += 7;
						double rmsErrorValue;
						if (!this.DeviceDecodeRms(num2, out rmsErrorValue))
						{
							result = false;
							return result;
						}
						num2 += 6;
						if (num3 >= this._markerList.Count)
						{
							this._markerList.Add(new MarkerInfo());
						}

                        num2 += 7;
                        
                        bool button_clicked = (this._deviceResponse[num2] & 64) > 0;

						this._markerList[num3].Update(num4, transX, transY, transZ, rotX, rotY, rotZ, q, qx, qy, qz, button_clicked, status, rmsErrorValue, this.GetNameForPortNumber(num4));
						num2 += 10;
					}
					num3++;
				}
				result = true;
			}
			return result;
		}
		private bool DeviceDecodeQ(int index, out double value)
		{
			value = 0.0;
			bool result;
			if (index < 0 || index >= this._deviceResponse.Length || (long)index + 6L > (long)this._deviceResponse.Length)
			{
				result = false;
			}
			else
			{
				string s = this._deviceResponse.Substring(index, 6);
				int num;
				if (!int.TryParse(s, out num))
				{
					result = false;
				}
				else
				{
					value = (double)num * 0.0001;
					result = true;
				}
			}
			return result;
		}
		private bool DeviceDecodeT(int index, out double value)
		{
			value = 0.0;
			bool result;
			if (index < 0 || index >= this._deviceResponse.Length || (long)index + 7L > (long)this._deviceResponse.Length)
			{
				result = false;
			}
			else
			{
				string s = this._deviceResponse.Substring(index, 7);
				int num;
				if (!int.TryParse(s, out num))
				{
					result = false;
				}
				else
				{
					value = (double)num * 0.01;
					result = true;
				}
			}
			return result;
		}
		private bool DeviceDecodeRms(int index, out double value)
		{
			value = double.NaN;
			bool result;
			if (index < 0 || index >= this._deviceResponse.Length || index + 6 > this._deviceResponse.Length)
			{
				result = false;
			}
			else
			{
				string s = this._deviceResponse.Substring(index, 6);
				int num;
				if (!int.TryParse(s, out num))
				{
					result = false;
				}
				else
				{
					value = (double)num * 0.0001;
					result = true;
				}
			}
			return result;
		}
		private string GetNameForPortNumber(uint portNumber)
		{
			string result;
			if (this._wirelessToolsInfoList == null || this._wirelessToolsInfoList.Count == 0)
			{
				result = "";
			}
			else
			{
				for (int i = 0; i < this._wirelessToolsInfoList.Count; i++)
				{
					if (this._wirelessToolsInfoList[i].PortNumber == portNumber)
					{
						result = this._wirelessToolsInfoList[i].Name;
						return result;
					}
				}
				result = "";
			}
			return result;
		}
		private bool DeviceSerialBreak()
		{
			this._commandCompleteEvent.Reset();
			this._deviceResponse = "";
			this._port.BreakState = true;
			this._port.DiscardInBuffer();
			this._port.DiscardOutBuffer();
			Thread.Sleep(500);
			this._port.BreakState = false;
			return this._commandCompleteEvent.WaitOne(this._portTimeoutMs) && this.VerifyDeviceResponse(ref this._deviceResponse, DeviceResponces.SerialBreak, DeviceResponseVerificationMode.Equals);
		}
		private bool PortNumberFromName(string name, out int portNumber)
		{
			string s = name.Substring(3);
			return int.TryParse(s, out portNumber);
		}
		private bool VerifyDeviceResponse(ref string inDeviceResponse, string inExpectedResponse, DeviceResponseVerificationMode vMode)
		{
			string text = inDeviceResponse.Substring(inDeviceResponse.Length - 5, 4);
			inDeviceResponse = inDeviceResponse.Substring(0, inDeviceResponse.Length - 5);
			bool result;
			if (!text.Equals(MathHelper.CalcCrc16(inDeviceResponse)))
			{
				result = false;
			}
			else
			{
				switch (vMode)
				{
				case DeviceResponseVerificationMode.Equals:
					result = inExpectedResponse.Equals(inDeviceResponse);
					break;
				case DeviceResponseVerificationMode.StartsWith:
					result = inExpectedResponse.StartsWith(inDeviceResponse);
					break;
				case DeviceResponseVerificationMode.NoVerification:
					result = true;
					break;
				default:
					result = false;
					break;
				}
			}
			return result;
		}
		private bool DeviceOpenPort(string portName)
		{
			bool result;
			try
			{
				if (this._port != null)
				{
					if (this._port.IsOpen)
					{
						this._port.Close();
					}
				}
				else
				{
					this._port = new SerialPort();
					this._port.DataReceived += new SerialDataReceivedEventHandler(this.SerialDataReceived);
				}
				this._port.PortName = portName;
				this._port.Open();
				this._port.DiscardInBuffer();
				this._port.DiscardOutBuffer();
			}
			catch
			{
				result = false;
				return result;
			}
			result = true;
			return result;
		}
		private void DeviceClosePort()
		{
			this._port.Close();
			this._port = null;
		}
		private bool SendCommand(string inCommand, string inExpectedAnswer, string inReplyOption, DeviceResponseVerificationMode vMode, bool addCRC)
		{
			this._commandCompleteEvent.Reset();
			this._deviceResponse = "";
			if (addCRC)
			{
				string text = string.Concat(new object[]
				{
					inCommand,
					inReplyOption,
					MathHelper.CalcCrc16(inCommand + inReplyOption),
					'\r'
				});
				this._port.Write(text.ToCharArray(), 0, text.Length);
			}
			else
			{
				this._port.WriteLine(inCommand + '\r');
			}
			return this._commandCompleteEvent.WaitOne(this._portTimeoutMs) && this.VerifyDeviceResponse(ref this._deviceResponse, inExpectedAnswer, vMode);
		}
		private bool PhsrDeviceResponseDecode(string inDeviceResponse, ref List<uint> portList)
		{
			bool result;
			if (portList == null)
			{
				result = false;
			}
			else
			{
				portList.Clear();
				string s = inDeviceResponse.Substring(0, 2);
				uint num;
				if (!uint.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out num))
				{
					result = false;
				}
				else if (num == 0u)
				{
					result = true;
				}
				else
				{
					for (int i = 2; i < inDeviceResponse.Length; i += 5)
					{
						if (i + 5 > inDeviceResponse.Length)
						{
							result = false;
							return result;
						}
						string s2 = inDeviceResponse.Substring(i, 2);
						uint item;
						if (!uint.TryParse(s2, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out item))
						{
							result = false;
							return result;
						}
						portList.Add(item);
					}
					result = true;
				}
			}
			return result;
		}
		private bool DeviceFreeHandles()
		{
			List<uint> list = new List<uint>();
			bool result;
			if (!this.SendCommand(DeviceCommands.Phsr, DeviceResponces.Empty, DeviceCommandOptions.Option01, DeviceResponseVerificationMode.NoVerification, true))
			{
				result = false;
			}
			else if (!this.PhsrDeviceResponseDecode(this._deviceResponse, ref list))
			{
				result = false;
			}
			else
			{
				for (int i = 0; i < list.Count; i++)
				{
					string text = list[i].ToString("X");
					if (text.Length < 2)
					{
						text = "00".Insert(1, text);
					}
					if (text.Length > 2)
					{
						result = false;
						return result;
					}
					if (!this.SendCommand(DeviceCommands.Phf + text, DeviceResponces.Ok, DeviceCommandOptions.OptionEmpty, DeviceResponseVerificationMode.Equals, true))
					{
						result = false;
						return result;
					}
				}
				result = true;
			}
			return result;
		}
		private bool DeviceInitHandles()
		{
			bool result;
			if (this._deviceKind == DeviceKinds.Polaris)
			{
				if (!this.SendCommand(DeviceCommands.Sflist, DeviceResponces.Empty, DeviceCommandOptions.Option02, DeviceResponseVerificationMode.NoVerification, true))
				{
					result = false;
					return result;
				}
				uint num;
				if (!uint.TryParse(this._deviceResponse, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out num))
				{
					result = false;
					return result;
				}
				if (!Directory.Exists(this._romFilesDirPath))
				{
					Directory.CreateDirectory(this._romFilesDirPath);
				}
				this._wirelessToolsInfoList.Clear();
				string[] files = Directory.GetFiles(this._romFilesDirPath);
				string[] array = files;
				for (int i = 0; i < array.Length; i++)
				{
					string path = array[i];
					if (File.Exists(path) && Path.GetExtension(path).Equals(".rom"))
					{
						using (FileStream fileStream = new FileStream(path, FileMode.Open))
						{
							fileStream.Seek(0L, SeekOrigin.Begin);
							byte[] array2 = new byte[1024];
							int num2 = fileStream.Read(array2, 0, array2.Length);
							if (!this.SendCommand(DeviceCommands.Phrq, DeviceResponces.Empty, DeviceCommandOptions.AllocWirelessMarkerPort, DeviceResponseVerificationMode.NoVerification, true))
							{
								result = false;
								return result;
							}
							uint num3;
							if (!uint.TryParse(this._deviceResponse, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out num3))
							{
								result = false;
								return result;
							}
							int j = 0;
							while (j < num2)
							{
								uint value = (uint)j;
								string text = "";
								for (int k = 0; k < 64; k++)
								{
									text += NavigationHelper.Uint2StrHex((uint)array2[j], 2);
									j++;
								}
								if (!this.SendCommand(DeviceCommands.Pvwr, DeviceResponces.Ok, NavigationHelper.Uint2StrHex(num3, 2) + NavigationHelper.Uint2StrHex(value, 4) + text, DeviceResponseVerificationMode.Equals, true))
								{
									result = false;
									return result;
								}
							}
							this._wirelessToolsInfoList.Add(new WirelessMarkerInfo(num3, Path.GetFileNameWithoutExtension(path)));
						}
					}
				}
			}
			List<uint> list = new List<uint>();
			if (!this.SendCommand(DeviceCommands.Phsr, DeviceResponces.Empty, DeviceCommandOptions.Option02, DeviceResponseVerificationMode.NoVerification, true))
			{
				result = false;
			}
			else if (!this.PhsrDeviceResponseDecode(this._deviceResponse, ref list))
			{
				result = false;
			}
			else
			{
				while (list.Count > 0)
				{
					for (int j = 0; j < list.Count; j++)
					{
						string text2 = list[j].ToString("X");
						if (text2.Length < 1)
						{
							result = false;
							return result;
						}
						if (text2.Length == 1)
						{
							text2 = "0" + text2;
						}
						if (text2.Length > 2)
						{
							result = false;
							return result;
						}
						if (!this.SendCommand(DeviceCommands.Pinit + text2, DeviceResponces.Ok, DeviceCommandOptions.OptionEmpty, DeviceResponseVerificationMode.NoVerification, true))
						{
							result = false;
							return result;
						}
					}
					if (!this.SendCommand(DeviceCommands.Phsr, DeviceResponces.Empty, DeviceCommandOptions.Option02, DeviceResponseVerificationMode.NoVerification, true))
					{
						result = false;
						return result;
					}
					if (!this.PhsrDeviceResponseDecode(this._deviceResponse, ref list))
					{
						result = false;
						return result;
					}
				}
				result = true;
			}
			return result;
		}
		private bool DeviceEnableHandles()
		{
			List<uint> list = new List<uint>();
			bool result;
			if (!this.SendCommand(DeviceCommands.Phsr, DeviceResponces.Empty, DeviceCommandOptions.Option03, DeviceResponseVerificationMode.NoVerification, true))
			{
				result = false;
			}
			else if (!this.PhsrDeviceResponseDecode(this._deviceResponse, ref list))
			{
				result = false;
			}
			else
			{
				while (list.Count > 0)
				{
					for (int i = 0; i < list.Count; i++)
					{
						string text = list[i].ToString("X");
						if (text.Length < 1)
						{
							result = false;
							return result;
						}
						if (text.Length == 1)
						{
							text = "0" + text;
						}
						if (text.Length > 2)
						{
							result = false;
							return result;
						}
						if (!this.SendCommand(DeviceCommands.Pena + text, DeviceResponces.Ok, DeviceCommandOptions.OptionD, DeviceResponseVerificationMode.NoVerification, true))
						{
							result = false;
							return result;
						}
					}
					if (!this.SendCommand(DeviceCommands.Phsr, DeviceResponces.Empty, DeviceCommandOptions.Option03, DeviceResponseVerificationMode.NoVerification, true))
					{
						result = false;
						return result;
					}
					if (!this.PhsrDeviceResponseDecode(this._deviceResponse, ref list))
					{
						result = false;
						return result;
					}
				}
				result = true;
			}
			return result;
		}
		private bool DeviceSetCommParams()
		{
			string[] array;
			int[] array2;
			bool result;
			switch (this._deviceKind)
			{
			case DeviceKinds.Aurora:
				array = new string[]
				{
					DeviceCommandOptions.CommASpeed,
					DeviceCommandOptions.Comm6Speed,
					DeviceCommandOptions.Comm5Speed,
					DeviceCommandOptions.Comm4Speed,
					DeviceCommandOptions.Comm3Speed,
					DeviceCommandOptions.Comm2Speed,
					DeviceCommandOptions.Comm1Speed,
					DeviceCommandOptions.Comm0Speed
				};
				array2 = new int[]
				{
					230400,
					921600,
					115200,
					57600,
					38400,
					19200,
					14400,
					9600
				};
				break;
			case DeviceKinds.Polaris:
				array = new string[]
				{
					DeviceCommandOptions.Comm7Speed,
					DeviceCommandOptions.Comm6Speed,
					DeviceCommandOptions.Comm5Speed,
					DeviceCommandOptions.Comm4Speed,
					DeviceCommandOptions.Comm3Speed,
					DeviceCommandOptions.Comm2Speed,
					DeviceCommandOptions.Comm1Speed,
					DeviceCommandOptions.Comm0Speed
				};
				array2 = new int[]
				{
					1228739,
					921600,
					115200,
					57600,
					38400,
					19200,
					14400,
					9600
				};
				break;
			case DeviceKinds.Unknown:
				array = new string[]
				{
					DeviceCommandOptions.Comm5Speed,
					DeviceCommandOptions.Comm4Speed,
					DeviceCommandOptions.Comm3Speed,
					DeviceCommandOptions.Comm2Speed,
					DeviceCommandOptions.Comm1Speed,
					DeviceCommandOptions.Comm0Speed
				};
				array2 = new int[]
				{
					115200,
					57600,
					38400,
					19200,
					14400,
					9600
				};
				break;
			default:
				result = false;
				return result;
			}
			for (int i = 0; i < array.Length; i++)
			{
				if (this.SendCommand(DeviceCommands.Comm, DeviceResponces.Ok, array[i], DeviceResponseVerificationMode.Equals, true))
				{
					Thread.Sleep(200);
					try
					{
						if (this._port.IsOpen)
						{
							this._port.Close();
						}
						this._port.Handshake = Handshake.RequestToSend;
						this._port.BaudRate = array2[i];
						this._port.Open();
					}
					catch
					{
						goto IL_1D3;
					}
					result = true;
					return result;
				}
				IL_1D3:;
			}
			result = false;
			return result;
		}
		private bool DeviceCheckDeviceKind()
		{
			bool result;
			if (!this.SendCommand(DeviceCommands.Ver, DeviceResponces.Empty, DeviceCommandOptions.Option0, DeviceResponseVerificationMode.NoVerification, true))
			{
				result = false;
			}
			else
			{
				int num = this._deviceResponse.IndexOf('\n');
				if (num < 0)
				{
					result = false;
				}
				else
				{
					string text = this._deviceResponse.Substring(0, num + 1);
					switch (this._deviceKind)
					{
					case DeviceKinds.Aurora:
						if (text.Contains("Aurora"))
						{
							result = true;
							return result;
						}
						break;
					case DeviceKinds.Polaris:
						if (text.Contains("Polaris"))
						{
							result = true;
							return result;
						}
						break;
					}
					result = false;
				}
			}
			return result;
		}
		public bool DeviceOpenConnection()
		{
			bool result;
			if (this.DeviceOpenPort("COM" + this._settings.PortNumber.ToString()))
			{
				if (this.DeviceSerialBreak())
				{
					if (this.DeviceCheckDeviceKind())
					{
						if (this.DeviceSetCommParams())
						{
							result = true;
							return result;
						}
					}
				}
			}
			string[] portNames = SerialPort.GetPortNames();
			for (int i = 0; i < portNames.Length; i++)
			{
				if (this.DeviceOpenPort(portNames[i]))
				{
					if (this.DeviceSerialBreak())
					{
						if (this.DeviceCheckDeviceKind())
						{
							if (this.DeviceSetCommParams())
							{
								int portNumber;
								if (this.PortNumberFromName(portNames[i], out portNumber))
								{
									this._settings.PortNumber = portNumber;
									result = true;
									return result;
								}
								this.DeviceClosePort();
							}
							else
							{
								this.DeviceClosePort();
							}
						}
						else
						{
							this.DeviceClosePort();
						}
					}
					else
					{
						this.DeviceClosePort();
					}
				}
			}
			result = false;
			return result;
		}
		public bool DeviceCloseConnection()
		{
			this.DeviceClosePort();
			return true;
		}
		public bool DeviceInit()
		{
			return this.SendCommand(DeviceCommands.Init, DeviceResponces.Ok, DeviceCommandOptions.OptionEmpty, DeviceResponseVerificationMode.Equals, true);
		}
		public bool DeviceActivateHandles()
		{
			return this.DeviceFreeHandles() && this.DeviceInitHandles() && this.DeviceEnableHandles();
		}
		public bool DeviceStartTracking()
		{
			bool result;
			if (!this.SendCommand(DeviceCommands.Tstart, DeviceResponces.Ok, DeviceCommandOptions.OptionEmpty, DeviceResponseVerificationMode.Equals, true))
			{
				result = false;
			}
			else
			{
				DeviceUpdateWorkingModes deviceUpdateWorkingMode = this._deviceUpdateWorkingMode;
				result = (deviceUpdateWorkingMode != DeviceUpdateWorkingModes.AutoUpdate || this._deviceBackgroundWorker.Start());
			}
			return result;
		}
		public bool DeviceStopTracking()
		{
			DeviceUpdateWorkingModes deviceUpdateWorkingMode = this._deviceUpdateWorkingMode;
			bool result;
			if (deviceUpdateWorkingMode == DeviceUpdateWorkingModes.AutoUpdate)
			{
				if (!this._deviceBackgroundWorker.Stop())
				{
					result = false;
					return result;
				}
			}
			result = this.SendCommand(DeviceCommands.Tstop, DeviceResponces.Ok, DeviceCommandOptions.OptionEmpty, DeviceResponseVerificationMode.Equals, true);
			return result;
		}
		public bool DeviceUpdatePositions(out MarkersPositionEventArgs e)
		{
			e = null;
			DeviceUpdateWorkingModes deviceUpdateWorkingMode = this._deviceUpdateWorkingMode;
			bool result;
			if (deviceUpdateWorkingMode != DeviceUpdateWorkingModes.ManualUpdate)
			{
				result = false;
			}
			else if (!this.SendCommand(DeviceCommands.Tx, DeviceResponces.Empty, DeviceCommandOptions.Option0001, DeviceResponseVerificationMode.NoVerification, true))
			{
				result = false;
			}
			else if (!this.DeviceDecodeTxResponse())
			{
				result = false;
			}
			else
			{
				e = new MarkersPositionEventArgs(this._markerList);
				result = true;
			}
			return result;
		}
		public Device(DeviceKinds deviceKind, string romFilesDirPath)
		{
			this._commandCompleteEvent = new AutoResetEvent(false);
			this._romFilesDirPath = romFilesDirPath;
			this._wirelessToolsInfoList = new List<WirelessMarkerInfo>();
			this._port = new SerialPort();
			this._port.DataReceived += new SerialDataReceivedEventHandler(this.SerialDataReceived);
			this._deviceKind = deviceKind;
			switch (this._deviceKind)
			{
			case DeviceKinds.Aurora:
				this._portTimeoutMs = 5000;
				this._settings = DeviceSettings.DeserializeFromXML(".//AuroraSettings.xml");
				break;
			case DeviceKinds.Polaris:
				this._portTimeoutMs = 10000;
				this._settings = DeviceSettings.DeserializeFromXML(".//PolarisSettings.xml");
				break;
			default:
				this._portTimeoutMs = 10000;
				this._settings = DeviceSettings.DeserializeFromXML("");
				break;
			}
			this._deviceBackgroundWorker = new UpdateBackgroudWorker();
			this._deviceBackgroundWorker.ThreadUpdated += new UpdateBackgroudWorker.ThreadUpdateHandler(this.OnUpdatePosition);
			this._markerList = new List<MarkerInfo>();
			this._deviceUpdateWorkingMode = DeviceUpdateWorkingModes.ManualUpdate;
			this._trackingStarted = false;
		}
	}
}
