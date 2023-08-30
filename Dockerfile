FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /App
COPY ./DontCommitSecrets.WebApp ./DontCommitSecrets.WebApp
COPY ./DontCommitSecrets.Client ./DontCommitSecrets.Client

RUN dotnet restore DontCommitSecrets.WebApp/DontCommitSecrets.WebApp.csproj
RUN dotnet publish DontCommitSecrets.WebApp/DontCommitSecrets.WebApp.csproj -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /App
COPY --from=build-env /App/out .

VOLUME ['./data']
EXPOSE 80

ENTRYPOINT ["dotnet", "DontCommitSecrets.WebApp.dll"]
