@echo off

rem Step 1 - run a restore on the Xmpp app
echo "Running dnu restore for Xmpp"
cd src/Moon.Aspnet.Authentication.Basic
call dnu restore >restore.log

rem Step 2 - run a restore on the alarmserver app
echo "Running dnu restore for AlarmServer"
cd ../j64.AlarmServer
call dnu restore >restore.log

rem Step 3 - run a restore on the WebApi app
echo "Running dnu restore for WebApi"
cd ../j64.AlarmServer.WebApi
call dnu restore >restore.log

rem Step 4 - run the web app
echo "Running the j64AlarmServer Web App"
cd .
dnx web



