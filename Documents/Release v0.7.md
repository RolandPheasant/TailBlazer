#Tail Blazer v0.7

It is not long since the previous version of Tail Blazer was released but I have decided to do another release because I have just completed what is probably the most requested feature - custom colours.  But as ever I never like to under deliver so I thought I would go the extra mile and enable a very rich means of custom colouring and row indication.

Before I outline the new features, you can download the release from either of the following

  - [github releases](https://github.com/RolandPheasant/TailBlazer/releases)
  - [chocolatey](https://chocolatey.org/packages/tailblazer) (there may be a delay before this version is available)

## Custom text highlighting

In earlier versions of Tail Blazer there was a single highlight colour.  Now you can choose from a much wider palette.

![Change colour ](https://github.com/RolandPheasant/TailBlazer/blob/master/Images/Release v0.7/ChangeColours.gif)

## Custom row indicator

Being able to apply a colour of choice to text which has matched some criteria is a standard feature of most file tail programs.  However I want Tail Blazer to have its own twist, so I have enabled custom row indicator icons. How popular this proves to be remains to be seen.

![Change row indicator](https://github.com/RolandPheasant/TailBlazer/blob/master/Images/Release v0.7/ChangeIcon.gif)

## Priority of matched text

The row indicator is particularly useful when you have more text than can be displayed.  However since only one icon can be displayed there is a mechanism to allow the user to decide the order of priority i.e. the first text which is matched in the list is displayed.

![Change priority](https://github.com/RolandPheasant/TailBlazer/blob/master/Images/Release v0.7/ChangePriority.gif)

## One last feature 

This next feature is completely unrelated to the above but I included it because I was able to implement it very quickly. You can now open the file directly or go to the file's containing  folder.

![Open file](https://github.com/RolandPheasant/TailBlazer/blob/master/Images/Release v0.7/OpenFolder.gif)

## So what's next. Any roadmap?

There loads of development coming in the future but more immediately I will be blogging about Tail Blazer with particular reference to it's use of Rx.  Also I need a rest so I think perhaps a couple weeks where I do no open source coding.
