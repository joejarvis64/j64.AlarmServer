# Step 1 - Install the Asp.NetCore Framework
Go to http://get.asp.net and install a copy of the dot net core framework on the machine you will use to run j64AlarmServer.  It is a pretty easy install process, just click the "Install for Mac" or "Install for Windows" button and run the executable that is downloaded.

# Step 2 - Download a copy of the j64AlarmServer
I do not have a installer package yet so you will need to download and install it manually.  You will need to be familiar with github to do this.  The easiest way is to clone a copy of the repository into a directory on your machine.  There is also an option to download a zip file from there at which you would just unzip it onto your local machine.  The github repository for j64AlarmServer is located here: https://github.com/joejarvis64/j64.AlarmServer.git

# Step 3 - Install the j64Alarm Smart App
Eventually I will get these published on smart things but for now you have to manually add these smart apps and device types into your developer account on smart things.  First make sure you have an account setup here:  https://graph.api.smartthings.com/login/auth.  It is free for everyone so just sign up if you don't have one already.

Next, select the SmartApps option at the top of the web page to show all of the smart apps you have already installed.  Once there, click on the big green "New SmartApp" button on the right side of the page.

 ![Smart Apps Page](images/SmartAppPicture.png "Smart Things Smart Apps Page")

The next page that comes up will have a form to create a new smart app.  You should select the "From Code" option at the top of the page.

 ![New Smart Apps Page](images/NewSmartApp1.png "New Smart App Page")

That will open up a blank text box.  Copy all of the code from j64alarmSmartApp.goovy into that text box.  Then click the Create button at the bottom of the page.

That will bring up the editor for the smart app.  At this point you will want to click the "Publish" button at the top of that page.  This will publish the smart app and make it avaialble for use under your personal ID.

 ![New Smart Apps Page](images/NewSmartApp2.png "Publish Page")

Now that you have published the app you will need to get an Oauth key that will be used down on step #9.  To do this click the "App Settings" button next the the publish button.  That will take you to the following page.  Open up the Oauth Section and click "Enable Oauth in Smart App" button. You will need the Oauth Client ID & Oauth Client Secret down in step #9.  

Be sure to click the Update button at the bottom of this page or it will not actually save that Oauth information.

 ![Smart Apps Page](images/OauthExample.png "Oauth Example")


# Step 4 - Install all of the j64 Device Types
The process for install custom device types is almost identical to smart apps.  The only difference is that you will select the "Device Handlers" option at the top of the page.  Once there you will click on "Create New Device Handler", then select the "From Code" option.  Finally just like you did for the smart app you will need to publish the device handler once you have created it.

You will want to crate custom device handlers for each of the following files:
* j64AlarmDevice.goovy
* j64ContactZoneDevice.groovy
* j64MotionZoneDevice.groovy
* j64PartitionDevice.groovy

I use this device handler so that I can integrate the alarm system with the Amazon Echo.  So i can say things like "Alexa, turn on security" or "Alexa, turn off security".  It is not required for the alarm server but is a nice addition if you have an amazon echo.
* j64AlarmAlexaDevice.groovy 

# Step 5 - Build the j64AlarmServer app

The exact steps for this build will vary slightly depending on whether you are running on a mac, windows or linux.  The following instructions are for a mac but should also apply for the other envronments. 

1. Bring up a terminal session and change to the directory where you cloned the respository in step 2 above

2. change to the src directory that contains each of the projects.  There are 3 projects that need to be restored and built.  It should look something like this:
 > ![Src Directory](images/srcdirectory.png "src directory")
 
3. Next change to the j64.AlarmServer directory and restore it with this command:
  > dnu restore
  
  You will see a bunch of packages being downloaded from github.  This is normal the first time you run and it may take a minute or two to get everything downloaded.
  
4. Next, build the j64.AlarmServer project
  > dnu build
  
  Again, you will see a bunch of messages flying across the screen.  The last messages displayed should indicate that it built successfully.
   > ![Build Message](images/BuildSuccess.png "Build Mesasge")
   
5. Restore & Build the Moon.AspNet.Authentication.Basic project
   ```
   cd Moon.AspNet.Authentication.Basic
   dnu restore
   dnu build
   ```
   Again, you should get a message indicating that the build succeeded.
   
