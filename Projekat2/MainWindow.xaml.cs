using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Projekat.Model;
using Projekat2.Utils;
using Point = System.Windows.Point;

namespace Projekat2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields

        private Point _start = new Point();
        private Point _diffOffset = new Point();
        private readonly int _zoomInMax = 50;
        private readonly int _zoomOutMax = 5;
        private int _zoomCurent = 1;

        private Grid _grid;
        private bool _isLoaded = false;

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            _grid = new Grid();
        }

        #region ZoomPan

        private void ViewPort_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ViewPort.CaptureMouse();
            _start = e.GetPosition(this);
            _diffOffset.X = Translate.OffsetX;
            _diffOffset.Y = Translate.OffsetY;
        }

        private void ViewPort_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ViewPort.ReleaseMouseCapture();
        }

        private void ViewPort_MouseMove(object sender, MouseEventArgs e)
        {
            if (ViewPort.IsMouseCaptured)
            {
                Point end = e.GetPosition(this);
                double offsetX = end.X - _start.X;
                double offsetY = end.Y - _start.Y;
                double w = this.Width;
                double h = this.Height;
                double translateX = (offsetX * 100) / w;
                double translateY = -(offsetY * 100) / h;
                Translate.OffsetX = _diffOffset.X + (translateX / (100 * Scale.ScaleX));
                Translate.OffsetY = _diffOffset.Y + (translateY / (100 * Scale.ScaleX));
            }
        }

        private void ViewPort_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point p = e.MouseDevice.GetPosition(this);
            double scaleX = 1;
            double scaleY = 1;
            if (e.Delta > 0 && _zoomCurent < _zoomInMax)
            {
                scaleX = Scale.ScaleX + 0.1;
                scaleY = Scale.ScaleY + 0.1;
                _zoomCurent++;
                Scale.ScaleX = scaleX;
                Scale.ScaleY = scaleY;
            }
            else if (e.Delta <= 0 && _zoomCurent > -_zoomOutMax)
            {
                scaleX = Scale.ScaleX - 0.1;
                scaleY = Scale.ScaleY - 0.1;
                _zoomCurent--;
                Scale.ScaleX = scaleX;
                Scale.ScaleY = scaleY;
            }
        }

        #endregion

        #region LoadModel

        private void LoadModel_Click(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded)
            {
                _grid.LoadModel();
                DrawEntities();
                DrawConnections();
            }
            _isLoaded = true;
        }

        private void DrawEntities()
        {
            DrawSubstations();
        }

        private void DrawSubstations()
        {
            foreach (var substationEntity in _grid.SubstationEntities)
            {
                double xPos = substationEntity.X;
                double yPos = substationEntity.Y;

                SolidColorBrush brush = new SolidColorBrush(Colors.GreenYellow);

                var material = new DiffuseMaterial(brush);
                var mesh = new MeshGeometry3D();

                GeometryModel3D gm = new GeometryModel3D(mesh, material); 
                GridView.Children.Add(gm);
            }
        }

        private void DrawConnections()
        {
           
        }

        #endregion
    }
}
