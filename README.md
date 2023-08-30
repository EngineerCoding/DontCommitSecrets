# DontCommitSecrets

This project has been born because of secrets that are required to be configured in your project, particularly when trying to reproduce an issue on production.


Usually this involves editing a file to provide the secrets, which actually could be a problem because this data might be committed to a git repository. Committing a secret can be reversed, along with editing the history, but is almost always very annoying to do. Therefore prevention is better than actual fixing the issue.


To solve this problem, the DontCommitSecrets server has been created! This server is meant to be run on the same machine as the server which your application runs at. Then your application calls the `GET /api/secrets` endpoint to fetch the secrets and stores that in-memory. The server includes an interface where secrets can be configured, and follow the dotnet conventions for subsections.


**Note**: this server solely should be used for development purposes. While it can be used in production, this is not recommended (as the server is single-threaded and multiple clients that might be fetching the data can be problematic).

## NuGet package

For dotnet a NuGet package is exposed: DontCommitSecrets. To add the server to your `IConfigurationBuilder`, call:
```
configurationBuilder.AddDontCommitSecrets("http://localhost:80");
```

Personally, I am only adding this source when running within a development environment, to prevent a production machine from fetching data to a non-existent server.

