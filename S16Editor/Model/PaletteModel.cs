using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace S16Editor.Model
{
    public class PaletteModel
    {
        private BitmapPalette palette;
        private string name;


        public PaletteModel(string name,BitmapPalette palette)
        {
            this.Name = name;
            this.Palette = palette;
        }

        public BitmapPalette Palette
        {
            get { return palette; }
            private set { palette = value; }
        }

        public string Name
        {
            get { return name; }
            private set { name = value; }
        }


    }
}
