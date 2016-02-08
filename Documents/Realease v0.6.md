#Tail Blazer v0.6

I have just published a new release of Tail Blazer - get the latest from  [github releases](https://github.com/RolandPheasant/TailBlazer/releases).  or alternatively get the latest from [chocolatey](https://chocolatey.org/packages/tailblazer).

Before going into anything new, I would like to take the opportunity to give a big thanks to the open source community for these two excellent pull requests

 1. Merge assemblies into as single assembly TailBlazer.exe - see [Pull Request #66](https://github.com/RolandPheasant/TailBlazer/pull/66)
 2. Make Tail Blazer available on Chocolatey [Pull Request # 73](https://github.com/RolandPheasant/TailBlazer/pull/73)

These are both a great contribution to this project, so again thanks guys.

So what ele is there?

## Better search and highlight options for Tail Blazer

When you type search or highlight, Tail Blazer will automatically detect whether you are typing regex or plain text 

![Auto detect ](https://github.com/RolandPheasant/TailBlazer/blob/master/Images/RegEx.gif)

If however it gets the wrong then after typing your search text, click the icon to show tail blazer that you know better

![Override](https://github.com/RolandPheasant/TailBlazer/blob/master/Images/RegExOverride.gif)

But that's not all, as you can choose whether any text can be used for a filter or a highlight.

![Search options](https://github.com/RolandPheasant/TailBlazer/blob/master/Images/SearchOptions.gif)

And there's more to come. In subsequent releases I will introduce the ability to choose different highlight colours as well as adding alert options when a tail line matches some search text.

## Better failure handling

Tail Blazer now gives a clear indication when for any reason a file cannot be opened or tailed.

![Error handling](https://github.com/RolandPheasant/TailBlazer/blob/master/Images/ErrorHandling.gif)


## So what's next. Any roadmap?

I am building up to v1.0 which will be the point at which I regard Tail Blazer as being rock solid and error free, as well as fulfilling some of the missing functions which I would expect from any good system.  

These missing functions include better keyboard navigation and saving of layout.  After that I have two major options for what version 2 would include. These are:

 1. Handle external  sources such as unix file system, event viewer
    or tx (see [issue #51](https://github.com/RolandPheasant/TailBlazer/issues/51))
    
 2. Multi-file handle such as folder tailing of collating result from multiple files.

What it will be I do not know yet. But before I start on that I will write some blog posts about how I built Tail Blazer using the uber cool Rx extensions.