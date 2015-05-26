using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HelixToolkit.Wpf;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows;
using MathCommon;

namespace JawMovementTool
{
    class ModelViewer : Observable
    {
        private readonly Dispatcher dispatcher;
        private readonly HelixViewport3D viewport;
        private Model3DGroup currentModelUp;
        private Model3DGroup currentModelDown;
       

        public ModelViewer(HelixViewport3D viewport)
        {
            this.dispatcher = Dispatcher.CurrentDispatcher;
            this.viewport = viewport;
            FileOpen();
        }

        private async void FileOpen()
        {
            this.currentModelUp = await this.LoadAsync("skull-up.stl", false, new DiffuseMaterial(Brushes.LightBlue));
            this.currentModelDown = await this.LoadAsync("skull-down.stl", false, new DiffuseMaterial(Brushes.Green));
            ResetView();

        }
        public void ResetView()
        {
            this.viewport.Children.Clear();
            ModelVisual3D mv3 = new ModelVisual3D();
            mv3.Content = this.currentModelUp;
            this.viewport.Children.Add(mv3);

            mv3 = new ModelVisual3D();
            mv3.Content = this.currentModelDown;
            this.viewport.Children.Add(mv3);

            SetCameraRotation(0, 0, 0);

            this.viewport.Children.Add(new DefaultLights());
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

            p.X = dpoint * Math.Cos(MathHelper.Deg2Rad(angle));
            p.Y = dpoint * Math.Sin(MathHelper.Deg2Rad(angle)); 

            return p;
        }
     

