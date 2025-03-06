using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;

namespace PongServer
{
  public class Program
  {
    public static List<Lobby> Lobbies { get; set; } = new List<Lobby>();

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

      app.MapPost("/lobby", async (HttpContext context, [FromBody] CreateLobbyRequest request) =>
      {
        Console.WriteLine(request.LobbyName);
        if (Lobbies.Any(l => l.Key == request.LobbyName))
        {
          context.Response.StatusCode = StatusCodes.Status409Conflict;
          await context.Response.WriteAsync($"Lobby with key {request.LobbyName} already exists.");
          return;
        }

        var lobby = new Lobby(request.LobbyName, request.PlayerName);
        Lobbies.Add(lobby);

        context.Response.StatusCode = StatusCodes.Status201Created;
        await context.Response.WriteAsync(lobby.Key); // instructs client to open ws and send this key
      });

      app.MapPost("/join", async (HttpContext context, [FromBody] CreateLobbyRequest request) =>
      {
        Console.WriteLine(request.LobbyName);
        var lobby = Lobbies.FirstOrDefault(l => l.Key == request.LobbyName && l.Game.Guest == null); // todo could message user if guest is taken
        if (lobby == null)
        {
          context.Response.StatusCode = StatusCodes.Status404NotFound;
          // TODO let client see list of lobbies?
          await context.Response.WriteAsync($"Lobby with key {request.LobbyName} was not found.");
          return;
        }

        lobby.GuestName = request.PlayerName;

        context.Response.StatusCode = StatusCodes.Status202Accepted;
        await context.Response.WriteAsync(lobby.Key); // instructs client to open ws and send this key
      });

      app.Use(async (context, next) =>
      {
        // TODO lobbies
        //   (host creates lobby with name, guest can type lobby name to join host)
        // perhaps another endpoint for create lobby and for join lobby. then once two players are in a lobby, tell the client to open the WS?
        if (context.Request.Path == "/ws")
        {
          if (context.WebSockets.IsWebSocketRequest)
          {
            try
            {
              using var ws = await context.WebSockets.AcceptWebSocketAsync();

              var buffer = new byte[64];
              var recieveResult = await ws.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);

              if (recieveResult.CloseStatus.HasValue)
              {
                Console.WriteLine("Did not receive expected lobby key as first message from socket.");
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
              }
              string lobbyKey = Encoding.UTF8.GetString(buffer, 0, recieveResult.Count);
              Console.WriteLine($"Lobby key: {lobbyKey}");
              var lobby = Lobbies.FirstOrDefault(l => l.Key == lobbyKey);
              if (lobby == null)
              {
                Console.WriteLine("Lobby not found.");
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
              }
              if (lobby.Game.Host == null)
              {
                await lobby.HandleHost(ws);
              }
              else if (lobby.Game.Guest == null)
              {
                await lobby.HandleGuest(ws);
              }

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

    /*private static async Task Handle(WebSocket ws, Lobby lobby)
    {
      Console.WriteLine("Socket opened");
      //await Echo(ws);
      while (ws.CloseStatus == null)
      {
        Console.WriteLine("waiting for client messagbe");
        var buffer = new byte[1];
        var recieveResult = await ws.ReceiveAsync(
          new ArraySegment<byte>(buffer), CancellationToken.None);
        Console.WriteLine("received client messagbe");
        Console.WriteLine(buffer[0].ToString("B8"));
        Console.WriteLine(buffer[0].ToString());
        bool up = (buffer[0] & 0b1) == 0b0;

        // update (both) client(s)
        await lobby.Game.ProcessMoveRequest(PlayerType.Guest, up);

      }
      //_ = Task.Run(async () => await PingAndListen(First, PlayerType.Host));
      //_ = Task.Run(async () => await PingAndListen(Second, PlayerType.Guest));
    }*/

    /*private static async Task PingAndListen(WebSocket ws, PlayerType playerType)
    {
      _ = Task.Run(async () =>
      {
        while (ws.CloseStatus == null)
        {
          Console.WriteLine("waiting for client messagbe");
          var buffer = new byte[1];
          var recieveResult = await ws.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None);
          Console.WriteLine("received client messagbe");
          Console.WriteLine(buffer[0].ToString("B8"));
          Console.WriteLine(buffer[0].ToString());
          var up = (buffer[0] & 0b1) == 0b0;
          var down = (buffer[0] & 0b1) == 0b1;

          // update (both) client(s)
          await Game.ProcessMoveRequest(playerType, up);

        }
      });

      while (ws.CloseStatus == null)
      {
        await ws.SendAsync(
          new ArraySegment<byte>([(byte)DateTime.Now.Second]),
          WebSocketMessageType.Binary,
          true,
          CancellationToken.None);

        Thread.Sleep(500);
      }
    }*/

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
