using RabbitMQ.Client;
using System.Text;

ConnectionFactory factory = new()
{
	Uri = new Uri("amqp://guest:guest@localhost:5672"),
	ClientProvidedName = "Rabbit sender app"
};

await using IConnection connection = await factory.CreateConnectionAsync();
await using IChannel channel = await connection.CreateChannelAsync();

string exchangename = "test_exchange";
string routingkey = "test_routing_key";
string queuename = "test_queue";

await channel.ExchangeDeclareAsync(exchangename, ExchangeType.Direct);
await channel.QueueDeclareAsync(queuename, false, false, false, null);
await channel.QueueBindAsync(queuename, exchangename, routingkey);

//byte[] messagebody = Encoding.UTF8.GetBytes("This is the second message");
//await channel.BasicPublishAsync(exchangename, routingkey, messagebody);

for(int i = 0; i<60; i++)
{
	Console.WriteLine($"Sending message number: {i}");

	byte[] messagebody = Encoding.UTF8.GetBytes($"Create a user: {i}");
	await channel.BasicPublishAsync(exchangename, routingkey, messagebody);
}

Console.WriteLine("Creating users");

await channel.CloseAsync();
await connection.CloseAsync();