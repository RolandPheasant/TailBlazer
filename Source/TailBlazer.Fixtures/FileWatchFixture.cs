 using System;
 using System.IO;
 using System.Linq;
 using FluentAssertions;
 using Microsoft.Reactive.Testing;
 using TailBlazer.Domain.FileHandling;
 using Xunit;

namespace TailBlazer.Fixtures
{
    public class FileWatchFixture
    {

        [Fact]
        public void Notify()
        {
            var file = Path.GetTempFileName();
            File.Delete(file);
            
            var info = new FileInfo(file);
            var scheduler = new TestScheduler();

            FileNotification result = null;

            using (info.WatchFile(TimeSpan.FromSeconds(1), scheduler).Subscribe(x => result = x))
            {
                scheduler.AdvanceBySeconds(1);
                result.NotificationType.Should().Be(FileNotificationType.Missing);

                File.AppendAllLines(file, Enumerable.Range(1, 10).Select(i => i.ToString()));
                scheduler.AdvanceBySeconds(1);
                result.NotificationType.Should().Be(FileNotificationType.CreatedOrOpened);
                result.NotificationType.Should().NotBe(0);

                File.AppendAllLines(file, Enumerable.Range(11, 10).Select(i => i.ToString()));
                scheduler.AdvanceBySeconds(1);
                result.NotificationType.Should().Be(FileNotificationType.Changed);
                
                File.Delete(file);
                scheduler.AdvanceBySeconds(1);
                result.NotificationType.Should().Be(FileNotificationType.Missing);
            }
        }
    }
}