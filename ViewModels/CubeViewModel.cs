using kostka_rgb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace kostka_rgb.VievModels
{
    class CubeViewModel : ViewModelBase
    {
        private Model3DGroup _cubeModel;
        public Model3DGroup CubeModel => _cubeModel;

        public CubeViewModel()
        {
            _cubeModel = new Model3DGroup();
            for (int i = 0; i < 6; i++)
            {
                ColorFormula colorFormula = new ColorFormula(); // Make sure this selects the correct formula
                _cubeModel.Children.Add(CreateFaceModel(i, colorFormula.GradientImage));
            }
        }

        private GeometryModel3D CreateFaceModel(int faceIndex, BitmapSource texture)
        {
            var mesh = new MeshGeometry3D();
            
            //To jest do poprawy, na razie po prostu niektóre idą w złe miejsca więc je przekładam (można zrobić gdzie indziej)
            
            if(faceIndex==0)faceIndex = 4;
            else if(faceIndex==4)faceIndex = 0;
            if (faceIndex == 5) faceIndex = 3;
            else if(faceIndex==3) faceIndex = 5;



            // Define your points and triangle indices per face
            Point3D[] facePoints = GetFacePoints(faceIndex);
            mesh.Positions = new Point3DCollection(facePoints);

            int[] triangleIndices = GetFaceTriangleIndices(faceIndex);
            mesh.TriangleIndices = new Int32Collection(triangleIndices);

            // Define texture coordinates for the square face
            mesh.TextureCoordinates = new PointCollection
        {
            new Point(0, 0),
            new Point(1, 0),
            new Point(1, 1),
            new Point(0, 1)
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
                case 0: return new[] { allPoints[2], allPoints[3], allPoints[1], allPoints[0] }; // Front face
                case 1: return new[] { allPoints[7], allPoints[3], allPoints[2], allPoints[6] }; // Top face
                case 2: return new[] { allPoints[7], allPoints[6], allPoints[4], allPoints[5] }; // Back face
                case 3: return new[] { allPoints[1], allPoints[5], allPoints[4], allPoints[0] }; // Bottom face
                case 4: return new[] { allPoints[6], allPoints[2], allPoints[0], allPoints[4] }; // Left face
                case 5: return new[] { allPoints[3], allPoints[7], allPoints[5], allPoints[1] }; // Right face
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