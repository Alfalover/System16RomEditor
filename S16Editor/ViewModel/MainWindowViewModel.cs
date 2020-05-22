using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using S16Editor.domain;
using S16Editor.Model;
using S16Editor.service;

namespace S16Editor.ViewModel
{
    public class MainWindowViewModel : BindableObject
    {
        private readonly romService romService;
        private readonly bitmapService bmpService;
        private readonly IEnumerable<PaletteModel> paletteList;
        private readonly List<int> tileSizes = new List<int> { 8, 16, 32, 64, 128, 256 };
        private readonly List<string> modes = new List<string> { "Square", "Rectangular" };

        private int selectedTileSize;
        private WriteableBitmap _bitmap;
        private PaletteModel _selectedPalette;
        private IMem memObject;


        public MainWindowViewModel()
        {
            this.romService = new romService();
            this.bmpService = new bitmapService();
            this.paletteList =  new List<PaletteModel>(){
                    new PaletteModel("WebPalette",BitmapPalettes.WebPalette),
                    new PaletteModel("Halftone64",BitmapPalettes.Halftone64),
                    new PaletteModel("Gray256",BitmapPalettes.Gray256) ,
                    new PaletteModel("Gray16",BitmapPalettes.Gray16),
                    new PaletteModel("Gray4",BitmapPalettes.Gray4)
                };
            this.SelectedPalette = this.paletteList.First();
            this.selectedOffset = 0;
            this.SelectedWidth = 88;
            this.SelectedHeight = 256;
            this.SelectedMode = "Rectangular";
            this.SelectedTileSize = 8;
        }

        public ICommand OpenFilesCommand { get { return new DelegateCommand(() => this.LoadFiles(), () => true); } }
        public ICommand OpenByteFilesCommand { get { return new DelegateCommand(() => this.LoadByteFiles(), () => true); } }
        public ICommand ScanCommand { get { return new DelegateCommand(() => this.Scan(), () => true); } }
        public ICommand SaveCommand { get { return new DelegateCommand(() => this.Save(), () => true); } }
        public ICommand ImportCommand { get { return new DelegateCommand(() => this.Import(), () => true); } }
        public ICommand UpdateRomCommand { get { return new DelegateCommand(() => this.UpdateRom(), () => true); } }

        private void UpdateRom()
        {
            var data = this.MemObject.GenerateOutput();

            foreach(var x in data)
            {
                File.WriteAllBytes(x.name, x.data);
            }
        }

        private async void Save()
        {

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Bmp (*.bmp)| *.bmp";

            dialog.FileName = $"{selectedScan.offset}-{selectedScan.width}-{selectedScan.height}.bmp";

            var result = dialog.ShowDialog();

            if (result == true)
            {

                using (FileStream fileStream = new FileStream(dialog.FileName, FileMode.OpenOrCreate))
                {
                    BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(this.Bitmap));
                    encoder.Save(fileStream);
                }
            }

        }

       
        private void Import()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "bmp (*.bmp)| *.bmp";

            dialog.Multiselect = true;

            var result = dialog.ShowDialog();

