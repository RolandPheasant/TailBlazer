using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Common;
using TailBlazer.Infrastucture;
using TailBlazer.Views.FileOpen;
using TailBlazer.Views.WindowManagement;
using Xunit;

namespace TailBlazer.Fixtures
{
    public class FileOpenFixture
    {
        [Fact]
        public void OpenFile()
        {
            using (var file = new TestFile())
            {
                file.Append("testLine");

                new FileOpenViewModel(null).FileAndDirectoryValidator(file.Info.FullName).Select(f => f.FullName).First().Should().Be(file.Info.FullName);
            }
        }

        [Fact]
        public void OpenEmptyFile()
        {
            using (var file = new TestFile())
            {
                new FileOpenViewModel(null).FileAndDirectoryValidator(file.Info.FullName).Select(f => f.FullName).First().Should().Be(file.Info.FullName);
            }
        }

        [Fact]
        public void OpenNotExistingFile()
        {
            using (var file = new TestFile())
            {
                file.Delete();
                new FileOpenViewModel(null).FileAndDirectoryValidator(file.Info.FullName).Should().BeNull();
            }
        }

        [Fact]
        public void OpenReadOnlyFile()
        {
            using (var file = new TestFile())
            {
                file.SetAttributeReadOnlyTrue();

                new FileOpenViewModel(null).FileAndDirectoryValidator(file.Info.FullName).Select(f => f.FullName).First().Should().Be(file.Info.FullName);
            }
        }

        [Fact]
        public void OpenHiddenFile()
        {
            using (var file = new TestFile())
            {
                file.SetAttributeHiddenTrue();

                new FileOpenViewModel(null).FileAndDirectoryValidator(file.Info.FullName).Select(f => f.FullName).First().Should().Be(file.Info.FullName);
            }
        }

        [Fact]
        public void OpenSystemFile()
        {
            using (var file = new TestFile())
            {
                file.SetAttributeSystemTrue();

                new FileOpenViewModel(null).FileAndDirectoryValidator(file.Info.FullName).Select(f => f.FullName).First().Should().Be(file.Info.FullName);
            }
        }

        [Fact]
        public void OpenEmptyDirectory()
        {
            using (var directory = new TestDirectory())
            {
                Assert.Empty(new FileOpenViewModel(null).FileAndDirectoryValidator(directory.Info.FullName));
            }
        }

        [Fact]
        public void OpenNotExistingFolder()
        {
            using (var directory = new TestDirectory())
            {
                directory.Delete();
                new FileOpenViewModel(null).FileAndDirectoryValidator(directory.Info.FullName).Should().BeNull();
            }
        }

        [Fact]
        public void CheckNumberOfOpenedFilesFromDirectoryWithOneFile()
        {
            using (var directory = new TestDirectory())
            using (var file = new TestFile())
            {
                directory.CopyTestFileToDirectory(file);
                new FileOpenViewModel(null).FileAndDirectoryValidator(directory.Info.FullName).Length.Should().Be(1);
            }
        }

        [Fact]
        public void CheckNumberOfOpenedFilesFromDirectoryWithMultipleFiles()
        {
            using (var directory = new TestDirectory())
            using (var file1 = new TestFile())
            using (var file2 = new TestFile())
            using (var file3 = new TestFile())
            {
                directory.CopyTestFileToDirectory(file1);
                directory.CopyTestFileToDirectory(file2);
                directory.CopyTestFileToDirectory(file3);
                new FileOpenViewModel(null).FileAndDirectoryValidator(directory.Info.FullName).Select(f => f.FullName).Count().Should().Be(3);
            }
        }

        [Fact]
        public void OpenDirectoryWithOneFileInIt()
        {
            using (var directory = new TestDirectory())
            using (var file = new TestFile())
            {
                directory.CopyTestFileToDirectory(file);

                var expected = directory.GetFiles().OrderBy(t => t.FullName);
                var actual = new FileOpenViewModel(null).FileAndDirectoryValidator(directory.Info.FullName).OrderBy(t => t.FullName);
                
                for (int i = 0; i < expected.Count(); i++)
                {
                    expected.ElementAt(i).FullName.ShouldBeEquivalentTo(actual.ElementAt(i).FullName);
                    expected.ElementAt(i).Length.ShouldBeEquivalentTo(actual.ElementAt(i).Length);
                }
            }
        }

        [Fact]
        public void OpenDirectoryWithMultipleFilesInIt()
        {
            using (var directory = new TestDirectory())
            using (var file1 = new TestFile())
            using (var file2 = new TestFile())
            using (var file3 = new TestFile())
            {

                directory.CopyTestFileToDirectory(file1);
                directory.CopyTestFileToDirectory(file2);
                directory.CopyTestFileToDirectory(file3);

                var expected = directory.GetFiles().OrderBy(t => t.FullName); 
                var actual = new FileOpenViewModel(null).FileAndDirectoryValidator(directory.Info.FullName).OrderBy(t => t.FullName);

                for (int i = 0; i < expected.Count(); i++)
                {
                    expected.ElementAt(i).FullName.ShouldBeEquivalentTo(actual.ElementAt(i).FullName);
                    expected.ElementAt(i).Length.ShouldBeEquivalentTo(actual.ElementAt(i).Length);
                }
            }
            


        }
    }
}