        public void DrawPlane(List<TMatrix> points, List<TMatrix> measured)
        {
         
            double [][] temp = new double[3][];
            for(int i = 0; i < 3; i++)
                temp[i] = new double[] { points[i+3].X, points[i+3].Y, points[i+3].Z};                

            double[] normal = new double[3];
            MathCommon.MathHelper.CalculateNormal(temp, ref normal);
      
            Plane3D planeX = new Plane3D();
            planeX.Position = points[5].point3D();
            planeX.Normal = new Vector3D(-normal[0], -normal[1], -normal[2]);


            Point3D nosePoint = points[4].point3D();
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
            Vector3D jawTranslate = points[1].point3D() - points[0].point3D();

            Matrix3D mJawTranslate = Matrix3D.Identity;
            mJawTranslate.Translate(jawTranslate);


            SphereVisual3D s = new SphereVisual3D();
            s.Center = points[0].point3D();
            s.Material = new DiffuseMaterial(Brushes.Yellow);
            s.Radius = 2;
            this.viewport.Children.Add(s);

            s = new SphereVisual3D();
            s.Center = points[1].point3D();
            s.Material = new DiffuseMaterial(Brushes.Yellow);
            s.Radius = 2;
            this.viewport.Children.Add(s);

            s = new SphereVisual3D();
            s.Center = points[2].point3D();
            s.Material = new DiffuseMaterial(Brushes.Yellow);
            s.Radius = 2;
            this.viewport.Children.Add(s);

            s = new SphereVisual3D();
            s.Center = points[3].point3D();
            s.Material = new DiffuseMaterial(Brushes.Red);
            s.Radius = 2;
            this.viewport.Children.Add(s);
            s = new SphereVisual3D();
            s.Center = points[4].point3D();
            s.Material = new DiffuseMaterial(Brushes.Green);
            s.Radius = 2;
            this.viewport.Children.Add(s);
            s = new SphereVisual3D();
            s.Center = points[5].point3D();
            s.Material = new DiffuseMaterial(Brushes.Blue);
            s.Radius = 2;
            this.viewport.Children.Add(s);

            RectangleVisual3D myCube = new RectangleVisual3D();
            myCube.Origin = planeX.Position; //Set this value to whatever you want your Cube Origen to be.
            myCube.Width = 500; //whatever width you would like.
            myCube.Length = 500;
            myCube.Normal = planeX.Normal;// if you want a cube that is not at some angle then use a vector in the direction of an axis such as this one or <1,0,0> and <0,0,1>
            myCube.Material = new DiffuseMaterial(Brushes.Red);
            this.viewport.Children.Add(myCube);    

            myCube = new RectangleVisual3D();
            myCube.Origin = planeY.Position; //Set this value to whatever you want your Cube Origen to be.
            myCube.Width = 500; //whatever width you would like.
            myCube.Length = 500;
            myCube.Normal = planeY.Normal;// if you want a cube that is not at some angle then use a vector in the direction of an axis such as this one or <1,0,0> and <0,0,1>
            myCube.Material = new DiffuseMaterial(Brushes.Green);
            this.viewport.Children.Add(myCube);

            myCube = new RectangleVisual3D();
            myCube.Origin = planeZ.Position; //Set this value to whatever you want your Cube Origen to be.
            myCube.Width = 500; //whatever width you would like.
            myCube.Length = 500;
            myCube.Normal = planeZ.Normal;// if you want a cube that is not at some angle then use a vector in the direction of an axis such as this one or <1,0,0> and <0,0,1>
            myCube.Material = new DiffuseMaterial(Brushes.Blue);
            this.viewport.Children.Add(myCube);

            ArrowVisual3D arr = new ArrowVisual3D();
            arr.Origin = planeX.Position;
            arr.Direction = xAxis;
            arr.Diameter = 10;
            arr.Fill = Brushes.Red;
            this.viewport.Children.Add(arr);

            arr = new ArrowVisual3D();
            arr.Origin = planeX.Position;
            arr.Direction = yAxis;
            arr.Diameter = 10;
            arr.Fill = Brushes.Green;
            this.viewport.Children.Add(arr);

            arr = new ArrowVisual3D();
            arr.Origin = planeX.Position;
            arr.Direction = zAxis;
            arr.Diameter = 10;
            arr.Fill = Brushes.Blue;
            this.viewport.Children.Add(arr);

            

            foreach(TMatrix t in measured)
            {
              
              //  t.Translate(points[0].X, points[0].Y, points[0].Z);
                Point3D pnt = t.point3D();
                pnt = mJawTranslate.Transform(pnt);
              //  pnt = pnt + jawTranslate;

                Point3D projectedX = ProjectOnPlane(pnt, planeX);
                Point3D projectedY = ProjectOnPlane(pnt, planeY);
                Point3D projectedZ = ProjectOnPlane(pnt, planeZ);

                

                s = new SphereVisual3D();
                s.Center = pnt;
                s.Material = new DiffuseMaterial(Brushes.Pink);
                s.Radius = 0.5;
                this.viewport.Children.Add(s);
                s = new SphereVisual3D();
                s.Center = projectedX;
                s.Material = new DiffuseMaterial(Brushes.Pink); ;
                s.Radius = 0.5;
                this.viewport.Children.Add(s);
                s = new SphereVisual3D();
                s.Center = projectedY;
                s.Material = new DiffuseMaterial(Brushes.Pink);
                s.Radius = 0.5;
                this.viewport.Children.Add(s);
                s = new SphereVisual3D();
                s.Center = projectedZ;
                s.Material = new DiffuseMaterial(Brushes.Pink);
                s.Radius = 0.5;
                this.viewport.Children.Add(s);


                    
                
            }


            this.viewport.ZoomExtents();
        }

