# Tail Blazer is fast

I have been doing some performance work over the last several days as Tail Blazer will only be useful to anyone if it can handle large files. I have managed to achieve what I believe to be impressive performance by indexing the files. 

Indexing has allowed 3 things

 - Rapid lookup of lines of text as a user scrolls or as the log is modified
 - True virtualisation where the only log entry lines in memory are those visible on the screen 
 - Reduced memory consumption

The proof of the pudding is in the eating so look at this:

![Fast scrolling](https://github.com/RolandPheasant/TailBlazer/blob/master/Images/FastScrolling.gif)

The scrolling is quick with a 76 MB file with almost 700k lines.

Ah but I hear you think what about if the logs are being written to rapidly.  Fair question which is answered in the image below where thousands of log lines are being written as the user scrolls.  So I generated a very busy log file and tried scrolling. 

![Fast scrolling when busy](https://github.com/RolandPheasant/TailBlazer/blob/master/Images/FastScrollingWhenBusy.gif)

And the scrolling is still quick.

Admittedly these log files are being scanned on my machine which is powerful and has an SSD so obviously I need to test the app  over a network share. I would also be happy to receive feedback from about how it performs for you.

## Further performance work

I have not tested this in Gigabyte files so I am not sure how it will perform. I suspect I will have to improve the initial indexing speed. Also I will be able to improve filtering speed with some parallel processing.  

Please feed back your experiences and when you do let me know the size of the file, number of lines and any other info you think useful.

But being I only started this project 2 weeks ago I am very happy for now.