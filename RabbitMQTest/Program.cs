using RabbitMQTest;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMessageRouting();
builder.Services.AddHostedService<SubscribeWorker>();
builder.Services.AddHostedService<PublishWorker>();

var host = builder.Build();
host.Run();
