using System.IO;
using System.Linq;
using System.Text;

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
            string fileName = @"U:\Large Files\WideFile.txt";

            //var file = File.Create(@"U:\WideFile.txt");

            var template = "0123456789abcdefghijklmnopqrstuvwxyz";
            var sb = new StringBuilder();

            for (int i = 0; i < 250; i++)
            {
                sb.Append(i);
                sb.Append("_");
                sb.Append(template);
             
            }

            for (int i = 0; i < 1000; i++)
            {


                File.AppendAllLines(fileName,new[] {sb.ToString()});
            }
        }

        public void AddToFile()
        {



        }
    }
}
