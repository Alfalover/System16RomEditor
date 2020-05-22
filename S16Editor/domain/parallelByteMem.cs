using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S16Editor.domain
{
    public class scanresult
    {

        public int offset;
        public int width;
        public int height;

        public override string ToString()
        {
            return $"{offset}->{width}×{height}";
        }
    }



    public class parallelByteMem : IMem
    {
        private List<string> filenames;
        private List<byte> data;
        private int interleaveOrder;

        public parallelByteMem(IEnumerable<(string name,byte[] data)> data)
        {
            // join blocks
            IEnumerable<byte> interleave = this.interleave(data.Select(x => x.data));
            filenames = data.Select(x => x.name).ToList();

            var temp = interleave.SelectMany(x => new byte[] { (byte)((x & 0xF0) >> 4), (byte)(x & 0x0F) }).ToList();

            this.data = temp.ToList();
        }

        private IEnumerable<byte> interleave(IEnumerable<byte[]> data)
        {
            var ldata = data.ToList();
            this.interleaveOrder = ldata.Count;
            int total = ldata.First().Length;

            for (int i = 0; i < total; i++)
            {
                foreach (var block in ldata)
                {
                    yield return block[i];
                }

            }
        }

        public int Size => data.Count;

        public IEnumerable<int> readpixels(int from, int length)
        {
            int pbytes = data.Count;

            for (int i = from; i < from + length; i++)
            {

                yield return Convert.ToInt32(data[i]);
            }   
        }

        public IEnumerable<scanresult> Scan()
        {
            var mark = new List<byte>() { 0xF, 0x0, 0x0, 0xF };

            var current = new scanresult();
            var offset = 0;
            var prevlineLength = 0;
            var qu = new Queue<byte>();

            for (int i = 0; i < data.Count; i++)
            {
                qu.Enqueue(data[i]);
                if (qu.Count > 4) qu.Dequeue();

                if (qu.Count == 4)
                {
                    var content = qu.Select(x => x);
                    if (content.SequenceEqual(mark))
                    {
                        var lineLength = i - offset - 1;
                        if (prevlineLength != lineLength)
                        {
                            yield return current;
                            current = new scanresult();
                            prevlineLength = lineLength;
                            current.offset = offset;
                            current.width = lineLength;

                        } else
                        {
                            current.height++;
                        }

                        offset = i - 1;
                    }
                }
            }

            yield return current;
        }

        public IEnumerable<(string name,byte[] data)> GenerateOutput() {

            var pair = this.data.Where( (x,i) => i % 2 == 0);
            var even = this.data.Where((x, i) => i % 2 == 1);

            var result = pair.Zip(even, (a, b) => (byte)(a << 4 | b));

            
            int number = this.interleaveOrder;

            var lresult = Enumerable.Range(0, number).Select(y => result.Where((x, i) => i % number == y).ToArray());
            return lresult.Zip(filenames,(x,y) => (y,x));
        }

        public void writepixels(byte[] idata, int from, int length)
        {
            for (int i = from; i < from + length; i++)
            {
                data[i] = idata[i - from];
                
            }
        }
    }
}
