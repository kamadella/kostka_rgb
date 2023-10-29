using kostka_rgb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace kostka_rgb.VievModels
{
    class CubeViewModel : ViewModelBase
    {
        private ColorFormula _colorFormula;

        public BitmapSource CubeGradient => _colorFormula.GradientImage;

        public CubeViewModel()
        {
            _colorFormula = new ColorFormula();
        }
    }
}
