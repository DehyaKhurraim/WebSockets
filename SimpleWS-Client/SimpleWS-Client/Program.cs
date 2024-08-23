using System.Net.WebSockets;
using System.Text;

var ws = new ClientWebSocket();

string name;
while(true)
{
    Console.WriteLine("Enter Your Name: ");
    name = Console.ReadLine();
    break;
}

Console.WriteLine("Connecting to server");
await ws.ConnectAsync(new Uri($"ws://localhost:6969/ws?name={name}"), CancellationToken.None);
Console.WriteLine("Connected!");

var receiveTask = Task.Run(async () =>
{
    var buffer = new byte[1024*4];
    while (true)
    {
        var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        if (result.MessageType == WebSocketMessageType.Close)
        {
            break;
        }

        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
        Console.WriteLine(message);

    }
});

var sendTask = Task.Run(async () =>
{
    while (true)
    {
        var msg = Console.ReadLine();
        if(msg ==  "exit")
        {
            break;
        }
        var bytes = Encoding.UTF8.GetBytes(msg);
        await ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
    }
});

await Task.WhenAll(receiveTask, sendTask);

if(ws.State != WebSocketState.Closed)
{
    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
}

await receiveTask;