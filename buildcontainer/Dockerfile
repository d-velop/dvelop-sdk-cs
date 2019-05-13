FROM mcr.microsoft.com/dotnet/core/sdk:2.1.603-nanoserver-1809

WORKDIR c:/build

COPY build.bat c:/build

ENTRYPOINT [ "c:/build.bat", "%*" ]