using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Projekat.Model;
using Grid = Projekat2.Utils.Grid;
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
        private Point _rotationStart = new Point();
        private readonly double _rotationScalar = 0.4;
        private readonly int _angleScalar = 180;
        private GeometryModel3D _hitModel;

        private readonly Grid _grid;
        private bool _isLoaded = false;

        private bool _isHidden = false;

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
            _grid = new Grid();
        }

        #endregion

        #region Zoom&Pan&Rotation

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
            if (ViewPort.IsMouseCaptured && e.LeftButton == MouseButtonState.Pressed)
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
            if (ViewPort.IsMouseCaptured && e.MiddleButton == MouseButtonState.Pressed)
            {
                Point end = e.GetPosition(this);
                double offsetX = end.X - _rotationStart.X;
                double offsetY = end.Y - _rotationStart.Y;
                if ((RotationY.Angle + (_rotationScalar) * offsetX < _angleScalar && RotationY.Angle + (_rotationScalar) * offsetX > - _angleScalar))
                {
                    RotationY.Angle += (_rotationScalar) * offsetX;
                }
                _rotationStart = end;
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

        private void ViewPort_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Released) ViewPort.ReleaseMouseCapture();
        }

        private void ViewPort_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_grid.EntitiesChanged)
            {
                ((SolidColorBrush)(_grid.FirstEnd.Material as DiffuseMaterial)?.Brush).Color = _grid.FirstColor;
                ((SolidColorBrush)(_grid.SecondEnd.Material as DiffuseMaterial)?.Brush).Color = _grid.SecondColor;

                _grid.EntitiesChanged = false;
            }
            if (e.MiddleButton != MouseButtonState.Pressed) return;
            _rotationStart = e.GetPosition(this);
            ViewPort.CaptureMouse();
        }

        private void ViewPort_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ViewPort.CaptureMouse();
            Point start = e.GetPosition(ViewPort);

            Point mouseposition = e.GetPosition(ViewPort);
            Point3D testpoint3D = new Point3D(mouseposition.X, mouseposition.Y, 0);
            Vector3D testdirection = new Vector3D(mouseposition.X, mouseposition.Y, 10);

            PointHitTestParameters pointparams = new PointHitTestParameters(mouseposition);
            RayHitTestParameters rayparams = new RayHitTestParameters(testpoint3D, testdirection);

            ViewportHitTest(start);
        }

        private void ViewportHitTest(Point position)
        {
            VisualTreeHelper.HitTest(ViewPort, null, HitTestResult, new PointHitTestParameters(position));

            if (_hitModel == null) return;
            if (_grid.SubstationModels.ContainsKey(_hitModel))
            {
                string txt = "[ID]: " + _grid.SubstationModels[_hitModel].Id + "\n[Name]: " + _grid.SubstationModels[_hitModel].Name;
                MessageBox.Show(txt, "Substation Entity", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            if (_grid.NodeModels.ContainsKey(_hitModel))
            {
                string txt = "[ID]: " + _grid.NodeModels[_hitModel].Id + "\n[Name]: " + _grid.NodeModels[_hitModel].Name;
                MessageBox.Show(txt, "Node Entity", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            if (_grid.SwitchModels.ContainsKey(_hitModel))
            {
                string txt = "[ID]: " + _grid.SwitchModels[_hitModel].Id + "\n[Name]: " + _grid.SwitchModels[_hitModel].Name;
                MessageBox.Show(txt, "Switch Entity", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            if (!_grid.LineModels.ContainsKey(_hitModel)) return;
            {
                if (_grid.EntitiesChanged)
                {
                    ((SolidColorBrush) (_grid.FirstEnd.Material as DiffuseMaterial)?.Brush).Color = _grid.FirstColor;
                    ((SolidColorBrush) (_grid.SecondEnd.Material as DiffuseMaterial)?.Brush).Color = _grid.SecondColor;

                    _grid.EntitiesChanged = false;
                }

                string txt = "[ID]: " + _grid.LineModels[_hitModel].Id + "\n[Name]: " + _grid.LineModels[_hitModel].Name;

                long first = _grid.LineModels[_hitModel].FirstEnd;
                long second = _grid.LineModels[_hitModel].SecondEnd;

                foreach (var kvp in _grid.SubstationModels)
                {
                    if (first == kvp.Value.Id)
                    {
                        _grid.FirstColor = ((SolidColorBrush) (kvp.Key.Material as DiffuseMaterial)?.Brush).Color;
                        ((SolidColorBrush)(kvp.Key.Material as DiffuseMaterial)?.Brush).Color = Colors.NavajoWhite;
                        _grid.FirstEnd = kvp.Key;
                    }
                    else if (second == kvp.Value.Id)
                    {
                        _grid.SecondColor = ((SolidColorBrush) (kvp.Key.Material as DiffuseMaterial)?.Brush).Color;
                        ((SolidColorBrush)(kvp.Key.Material as DiffuseMaterial)?.Brush).Color = Colors.NavajoWhite;
                        _grid.SecondEnd = kvp.Key;
                    }
                }

                foreach (var kvp in _grid.NodeModels)
                {
                    if (first == kvp.Value.Id)
                    {
                        _grid.FirstColor = ((SolidColorBrush)(kvp.Key.Material as DiffuseMaterial)?.Brush).Color;
                        ((SolidColorBrush)(kvp.Key.Material as DiffuseMaterial)?.Brush).Color = Colors.NavajoWhite;
                        _grid.FirstEnd = kvp.Key;
                    }
                    else if (second == kvp.Value.Id)
                    {
                        _grid.SecondColor = ((SolidColorBrush)(kvp.Key.Material as DiffuseMaterial)?.Brush).Color;
                        ((SolidColorBrush)(kvp.Key.Material as DiffuseMaterial)?.Brush).Color = Colors.NavajoWhite;
                        _grid.SecondEnd = kvp.Key;
                    }
                }

                foreach (var kvp in _grid.SwitchModels)
                {
                    if (first == kvp.Value.Id)
                    {
                        _grid.FirstColor = ((SolidColorBrush)(kvp.Key.Material as DiffuseMaterial)?.Brush).Color;
                        ((SolidColorBrush)(kvp.Key.Material as DiffuseMaterial)?.Brush).Color = Colors.NavajoWhite;
                        _grid.FirstEnd = kvp.Key;
                    }
                    else if (second == kvp.Value.Id)
                    {
                        _grid.SecondColor = ((SolidColorBrush)(kvp.Key.Material as DiffuseMaterial)?.Brush).Color;
                        ((SolidColorBrush)(kvp.Key.Material as DiffuseMaterial)?.Brush).Color = Colors.NavajoWhite;
                        _grid.SecondEnd = kvp.Key;
                    }
                }

                _grid.EntitiesChanged = true;

                MessageBox.Show(txt, "Line Entity", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private HitTestResultBehavior HitTestResult(HitTestResult result)
        {
            if (!(result is RayMeshGeometry3DHitTestResult rayHtResult)) return HitTestResultBehavior.Continue;
            _hitModel = rayHtResult.ModelHit as GeometryModel3D;
            return HitTestResultBehavior.Stop;
        }

        #endregion

        #region LoadModel

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
                DiffuseMaterial material = new DiffuseMaterial(new SolidColorBrush(Colors.Crimson));
                MeshGeometry3D mesh = new MeshGeometry3D();

                foreach (var vertex in CreateVerteces(substationEntity.X, substationEntity.Y)) mesh.Positions.Add(vertex);
                SetTriangleIndices(ref mesh);

                GeometryModel3D gm = new GeometryModel3D(mesh, material);
                Transform3DGroup group = new Transform3DGroup();
                TranslateTransform3D translateTransform = new TranslateTransform3D(0, 0, 0);
                group.Children.Add(translateTransform);
                gm.Transform = group;
                _grid.SubstationModels.Add(gm, substationEntity);

                foreach (var substationModel in _grid.SubstationModels.Keys.Where(substationModel => substationModel.Bounds.IntersectsWith(gm.Bounds)))
                {
                    substationEntity.X = _grid.SubstationModels[substationModel].X;
                    substationEntity.Y = _grid.SubstationModels[substationModel].Y;

                    translateTransform.OffsetZ += 0.01;
                    translateTransform.OffsetX += substationModel.Bounds.X - gm.Bounds.X;
                    translateTransform.OffsetY += substationModel.Bounds.Y - gm.Bounds.Y;

                    foreach (var lineEntity in _grid.LineEntities.Values)
                    {
                        if (lineEntity.FirstEnd == substationEntity.Id)
                        {
                            lineEntity.Vertices.First().X = _grid.SubstationModels[substationModel].X;
                            lineEntity.Vertices.First().Y = _grid.SubstationModels[substationModel].Y;
                        }

                        if (lineEntity.SecondEnd != substationEntity.Id) continue;
                        lineEntity.Vertices.Last().X = _grid.SubstationModels[substationModel].X;
                        lineEntity.Vertices.Last().Y = _grid.SubstationModels[substationModel].Y;
                    }
                }

                foreach (var nodeModel in _grid.NodeModels.Keys.Where(nodeModel => nodeModel.Bounds.IntersectsWith(gm.Bounds)))
                {
                    substationEntity.X = _grid.NodeModels[nodeModel].X;
                    substationEntity.Y = _grid.NodeModels[nodeModel].Y;

                    translateTransform.OffsetZ += 0.01;
                    translateTransform.OffsetX += nodeModel.Bounds.X - gm.Bounds.X;
                    translateTransform.OffsetY += nodeModel.Bounds.Y - gm.Bounds.Y;

                    foreach (var lineEntity in _grid.LineEntities.Values)
                    {
                        if (lineEntity.FirstEnd == substationEntity.Id)
                        {
                            lineEntity.Vertices.First().X = _grid.NodeModels[nodeModel].X;
                            lineEntity.Vertices.First().Y = _grid.NodeModels[nodeModel].Y;
                        }

                        if (lineEntity.SecondEnd != substationEntity.Id) continue;
                        lineEntity.Vertices.Last().X = _grid.NodeModels[nodeModel].X;
                        lineEntity.Vertices.Last().Y = _grid.NodeModels[nodeModel].Y;
                    }
                }

                foreach (var switchModel in _grid.SwitchModels.Keys.Where(switchModel => switchModel.Bounds.IntersectsWith(gm.Bounds)))
                {
                    substationEntity.X = _grid.SwitchModels[switchModel].X;
                    substationEntity.Y = _grid.SwitchModels[switchModel].Y;

                    translateTransform.OffsetZ += 0.01;
                    translateTransform.OffsetX += switchModel.Bounds.X - gm.Bounds.X;
                    translateTransform.OffsetY += switchModel.Bounds.Y - gm.Bounds.Y;

                    foreach (var lineEntity in _grid.LineEntities.Values)
                    {
                        if (lineEntity.FirstEnd == substationEntity.Id)
                        {
                            lineEntity.Vertices.First().X = _grid.SwitchModels[switchModel].X;
                            lineEntity.Vertices.First().Y = _grid.SwitchModels[switchModel].Y;
                        }

                        if (lineEntity.SecondEnd != substationEntity.Id) continue;
                        lineEntity.Vertices.Last().X = _grid.SwitchModels[switchModel].X;
                        lineEntity.Vertices.Last().Y = _grid.SwitchModels[switchModel].Y;
                    }
                }

                GridView.Children.Add(gm);
            }
        }

        private void DrawNodes()
        {
            foreach (var nodeEntity in _grid.NodeEntities.Values)
            {
                DiffuseMaterial material = new DiffuseMaterial(new SolidColorBrush(Colors.CornflowerBlue));
                MeshGeometry3D mesh = new MeshGeometry3D();

                foreach (var vertex in CreateVerteces(nodeEntity.X, nodeEntity.Y)) mesh.Positions.Add(vertex);
                SetTriangleIndices(ref mesh);

                GeometryModel3D gm = new GeometryModel3D(mesh, material);
                Transform3DGroup group = new Transform3DGroup();
                TranslateTransform3D translateTransform = new TranslateTransform3D(0, 0, 0);
                group.Children.Add(translateTransform);
                gm.Transform = group;
                _grid.NodeModels.Add(gm, nodeEntity);

                foreach (var substationModel in _grid.SubstationModels.Keys.Where(substationModel => substationModel.Bounds.IntersectsWith(gm.Bounds)))
                {
                    nodeEntity.X = _grid.SubstationModels[substationModel].X;
                    nodeEntity.Y = _grid.SubstationModels[substationModel].Y;

                    translateTransform.OffsetZ += 0.01;
                    translateTransform.OffsetX += substationModel.Bounds.X - gm.Bounds.X;
                    translateTransform.OffsetY += substationModel.Bounds.Y - gm.Bounds.Y;

                    foreach (var lineEntity in _grid.LineEntities.Values)
                    {
                        if (lineEntity.FirstEnd == nodeEntity.Id)
                        {
                            lineEntity.Vertices.First().X = _grid.SubstationModels[substationModel].X;
                            lineEntity.Vertices.First().Y = _grid.SubstationModels[substationModel].Y;
                        }

                        if (lineEntity.SecondEnd != nodeEntity.Id) continue;
                        lineEntity.Vertices.Last().X = _grid.SubstationModels[substationModel].X;
                        lineEntity.Vertices.Last().Y = _grid.SubstationModels[substationModel].Y;
                    }
                }

                foreach (var nodeModel in _grid.NodeModels.Keys.Where(nodeModel => nodeModel.Bounds.IntersectsWith(gm.Bounds)))
                {
                    nodeEntity.X = _grid.NodeModels[nodeModel].X;
                    nodeEntity.Y = _grid.NodeModels[nodeModel].Y;

                    translateTransform.OffsetZ += 0.01;
                    translateTransform.OffsetX += nodeModel.Bounds.X - gm.Bounds.X;
                    translateTransform.OffsetY += nodeModel.Bounds.Y - gm.Bounds.Y;

                    foreach (var lineEntity in _grid.LineEntities.Values)
                    {
                        if (lineEntity.FirstEnd == nodeEntity.Id)
                        {
                            lineEntity.Vertices.First().X = _grid.NodeModels[nodeModel].X;
                            lineEntity.Vertices.First().Y = _grid.NodeModels[nodeModel].Y;
                        }

                        if (lineEntity.SecondEnd != nodeEntity.Id) continue;
                        lineEntity.Vertices.Last().X = _grid.NodeModels[nodeModel].X;
                        lineEntity.Vertices.Last().Y = _grid.NodeModels[nodeModel].Y;
                    }
                }

                foreach (var switchModel in _grid.SwitchModels.Keys.Where(switchModel => switchModel.Bounds.IntersectsWith(gm.Bounds)))
                {
                    nodeEntity.X = _grid.SwitchModels[switchModel].X;
                    nodeEntity.Y = _grid.SwitchModels[switchModel].Y;

                    translateTransform.OffsetZ += 0.01;
                    translateTransform.OffsetX += switchModel.Bounds.X - gm.Bounds.X;
                    translateTransform.OffsetY += switchModel.Bounds.Y - gm.Bounds.Y;

                    foreach (var lineEntity in _grid.LineEntities.Values)
                    {
                        if (lineEntity.FirstEnd == nodeEntity.Id)
                        {
                            lineEntity.Vertices.First().X = _grid.SwitchModels[switchModel].X;
                            lineEntity.Vertices.First().Y = _grid.SwitchModels[switchModel].Y;
                        }

                        if (lineEntity.SecondEnd != nodeEntity.Id) continue;
                        lineEntity.Vertices.Last().X = _grid.SwitchModels[switchModel].X;
                        lineEntity.Vertices.Last().Y = _grid.SwitchModels[switchModel].Y;
                    }
                }

                GridView.Children.Add(gm);
            }
        }

        private void DrawSwitches()
        {
            foreach (var switchEntity in _grid.SwitchEntities.Values)
            {
                DiffuseMaterial material = new DiffuseMaterial(new SolidColorBrush(Colors.Indigo));
                MeshGeometry3D mesh = new MeshGeometry3D();

                foreach (var vertex in CreateVerteces(switchEntity.X, switchEntity.Y)) mesh.Positions.Add(vertex);
                SetTriangleIndices(ref mesh);

                GeometryModel3D gm = new GeometryModel3D(mesh, material);
                Transform3DGroup group = new Transform3DGroup();
                TranslateTransform3D translateTransform = new TranslateTransform3D(0, 0, 0);
                group.Children.Add(translateTransform);
                gm.Transform = group;
                _grid.SwitchModels.Add(gm, switchEntity);


                foreach (var substationModel in _grid.SubstationModels.Keys.Where(substationModel => substationModel.Bounds.IntersectsWith(gm.Bounds)))
                {
                    switchEntity.X = _grid.SubstationModels[substationModel].X;
                    switchEntity.Y = _grid.SubstationModels[substationModel].Y;

                    translateTransform.OffsetZ += 0.01;
                    translateTransform.OffsetX += substationModel.Bounds.X - gm.Bounds.X;
                    translateTransform.OffsetY += substationModel.Bounds.Y - gm.Bounds.Y;

                    foreach (var lineEntity in _grid.LineEntities.Values)
                    {
                        if (lineEntity.FirstEnd == switchEntity.Id)
                        {
                            lineEntity.Vertices.First().X = _grid.SubstationModels[substationModel].X;
                            lineEntity.Vertices.First().Y = _grid.SubstationModels[substationModel].Y;
                        }

                        if (lineEntity.SecondEnd != switchEntity.Id) continue;
                        lineEntity.Vertices.Last().X = _grid.SubstationModels[substationModel].X;
                        lineEntity.Vertices.Last().Y = _grid.SubstationModels[substationModel].Y;
                    }
                }

                foreach (var nodeModel in _grid.NodeModels.Keys.Where(nodeModel => nodeModel.Bounds.IntersectsWith(gm.Bounds)))
                {
                    switchEntity.X = _grid.NodeModels[nodeModel].X;
                    switchEntity.Y = _grid.NodeModels[nodeModel].Y;

                    translateTransform.OffsetZ += 0.01;
                    translateTransform.OffsetX += nodeModel.Bounds.X - gm.Bounds.X;
                    translateTransform.OffsetY += nodeModel.Bounds.Y - gm.Bounds.Y;

                    foreach (var lineEntity in _grid.LineEntities.Values)
                    {
                        if (lineEntity.FirstEnd == switchEntity.Id)
                        {
                            lineEntity.Vertices.First().X = _grid.NodeModels[nodeModel].X;
                            lineEntity.Vertices.First().Y = _grid.NodeModels[nodeModel].Y;
                        }

                        if (lineEntity.SecondEnd != switchEntity.Id) continue;
                        lineEntity.Vertices.Last().X = _grid.NodeModels[nodeModel].X;
                        lineEntity.Vertices.Last().Y = _grid.NodeModels[nodeModel].Y;
                    }
                }

                foreach (var switchModel in _grid.SwitchModels.Keys.Where(switchModel => switchModel.Bounds.IntersectsWith(gm.Bounds)))
                {
                    switchEntity.X = _grid.SwitchModels[switchModel].X;
                    switchEntity.Y = _grid.SwitchModels[switchModel].Y;

                    translateTransform.OffsetZ += 0.01;
                    translateTransform.OffsetX += switchModel.Bounds.X - gm.Bounds.X;
                    translateTransform.OffsetY += switchModel.Bounds.Y - gm.Bounds.Y;

                    foreach (var lineEntity in _grid.LineEntities.Values)
                    {
                        if (lineEntity.FirstEnd == switchEntity.Id)
                        {
                            lineEntity.Vertices.First().X = _grid.SwitchModels[switchModel].X;
                            lineEntity.Vertices.First().Y = _grid.SwitchModels[switchModel].Y;
                        }

                        if (lineEntity.SecondEnd != switchEntity.Id) continue;
                        lineEntity.Vertices.Last().X = _grid.SwitchModels[switchModel].X;
                        lineEntity.Vertices.Last().Y = _grid.SwitchModels[switchModel].Y;
                    }
                }

                GridView.Children.Add(gm);
            }
        }

        private List<Point3D> CreateVerteces(double x, double y) =>
        new List<Point3D>(8)
        {
            new Point3D(-1    + 2 * y / _grid.RelativeMaxLon, -1    + 2 * x / _grid.RelativeMaxLat, 0),
            new Point3D(-0.99 + 2 * y / _grid.RelativeMaxLon, -1    + 2 * x / _grid.RelativeMaxLat, 0),
            new Point3D(-1    + 2 * y / _grid.RelativeMaxLon, -0.99 + 2 * x / _grid.RelativeMaxLat, 0),
            new Point3D(-0.99 + 2 * y / _grid.RelativeMaxLon, -0.99 + 2 * x / _grid.RelativeMaxLat, 0),
            new Point3D(-1    + 2 * y / _grid.RelativeMaxLon, -1    + 2 * x / _grid.RelativeMaxLat, 0.01),
            new Point3D(-0.99 + 2 * y / _grid.RelativeMaxLon, -1    + 2 * x / _grid.RelativeMaxLat, 0.01),
            new Point3D(-1    + 2 * y / _grid.RelativeMaxLon, -0.99 + 2 * x / _grid.RelativeMaxLat, 0.01),
            new Point3D(-0.99 + 2 * y / _grid.RelativeMaxLon, -0.99 + 2 * x / _grid.RelativeMaxLat, 0.01),

        };

        private void SetTriangleIndices(ref MeshGeometry3D mesh)
        {
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
            mesh.TriangleIndices.Add(4);

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
        }

        private void DrawConnections()
        {
            foreach (var lineEntity in _grid.LineEntities.Values)
            {
                if (lineEntity.Vertices.Count > 1)
                    if (Math.Abs(lineEntity.Vertices.First().X - lineEntity.Vertices.Last().X) < 0.00000001
                        && Math.Abs(lineEntity.Vertices.First().Y - lineEntity.Vertices.Last().Y) < 0.00000001) continue;

                for (int i = 0; i < lineEntity.Vertices.Count - 1; i++)
                {
                    double x1Pos = lineEntity.Vertices[i].X;
                    double y1Pos = lineEntity.Vertices[i].Y;

                    double x2Pos = lineEntity.Vertices[i + 1].X;
                    double y2Pos = lineEntity.Vertices[i + 1].Y;

                    DiffuseMaterial material = new DiffuseMaterial(new SolidColorBrush(Colors.DarkSlateGray));
                    MeshGeometry3D mesh = new MeshGeometry3D();

                    double x1 = -1 + 2 * y1Pos / _grid.RelativeMaxLon;
                    double y1 = -1 + 2 * x1Pos / _grid.RelativeMaxLat;
                    double x2 = -1 + 2 * y2Pos / _grid.RelativeMaxLon;
                    double y2 = -1 + 2 * x2Pos / _grid.RelativeMaxLat;

                    mesh.Positions.Add(new Point3D(x1 + 0.005        , y1 + 0.005        , 0.001 * 1));
                    mesh.Positions.Add(new Point3D(x1 + 0.005 + 0.001, y1 + 0.005 + 0.001, 0.001 * 1));
                    mesh.Positions.Add(new Point3D(x1 + 0.005 + 0.001, y1 + 0.005 + 0.001, 0.001 * 2));
                    mesh.Positions.Add(new Point3D(x1 + 0.005        , y1 + 0.005        , 0.001 * 2));
                    mesh.Positions.Add(new Point3D(x2 + 0.005        , y2 + 0.005        , 0.001 * 1));
                    mesh.Positions.Add(new Point3D(x2 + 0.005 + 0.001, y2 + 0.005 + 0.001, 0.001 * 1));
                    mesh.Positions.Add(new Point3D(x2 + 0.005 + 0.001, y2 + 0.005 + 0.001, 0.001 * 2));
                    mesh.Positions.Add(new Point3D(x2 + 0.005        , y2 + 0.005        , 0.001 * 2));

                    mesh.TriangleIndices.Add(0);
                    mesh.TriangleIndices.Add(1);
                    mesh.TriangleIndices.Add(2);

                    mesh.TriangleIndices.Add(0);
                    mesh.TriangleIndices.Add(2);
                    mesh.TriangleIndices.Add(3);

                    mesh.TriangleIndices.Add(0);
                    mesh.TriangleIndices.Add(3);
                    mesh.TriangleIndices.Add(4);

                    mesh.TriangleIndices.Add(3);
                    mesh.TriangleIndices.Add(7);
                    mesh.TriangleIndices.Add(4);


                    mesh.TriangleIndices.Add(2);
                    mesh.TriangleIndices.Add(6);
                    mesh.TriangleIndices.Add(3);

                    mesh.TriangleIndices.Add(3);
                    mesh.TriangleIndices.Add(6);
                    mesh.TriangleIndices.Add(7);

                    mesh.TriangleIndices.Add(1);
                    mesh.TriangleIndices.Add(5);
                    mesh.TriangleIndices.Add(2);

                    mesh.TriangleIndices.Add(2);
                    mesh.TriangleIndices.Add(5);
                    mesh.TriangleIndices.Add(6);

                    mesh.TriangleIndices.Add(0);
                    mesh.TriangleIndices.Add(5);
                    mesh.TriangleIndices.Add(1);

                    mesh.TriangleIndices.Add(0);
                    mesh.TriangleIndices.Add(4);
                    mesh.TriangleIndices.Add(5);

                    mesh.TriangleIndices.Add(4);
                    mesh.TriangleIndices.Add(6);
                    mesh.TriangleIndices.Add(5);

                    mesh.TriangleIndices.Add(4);
                    mesh.TriangleIndices.Add(7);
                    mesh.TriangleIndices.Add(6);

                    GeometryModel3D gm = new GeometryModel3D(mesh, material);
                    GridView.Children.Add(gm);
                    _grid.LineModels.Add(gm, lineEntity);
                }
            }
        }

        #endregion

        #region Buttons

        private void LoadModel_Click(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded)
            {
                _grid.LoadModel();
                DrawEntities();
                DrawConnections();
            }
            _isLoaded = true;
            HideShowInactiveGridBtn.IsEnabled = true;
            SwitchComboBox.IsEnabled = true;
            LineComboBox.IsEnabled = true;
            ConnectionComboBox.IsEnabled = true;
        }

        private void HideShowInactiveGrid(object sender, RoutedEventArgs e)
        {
            if (!_isHidden)
            {
                List<long> ids = (from switchEntity in _grid.SwitchEntities.Values where switchEntity.Status.Equals("Open") select switchEntity.Id).ToList();
                List<long> secondEnds = new List<long>();

                foreach (var kvp in _grid.LineModels.Where(kvp => ids.Any(id => kvp.Value.FirstEnd == id)))
                {
                    secondEnds.Add(kvp.Value.SecondEnd);
                    _grid.HiddenEntities.Add(kvp.Key);
                    GridView.Children.Remove(kvp.Key);
                }

                foreach (var kvp in from kvp in _grid.SubstationModels from secondEnd in secondEnds.Where(secondEnd => secondEnd == kvp.Value.Id) select kvp)
                {
                    _grid.HiddenEntities.Add(kvp.Key);
                    GridView.Children.Remove(kvp.Key);
                }

                foreach (var kvp in from kvp in _grid.NodeModels from secondEnd in secondEnds.Where(secondEnd => secondEnd == kvp.Value.Id) select kvp)
                {
                    _grid.HiddenEntities.Add(kvp.Key);
                    GridView.Children.Remove(kvp.Key);
                }

                foreach (var kvp in from kvp in _grid.SwitchModels from secondEnd in secondEnds.Where(secondEnd => secondEnd == kvp.Value.Id) select kvp)
                {
                    _grid.HiddenEntities.Add(kvp.Key);
                    GridView.Children.Remove(kvp.Key);
                }

                HideShowInactiveGridBtn.Content = "Show Inactive Grid";
                _isHidden = true;
            }
            else
            {
                foreach (var gm in _grid.HiddenEntities) GridView.Children.Add(gm);
                _grid.HiddenEntities.Clear();

                HideShowInactiveGridBtn.Content = "Hide Inactive Grid";
                _isHidden = false;
            }
        }

        private void SwitchComboBox_DropDownClosed(object sender, EventArgs e)
        {
            switch (SwitchComboBox.Text)
            {
                case "Open Switches":
                    foreach (var gm in _grid.BrushedSwitches) ((SolidColorBrush) (gm.Material as DiffuseMaterial)?.Brush).Color = Colors.Indigo;
                   _grid.BrushedSwitches.Clear();

                    foreach (var kvp in _grid.SwitchModels.Where(kvp => kvp.Value.Status.Equals("Open")))
                    {
                        ((SolidColorBrush)(kvp.Key.Material as DiffuseMaterial)?.Brush).Color = Colors.DarkGreen;
                        _grid.BrushedSwitches.Add(kvp.Key);
                    }
                    break;
                case "Closed Switches":
                    foreach (var gm in _grid.BrushedSwitches) ((SolidColorBrush)(gm.Material as DiffuseMaterial)?.Brush).Color = Colors.Indigo;
                    _grid.BrushedSwitches.Clear();

                    foreach (var kvp in _grid.SwitchModels.Where(kvp => kvp.Value.Status.Equals("Closed")))
                    {
                        ((SolidColorBrush)(kvp.Key.Material as DiffuseMaterial)?.Brush).Color = Colors.DarkRed;
                        _grid.BrushedSwitches.Add(kvp.Key);
                    }
                    break;
                case "All Switches":
                    foreach (var gm in _grid.BrushedSwitches) ((SolidColorBrush)(gm.Material as DiffuseMaterial)?.Brush).Color = Colors.Indigo;
                    _grid.BrushedSwitches.Clear();
                    break;
            }
        }

        private void LinesComboBox_DropDownClosed(object sender, EventArgs e)
        {
            switch (LineComboBox.Text)
            {
                case "Low Resistance":
                    foreach (var gm in _grid.BrushedLines) ((SolidColorBrush)(gm.Material as DiffuseMaterial)?.Brush).Color = Colors.DarkSlateGray;
                    _grid.BrushedLines.Clear();

                    foreach (var kvp in _grid.LineModels.Where(kvp => kvp.Value.R < 1.0f))
                    {
                        ((SolidColorBrush)(kvp.Key.Material as DiffuseMaterial)?.Brush).Color = Colors.DarkRed;
                        _grid.BrushedLines.Add(kvp.Key);
                    }
                    break;
                case "Medium Resistance":
                    foreach (var gm in _grid.BrushedLines) ((SolidColorBrush)(gm.Material as DiffuseMaterial)?.Brush).Color = Colors.DarkSlateGray;
                    _grid.BrushedLines.Clear();

                    foreach (var kvp in _grid.LineModels.Where(kvp => kvp.Value.R >= 1.0f && kvp.Value.R <= 2.0f))
                    {
                        ((SolidColorBrush)(kvp.Key.Material as DiffuseMaterial)?.Brush).Color = Colors.DarkOrange;
                        _grid.BrushedLines.Add(kvp.Key);
                    }
                    break;
                case "High Resistance":
                    foreach (var gm in _grid.BrushedLines) ((SolidColorBrush)(gm.Material as DiffuseMaterial)?.Brush).Color = Colors.DarkSlateGray;
                    _grid.BrushedLines.Clear();

                    foreach (var kvp in _grid.LineModels.Where(kvp => kvp.Value.R > 2.0f))
                    {
                        ((SolidColorBrush)(kvp.Key.Material as DiffuseMaterial)?.Brush).Color = Colors.Yellow;
                        _grid.BrushedLines.Add(kvp.Key);
                    }
                    break;
                case "All Lines":
                    foreach (var gm in _grid.BrushedLines) ((SolidColorBrush)(gm.Material as DiffuseMaterial)?.Brush).Color = Colors.DarkSlateGray;
                    _grid.BrushedLines.Clear();
                    break;
            }
        }

        private void ConnectionComboBox_DropDownClosed(object sender, EventArgs e)
        {
            switch (ConnectionComboBox.Text)
            {
                case "Less Than 3":
                    foreach (var gm in _grid.HiddenEntitiesConn) GridView.Children.Add(gm);
                    _grid.HiddenEntitiesConn.Clear();

                    foreach (var conn in _grid.NumberOfConnectionsByEntity.Where(conn => conn.Value < 3))
                    {
                        foreach (var kvp in _grid.SubstationModels.Where(kvp => kvp.Value.Id == conn.Key))
                        {
                            GridView.Children.Remove(kvp.Key);
                            _grid.HiddenEntitiesConn.Add(kvp.Key);
                        }

                        foreach (var kvp in _grid.NodeModels.Where(kvp => kvp.Value.Id == conn.Key))
                        {
                            GridView.Children.Remove(kvp.Key);
                            _grid.HiddenEntitiesConn.Add(kvp.Key);
                        }

                        foreach (var kvp in _grid.SwitchModels.Where(kvp => kvp.Value.Id == conn.Key))
                        {
                            GridView.Children.Remove(kvp.Key);
                            _grid.HiddenEntitiesConn.Add(kvp.Key);
                        }
                    }
                    break;
                case "From 3 To 5":
                    foreach (var gm in _grid.HiddenEntitiesConn) GridView.Children.Add(gm);
                    _grid.HiddenEntitiesConn.Clear();

                    foreach (var conn in _grid.NumberOfConnectionsByEntity.Where(conn => conn.Value >= 3 && conn.Value < 5))
                    {
                        foreach (var kvp in _grid.SubstationModels.Where(kvp => kvp.Value.Id == conn.Key))
                        {
                            GridView.Children.Remove(kvp.Key);
                            _grid.HiddenEntitiesConn.Add(kvp.Key);
                        }

                        foreach (var kvp in _grid.NodeModels.Where(kvp => kvp.Value.Id == conn.Key))
                        {
                            GridView.Children.Remove(kvp.Key);
                            _grid.HiddenEntitiesConn.Add(kvp.Key);
                        }

                        foreach (var kvp in _grid.SwitchModels.Where(kvp => kvp.Value.Id == conn.Key))
                        {
                            GridView.Children.Remove(kvp.Key);
                            _grid.HiddenEntitiesConn.Add(kvp.Key);
                        }
                    }
                    break;
                case "Greater Than 5":
                    foreach (var gm in _grid.HiddenEntitiesConn) GridView.Children.Add(gm);
                    _grid.HiddenEntitiesConn.Clear();

                    foreach (var conn in _grid.NumberOfConnectionsByEntity.Where(conn => conn.Value > 5))
                    {
                        foreach (var kvp in _grid.SubstationModels.Where(kvp => kvp.Value.Id == conn.Key))
                        {
                            GridView.Children.Remove(kvp.Key);
                            _grid.HiddenEntitiesConn.Add(kvp.Key);
                        }

                        foreach (var kvp in _grid.NodeModels.Where(kvp => kvp.Value.Id == conn.Key))
                        {
                            GridView.Children.Remove(kvp.Key);
                            _grid.HiddenEntitiesConn.Add(kvp.Key);
                        }

                        foreach (var kvp in _grid.SwitchModels.Where(kvp => kvp.Value.Id == conn.Key))
                        {
                            GridView.Children.Remove(kvp.Key);
                            _grid.HiddenEntitiesConn.Add(kvp.Key);
                        }
                    }
                    break;
                case "Show All Entities":
                    foreach (var gm in _grid.HiddenEntitiesConn) GridView.Children.Add(gm);
                    _grid.HiddenEntitiesConn.Clear();
                    break;
            }
        }

        #endregion
    }
}
