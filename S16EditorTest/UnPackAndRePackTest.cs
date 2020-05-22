using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using S16Editor.domain;
using S16Editor.service;
using S16Editor.ViewModel;

namespace S16EditorTest
{
    [TestClass]
    public class UnPackAndRePackTest
    {
        private romService romService;
        private crcService crcService;
        private string[] files = new string[]
           {

                "./samples/mpr-10371.9",
                "./samples/mpr-10373.10",
                "./samples/mpr-10375.11",
                "./samples/mpr-10377.12",
           };
        private string[] files2 = new string[]
       {

                "./samples/mpr-10372.13",
                "./samples/mpr-10374.14",
                "./samples/mpr-10376.15",
                "./samples/mpr-10378.16",
       };
       private string[] expectedCrc = new string[]
            {
                "7cc86208",
                "b0d26ac9",
                "59b60bd7",
                "17a1b04a"
            };
        private string[] expectedCrc2 = new string[]
      {
                "b557078c",
                "8051e517",
                "f3b8f318",
                "a1062984"
      };

        [TestInitialize]
        public void initialize()
        {
            this.romService = new romService();
            this.crcService = new crcService();
        }

        [TestMethod]
        public void UnpackAndRepackEnsureIsTheSame()
        {
           

            var ordered = files.ToList().OrderBy(x => x).Reverse();
            var expected_data = ordered.Select(x => File.ReadAllBytes(x)).ToList();


            var memObject = romService.LoadParallelByteMem(files) as parallelByteMem;

            var result = memObject.GenerateOutput().ToList();

            var res = result.Zip(expected_data, (x, y) => x.data.SequenceEqual(y));

            Assert.IsTrue(res.All(x => x));
        }

        [TestMethod]
        public void Crc32Check()
        {
          

            var ordered = files.ToList().OrderBy(x => x);
            var data = ordered.Select(x => File.ReadAllBytes(x)).ToList();

            var crcs = data.Select(x => this.crcService.ComputeCrc32(x).ToString("x")).ToList();

            Assert.IsTrue(expectedCrc.SequenceEqual(crcs));
        }

        [TestMethod]
        public void UnpackAndRepackEnsureCrcIsOk()
        {


            var ordered = files2.ToList().OrderBy(x => x).Reverse();
            var expected_data = ordered.Select(x => File.ReadAllBytes(x)).ToList();


            var memObject = romService.LoadParallelByteMem(files2) as parallelByteMem;

            var result = memObject.GenerateOutput().ToList();

            var crcs = result.Select(x => (x.name,crc: this.crcService.ComputeCrc32(x.data).ToString("x"))).ToList();

            Assert.IsTrue(expectedCrc2.Reverse().SequenceEqual(crcs.Select(x => x.crc)));
        }

        [TestMethod]
        public void UnpackModifyAndRepackEnsureCrcIsOk()
        {
            var bmpfile = "./samples/335176-144-35.bmp";

            var ordered = files2.ToList().OrderBy(x => x).Reverse();
            var expected_data = ordered.Select(x => File.ReadAllBytes(x)).ToList();


            var memObject = romService.LoadParallelByteMem(files2) as parallelByteMem;

            MainWindowViewModel.ImportFile(bmpfile,memObject);

            var result = memObject.GenerateOutput().ToList();

            var crcs = result.Select(x => (x.name, crc: this.crcService.ComputeCrc32(x.data).ToString("x"))).ToList();

            var one = result.Last().data;

            for (int i = 0; i < Int32.MaxValue; i++) {

                var bb =BitConverter.GetBytes(i);

                one[one.Length - 4] = bb[0];
                one[one.Length - 3] = bb[1];
                one[one.Length - 2] = bb[2];
                one[one.Length - 1] = bb[3];

                var crc = this.crcService.ComputeCrc32(one).ToString("x");
                Console.WriteLine($"{crc} --- {expectedCrc2.First()} ");
                if (crc == expectedCrc2.First())
                    Console.WriteLine("YEES");
            };

            //Assert.IsTrue(expectedCrc2.Reverse().SequenceEqual(crcs.Select(x => x.crc)));
        }
    }
}
