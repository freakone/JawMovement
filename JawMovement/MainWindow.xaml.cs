using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MathCommon;
using NavigationLibrary;
using System.Windows.Media.Media3D;
using System.IO;
using HelixToolkit.Wpf;
using System.Windows.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace JawMovementTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        /// <summary>
        /// aktualny krok pomiaru
        /// </summary>
        private int _step = 0;
       

        /// <summary>
        /// uchwyt do nawigacji
        /// </summary>
        private Navigation _nav;
        private ModelViewer _viewer;        
        private IToolInfo _toolMarkerTop;
        private IToolInfo _toolMarkerBottom;
        private IToolInfo _toolPointer;
        private NavigationData _navigationData;
        private TMatrix pointerPoint;
        private TMatrix pointerPointCopy;
        private bool _allVisible = false;

        #region contructor
        public MainWindow()
        {
            InitializeComponent();           

            this._viewer = new ModelViewer(this.view1);
            this.DataContext = this;//this._viewer;
            _navigationData = NavigationData.DeserializeFromXML("measurements.xml");
            
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (_nav != null)
            {
                _nav.Stop();
                _nav.Close();
            }
        }
      
        #endregion

        #region UI


        private void SetLabel(Label lbl, bool visible)
        {
            this.Dispatcher.BeginInvoke(new Action(delegate()
            {
                if (visible)
                {
                    lbl.Content = "widoczny";
                    lbl.Background = Brushes.Green;
                }
                else
                {
                    lbl.Content = "niewidoczny";
                    lbl.Background = Brushes.Red;
                }
            }));
        }

        private void SetNextEnable()
        {
            this.Dispatcher.BeginInvoke(new Action(delegate()
            {

                if (!_allVisible || _step == 7)
                {
                    this.btnNext.IsEnabled = false;
                    return;
                }

                if (_step > 0 && _step < 6 && pointerPoint == null)
                {
                    this.btnNext.IsEnabled = false;
                    return;
                }

                this.btnNext.IsEnabled = true;


            }));
        }

        private void UpdatePointerPoint()
        {
            this.Dispatcher.BeginInvoke(new Action(delegate()
            {
                if (pointerPoint == null)
                {
                    lblPointerPoint.Background = Brushes.Orange;
                    lblPointerPoint.Content = "oczekuje";
                }
                else
                {
                    lblPointerPoint.Content = string.Format("{0:0.0}|{1:0.0}|{2:0.0};rot{3:0.0}|{4:0.0}|{5:0.0}", pointerPoint.X, pointerPoint.Y, pointerPoint.Z, pointerPoint.OX_Deg, pointerPoint.OY_Deg, pointerPoint.OZ_Deg);
                    pointerPointCopy = new TMatrix(pointerPoint);
                    lblPointerPoint.Background = Brushes.Green;
                }
            }));
        }

        private void ClearSteps()
        {
            foreach (CheckBox c in tvStepList.Items)
            {
                c.IsChecked = false;
                c.Foreground = Brushes.Black;
            }

        }

        private void PointerToRef()
        {
            if (pointerPoint == null)
                return;


            _navigationData.pointRef.Add(pointerPointCopy);
            pointerPoint = null;
            UpdatePointerPoint();
        }
        #endregion

        #region buttonz


       private void btnStart_Click(object sender, RoutedEventArgs e)
       {
           if (_nav == null)
           {
               NavigationErrorCodes err = Navigation_Init();
               if (err == NavigationErrorCodes.Ok)
                   btnStart.Content = "Zatrzymaj pomiary";
               RestartMeasurements();

           }
           else
           {
               _nav.Stop();
               _nav.Close();
               _nav = null;
               btnStart.Content = "Rozpocznij pomiary";
           }

           UpdateStep();

       }



       private void btnNext_Click(object sender, RoutedEventArgs e)
       {
           _step++;
           UpdateStep();
       }

       private void CheckBox_Checked(object sender, RoutedEventArgs e)
       {

       }

       private void btnCharts_Click(object sender, RoutedEventArgs e)
       {
           if (_navigationData == null || _navigationData.pointRef.Count < 6 || _navigationData.pointsMeasure.Count == 0)
           {
               MessageBox.Show("Brak danych! Wykonaj pomiary lub wczytaj plik z danymi");
               return;
           }

           ChartWindow cw = new ChartWindow();
           cw.ImportNavigationData(_navigationData);
           cw.Show();

       }

       private void test_Click(object sender, RoutedEventArgs e)
       {
           _viewer.DrawPlane(_navigationData.pointRef, _navigationData.pointsMeasure);
       }


       private void save_Click(object sender, RoutedEventArgs e)
       {

           SaveFileDialog saveFileDialog = new SaveFileDialog();
           saveFileDialog.Filter = "*.xml|*.xml";
           saveFileDialog.ShowDialog();

           if (saveFileDialog.FileName.Length > 1)
           {
               _navigationData._settingsFileName = saveFileDialog.FileName;
               NavigationData.SerializeToXML(_navigationData);
           }
       }

       private void btnRead_Click(object sender, RoutedEventArgs e)
       {
           OpenFileDialog opd = new OpenFileDialog();
           opd.Filter = "*.xml|*.xml";
           opd.CheckFileExists = true;
           opd.ShowDialog();

           if(opd.FileName.Length > 1)
             _navigationData = NavigationData.DeserializeFromXML(opd.FileName);
       }

       private void Window_KeyDown(object sender, KeyEventArgs e)
       {
           if (e.Key == Key.Right && btnNext.IsEnabled)
               btnNext_Click(null, null);
       }
       
       private void RestartMeasurements()
       {
           _navigationData = NavigationData.DeserializeFromXML("temp.xml");
           _step = 0;
           UpdateStep();
       }

       private void btnBack_Click(object sender, RoutedEventArgs e)
       {
           RestartMeasurements();
       }
       #endregion

       private void ToolsPositionUpdate(object sender, ToolsPositionEventArgs e)
       {
           System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
           customCulture.NumberFormat.NumberDecimalSeparator = ".";
           System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

           #region markers process

           _allVisible = true;

           foreach (var tool in e.ToolInfoList)
           {
               if (tool.Status != DeviceMarkerStatus.Ok && tool.ElectroIndex != 2)
                   _allVisible = false;

               switch (tool.ElectroIndex)
               {
                   case 0:
                       SetLabel(lblSkullVisivility, tool.Status == DeviceMarkerStatus.Ok);
                       break;
                   case 1:
                       SetLabel(lblJawVisibility, tool.Status == DeviceMarkerStatus.Ok);
                       if (_step == 6 && _allVisible)
                       {
                           TMatrix t = new TMatrix(tool.TransformationMatrix);
                           _navigationData.pointsMeasure.Add(t);
                       }
                       if (tool.Status == DeviceMarkerStatus.Ok && _step == 0)
                       {
                           pointerPoint = tool.TransformationMatrix;
                           UpdatePointerPoint();
                       }
                       break;
                   case 2:
                       SetLabel(lblMarkerVisibility, tool.Status == DeviceMarkerStatus.Ok);
                       this.Dispatcher.BeginInvoke(new Action(delegate() { cClicked.IsChecked = tool.Clicked; }));
                       if (tool.Status == DeviceMarkerStatus.Ok && tool.Clicked && _step > 0 && _step < 6)
                       {
                           pointerPoint = tool.TransformationMatrix;
                           UpdatePointerPoint();
                       }                        
                       break;

               }
           }

           #endregion

           SetNextEnable();

       }

       private NavigationErrorCodes Navigation_Init()
       {
           if (_nav == null)
           {
               _nav = new Navigation();
               _nav.ToolsPositionUpdate += ToolsPositionUpdate;

               _toolMarkerTop = _nav.AddToolInfo(ToolInfoMarkerSelectioKinds.Index, -1, "", null, 0, null);
               _toolMarkerBottom = _nav.AddToolInfo(ToolInfoMarkerSelectioKinds.Index, -1, "", null, 1, null);
               _toolPointer = _nav.AddToolInfo(ToolInfoMarkerSelectioKinds.Index, -1, "", null, 2, null);
               //_nav.AddToolInfo(ToolInfoMarkerSelectioKinds.Index, -1, "", null, 3, null);

               NavigationErrorCodes err = _nav.Init(NavigationInitKinds.Synchronous, NavigationInitDeviceConfigs.Electro);
               if (err != NavigationErrorCodes.Ok)
               {
                   _nav.Close();
                   _nav = null;
                   return err;
               }

               err = _nav.Play();
               if (err != NavigationErrorCodes.Ok)
               {
                   _nav.Close();
                   _nav = null;
               }

               _nav.SetReferenceTool(_toolMarkerTop);

               return err;
           }

           return NavigationErrorCodes.Ok;
       }




       private void UpdateStep()
       {
           if (_step < 7)
           {
               ClearSteps();
               CheckBox current = ((CheckBox)tvStepList.Items[_step]);

               for (int i = 0; i < _step; i++)
                   ((CheckBox)tvStepList.Items[i]).IsChecked = true;              
               
               current.FontStyle = FontStyles.Italic;
               current.Foreground = Brushes.Green;
           }
           else
           {
               for (int i = 0; i < tvStepList.Items.Count; i++)
                   ((CheckBox)tvStepList.Items[i]).IsChecked = true;       
           }

           _viewer.ShowStep(_step);

           btnNext.IsEnabled = true;
           btnBack.IsEnabled = true;

           switch (_step)
           {
               case 0:
                   btnBack.IsEnabled = false;
                   txtHint.Text = "Umieść referencję na żuchwie i czaszce w zaznaczonych miejscach.";
                   break;

               case 1:
                   PointerToRef();
                   txtHint.Text = "Wskaż markerem punkt środkowy dolnej krawędzi żuchwy i naciśnij przycisk na markerze";
                   break;

               case 2:
                   PointerToRef();
                   txtHint.Text = "Wskaż markerem węzidełko wargi dolnej i naciśnij przycisk na markerze";
                   break;

               case 3:
                   PointerToRef();
                   txtHint.Text = "Wskaż markerem węzidełko wargi górnej i naciśnij przycisk na markerze";
                   break;

               case 4:
                   PointerToRef();
                   txtHint.Text = "Wskaż markerem nasion i naciśnij przycisk na markerze";
                   break;

               case 5:
                   PointerToRef();
                   txtHint.Text = "Wskaż markerem czubek czaszki i naciśnij przycisk na markerze";
                   break;

               case 6:
                   PointerToRef();     
                   txtHint.Text = "Wykonuj wszystkie możliwe ruchy żuchwą aby zarejestrować zakres ruchów. Po zakończeniu naciśnij przycisk 'Dalej'";
                   break;
               case 7:
                   txtHint.Text = "Pomiary zakończone. Możesz wyświetlić wykresy.";
                   break;

           }
       }

  
      
    

    }
}