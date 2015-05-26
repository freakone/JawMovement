using System;
using System.IO;
using System.Xml.Serialization;
namespace NavigationLibrary
{
	public class NavigationSettings
	{
		[XmlAttribute("RomFilesDirPath")]
		public string RomFilesDirPath
		{
			get;
			set;
		}
		~NavigationSettings()
		{
			NavigationSettings.SerializeToXML(this);
		}
		public void InitDefaultValues()
		{
			this.RomFilesDirPath = ".\\ROM\\";
		}
		public static void SerializeToXML(NavigationSettings nav)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(NavigationSettings));
			TextWriter textWriter = new StreamWriter(".\\NavigationParameters.xml");
			xmlSerializer.Serialize(textWriter, nav);
			textWriter.Close();
		}
		public static NavigationSettings DeserializeFromXML()
		{
			NavigationSettings navigationSettings;
			try
			{
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(NavigationSettings));
				TextReader textReader = new StreamReader(".\\NavigationParameters.xml");
				navigationSettings = (NavigationSettings)xmlSerializer.Deserialize(textReader);
				if (navigationSettings.RomFilesDirPath == null)
				{
					navigationSettings.RomFilesDirPath = ".\\ROM\\";
				}
				textReader.Close();
			}
			catch
			{
				navigationSettings = new NavigationSettings();
				navigationSettings.InitDefaultValues();
			}
			return navigationSettings;
		}
	}
}