        public void ShowStep(int iStep)
        {

            ResetView();

            ArrowVisual3D arr = new ArrowVisual3D();
            arr.Diameter = 0.4;
            arr.Fill = Brushes.Red;

            switch(iStep)
            {
                case 0:               
                    
                    arr.Point1 = new Point3D(5, 5,5);
                    arr.Point2 = new Point3D(2.33, 1.09,5.71);                   
                    this.viewport.Children.Add(arr);

                    ArrowVisual3D arr2 = new ArrowVisual3D();
                    arr2.Diameter = 0.4;
                    arr2.Point1 = new Point3D(3, 3,0);
                    arr2.Point2 = new Point3D(1.86, -1.68,0.22);
                    arr2.Fill = Brushes.Red;
                    this.viewport.Children.Add(arr2);

                    SetCameraRotation(45, -45, 0);
                    break;

                case 1:
                   
                    arr.Point1 = new Point3D(0, -5,5);
                    arr.Point2 = new Point3D(-0.13, -3.81, 0.2);                   
                    this.viewport.Children.Add(arr);
                    SetCameraRotation(45, -45, 0);
                    break;

                case 2:

                    arr.Point1 = new Point3D(0, -5, 5);
                    arr.Point2 = new Point3D(0, -3.51, 1.33);
                    this.viewport.Children.Add(arr);
                    SetCameraRotation(45, -45, 0);
                    break;

                case 3:

                    arr.Point1 = new Point3D(0, -5, 5);
                    arr.Point2 = new Point3D(0, -3.19, 2.26);
                    this.viewport.Children.Add(arr);
                    SetCameraRotation(45, -45, 0);
                    break;

                case 4:

                    arr.Point1 = new Point3D(0, -5, 5);
                    arr.Point2 = new Point3D(0, -2.27, 4.16);
                    this.viewport.Children.Add(arr);
                    SetCameraRotation(45, -45, 0);
                    break;

                case 5:

                    arr.Point1 = new Point3D(8, 8, 8);
                    arr.Point2 = new Point3D(0.04, 2.21, 6.70);
                    this.viewport.Children.Add(arr);
                    SetCameraRotation(45, -45, 0);
                    break;

                case 6:
                    MoveJaw();
                    break;

            }
        }

        public void MoveJaw()
        {
              
            TranslateTransform3D translateTransform3D = new TranslateTransform3D();
            translateTransform3D.OffsetX = 1;
            DoubleAnimation da = new DoubleAnimation();
            da.AutoReverse = true;
            da.From = -0.5;
            da.To = 0.5;
            da.Duration = new Duration(TimeSpan.FromSeconds(3));
            da.RepeatBehavior = RepeatBehavior.Forever;

            DoubleAnimation da2 = new DoubleAnimation();
            da2.AutoReverse = true;
            da2.From = -0.5;
            da2.To = 0;
            da2.Duration = new Duration(TimeSpan.FromSeconds(1.5));
            da2.RepeatBehavior = RepeatBehavior.Forever;

            this.currentModelDown.Transform = translateTransform3D;
        
            translateTransform3D.BeginAnimation(TranslateTransform3D.OffsetXProperty, da);
            translateTransform3D.BeginAnimation(TranslateTransform3D.OffsetZProperty, da2);         
           
        }

        public void SetCameraRotation(double x, double y, double z)
        {
            this.dispatcher.BeginInvoke(new Action(delegate()
            {
                Transform3DGroup group = new Transform3DGroup();

                this.viewport.ResetCamera();
                this.viewport.ZoomExtents();
                this.viewport.Camera.Position = new Point3D(0, -15, 2.5);
                this.viewport.Camera.LookDirection = new Vector3D(0, 1, 0);

                TranslateTransform3D tr = new TranslateTransform3D(0, 0, 0);
  
                RotateTransform3D rtx = new RotateTransform3D();
                rtx.Rotation = new AxisAngleRotation3D(new Vector3D(1, 0, 0), x);

                RotateTransform3D rty = new RotateTransform3D();
                rty.Rotation = new AxisAngleRotation3D(new Vector3D(0, 0, 1), y);

                RotateTransform3D rtz = new RotateTransform3D();
                rtz.Rotation = new AxisAngleRotation3D(new Vector3D(0, 1, 0), z);

                group.Children.Add(tr);
                group.Children.Add(rtx);
                group.Children.Add(rty);
                group.Children.Add(rtz);
                

                foreach(Visual3D v in this.viewport.Children)
                    v.Transform = group;
              

            }));
        }

        public void SetCameraPosition(double x, double y, double z)
        {
            this.dispatcher.BeginInvoke(new Action(delegate()
            {
                this.viewport.Camera.Position = new Point3D(x, y, z);
            }));
        }

        private async Task<Model3DGroup> LoadAsync(string model3DPath, bool freeze, Material defaultMaterial)
        {
            return await Task.Factory.StartNew(() =>
            {
                var mi = new ModelImporter();
                mi.DefaultMaterial = defaultMaterial;
                if (freeze)
                {
                    // Alt 1. - freeze the model 
                    return mi.Load(model3DPath, null, true);
                }

                // Alt. 2 - create the model on the UI dispatcher
                return mi.Load(model3DPath, this.dispatcher);
            });
        }

    }
}
