using S16Editor.domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace S16Editor.service
{
    public class bitmapService
    {

        public WriteableBitmap GenerateSerialNxNWritableBitmap(IMem pixels, int width, int height,int tileSize, BitmapPalette palette)
        {

             
            var wb = new WriteableBitmap(width, height, 96, 96, PixelFormats.Indexed8 , palette);

            int tileLinearLength = tileSize * tileSize;
            int tilewidth = width / tileSize;
            int tileheight = height / tileSize;


            for (int tj = 0; tj < tileheight; tj++)
            {
                for (int ti = 0; ti < tilewidth; ti++)
                {

                    var left = ti * tileSize;
                    var top = tj * tileSize;
                    var offset = (ti * tileLinearLength) + (tj * tileLinearLength * tilewidth);
                    var arr = pixels.readpixels(offset, tileLinearLength);

                    // Reverse X
                    var ar1 = arr.Select((x,idx) => new { x, idx } )
                                 .GroupBy(x => x.idx / tileSize)
                                 .Select(x => x.Select(y => y.x).Reverse());



                    // Reverse Y
                    arr = ar1.SelectMany(x => x);

                    var ar3 = arr.Select(x => Convert.ToByte(x)).ToArray();
                    wb.WritePixels(new Int32Rect(left, top, tileSize, tileSize), ar3, tileSize, 0);
                }
            }


            return wb;
        }


        public WriteableBitmap GenerateSerialNxMWritableBitmap(IMem pixels,int loffset, int width, int height, int tileSizeX, int tileSizeY, BitmapPalette palette)
        {


            var wb = new WriteableBitmap(width, height, 96, 96, PixelFormats.Indexed8, palette);

            int tileLinearLength = tileSizeX * tileSizeY;
            int tilewidth = width / tileSizeX;
            int tileheight = height / tileSizeY;


            for (int tj = 0; tj < tileheight; tj++)
            {
                for (int ti = 0; ti < tilewidth; ti++)
                {

                    var left = ti * tileSizeX;
                    var top = tj * tileSizeY;
                    var offset = loffset + (ti * tileLinearLength) + (tj * tileLinearLength * tilewidth);
                    var arr = pixels.readpixels(offset, tileLinearLength);

                    //// Reverse X
                    //var ar1 = arr.Select((x, idx) => new { x, idx })
                    //             .GroupBy(x => x.idx / tileSizeX)
                    //             .Select(x => x.Select(y => y.x).Reverse());



                    //// Reverse Y
                    //arr = ar1.SelectMany(x => x);

                    var ar3 = arr.Select(x => Convert.ToByte(x)).ToArray();
                    wb.WritePixels(new Int32Rect(left, top, tileSizeX, tileSizeY), ar3, tileSizeX, 0);
                }
            }


            return wb;
        }

    }
}
