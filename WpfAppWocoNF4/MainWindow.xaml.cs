using System.IO;
using Microsoft.Win32;
using System.Windows;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Globalization;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using System.Diagnostics;
using System.Windows.Media;
using WpfAppWocoNF4.Models;

namespace WpfAppWoCoNF4
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Repository _repository;

        public MainWindow()
        {
            InitializeComponent();

            InitializeSettings();

            _repository = new Repository(); 
            DataContext = this;

            SetupViewportCamera();
            SetupClosedShellCamera();

        }

        private void SetupViewportCamera()
        {
            var camera = new PerspectiveCamera
            {
                Position = new Point3D(15, -25, 15), 
                LookDirection = new Vector3D(-15, 25, -15),
                UpDirection = new Vector3D(0, 0, 1),
            };
            viewport.Camera = camera;
        }

        private void SetupClosedShellCamera()
        {
            var camera = new PerspectiveCamera
            {
                Position = new Point3D(15, -25, 15), 
                LookDirection = new Vector3D(-15, 25, -15),
                UpDirection = new Vector3D(0, 0, 1),
            };
            shellviewport.Camera = camera;
        }

        private static void InitializeSettings()
        {
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");
        }

        private void XmlFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*",
                Title = "Open XML File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string fileName = openFileDialog.FileName;
                FileNameLabel.Visibility = Visibility.Visible;
                FileNameLabel.Content = $"Bestand: {Path.GetFileName(fileName)}";
                DetailPanel.Visibility = Visibility.Visible;

                _repository.LoadFromXml(fileName);


                LocationLabel.Content = $"Locatie: {_repository.Location.Name}";
                LocationLabel.Visibility = Visibility.Visible;

                SpaceDataGrid.ItemsSource = _repository.GetAllSummerized();
                DrawBuilding();
            }
        }

        private void DrawBuilding()
        {
            var spaces = _repository.GetAll();

            IEnumerable<ClosedShell> closedShells = spaces.Select(s => s.ShellGeometry.ClosedShell);
            Draw(closedShells, viewport);
        }

        private void Draw(IEnumerable<ClosedShell> closedShells, HelixViewport3D currentviewport)
        {
            currentviewport.Children.Clear();
            List<Point3D> allPoints = new List<Point3D>();
            foreach (var closedShell in closedShells)
            {
                var polyLoops = closedShell.PolyLoops;

                var wireframe = new LinesVisual3D { Color = Colors.Black, Thickness = 1 };

                foreach (var polyLoop in polyLoops)
                {
                    var points = polyLoop.CartesianPoints.Select( p => p.Coordinates.Select(c => c.Value).ToArray())
                        .Where(arr => arr.Count() == 3)
                        .Select(arr => new Point3D(arr[0], arr[1], arr[2]))
                        .ToList();

                    allPoints.AddRange(points);

                    var pointCount = points.Count;
                    for (int p = 0; p < pointCount; p++)
                    {
                        if (p == pointCount - 1)
                        {
                            // Connect last point to the first point
                            wireframe.Points.Add(points[p]);
                            wireframe.Points.Add(points[0]);
                        }
                        else
                        {
                            wireframe.Points.Add(points[p]);
                            wireframe.Points.Add(points[p + 1]);
                        }

                    }
                }

                currentviewport.Children.Add(wireframe);
            }

            var viewportChildren = currentviewport.Children.OfType<LinesVisual3D>().SelectMany(c => c.Points);


            var offsetX = viewportChildren.Min(p => p.X);
            var offsetY = viewportChildren.Min(p => p.Y);
            var offsetZ = viewportChildren.Min(p => p.Z);
            viewportChildren = viewportChildren.Select(p => new Point3D(p.X - offsetX, p.Y - offsetY, p.Z - offsetZ));

            var maxX = viewportChildren.Max(p => p.X);
            var maxY = viewportChildren.Max(p => p.Y);
            var maxZ = viewportChildren.Max(p => p.Z);
            Debug.WriteLine($"Min: ({offsetX},{offsetY},{offsetZ}), Max: ({maxX},{maxY},{maxZ})");

            
            var viewportWidth = 15; 
            var viewportHeight = 15;
            var scale = Math.Min(viewportWidth / maxX, viewportHeight / maxY); // * 0.8;

            viewportChildren = viewportChildren.Select(p => new Point3D(p.X * scale, p.Y * scale, p.Z * scale));

            // Compute center of all spaces
            double centerX = viewportChildren.Average(p => p.X);
            double centerY = viewportChildren.Average(p => p.Y);
            double centerZ = viewportChildren.Average(p => p.Z);

            foreach (var child in currentviewport.Children.OfType<LinesVisual3D>())
            {
                for (int i = 0; i < child.Points.Count; i++)
                {
                    var p = child.Points[i];
                    child.Points[i] = new Point3D((p.X - offsetX) * scale - centerX, (p.Y - offsetY) * scale - centerY, (p.Z - offsetZ) * scale - centerZ);
                }
            }
        }

        private void SpaceDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var selected = SpaceDataGrid.SelectedItem as SpaceViewModel;

            if (selected != null)
            {
                ValideerButton.Visibility = Visibility.Visible;

                NameTextBlock.Text = selected.Name;
                AreaTextBlock.Text = selected.Area.ToString();
                VolumeTextBlock.Text = selected.Volume.ToString();
                PointsDataGrid.ItemsSource = selected.CartesianPoints?.Select(c => new { X = c.X, Y = c.Y, Z = c.Z });


                IEnumerable<ClosedShell> closedShells = new List<ClosedShell> { selected.ClosedShell };
                Draw(closedShells, shellviewport);

            }
        }

        private void ValideerButton_OnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Het valideren van het volume en de oppervlakte is nog niet gemaakt.",
                "Valideren",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}