using VibeBotApi;


var builder = WebApplication.CreateBuilder(args);
await StartupHelper.RegisterDependencies(builder);

var app = builder.Build();
await StartupHelper.Init(app);
