@echo off

rem Step 1 - run a restore on the Xmpp app
echo "Running dnu restore for Xmpp"
cd src/Moon.Aspnet.Authentication.Basic
call dotnet restore >restore.log

rem Step 2 - run a restore on the alarmserver app
echo "Running dnu restore for AlarmServer"
cd ../j64.AlarmServer
call dotnet restore >restore.log

rem Step 3 - run a restore on the WebApi app
echo "Running dnu restore for WebApi"
cd ../j64.AlarmServer.Web
call dotnet restore >restore.log

rem Step 4 - run the web app
echo "Running the j64AlarmServer Web App"
cd .
dotnet run



