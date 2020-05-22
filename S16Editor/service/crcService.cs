using S16Editor.domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S16Editor.service
{
    public class crcService
    {

        public uint ComputeCrc32(IEnumerable<byte> data) {

            var crc32 = new Crc32();
            return crc32.Get(data);

        }   
            
    }
}
