# j64.AlarmServer
Server to integrate Envisalink DSC controller with Smart Things

The reason I wrote this code was not really because there was anything wrong with other AlarmServer packages that work with SmartThings (I mostly got them working), it was more because I wanted to learn a little bit about SmartThings development as well as learn how to use the new cross platform framework that MS has come out with: dotnetcore aka Asp.NetCore 1.0. I am always pretty skeptical when someone tells me they built a better mousetrap so I am not going to say that but am willing to share it with anyone that is interested. I would hope that someone takes the time to write up some nice doco on how to quickly/easily install everything as that would help me out quite a bit.

Right now I don't have an install package for this but can probably get one created this weekend to make it easier to get up and running. As of this moment if you wanted to get it running you would have quite a few manual steps to build it get it running on your local machine.

So, if your interested here are the basic steps to get it running:

1) go to http://get.asp.net and get the dot net core framework and runtime up and going on your machine

2) pull a copy of the code from here: https://github.com/joejarvis64/j64.AlarmServer.git

3) Install the j64*Device.groovy source members as new device types in the the smart things IDE

4) Install the j64*SmartApp.grooy source member as a new smart app in the smart things IDE

5) Click the publish button for all of the device types and smart apps that you loaded into the smart things IDE

6) Build the j64AlarmServer app using dnu restore, dnu build in the various source directories. I use visual studio so this is easy on my windows machine. On my mac i have to run those form a terminal window. I've not tried on linux yet but assume it is the same as on the mac.

7) Go to the webapi direcotry and run the web app with "dnx web" command.

8) Once the web app is running go to the "Configure" page and define all of your partitions, zones, and TPI connection info. Be sure to click save

9) Go to the Install Smart App page on the web app. Enter your client secret key for the smart app you published in step 5

10) click the authorize button and it will install all of your partitions, zones, alarms, etc into smart things

11) Pull up your mobile app and it should all be there under devices named j64xxxx. I organize everything into a "room" within the smart app to make it easier to get at all the zones and partitions.


Is this any easier to get running than the other smart things alarm packages out there? Probably not at this moment. However, I think I could eliminate step 1, 2, 6 and 7 with a nice install package for the app and I can get rid of step 3,4,5 by submitting my smart app/device types to smart things for publication.

In my perfect world this would basically be running the install app and then following the nicely documented process on the "Documentation" page of that web site.

If anyone is up for giving it a try let me know how it goes. I will fix any bugs in the code that you might find and also add any features that might be missing.
