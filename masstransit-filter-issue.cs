using MassTransit;
using MassTransit.Issues;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(bus =>
{
    bus.AddConsumer<MyConsumer>();
    bus.UsingInMemory((context, cfg) =>
    {
        cfg.UseConsumeFilter<MyConsumerFilter>(context);
        
        cfg.ReceiveEndpoint("my-queue", e =>
        {
            e.UseConsumeFilter<MyConsumerFilter>(context);
            e.ConfigureConsumer<MyConsumer>(context);
        });
    });
});

var app = builder.Build();

app.MapGet("/send-message", async (IPublishEndpoint publishEndpoint) =>
{
    await publishEndpoint.Publish<MyMessage>(new { Message = "TEST MESSAGE" });
});

app.Run();

namespace MassTransit.Issues
{
    public class MyMessage
    {
        public string? Message { get; set; }
    }

    public class MyConsumer : IConsumer<MyMessage>
    {
        public Task Consume(ConsumeContext<MyMessage> context)
        {
            return Task.CompletedTask;
        }
    }

    public class MyConsumerFilter : IFilter<ConsumeContext<MyMessage>>
    {
        public void Probe(ProbeContext context) { }

        public async Task Send(ConsumeContext<MyMessage> context, IPipe<ConsumeContext<MyMessage>> next)
        {
            await next.Send(context);
        }
    }
}
