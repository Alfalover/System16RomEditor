using S16Editor.domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq; 

namespace S16Editor.service
{
    public class romService
    {

        public IMem LoadParallelBitMem (IEnumerable<string> files)
        {
            var ordered = files.OrderBy(x => x);            
            var data = files.Select(x => File.ReadAllBytes(x));
                             
            return new parallelBitMem(data);
        }

        public IMem LoadParallelByteMem(string[] files)
        {
            var ordered = files.OrderBy(x => x).Reverse();
            var data = ordered.Select(x => (x,File.ReadAllBytes(x)));

            return new parallelByteMem(data);
        }
    }
}
