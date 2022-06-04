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

        private readonly Grid _grid;
        private bool _isLoaded = false;

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
            _grid = new Grid();
        }

        #endregion

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
            DrawNodes();
            DrawSwitches();
        }

        private void DrawSubstations()
        {
            foreach (var substationEntity in _grid.SubstationEntities.Values)
            {
                DiffuseMaterial material = new DiffuseMaterial(new SolidColorBrush(Colors.Green));
                MeshGeometry3D mesh = new MeshGeometry3D();

                Point3D vertex1 = new Point3D(-1 + 2 * substationEntity.Y / _grid.Coordinates["maxLon"], -1 + 2 * substationEntity.X / _grid.Coordinates["maxLat"], 0);
                Point3D vertex2 = new Point3D(-0.99 + 2 * substationEntity.Y / _grid.Coordinates["maxLon"], -1 + 2 * substationEntity.X / _grid.Coordinates["maxLat"], 0);
                Point3D vertex3 = new Point3D(-1 + 2 * substationEntity.Y / _grid.Coordinates["maxLon"], -0.99 + 2 * substationEntity.X / _grid.Coordinates["maxLat"], 0);
                Point3D vertex4 = new Point3D(-0.99 + 2 * substationEntity.Y / _grid.Coordinates["maxLon"], -0.99 + 2 * substationEntity.X / _grid.Coordinates["maxLat"], 0);
                Point3D vertex5 = new Point3D(-1 + 2 * substationEntity.Y / _grid.Coordinates["maxLon"], -1 + 2 * substationEntity.X / _grid.Coordinates["maxLat"], 0.01);
                Point3D vertex6 = new Point3D(-0.99 + 2 * substationEntity.Y / _grid.Coordinates["maxLon"], -1 + 2 * substationEntity.X / _grid.Coordinates["maxLat"], 0.01);
                Point3D vertex7 = new Point3D(-1 + 2 * substationEntity.Y / _grid.Coordinates["maxLon"], -0.99 + 2 * substationEntity.X / _grid.Coordinates["maxLat"], 0.01);
                Point3D vertex8 = new Point3D(-0.99 + 2 * substationEntity.Y / _grid.Coordinates["maxLon"], -0.99 + 2 * substationEntity.X / _grid.Coordinates["maxLat"], 0.01);

                mesh.Positions.Add(vertex1);
                mesh.Positions.Add(vertex2);
                mesh.Positions.Add(vertex3);
                mesh.Positions.Add(vertex4);
                mesh.Positions.Add(vertex5);
                mesh.Positions.Add(vertex6);
                mesh.Positions.Add(vertex7);
                mesh.Positions.Add(vertex8);

                mesh.TriangleIndices.Add(2);
                mesh.TriangleIndices.Add(3);
                mesh.TriangleIndices.Add(1);

                mesh.TriangleIndices.Add(3);
                mesh.TriangleIndices.Add(1);
                mesh.TriangleIndices.Add(0);

                mesh.TriangleIndices.Add(7);
                mesh.TriangleIndices.Add(1);
                mesh.TriangleIndices.Add(3);

                mesh.TriangleIndices.Add(7);
                mesh.TriangleIndices.Add(5);
                mesh.TriangleIndices.Add(1);

                mesh.TriangleIndices.Add(6);
                mesh.TriangleIndices.Add(5);
                mesh.TriangleIndices.Add(7);

                mesh.TriangleIndices.Add(6);
                mesh.TriangleIndices.Add(4);
                mesh.TriangleIndices.Add(5);

                mesh.TriangleIndices.Add(6);
                mesh.TriangleIndices.Add(2);
                mesh.TriangleIndices.Add(0);

                mesh.TriangleIndices.Add(2);
                mesh.TriangleIndices.Add(0);
                mesh.TriangleIndices.Add(4);

                mesh.TriangleIndices.Add(2);
                mesh.TriangleIndices.Add(7);
                mesh.TriangleIndices.Add(3);

                mesh.TriangleIndices.Add(2);
                mesh.TriangleIndices.Add(6);
                mesh.TriangleIndices.Add(7);

                mesh.TriangleIndices.Add(0);
                mesh.TriangleIndices.Add(1);
                mesh.TriangleIndices.Add(5);

                mesh.TriangleIndices.Add(0);
                mesh.TriangleIndices.Add(5);
                mesh.TriangleIndices.Add(4);

                GeometryModel3D gm = new GeometryModel3D(mesh, material);
                GridView.Children.Add(gm);
            }
        }

        private void DrawNodes()
        {
            foreach (var substationEntity in _grid.NodeEntities.Values)
            {
                DiffuseMaterial material = new DiffuseMaterial(new SolidColorBrush(Colors.Blue));
                MeshGeometry3D mesh = new MeshGeometry3D();

                Point3D vertex1 = new Point3D(-1 + 2 * substationEntity.Y / _grid.Coordinates["maxLon"], -1 + 2 * substationEntity.X / _grid.Coordinates["maxLat"], 0);
                Point3D vertex2 = new Point3D(-0.99 + 2 * substationEntity.Y / _grid.Coordinates["maxLon"], -1 + 2 * substationEntity.X / _grid.Coordinates["maxLat"], 0);
                Point3D vertex3 = new Point3D(-1 + 2 * substationEntity.Y / _grid.Coordinates["maxLon"], -0.99 + 2 * substationEntity.X / _grid.Coordinates["maxLat"], 0);
                Point3D vertex4 = new Point3D(-0.99 + 2 * substationEntity.Y / _grid.Coordinates["maxLon"], -0.99 + 2 * substationEntity.X / _grid.Coordinates["maxLat"], 0);
                Point3D vertex5 = new Point3D(-1 + 2 * substationEntity.Y / _grid.Coordinates["maxLon"], -1 + 2 * substationEntity.X / _grid.Coordinates["maxLat"], 0.01);
                Point3D vertex6 = new Point3D(-0.99 + 2 * substationEntity.Y / _grid.Coordinates["maxLon"], -1 + 2 * substationEntity.X / _grid.Coordinates["maxLat"], 0.01);
                Point3D vertex7 = new Point3D(-1 + 2 * substationEntity.Y / _grid.Coordinates["maxLon"], -0.99 + 2 * substationEntity.X / _grid.Coordinates["maxLat"], 0.01);
                Point3D vertex8 = new Point3D(-0.99 + 2 * substationEntity.Y / _grid.Coordinates["maxLon"], -0.99 + 2 * substationEntity.X / _grid.Coordinates["maxLat"], 0.01);

                mesh.Positions.Add(vertex1);
                mesh.Positions.Add(vertex2);
                mesh.Positions.Add(vertex3);
                mesh.Positions.Add(vertex4);
                mesh.Positions.Add(vertex5);
                mesh.Positions.Add(vertex6);
                mesh.Positions.Add(vertex7);
                mesh.Positions.Add(vertex8);

                mesh.TriangleIndices.Add(2);
                mesh.TriangleIndices.Add(3);
                mesh.TriangleIndices.Add(1);

                mesh.TriangleIndices.Add(3);
                mesh.TriangleIndices.Add(1);
                mesh.TriangleIndices.Add(0);

                mesh.TriangleIndices.Add(7);
                mesh.TriangleIndices.Add(1);
                mesh.TriangleIndices.Add(3);

                mesh.TriangleIndices.Add(7);
                mesh.TriangleIndices.Add(5);
                mesh.TriangleIndices.Add(1);

                mesh.TriangleIndices.Add(6);
                mesh.TriangleIndices.Add(5);
                mesh.TriangleIndices.Add(7);

                mesh.TriangleIndices.Add(6);
                mesh.TriangleIndices.Add(4);
                mesh.TriangleIndices.Add(5);

                mesh.TriangleIndices.Add(6);
                mesh.TriangleIndices.Add(2);
                mesh.TriangleIndices.Add(0);

                mesh.TriangleIndices.Add(2);
                mesh.TriangleIndices.Add(0);
                mesh.TriangleIndices.Add(4);

                mesh.TriangleIndices.Add(2);
                mesh.TriangleIndices.Add(7);
                mesh.TriangleIndices.Add(3);

                mesh.TriangleIndices.Add(2);
                mesh.TriangleIndices.Add(6);
                mesh.TriangleIndices.Add(7);

                mesh.TriangleIndices.Add(0);
                mesh.TriangleIndices.Add(1);
                mesh.TriangleIndices.Add(5);

                mesh.TriangleIndices.Add(0);
                mesh.TriangleIndices.Add(5);
                mesh.TriangleIndices.Add(4);

                GeometryModel3D gm = new GeometryModel3D(mesh, material);
                GridView.Children.Add(gm);
            }
        }

        private void DrawSwitches()
        {
            foreach (var substationEntity in _grid.SwitchEntities.Values)
            {
                DiffuseMaterial material = new DiffuseMaterial(new SolidColorBrush(Colors.Coral));
                MeshGeometry3D mesh = new MeshGeometry3D();

                Point3D vertex1 = new Point3D(-1 + 2 * substationEntity.Y / _grid.Coordinates["maxLon"], -1 + 2 * substationEntity.X / _grid.Coordinates["maxLat"], 0);
                Point3D vertex2 = new Point3D(-0.99 + 2 * substationEntity.Y / _grid.Coordinates["maxLon"], -1 + 2 * substationEntity.X / _grid.Coordinates["maxLat"], 0);
                Point3D vertex3 = new Point3D(-1 + 2 * substationEntity.Y / _grid.Coordinates["maxLon"], -0.99 + 2 * substationEntity.X / _grid.Coordinates["maxLat"], 0);
                Point3D vertex4 = new Point3D(-0.99 + 2 * substationEntity.Y / _grid.Coordinates["maxLon"], -0.99 + 2 * substationEntity.X / _grid.Coordinates["maxLat"], 0);
                Point3D vertex5 = new Point3D(-1 + 2 * substationEntity.Y / _grid.Coordinates["maxLon"], -1 + 2 * substationEntity.X / _grid.Coordinates["maxLat"], 0.01);
                Point3D vertex6 = new Point3D(-0.99 + 2 * substationEntity.Y / _grid.Coordinates["maxLon"], -1 + 2 * substationEntity.X / _grid.Coordinates["maxLat"], 0.01);
                Point3D vertex7 = new Point3D(-1 + 2 * substationEntity.Y / _grid.Coordinates["maxLon"], -0.99 + 2 * substationEntity.X / _grid.Coordinates["maxLat"], 0.01);
                Point3D vertex8 = new Point3D(-0.99 + 2 * substationEntity.Y / _grid.Coordinates["maxLon"], -0.99 + 2 * substationEntity.X / _grid.Coordinates["maxLat"], 0.01);

                mesh.Positions.Add(vertex1);
                mesh.Positions.Add(vertex2);
                mesh.Positions.Add(vertex3);
                mesh.Positions.Add(vertex4);
                mesh.Positions.Add(vertex5);
                mesh.Positions.Add(vertex6);
                mesh.Positions.Add(vertex7);
                mesh.Positions.Add(vertex8);

                mesh.TriangleIndices.Add(2);
                mesh.TriangleIndices.Add(3);
                mesh.TriangleIndices.Add(1);

                mesh.TriangleIndices.Add(3);
                mesh.TriangleIndices.Add(1);
                mesh.TriangleIndices.Add(0);

                mesh.TriangleIndices.Add(7);
                mesh.TriangleIndices.Add(1);
                mesh.TriangleIndices.Add(3);

                mesh.TriangleIndices.Add(7);
                mesh.TriangleIndices.Add(5);
                mesh.TriangleIndices.Add(1);

                mesh.TriangleIndices.Add(6);
                mesh.TriangleIndices.Add(5);
                mesh.TriangleIndices.Add(7);

                mesh.TriangleIndices.Add(6);
                mesh.TriangleIndices.Add(4);
                mesh.TriangleIndices.Add(5);

                mesh.TriangleIndices.Add(6);
                mesh.TriangleIndices.Add(2);
                mesh.TriangleIndices.Add(0);

                mesh.TriangleIndices.Add(2);
                mesh.TriangleIndices.Add(0);
                mesh.TriangleIndices.Add(4);

                mesh.TriangleIndices.Add(2);
                mesh.TriangleIndices.Add(7);
                mesh.TriangleIndices.Add(3);

                mesh.TriangleIndices.Add(2);
                mesh.TriangleIndices.Add(6);
                mesh.TriangleIndices.Add(7);

                mesh.TriangleIndices.Add(0);
                mesh.TriangleIndices.Add(1);
                mesh.TriangleIndices.Add(5);

                mesh.TriangleIndices.Add(0);
                mesh.TriangleIndices.Add(5);
                mesh.TriangleIndices.Add(4);

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
