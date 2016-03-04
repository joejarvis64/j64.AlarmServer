FROM microsoft/aspnet:1.0.0-rc1-final-coreclr

COPY . /app

RUN apt-get update && apt-get install -y libsqlite3-dev

WORKDIR /app/src/j64.AlarmServer
RUN ["dnu", "restore"]
RUN ["dnu", "build"]

WORKDIR /app/src/Moon.AspNet.Authentication.Basic
RUN ["dnu", "restore"]
RUN ["dnu", "build"]

WORKDIR /app/src/j64.AlarmServer.WebApi
RUN ["dnu", "restore"]
RUN ["dnu", "build"]

EXPOSE 2064/tcp
ENTRYPOINT ["dnx", "-p", "project.json", "web"]
