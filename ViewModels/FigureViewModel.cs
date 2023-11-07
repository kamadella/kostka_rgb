using kostka_rgb.Models;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace kostka_rgb.VievModels
{
    class FigureViewModel : ViewModelBase
    {
        private Model3DGroup _shapeModel = new Model3DGroup();
        public Model3DGroup ShapeModel => _shapeModel;
        public bool Cone = true;

        public FigureViewModel()
        {
            if (!Cone)
            {
                for (int i = 0; i < 6; i++)
                {
                    _shapeModel.Children.Add(CreateFaceModel(i, ColorFormula.GetNextGradient()));
                }
            }
            else
            {
                _shapeModel.Children.Add(CreateConeModel(ColorFormula.GetNextGradient()));
            }
        }

        public GeometryModel3D CreateConeModel(BitmapSource hsvTexture)
        {
            int baseSegmentCount = 64;
            double radius = 1;

            MeshGeometry3D mesh = new MeshGeometry3D();

            // Create the top point of the cone.
            Point3D topPoint = new Point3D(0, -2, 0);
            mesh.Positions.Add(topPoint);

            mesh.TextureCoordinates.Add(new Point(0, 0));

            // Create the bottom circle points.
            double angleStep = 2 * Math.PI / baseSegmentCount;
            for (int i = 0; i < baseSegmentCount; i++)
            {
                double angle = i * angleStep;
                double x = radius * Math.Cos(angle);
                double z = radius * Math.Sin(angle);
                mesh.Positions.Add(new Point3D(x, 0, z));
                mesh.TextureCoordinates.Add(new Point(x, z));
            }

            // Add the center point for the bottom circle to create a fan of triangles for the base.
            mesh.Positions.Add(new Point3D(0, 0, 0)); // Center point
            mesh.TextureCoordinates.Add(new Point(0, -1.0)); // Assuming bottom color is at the bottom center of the texture

            // Create the side triangles (cone surface).
            for (int i = 0; i < baseSegmentCount; i++)
            {
                mesh.TriangleIndices.Add(0); // Top point
                mesh.TriangleIndices.Add(i + 1);
                mesh.TriangleIndices.Add(i < baseSegmentCount - 1 ? i + 2 : 1);
            }

            // Create the bottom circle triangles (cone base).
            for (int i = 0; i < baseSegmentCount; i++)
            {
                mesh.TriangleIndices.Add(baseSegmentCount + 1); // Center point
                mesh.TriangleIndices.Add(i < baseSegmentCount - 1 ? i + 2 : 1);
                mesh.TriangleIndices.Add(i + 1);
            }
            
            var material = new DiffuseMaterial(new ImageBrush(ColorFormula.GenerateHSVGradient()));
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
                new Point3D(0, 0, 0), // 0
                new Point3D(1, 0, 0), // 1
                new Point3D(0, 1, 0), // 2
                new Point3D(1, 1, 0), // 3
                new Point3D(0, 0, 1), // 4
                new Point3D(1, 0, 1), // 5
                new Point3D(0, 1, 1), // 6
                new Point3D(1, 1, 1)  // 7
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