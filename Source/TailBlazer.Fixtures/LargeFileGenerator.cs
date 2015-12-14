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
     //  [Fact]
        public void GenerateFile()
        {
            string fileName = @"C:\Work\File2.txt";

            //var file = File.Create(@"U:\GigFile.txt");

            for (int i = 0; i < 10; i++)
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
