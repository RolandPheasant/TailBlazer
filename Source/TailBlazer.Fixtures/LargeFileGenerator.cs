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
            string fileName = @"U:\Large Files\SuperGiantFile.txt";

            //var file = File.Create(@"U:\GigFile.txt");

            for (int i = 0; i < 1000; i++)
            {
                int start = 1000000 * i + 1;
                File.AppendAllLines(fileName,Enumerable.Range(start,1000000).Select(line=>$"This is line number {line.ToString("0000000000")} in a very large file"));
            } 
        }

     //  [Fact]
        public void GenerateWideLinesInFile()
        {
           // string fileName = @"U:\Large Files\WideFile.txt";
            string fileName = @"c:\work\LargeFiles\WideFile.txt";
           //s var file = File.Create();

            var template = "0123456789abcdefghijklmnopqrstuvwxyz";
            var sb = new StringBuilder();


            long x;
            for (int i = 0; i < 1000; i++)
            {
                for (int j = 0; i < 200; i++)
                {
                    sb.Append(j);
                    sb.Append("_");
                    sb.Append(template);
                }

                File.AppendAllLines(fileName,new string[] {sb.Append(i).ToString()});
            }
        }

        public void AddToFile()
        {



        }
    }
}
