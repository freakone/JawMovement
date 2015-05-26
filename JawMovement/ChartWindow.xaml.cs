using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay;
using System.ComponentModel;
using System.Windows.Media.Media3D;
using Microsoft.Research.DynamicDataDisplay.PointMarkers;
using System.Windows.Threading;
using System.Threading.Tasks;
using HelixToolkit.Wpf;
using MathCommon;
using Microsoft.Win32;
using System.IO;

namespace JawMovementTool
{
    /// <summary>
    /// Interaction logic for ChartWindow.xaml
    /// </summary>
    public partial class ChartWindow : Window
    {
        public List<ChartPoint> pointsFrontal;
        public List<ChartPoint> pointsSagital;
        public List<ChartPoint> pointsHorizontal;
        public ChartPointsCollection chartPointsCollection; 
        private int _currentChart = 0;

    
        public ChartWindow()
        {
            InitializeComponent();

            chartPointsCollection = new ChartPointsCollection();
            pointsFrontal = new List<ChartPoint>();
            pointsSagital = new List<ChartPoint>();
            pointsHorizontal = new List<ChartPoint>();
           

            var ds = new EnumerableDataSource<ChartPoint>(chartPointsCollection);
            ds.SetXMapping(x => x.x);
            ds.SetYMapping(y => y.y);
            ds.AddMapping(ShapePointMarker.SizeProperty, (ChartPoint p) => 3.0);
            ds.AddMapping(ShapePointMarker.FillProperty, (ChartPoint p) => new SolidColorBrush(Colors.Blue));
            CompositeDataSource pointSource = new CompositeDataSource(new IPointDataSource[]
	        {
		        ds
	        });
            

            this.plotter.AddLineGraph(pointSource, new Pen(Brushes.Green, 0), new CirclePointMarker(), new PenDescription("Pozycja żuchwy"));
            this.plotter.Viewport.Visible = new Rect(-50, -10, 100, 100);
        }
        public void AddPoint(double x, double y)
        {
            chartPointsCollection.Add(new ChartPoint(x, y));
        }

        private Point3D ProjectOnPlane(Point3D point, Plane3D plane)
        {
            Vector3D v = point - plane.Position;
            double dist = v.X * plane.Normal.X + v.Y * plane.Normal.Y + v.Z * plane.Normal.Z;
            return point - dist * plane.Normal;
        }

        private Point GetLocalCoords(Point3D point, Plane3D plane, Vector3D axis)
        {
            Point p = new Point();

            double dpoint = MathCommon.MathHelper.CalcDistanceBetweenPoints(point, plane.Position);
            double angle = Vector3D.AngleBetween(point - plane.Position, axis);

            p.X = dpoint * Math.Cos(MathCommon.MathHelper.Deg2Rad(angle));
            p.Y = dpoint * Math.Sin(MathCommon.MathHelper.Deg2Rad(angle));

            return p;
        }

        public void ImportNavigationData(NavigationData n)
        {
            pointsSagital.Clear();
            pointsHorizontal.Clear();
            pointsFrontal.Clear();
            
            double[][] temp = new double[3][];
            for (int i = 0; i < 3; i++)
                temp[i] = new double[] { n.pointRef[i + 3].X, n.pointRef[i + 3].Y, n.pointRef[i + 3].Z };

            double[] normal = new double[3];
            MathCommon.MathHelper.CalculateNormal(temp, ref normal);

            Plane3D planeX = new Plane3D();
            planeX.Position = n.pointRef[5].point3D();
            planeX.Normal = new Vector3D(-normal[0], -normal[1], -normal[2]);


            Point3D nosePoint = n.pointRef[4].point3D();
            Vector3D xAxis = nosePoint - planeX.Position;

            Quaternion q1 = new Quaternion(xAxis, -90);
            Matrix3D m1 = Matrix3D.Identity;
            m1.Rotate(q1);

            Plane3D planeY = new Plane3D();
            planeY.Position = planeX.Position;
            planeY.Normal = m1.Transform(planeX.Normal);

            Quaternion q2 = new Quaternion(planeX.Normal, -90);
            Matrix3D m2 = Matrix3D.Identity;
            m2.Rotate(q2);

            Plane3D planeZ = new Plane3D();
            planeZ.Position = planeX.Position;
            planeZ.Normal = m2.Transform(planeY.Normal);

            Quaternion q3 = new Quaternion(planeX.Normal, 90);
            Matrix3D m3 = Matrix3D.Identity;
            m3.Rotate(q3);

            Quaternion q4 = new Quaternion(xAxis, 90);
            Matrix3D m4 = Matrix3D.Identity;
            m4.Rotate(q4);

            Vector3D zAxis = m3.Transform(xAxis);
            Vector3D yAxis = m4.Transform(zAxis);
            Vector3D jawTranslate = n.pointRef[1].point3D() - n.pointRef[0].point3D();

            Matrix3D mJawTranslate = Matrix3D.Identity;
            mJawTranslate.Translate(jawTranslate);


            foreach (TMatrix t in n.pointsMeasure)
            {

                Point3D pnt = mJawTranslate.Transform(t.point3D());    
                Point3D projectedX = ProjectOnPlane(pnt, planeX);
                Point3D projectedY = ProjectOnPlane(pnt, planeY);
                Point3D projectedZ = ProjectOnPlane(pnt, planeZ);

                Point x = GetLocalCoords(projectedX, planeX, xAxis);
                Point y = GetLocalCoords(projectedY, planeY, yAxis);
                Point z = GetLocalCoords(projectedZ, planeZ, zAxis);

                pointsSagital.Add(new ChartPoint(x.X, -x.Y));
                pointsHorizontal.Add(new ChartPoint(y.X, -y.Y));
                pointsFrontal.Add(new ChartPoint(z.X, -z.Y));     
            }

            pointsSagital = CenterPoints(pointsSagital);
            pointsFrontal = CenterPoints(pointsFrontal);
            pointsHorizontal = CenterPoints(pointsHorizontal);

            ChartRefresh();
        }

