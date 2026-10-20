using Sinaitv.BiometricSync;

var builder = Host.CreateApplicationBuilder(args);

// Usar AddWindowsService en lugar de UseWindowsService
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "SinaitvBiometricSyncService";
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();