FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env

WORKDIR /app/build

COPY ./Content/Bloodmasters.Content.proj ./Content/
COPY ./DedicatedServer/Bloodmasters.DedicatedServer.csproj ./DedicatedServer/
COPY ./Server/Bloodmasters.Server.csproj ./Server/
COPY ./Shared/Bloodmasters.Shared.csproj ./Shared/
RUN dotnet restore ./Server/Bloodmasters.Server.csproj

COPY ./Config/* ./Config/
COPY ./Content/ ./Content/
COPY ./DedicatedServer/ ./DedicatedServer/
COPY ./Server/ ./Server/
COPY ./Shared/ ./Shared/
COPY ./Directory.Build.props ./
RUN dotnet publish ./Content/Bloodmasters.Content.proj -p:SkipClient=true
RUN dotnet publish ./DedicatedServer/Bloodmasters.DedicatedServer.csproj --configuration Release

FROM mcr.microsoft.com/dotnet/runtime:7.0

WORKDIR /app

COPY --from=build-env /app/Publish/Server .

ENTRYPOINT ["dotnet", "BMDedicatedServer.dll"]