        private List<ChartPoint> CenterPoints(List<ChartPoint> chart)
        {
            ChartPoint yMin = chart.OrderByDescending(item => item.y).Last();
            ChartPoint center = chart.OrderByDescending(item => item.x).ElementAt(chart.Count / 2);

            List<ChartPoint> offset = new List<ChartPoint>();

            foreach (ChartPoint p in chart)
            {

                offset.Add(new ChartPoint(p.x - center.x, p.y - yMin.y));
            }

            return offset;
            //centrowanie
        }

        private List<ChartPoint> FilterPoints(List<ChartPoint> chart)
        {
            List<ChartPoint> filtered = new List<ChartPoint>();

            ChartPoint xMax = chart.OrderByDescending(item => item.x).First();
            ChartPoint xMin = chart.OrderByDescending(item => item.x).Last();
            ChartPoint yMax = chart.OrderByDescending(item => item.y).First();
            ChartPoint yMin = chart.OrderByDescending(item => item.y).Last();

            double range = 0.4;

            for(double i = xMin.x; i <= xMax.x; i+=range)
            {
               List<ChartPoint> fil = chart.FindAll(item => item.x < i + range && item.x > i - range);
               if (fil.Count > 0)
               {
                   ChartPoint c = fil.OrderByDescending(item => item.y).First();
                   c.index = chart.IndexOf(c);
                   filtered.Add(c);

                   c = fil.OrderByDescending(item => item.y).Last();
                   c.index = chart.IndexOf(c);
                   filtered.Add(c);
               }
            }

            for (double i = yMin.y; i <= yMax.y; i += range)
            {
                List<ChartPoint> fil = chart.FindAll(item => item.y < i + range && item.y > i - range);
                if (fil.Count > 0)
                {
                    ChartPoint c = fil.OrderByDescending(item => item.x).First();
                    c.index = chart.IndexOf(c);
                    filtered.Add(c);

                    c = fil.OrderByDescending(item => item.x).Last();
                    c.index = chart.IndexOf(c);
                    filtered.Add(c);
                }
            } // filtrowanie
                     



            return filtered.OrderBy(item => item.index).ToList();
        }

        private void ChartRefresh()
        {
            chartPointsCollection.Clear();
            switch(this._currentChart)
            {
                case 0:
                    this.chartPointsCollection.AddMany(cFilter.IsChecked.Value ? FilterPoints(pointsSagital) : pointsSagital);
                    ChartHeader.Content = "Wykres ruchomości żuchwy - płaszczyzna strzałkowa";
                    break;
                case 1:
                    this.chartPointsCollection.AddMany(cFilter.IsChecked.Value ?  FilterPoints(pointsFrontal) : pointsFrontal);
                    ChartHeader.Content = "Wykres ruchomości żuchwy - płaszczyzna czołowa";
                    break;
                case 2:
                    this.chartPointsCollection.AddMany(cFilter.IsChecked.Value ? FilterPoints(pointsHorizontal) : pointsHorizontal);
                    ChartHeader.Content = "Wykres ruchomości żuchwy - płaszczyzna poprzeczna";
                    break;

            }
        }

        private void btnSagital_Click(object sender, RoutedEventArgs e)
        {
            _currentChart = 0;
            ChartRefresh();
        }

        private void btnFrontal_Click(object sender, RoutedEventArgs e)
        {
            _currentChart = 1;
            ChartRefresh();
        }

        private void btnHorizontal_Click(object sender, RoutedEventArgs e)
        {
            _currentChart = 2;
            ChartRefresh();
        }

        private void SavePoints(ChartPoint[] pnts, string path)
        {
            string txt = "";
            foreach(ChartPoint p in pnts)
            {
                txt += String.Join(";", p.x, p.y);
                txt += "\r\n";
            }

            File.WriteAllText(path, txt);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "*.csv|*.csv";
            saveFileDialog.ShowDialog();

            if (saveFileDialog.FileName == "")
                return;

            string txt = "";
            foreach (ChartPoint p in chartPointsCollection)
            {
                txt += String.Join(";", p.x, p.y);
                txt += "\r\n";
            }

            File.WriteAllText(saveFileDialog.FileName, txt);  
        }

        private void cFilter_Checked(object sender, RoutedEventArgs e)
        {
            ChartRefresh();
        }

    }
}
