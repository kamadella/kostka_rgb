using kostka_rgb.Models;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace kostka_rgb.VievModels
{
    class FigureViewModel : ViewModelBase
    {
        private Model3DGroup _shapeModel = new Model3DGroup();
        private Model3DGroup _cube = new Model3DGroup();
        private Model3DGroup _cone = new Model3DGroup();
        public Model3DGroup ShapeModel
        {
            get => _shapeModel;
            set
            {
                if (_shapeModel != value)
                {
                    _shapeModel = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool Cone = false;

        // ICommand property to switch shapes
        public ICommand ChangeShapeCommand { get; }

        public FigureViewModel()
        {
            ChangeShapeCommand = new RelayCommand(ChangeShape);

            for (int i = 0; i < 6; i++)
            {
                _cube.Children.Add(CreateFaceModel(i, ColorFormula.GetNextGradient()));
            }


            _cone.Children.Add(CreateConeSurfaceModel());
            _cone.Children.Add(CreateConeBaseModel());


            _shapeModel = (Cone)? _cone: _cube;
        }

        private void ChangeShape(object parameter)
        {
            if (parameter is string shape)
            {
                switch (shape)
                {
                    case "Cube":
                        _shapeModel = _cube;
                        break;
                    case "Cone":
                        _shapeModel = _cone;
                        break;
                    default:
                        throw new ArgumentException("Invalid shape type", nameof(shape));
                }

                // Notify the view of the change
                OnPropertyChanged(nameof(ShapeModel));
            }
        }

        public GeometryModel3D CreateConeSurfaceModel()
        {
            int baseSegmentCount = 64; // Number of segments to represent the circle
            double radius = 1; // Radius of the base of the cone
            MeshGeometry3D mesh = new MeshGeometry3D();

            // Create the top point of the cone.
            Point3D topPoint = new Point3D(0, -1, 0); // This is the tip of the cone
            mesh.Positions.Add(topPoint);
            // Texture coordinate for the tip is at the center top of the texture (U = 0.5, V = 0)
            mesh.TextureCoordinates.Add(new Point(0.5, 0.5)); // Texture coordinate for the center point

            // Create the bottom circle points and texture coordinates for the curved surface.
            double angleStep = 2 * Math.PI / baseSegmentCount;
            for (int i = 0; i <= baseSegmentCount; i++) // Include the first point again to close the loop
            {
                double angle = i * angleStep;
                double x = radius * Math.Cos(angle);
                double z = radius * Math.Sin(angle);
                mesh.Positions.Add(new Point3D(x, 1, z)); // This is the base of the cone

                // Map the texture coordinates for the base
                double u = 0.5 + 0.5 * Math.Cos(angle);
                double v = 0.5 + 0.5 * Math.Sin(angle);
                mesh.TextureCoordinates.Add(new Point(u, v));
            }

            // Create the side triangles (cone surface).
            for (int i = 0; i < baseSegmentCount; i++)
            {
                mesh.TriangleIndices.Add(0); // Top point - tip of the cone
                mesh.TriangleIndices.Add(i + 1); // Current base point
                mesh.TriangleIndices.Add(i + 2); // Next base point
            }

            var material = new DiffuseMaterial(new ImageBrush(ColorFormula.GenerateHSVGradient(false)));
            return new GeometryModel3D(mesh, material);
        }

        public GeometryModel3D CreateConeBaseModel()
        {
            int baseSegmentCount = 64;
            MeshGeometry3D mesh = new MeshGeometry3D();

            // Create the center point for the bottom circle.
            mesh.Positions.Add(new Point3D(0, 1, 0));
            mesh.TextureCoordinates.Add(new Point(0.5, 0.5)); // Texture coordinate for the center point

            // Create the bottom circle points and texture coordinates for the base.
            double angleStep = 2 * Math.PI / baseSegmentCount;
            double radius = 1;
            for (int i = 0; i < baseSegmentCount; i++)
            {
                double angle = i * angleStep;
                double x = radius * Math.Cos(angle);
                double z = radius * Math.Sin(angle);
                mesh.Positions.Add(new Point3D(x, 1, z));

                // Map the texture coordinates for the base
                double u = 0.5 + 0.5 * Math.Cos(angle);
                double v = 0.5 + 0.5 * Math.Sin(angle);
                mesh.TextureCoordinates.Add(new Point(u, v));
            }

            // Create the bottom circle triangles (cone base).
            for (int i = 1; i <= baseSegmentCount; i++)
            {
                mesh.TriangleIndices.Add(0); // Center point
                mesh.TriangleIndices.Add(i < baseSegmentCount ? i + 1 : 1);
                mesh.TriangleIndices.Add(i);
            }

            var material = new DiffuseMaterial(new ImageBrush(ColorFormula.GenerateHSVGradient(true)));
            return new GeometryModel3D(mesh, material);
        }


        private GeometryModel3D CreateFaceModel(int faceIndex, BitmapSource texture)
        {
            var mesh = new MeshGeometry3D();
            // Define your points and triangle indices per face
            Point3D[] facePoints = GetFacePoints(faceIndex);
            mesh.Positions = new Point3DCollection(facePoints);

            int[] triangleIndices = GetFaceTriangleIndices(faceIndex);
            mesh.TriangleIndices = new Int32Collection(triangleIndices);

            // Define texture coordinates for the square face
            mesh.TextureCoordinates = new PointCollection
            {
                new Point(0, 0),
                (faceIndex==0||faceIndex==3)? new Point(0, 1):new Point(1, 0),
                new Point(1, 1),
                (faceIndex==0||faceIndex==3)? new Point(1, 0):new Point(0, 1)
            };

            var material = new DiffuseMaterial(new ImageBrush(texture));
            return new GeometryModel3D(mesh, material);
        }

        private Point3D[] GetFacePoints(int faceIndex)
        {
            Point3D[] allPoints = new[]
            {
                new Point3D(-0.5, -0.5, -0.5), // 0
                new Point3D( 0.5, -0.5, -0.5), // 1
                new Point3D(-0.5,  0.5, -0.5), // 2
                new Point3D( 0.5,  0.5, -0.5), // 3
                new Point3D(-0.5, -0.5,  0.5), // 4
                new Point3D( 0.5, -0.5,  0.5), // 5
                new Point3D(-0.5,  0.5,  0.5), // 6
                new Point3D( 0.5,  0.5,  0.5)  // 7
            };

            switch (faceIndex)
            {
                case 0: return new[] { allPoints[6], allPoints[2], allPoints[0], allPoints[4] }; // Left face
                case 1: return new[] { allPoints[7], allPoints[3], allPoints[2], allPoints[6] }; // Top face
                case 2: return new[] { allPoints[7], allPoints[6], allPoints[4], allPoints[5] }; // Back face
                case 3: return new[] { allPoints[7], allPoints[5], allPoints[1], allPoints[3] }; // Right face
                case 4: return new[] { allPoints[3], allPoints[1], allPoints[0], allPoints[2] }; // Front face
                case 5: return new[] { allPoints[5], allPoints[4], allPoints[0], allPoints[1] }; // Bottom face
                default: throw new ArgumentOutOfRangeException(nameof(faceIndex), "Face index must be between 0 and 5.");
            }
        }

        private int[] GetFaceTriangleIndices(int faceIndex)
        {
            // Each face is made up of 2 triangles, so we need 6 indices to define the triangles
            // The winding order is important for the triangles to face outwards - counter-clockwise is the usual in WPF 3D
            return new[] { 0, 1, 2, 0, 2, 3 };
        }

    }
}