6. Restore & Build the j64.AlarmServer.WebApi
   ```
   cd j64.AlarmServer.WebApi
   dnu restore
   dnu build
   ```
Again, you should get a message indicating that the build succeeded.  If you are getting errors with the build it likely means that the Asp.NetCore framework did not install properly. I have tested building and running all of this on a windows and mac machine so it does work.  On windows i have used visual studio so am not 100% certain of the command line parameters.  i.e. you may need to do a little digging around if this does not compile perfectly.  Let me know on the forum of any issues you might hit with it.  
 
# Step 6 - Run the Web App

Now you are ready to run the app.  Change ot the j64.AlarmServer.WebApi directory and run the app with "dnx web".

```
cd j64.AlarmServer.WebApi
dnx web
```


At this point you will see some messages displaying on the screen that look something like this:

```
Joes-iMac:j64.AlarmServer.WebApi joe$ dnx web
info: Microsoft.Data.Entity.Storage.Internal.RelationalCommandBuilderFactory[1]
      Executed DbCommand (5ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      PRAGMA foreign_keys=ON;
info: Microsoft.Data.Entity.Storage.Internal.RelationalCommandBuilderFactory[1]
      Executed DbCommand (2ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT COUNT(*) FROM "sqlite_master" WHERE "name" = '__EFMigrationsHistory' AND "type" = 'table';
info: Microsoft.Data.Entity.Storage.Internal.RelationalCommandBuilderFactory[1]
      Executed DbCommand (0ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      PRAGMA foreign_keys=ON;
info: Microsoft.Data.Entity.Storage.Internal.RelationalCommandBuilderFactory[1]
      Executed DbCommand (0ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT COUNT(*) FROM "sqlite_master" WHERE "name" = '__EFMigrationsHistory' AND "type" = 'table';
info: Microsoft.Data.Entity.Storage.Internal.RelationalCommandBuilderFactory[1]
      Executed DbCommand (0ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      PRAGMA foreign_keys=ON;
info: Microsoft.Data.Entity.Storage.Internal.RelationalCommandBuilderFactory[1]
      Executed DbCommand (0ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT "MigrationId", "ProductVersion"
      FROM "__EFMigrationsHistory"
      ORDER BY "MigrationId";
info: Microsoft.Data.Entity.Storage.Internal.RelationalCommandBuilderFactory[1]
      Executed DbCommand (0ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      PRAGMA foreign_keys=ON;
info: Microsoft.Data.Entity.Storage.Internal.RelationalCommandBuilderFactory[1]
      Executed DbCommand (9ms) [Parameters=[@__normalizedUserName_0='?'], CommandType='Text', CommandTimeout='30']
      SELECT "u"."Id", "u"."AccessFailedCount", "u"."ConcurrencyStamp", "u"."Email", "u"."EmailConfirmed", "u"."LockoutEnabled", "u"."LockoutEnd", "u"."NormalizedEmail", "u"."NormalizedUserName", "u"."PasswordHash", "u"."PhoneNumber", "u"."PhoneNumberConfirmed", "u"."SecurityStamp", "u"."TwoFactorEnabled", "u"."UserName"
      FROM "AspNetUsers" AS "u"
      WHERE "u"."NormalizedUserName" = @__normalizedUserName_0
      LIMIT 1
Hosting environment: Production
Now listening on: http://0.0.0.0:2064
Application started. Press Ctrl+C to shut down.
info: Microsoft.Data.Entity.Storage.Internal.RelationalCommandBuilderFactory[1]
      Executed DbCommand (0ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      PRAGMA foreign_keys=ON;
info: Microsoft.Data.Entity.Storage.Internal.RelationalCommandBuilderFactory[1]
      Executed DbCommand (0ms) [Parameters=[@__user_Id_0='?'], CommandType='Text', CommandTimeout='30']
      SELECT "uc"."ClaimType", "uc"."ClaimValue"
```

One thing to notice is the line near the bottom that show what port it is listening on.  This line means that it is listening on all of your network interfaces (0.0.0.0) on TCP port 2064.   
```
Now listening on: http://0.0.0.0:2064
```

# Step 7 - Open the App

Now that you have a web server running, you can bring up a browser on the machine and go to the app.  So you would open this url:  http://localhost:2064