            if (result == true)
            {
                foreach(string filename in dialog.FileNames)
                {
                    ImportFile(filename,this.MemObject);

                }
            }

        }

        public static void ImportFile(string filename,IMem memobject)
        {
            var parts = Path.GetFileNameWithoutExtension(filename).Split('-').Select(x => Convert.ToInt32(x)).ToArray();
             Uri fname = new Uri(filename, UriKind.RelativeOrAbsolute); 

         

            BmpBitmapDecoder bd = new BmpBitmapDecoder(fname, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.None);

            int width = parts[1];
            int height = parts[2];
            int size = width * height;
            var data = new byte[size];
            bd.Frames.Single().CopyPixels(data, width, 0);

            memobject.writepixels(data, parts[0], size);
        }

        private void Scan()
        {
            if(memObject is parallelByteMem pbm)
            {
                this.ScanList= pbm.Scan().Where(x =>x.height > 1).ToList();
            }
        }

        private void LoadFiles()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Any (*.*)| *.*";

            dialog.Multiselect = true;

            var result = dialog.ShowDialog();

            if (result == true)
            {
                this.MemObject = romService.LoadParallelBitMem(dialog.FileNames);
            }

        }


        private void LoadByteFiles()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Any (*.*)| *.*";

            dialog.Multiselect = true;

            var result = dialog.ShowDialog();

            if (result == true)
            {

                this.MemObject = romService.LoadParallelByteMem(dialog.FileNames);
            }

        }

        private void GenerateBitmap()
        {
            if (this.MemObject != null)
            {
                switch (this.SelectedMode) {

                    case "Square":
                        var maxheight = this.MemObject.Size / SelectedWidth;
                        var height = Math.Min(maxheight, SelectedHeight);
                        var bitmap = bmpService.GenerateSerialNxNWritableBitmap(MemObject, this.SelectedWidth, height, this.SelectedTileSize, this.SelectedPalette.Palette);
                        this.Bitmap = bitmap;
                        break;

                    case "Rectangular":
                        var bitmap2 = bmpService.GenerateSerialNxMWritableBitmap(MemObject,this.SelectedOffset, this.SelectedWidth, this.SelectedHeight, this.SelectedWidth,SelectedHeight, this.SelectedPalette.Palette);
                        this.Bitmap = bitmap2;
                        break;

                }
            }
        }

        public WriteableBitmap Bitmap
        {
            get { return _bitmap; }
            set
            {
                _bitmap = value;
                this.RaisePropertyChangedEvent("Bitmap");
            }
        }

        public PaletteModel SelectedPalette
        {
            get { return _selectedPalette; }
            set
            {
                _selectedPalette = value;
                this.RaisePropertyChangedEvent("SelectedPalette");
                this.UpdatePalette();
            }
        }

        public List<int> TileSizes => this.tileSizes;
        public List<string> Modes => this.modes;


        public int SelectedTileSize
        {
            get { return selectedTileSize; }
            set {
                    selectedTileSize = value;
                    RaisePropertyChangedEvent("SelectedTileSize");
                    this.GenerateBitmap();
            }
        }


        public int MaxWidthSize
        {
            get {
                if (this.MemObject != null) {
                    return MemObject.Size / 128 ;
                }
                return 8;
            }

        }

        public int MaxOffset
        {
            get
            {
                if (this.MemObject != null)
                {
                    return MemObject.Size;
                }
                return 8;
            }

        }

        private int selectedWidth;
        private int selectedHeight;
        private string selectedMode;
        private int selectedOffset;
        private IEnumerable<scanresult> scanList;
        private scanresult selectedScan;

        public int SelectedWidth
        {
            get { return selectedWidth; }
            set {
                selectedWidth = value;
                RaisePropertyChangedEvent("SelectedWidth");
                this.GenerateBitmap();
            }
        }

        public int SelectedOffset
        {
            get { return selectedOffset; }
            set
            {
                selectedOffset = value;
                RaisePropertyChangedEvent("SelectedOffset");
                this.GenerateBitmap();
            }
        }

        public int SelectedHeight
        {
            get { return selectedHeight; }
            set
            {
                selectedHeight = value;
                RaisePropertyChangedEvent("selectedHeight");
                this.GenerateBitmap();
            }
        }

        public string SelectedMode { get { return this.selectedMode; }
            set {
                selectedMode = value;
                RaisePropertyChangedEvent("SelectedMode");
                this.GenerateBitmap();
            } }

        public scanresult SelectedScan
        {
            get { return this.selectedScan; }
            set
            {
                selectedScan = value;
                RaisePropertyChangedEvent("SelectedScan");

                if (selectedScan != null)
                {
                    this.SelectedOffset = selectedScan.offset;
                    this.SelectedWidth = selectedScan.width;
                    this.SelectedHeight = selectedScan.height;
                }
            }
        }

        private void UpdatePalette()
        {
            if(this.Bitmap != null)
            {
                var bitmp = new WriteableBitmap(this.Bitmap.PixelWidth,this.Bitmap.PixelHeight,96,96,this.Bitmap.Format,this.SelectedPalette.Palette);
                var pixels = new int[this.Bitmap.PixelWidth* this.Bitmap.PixelHeight];
                this.Bitmap.CopyPixels(pixels, this.Bitmap.PixelWidth, 0);
                bitmp.WritePixels(new System.Windows.Int32Rect(0, 0, this.Bitmap.PixelWidth, this.Bitmap.PixelHeight), pixels, this.Bitmap.PixelWidth, 0);
                this.Bitmap = bitmp;

            }
        }

        public IEnumerable<PaletteModel> Palettes
        {

            get
            {
                return this.paletteList;

            }
        }

        public IMem MemObject
        {
            get { return memObject; }
            set
            {
                memObject = value;
                GenerateBitmap();
                ((DelegateCommand)ScanCommand).Check();
                RaisePropertyChangedEvent("MemObject");
                RaisePropertyChangedEvent("MaxWidthSize");
                RaisePropertyChangedEvent("MaxOffset");
            }
        }

        public IEnumerable<scanresult> ScanList {
           get { return scanList; }
            set {
                this.scanList = value;
                RaisePropertyChangedEvent("ScanList");
            }
        }
    }
}
