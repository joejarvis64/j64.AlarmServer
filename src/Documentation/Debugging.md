# Finding the log file
The log file is located under the wwwroot directory in a file called LogMessages.txt.  You can also see everything that is in the log file on the home page of the web app.
 ![j64 Home Page](images/HomePage.png "j64 Home Page")

# Reading the j64 AlarmServer Log File
The log file shows all of the commands that were either sent to or received from the Envisalink Controller.  When the application starts it will open a socket to the Envisalink controller and begin the authentication process.  This authentication process show up in the log with something that looks like this:
```
INFO@2/26/2016 5:11:06 PM@Added the default roles to admin user with default password and roles
INFO@2/26/2016 5:11:09 PM@< 505:LoginInteraction     - 3
INFO@2/26/2016 5:11:09 PM@> 005:NetworkLogin         - xxxx
INFO@2/26/2016 5:11:10 PM@< 500:CommandAcknowledge   - 005
INFO@2/26/2016 5:11:10 PM@< 505:LoginInteraction     - 1
INFO@2/26/2016 5:11:10 PM@> 055:TimeStampControl     - 1
INFO@2/26/2016 5:11:11 PM@> 001:StatusReport         - 
INFO@2/26/2016 5:11:11 PM@< 500:CommandAcknowledge   - 055
INFO@2/26/2016 5:11:12 PM@< 500:CommandAcknowledge   - 001
```

Each line in the file is formatted with the type of log entry, the time it occurred, the command code, and the data that was sent.  In the example above, a 005:NetworkLogin command was sent to the TPI at 5:11:09PM with a user id of xxxx.

See the EnvisalinkTPI-1-04 2.pdf doc for a full description of each command code.

# Debugging the Envisalink/TPI connection
If you are getting errors like the following in the log file, it means that the j64AlarmServer could not connect with the envisalink controller for some reason.

```
INFO@2/27/2016 8:32:08 AM@Waiting 5 seconds before trying to re-connect
INFO@2/27/2016 8:51:15 AM@> 000:Poll                 - 
ERROR@2/27/2016 8:51:15 AM@Could not execute polling command Unable to write data to the transport connection: Unknown error: -1.:Unknown error: -1:
````

There are many things that can go wrong when trying to connect to the TPI, but are the most common issues I have found:

1. Is the Host Name and TCP Port # defined properly in your configuration?  To verify this, go to the configuration section of the j64Web site and see if they are defined properly.
![TPI Host Config](images/ConfigureHost.png "TPI Host Configuration")

2. Can you access the envisalink web app from the machine that is running j64AlarmServer?  The easiest way to determine this is to just access the configured Host Name from the same machine.  In the screen shot above you just point your browser at http://envisalink.attlocal.net.  If you cannot access the web site that runs on your envisalink controller then nothing is going to work and you must get that fixed first.  You may also want to try using the IP address as well if the host name is not resolving. Beyond that you will want to consult the user manual with envisalink to get that setup properly. 

3.  If you can access the envisalink web app but are still having problems getting j64AlarmServer to login, you should go to the Network Settings page on the envisalink web site and check to see that is looks something like this.  If j64AlarmServer is not connected the "Envisalink TPI Status" at the bottom of the page will show offline.  If it is not offline that something else is connected to it so you would not be able to connect.  I have had to click the "Reboot Envislink" option a couple times in the past to get things reset and connecting properly so that is always something you can try.
   
![TPI Host Config](images/EnvisalinkNetworkSettings.png "TPI Host Configuration")
