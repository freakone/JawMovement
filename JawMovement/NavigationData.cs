using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using NavigationLibrary;
using MathCommon;

namespace JawMovementTool
{
    [Serializable]
    public class NavigationData
    {
        [XmlIgnore]
		public string _settingsFileName;

        [XmlIgnore]
        public List<TMatrix> pointsMeasure
        {
            get;
            set;
        }

       [XmlIgnore]
        public List<TMatrix> pointRef
        {
            get;
            set;
        }

        [XmlArray("MeasuredPoints")]
        [XmlArrayItem("Point")]
        public List<double[]> pointsMeasureMatrix
        {
            get;
            set;
        }

        [XmlArray("RefPoints")]
        [XmlArrayItem("Point")]
        public  List<double[]> pointRefMatrix
        {
            get;
            set;
        }

        private NavigationData()
        {
            this.pointsMeasure = new List<TMatrix>();
            this.pointRef = new List<TMatrix>();
        }

        public void SavePointsToStruct()
        {
            pointsMeasureMatrix = new List<double[]>();
            foreach(TMatrix t in pointsMeasure)
            {
                double[] m = new double[16];
                t.GetMatrix(ref m);
                pointsMeasureMatrix.Add(m);               
            }

            pointRefMatrix = new List<double[]>();
            foreach (TMatrix t in pointRef)
            {
                double[] m = new double[16];
                t.GetMatrix(ref m);
                pointRefMatrix.Add(m);
            }
        }

        public void ReadPointsFromStruct()
        {
            pointRef = new List<TMatrix>();
            foreach(double[] t in pointRefMatrix)
            {
                TMatrix m = new TMatrix();
                m.SetMatrix(t);
                pointRef.Add(m);                
            }

            pointsMeasure = new List<TMatrix>();
            foreach (double[] t in pointsMeasureMatrix)
            {
                TMatrix m = new TMatrix();
                m.SetMatrix(t);
                pointsMeasure.Add(m);
            }
        }

        public static void SerializeToXML(NavigationData settings)
		{
			if (settings != null)
			{
                settings.SavePointsToStruct();
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(NavigationData));
				TextWriter textWriter = new StreamWriter(settings._settingsFileName);
				xmlSerializer.Serialize(textWriter, settings);
				textWriter.Close();
			}
		}
        public static NavigationData DeserializeFromXML(string filePath)
		{
            NavigationData navigationData;
			try
			{
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(NavigationData));
				TextReader textReader = new StreamReader(filePath);
                navigationData = (NavigationData)xmlSerializer.Deserialize(textReader);
				textReader.Close();
                navigationData.ReadPointsFromStruct();
			}
			catch
			{
                navigationData = new NavigationData();                
			}
			navigationData._settingsFileName = filePath;
			return navigationData;
		}
    }
}
