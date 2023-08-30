using DontCommitSecrets.WebApp.Models;
using DontCommitSecrets.WebApp.Services;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ISecretSyncService, FileSyncService>();
builder.Services.AddSingleton<IStorageService, StorageService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDirectoryBrowser();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".blat"] = "application/octet-stream";
provider.Mappings[".dll"] = "application/octet-stream";
provider.Mappings[".pdb"] = "application/octet-stream";
provider.Mappings[".dat"] = "application/octet-stream";
provider.Mappings[".wasm"] = "application/wasm";

app.UseStaticFiles(new StaticFileOptions()
{
    ContentTypeProvider = provider
});

app.MapGet("/api/secrets", async (IStorageService storageService, CancellationToken cancellationToken) =>
{
    var secrets = await storageService.GetSecrets(cancellationToken);
    return secrets;
})
.WithName("GetSecrets")
.WithOpenApi();

app.MapPost("/api/secret", async (Secret secret, IStorageService storageService, CancellationToken cancellationToken) =>
{
    await storageService.StoreSecret(secret.Key, secret.Value, cancellationToken);
})
.WithName("AddSecret")
.WithOpenApi();

app.MapDelete("/api/secret", async (string secretKey, IStorageService storageService, CancellationToken cancellationToken) =>
{
    await storageService.RemoveSecret(secretKey, cancellationToken);
})
.WithName("RemoveSecret")
.WithOpenApi();

app.UseDirectoryBrowser(new DirectoryBrowserOptions
{
    RequestPath = "/browser"
});

app.MapFallbackToFile("index.html");

app.Run();
