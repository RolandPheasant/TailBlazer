# ![Comet](Images/CometGreen.svg "Tail Blazer") Tail Blazer

[![Join the chat at https://gitter.im/RolandPheasant/TailBlazer](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/RolandPheasant/TailBlazer?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge) [![Build status](https://ci.appveyor.com/api/projects/status/yot4rioy393j52eg?svg=true)](https://ci.appveyor.com/project/RolandPheasant/tailblazer) [![GitHub issues](https://img.shields.io/github/issues/RolandPheasant/TailBlazer.svg)](https://github.com/RolandPheasant/TailBlazer/issues)


In my day to day professional life I am always tailing log files but have always been disappointed with the utilities on offer. The current crop of free ones all look like they were written in the 1990s, are very ugly and have limited functionality.

So I have decided to rectify this by creating a more modern version.  The mission statement is:  

>It has to be fast, intuitive, functionally rich and the code has to be 100% reactive.

After a few iterations I have come up with this:  

![Tail Blazer](Images/Tailing.gif)

That's better. A tail program which is pleasing to my eyes!

## Current feature list

 - Drag and drop to tail a file
 - Virtual file scrolling
 - Highlight new lines (can be disabled)
 - Side by side monitoring of files
 - Auto tail or scroll mode
 - Filter lines to match text
 - Apply multiple searches and toogle between search results
 - View search result in original position in file
 - Copy to clipboard
 - Remembers recent files
 - Handle extemely large files (I have opened a 45 Gb file)
 - Dark and light theme

## Future feature list

 - Filter lines on regex
 - Conditionally highlight text
 - Automatic creation of columns
 - Pipe filter to file
 - Ability to pin a file
 - Plug in to external providers (see [#51](https://github.com/RolandPheasant/TailBlazer/issues/51))

## Future feature list (Folder tail)

 - Find in entire folder
 - Tail entire folder

## Feature requests and any other issues

Feel free to get involved by reporting issues and submitting a feature request. Feedback is welcome and and is required to make this a first class system. But before raising an issue check out [issues](https://github.com/RolandPheasant/TailBlazer/issues) to see whether one has already been raised.  

## Building Tail Blazer

There has been an enthusiastic and indeed overwhelming demand for Tail Blazer and many people have asked where can they get it from.  When I get the time I will create an official packaged release but for now anyone who want to use it can either

 - Fork, clone or download the source code and build using Visual Studio 2015
 - Grab the binaries from the [release page](https://github.com/RolandPheasant/TailBlazer/releases), extract and double click TailBlazer.exe to run. These releases will be regularly updated.

## Very large files and fast scrolling

Tail Blazer can easily handle a file of any size. The largest file I have tested was 47 Gb which was the maximum file size I could create before my disk would become full.

![Dark theme](Images/47GbFile.gif)


## User Settings

Don't like the light background? Change it to dark.

![Dark theme](Images/LightAndDarkTheme.gif)

Text too small? Then zoom.

![Zoom](Images/Zoom.gif)

Don't like new line highlight? Turn it off. 

![Zoom](Images/NoHighlight.gif)

## Roll call of honour

For me to produce this application I have used several community led open source projects all of which I love. So if you like this app I recommend checking out the following.

 - [Dynamic Data](https://gitter.im/RolandPheasant/DynamicData) Rx based collections developed by me.
 - [Material Design ToolKit](https://github.com/ButchersBoy/MaterialDesignInXamlToolkit) by my friend and work colleage ButchersBoy. This brings WPF into the 21st century.
 - [Dragablz](https://github.com/ButchersBoy/Dragablz)  also by ButchersBoy. I think he is showing off now.
 - [MahApps](https://github.com/MahApps/MahApps.Metro) which was the first open source project to make WPF truly modern.
 - [Structure Map](https://github.com/structuremap/structuremap) which is a dependency injection library which defines what a good API is all about.

I have used more projects than these but these are the ones which I feel to be indispensable for any desk top project.
