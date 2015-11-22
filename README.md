# Tail Blazer

[![Join the chat at https://gitter.im/RolandPheasant/TailBlazer](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/RolandPheasant/TailBlazer?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge) [![Build status](https://ci.appveyor.com/api/projects/status/yot4rioy393j52eg?svg=true)](https://ci.appveyor.com/project/RolandPheasant/tailblazer)


In my day to day professional life I am always tailing log files but have always been disappointed with the freebies on offer.The current crop of free ones all look like they were written in the 1990s, are very ugly and have limited functionality.

So I have decided to rectify this by creating a more modern version.  The mission statement is:  

>It has to be fast, intuitive, functionally rich and the code has to be 100% reactive.

So here we go, my first attempt I have come up with this:  

![Tail Blazer](https://github.com/RolandPheasant/TailBlazer/blob/master/Images/Tailing.gif)

That's better. A tail program which is pleasing to my eyes!

## Current feature list

 - Drag and drop to tail a file
 - Virtual file scrolling
 - Highlight new lines
 - Side by side monitoring of files
 - Auto tail or scroll mode
 - Filter lines to match text

## Future feature list

 - Filter lines on regex
 - Find in file
 - Conditionally highlight text
 - Store recent files and folders
 - Automatic creation of columns
 - Pipe filter to file
 - Copy text to clipboard
 - Remember recent files and folders
 - Ability to pin a file

## Future feature list (Folder tail)

 - Find in entire folder
 - Tail entire folder

## Feature requests and any other issues

Feel free to get involved by reporting issues and submitting a feature request. Feedback is welcome and and is required to make this a first class system. But before raising an issue check out [issues](https://github.com/RolandPheasant/TailBlazer/issues) to see whether one has already been raised.  

## Update 1: Building Tail Blazer

There has been an enthusiastic and indeed overwhelming demand for Tail Blazer and many people have asked where can they get it from.  When I get the time I will create an official packaged release but for now anyone who want to use it can either

 - Fork, clone or download the source code and build using Visual Studio 2015
 - Grab the binaries from the [release page](https://github.com/RolandPheasant/TailBlazer/releases), extract and double click TailBlazer.exe to run. These releases will be regularly updated.

## Update 2: Performance

I have been doing some major performance work see this  [Fast scrolling](https://github.com/RolandPheasant/TailBlazer/blob/master/Documents/Fast%20Scrolling.md). And I have to say I am absolutely delighted with the response.


## Roll call of honour

For me to produce this application I have used several community led open source projects all of which I love. So if you like this app I recommend checking out the following.

 - [Dynamic Data](https://gitter.im/RolandPheasant/DynamicData) Rx based collections developed by me. So please forgive the self-plug.
 - [Material Design ToolKit](https://github.com/ButchersBoy/MaterialDesignInXamlToolkit) by my friend and work colleage ButchersBoy.
 - [Dragablz](https://github.com/ButchersBoy/Dragablz)  also by ButchersBoy. I think he is showing off now.
 - [MahApps](https://github.com/MahApps/MahApps.Metro) which was the first open source project to make WPF truly modern.
 - [Structure Map](https://github.com/structuremap/structuremap) which is a dependency injection library which defines what a good API is all about.

I have used more projects than these but these are the ones which I feel to be indispensable for any desk top project.
