using System.Collections.Generic;

namespace S16Editor.domain
{
    public interface IMem
    {
        IEnumerable<int> readpixels(int from, int length);
        int Size { get; }
        IEnumerable<(string name ,byte[] data)> GenerateOutput();
        void writepixels(byte[] data, int from, int length);
    }
}