Log into the site with userId admin and password Admin_01.  Once logged in you should see a home screen that looks something like this:

 > ![Home Screen](images/SampleHomeScreen.png "Home Screen")
 
 You can click on the "Hello admin!" button in the top right and change the password for the admin id.  This is definitely a recommended first step.  If you ever forget your password you can delete a file called j64.AlarmServer.db in the WebApi directory and restart the app.  That will reset the id to its default value.

# Step 8 - Configure your System, Partition & Zones

Click on the "Configure" button at the top of the page.  That will bring you to a page where you can enter the hostname of your envisalink server, specify the password for the server, specify a disarming code, and name all of your partitions and zones.

** The disarm code and Envisalink Password will be stored in a file on your local pc called AlarmSystemInfo.json.   These are obviously codes you do not want to get out so should be protected accordingly.  This application does not send them off anywhere **
 
 > ![Sample Configure Screen](images/SampleConfigureScreen.png "Configure Screen")
 
 
When you go to the next step and install the smart app, it will create a device for every zone you have defined on this page.  So in this example it will create 5 contact device and 1 motion sensor device in smart things.  The contact devices are open/close type of switches (for example the windows are either opened or close).  Most sensors are a different type of device in Smart Things and indicate that some type of motion has been detected on that device.

I have a fairly simple configuration here.  However, you can define multiple partitions in your alarm system and they will each create a seperate device type in Smart Things.  Partition 1 is associated to the smart home monitor dashboard in the mobile app.  So if you arm/disarm via that app you are talking about partition 1.  You can have more than one defined but at that point you would have to go to the individual device in the mobile app to arm/disarm or sound the siren.

Each partition is also associated to a single siren device in the smart things mobile app.  Having an independent siren is nice because you can trigger it based on other events that may occur in your home. 
 
 # Step 9 - Install the SmartApp

Click on the "Install SmartApp" button at the top of the page.  This will bring up a page where you can put in the client id and secret key you created when you installed the smart app into your Smart Things developer IDE (see step #3 above).

Paste in both the client key and secrect key.  Then click the "Begin Authorization" button.  

 > ![Install Smart App](images/InstallSmartAppScreen1.png "Install Smart App")
 
This will take you to a SmartThings authorization screen.  If you are sent to a smart things error page it typically means that you have entered an invalid client key or secret key.  Double check you work from step #3 and make sure you actually enabled and updated the oauth key.  Also be sure to cut/paste since typing those guids is nearly impossible.

The SmartThings authorization screen you see will look like this.  Select the hub you want to install this onto and then click the authoize button.  It can take 20-30 seconds for the authorization to happen so be patient.

 > ![Authorize Smart App](images/InstallSmartAppScreen2.png "Authorize  Smart App")
 
 
 
 After you have successfully authorized you are redirected back to the j64AlarmServer web app.  It should say that you have succesfully authorized the smart app.  At this point, you can bring up your mobile app and you should see the j64 smart app and j64 device types installed.  Again, it will install and name the devices according to the information that you setup on the configure page.

> ![Authorize Smart App](images/InstallSmartAppScreen3.png "Authorize  Smart App")
 
 # Step 10 - Test it out
 
 If you have reached this point everything should be ready to go.  Pull up your Smart Things mobile app and you should be able to find the devices in your "Things" section.  Everything is prefixed with j64 so they will all be grouped together.  
 
 > ![Mobile App](images/MobileAppThings.png "Mobile App")
 
Next go into the j64:Home Partition device That will take you to an area where you can see the status of the partition and arm or disarm it.  In this example, mine says "disarmed" and "zone not ready".  This is because the j64:Other Doors showed as Open.  My alarm system will not let me arm it if one of the zones is open.  So this is an accurate display.  If I were to close the Other Doors or bypass that zone I would then be able to arm the system.

Go ahead and try everything out.  I am currently writing a debugging document in case something does not work for you so that is coming soon.  In the mean time, the smart app and devices will write to the Smart Things Developer Logs so one easy way to start tracking down errors are to monitor those logs via the IDE looking for error messages.  Also, the web app has the full TPI log that shows all interactions with the Envisalink server so is another good source of debugging information.  And finally, the j64 web app will print messages to the console so it would also be a source of good info when trying to track down problems.

 