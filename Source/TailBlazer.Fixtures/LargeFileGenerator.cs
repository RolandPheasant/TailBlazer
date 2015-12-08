using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TailBlazer.Fixtures
{
    public class LargeFileGenerator
    {
        [Fact]
        public void GenerateFile()
        {
            string fileName = @"U:\VeryLargeFile4.txt";

            //var file = File.Create(@"U:\GigFile.txt");

            for (int i = 1; i < 100; i++)
            {
                int start = 1000000 * i + 1;
                File.AppendAllLines(fileName,Enumerable.Range(start,1000000).Select(line=>$"This is line number {line.ToString("0000000000")} in a very large file"));
            } 
        }

        public void AddToFile()
        {



        }
    }
}
