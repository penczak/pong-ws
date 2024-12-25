using System.Net.WebSockets;

namespace PongServer
{
  public class Program
  {
    public static void Main(string[] args)
    {
      var builder = WebApplication.CreateBuilder(args);
      var app = builder.Build();

      app.UseWebSockets();

      app.Use(async (context, next) =>
      {
        if (context.Request.Path == "/ws")
        {
          if (context.WebSockets.IsWebSocketRequest)
          {
            using var ws = await context.WebSockets.AcceptWebSocketAsync();
            await Echo(ws);
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
      var buffer = new byte[1024 * 4];
      var recieveResult = await ws.ReceiveAsync(
        new ArraySegment<byte>(buffer), CancellationToken.None);

      while (!recieveResult.CloseStatus.HasValue)
      {
        await ws.SendAsync(
          new ArraySegment<byte>(buffer, 0, recieveResult.Count),
          recieveResult.MessageType,
          recieveResult.EndOfMessage,
          CancellationToken.None);

        recieveResult = await ws.ReceiveAsync(
          new ArraySegment<byte>(buffer), CancellationToken.None);
      }

      await ws.CloseAsync(
        recieveResult.CloseStatus.Value,
        recieveResult.CloseStatusDescription,
        CancellationToken.None);
    }
  }
}
