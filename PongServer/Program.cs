using System.IO;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;

namespace PongServer
{
  public class Program
  {
    public static void Main(string[] args)
    {
      var builder = WebApplication.CreateBuilder(args);
      var app = builder.Build();

      var webSocketOptions = new WebSocketOptions
      {
        KeepAliveInterval = TimeSpan.FromMinutes(5),
      };

      app.UseWebSockets();

      app.UseDefaultFiles();
      app.UseStaticFiles();

      app.Use(async (context, next) =>
      {
        if (context.Request.Path == "/ws")
        {
          if (context.WebSockets.IsWebSocketRequest)
          {
            try
            {
              using var ws = await context.WebSockets.AcceptWebSocketAsync();
              await Echo(ws);
            }
            catch (System.Net.WebSockets.WebSocketException wsEx)
            {
              Console.WriteLine("Socket unexpectedly closed!");
              if (wsEx?.Message == "The remote party closed the WebSocket connection without completing the close handshake.")
              {
                Console.WriteLine(wsEx.Message);
                return;
              }
              throw;
            }
          }
          else
          {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
          }
        }
        else
        {
          await next(context);
        }
      });

      app.Run();
    }

    public static async Task Echo(WebSocket ws)
    {
      Console.WriteLine("Socket opened");

      var buffer = new byte[16];
      var recieveResult = await ws.ReceiveAsync(
        new ArraySegment<byte>(buffer), CancellationToken.None);

      while (!recieveResult.CloseStatus.HasValue)
      {
        Console.WriteLine("Message Received (UTF8):");
        Console.WriteLine(Encoding.UTF8.GetString(buffer));
        Console.WriteLine("Bytes:");
        foreach (var bt in buffer)
        {
          Console.WriteLine(bt.ToString("X4"));
        }
        Console.WriteLine();

        await ws.SendAsync(
          new ArraySegment<byte>(buffer, 0, recieveResult.Count),
          recieveResult.MessageType,
          recieveResult.EndOfMessage,
          CancellationToken.None);

        recieveResult = await ws.ReceiveAsync(
          new ArraySegment<byte>(buffer), CancellationToken.None);
      }
      Console.WriteLine("Socket closed: {0}", recieveResult.CloseStatus.ToString());

      await ws.CloseAsync(
        recieveResult.CloseStatus.Value,
        recieveResult.CloseStatusDescription,
        CancellationToken.None);
    }
  }
}
