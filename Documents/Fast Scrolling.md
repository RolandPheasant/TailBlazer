# Tail Blazer has just got faster

I have manage to achieve significant performance improvements in tail blazer by indexing files.  This has allowed 3 things

 - Rapid lookup of lines of text as a user scrolls or as the log is modified
 - True virtualisation where the only log entry lines in memory are those visible on the screen 
 - Reduced memory consumption

The proof of the pudding is in the eating so look at this:

![Fast scrolling](https://github.com/RolandPheasant/TailBlazer/blob/master/Images/FastScrolling.gif)

The scrolling is quick with a 76 MB file with almost 700k lines.

Ah but I hear you think what about if the logs are being written to rapidly.  So I generated a very busy log file and tried scrolling to see what would happen.  I abide by the maxim of a picture is worth a thousand words, so here is a picture of a rapidly updating log file whilst a user is interacting.

![Fast scrolling when busy](https://github.com/RolandPheasant/TailBlazer/blob/master/Images/FastScrollingWhenBusy.gif)

And the scrolling is still quick. So many people have said to me WPF is slow! This proves otherwise.

Admittedly these log files are being scanned on my machine which is powerful and has an SSD so obviously I need to test the app  over a network share.

## Further performance work and feedback

I have not tested this for gigabyte sized files so I am not sure how it will perform. I suspect I will have to improve the initial indexing speed and perhaps have to look at whether I need another indexing strategy.Additionally I know I will be able to improve the speed of filtering by using parallel processing.

I would love you tech people, testers or system adminstrators out there to trial this app and feed back your experiences.  Should you do so, let me know the size of the file, number of lines and any other info you think useful. The more feedback I get the better I can make this app. 

But being as I only started this project 2 weeks ago I am very happy for now.

For you techs out there feel free to build this yourself. For the non-techs or people without Visual Studio 2015 I will regularly be putting releases  @ [Tail Blazer Release Page](https://github.com/RolandPheasant/TailBlazer/releases) where all you have to do is extract the file and double click TrailBlazer.exe
