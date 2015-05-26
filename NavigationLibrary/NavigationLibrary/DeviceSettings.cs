using System;
using System.IO;
using System.Xml.Serialization;
namespace NavigationLibrary
{
	public class DeviceSettings
	{
		[XmlIgnore]
		private string _settingsFileName;
		[XmlAttribute("PortNumber")]
		public int PortNumber
		{
			get;
			set;
		}
		~DeviceSettings()
		{
			DeviceSettings.SerializeToXML(this);
		}
		public static void SerializeToXML(DeviceSettings settings)
		{
			if (settings != null)
			{
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(DeviceSettings));
				TextWriter textWriter = new StreamWriter(settings._settingsFileName);
				xmlSerializer.Serialize(textWriter, settings);
				textWriter.Close();
			}
		}
		public static DeviceSettings DeserializeFromXML(string filePath)
		{
			DeviceSettings deviceSettings;
			try
			{
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(DeviceSettings));
				TextReader textReader = new StreamReader(filePath);
				deviceSettings = (DeviceSettings)xmlSerializer.Deserialize(textReader);
				textReader.Close();
			}
			catch
			{
				deviceSettings = new DeviceSettings();
			}
			deviceSettings._settingsFileName = filePath;
			return deviceSettings;
		}
	}
}
