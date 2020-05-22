using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S16Editor.domain
{
    public class parallelBitMem : IMem
    {

        private IList<BitArray> Data { get; }
        
        public parallelBitMem(IEnumerable<byte[]> data)
        {
            Data = data.Select(x => new BitArray(x)).ToList();
        }

        public IEnumerable<int> readpixels(int from, int length)
        {
            for (int i = from; i < from+length; i++)
            {
                var value = Data.Select((d, b) => {
                                        return (d.Get(i) ? (Math.Pow(2, b)) : 0);
                                                    }
                ).Sum();
                yield return Convert.ToInt32(value);
            }
        }

        public IEnumerable<(string name, byte[] data)> GenerateOutput()
        {
            throw new NotImplementedException();
        }

        public void writepixels(byte[] data, int from, int length)
        {
            throw new NotImplementedException();
        }

        public int Size => Data.Min(x => x.Length/8);


    }
}
