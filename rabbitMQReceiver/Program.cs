using Microsoft.AspNetCore.Connections;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Net.Http.Json;

ConnectionFactory factory = new()
{
	Uri = new Uri("amqp://guest:guest@localhost:5672/"),
	ClientProvidedName = "Rabbit Receiver app"
};

await using IConnection connection = await factory.CreateConnectionAsync();
await using IChannel channel = await connection.CreateChannelAsync();

string exchangename = "test_exchange";
string routingkey = "test_routing_key";
string queuename = "test_queue";

await channel.ExchangeDeclareAsync(exchangename, ExchangeType.Direct);
await channel.QueueDeclareAsync(queuename, false, false, false, null);
await channel.QueueBindAsync(queuename, exchangename, routingkey);


await channel.BasicQosAsync(0,1,false);
using var httpClient = new HttpClient();

var consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += async (sender,args) =>
{
    var message = Encoding.UTF8.GetString(args.Body.ToArray());
    Console.WriteLine($"Received: {message}");

    try
    {
        using HttpResponseMessage response = await httpClient.PostAsJsonAsync(
            "http://localhost:5026/users?lastName=rabbit&firstName=MQ", new {});

        if (!response.IsSuccessStatusCode)
        {
            Console.Error.WriteLine(
                $"REST endpoint returned {(int)response.StatusCode} ({response.StatusCode}); requeueing message.");
            await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: true);
            return;
        }

        Console.WriteLine($"Forwarded message to http://localhost:5026/users?lastName=rabbit&firstName=MQ.");
        await channel.BasicAckAsync(args.DeliveryTag, multiple: false);
    }
    catch (HttpRequestException exception)
    {
        Console.Error.WriteLine($"REST request failed: {exception.Message}. Requeueing message.");
        await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: true);
    }

};

string consumerTag = await channel.BasicConsumeAsync(queuename, autoAck: false, consumer);

Console.ReadLine();

await channel.BasicCancelAsync(consumerTag);
await channel.CloseAsync();
await connection.CloseAsync();
