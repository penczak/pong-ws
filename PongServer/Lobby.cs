using System.Net.WebSockets;

namespace PongServer;

public class Lobby(string key, string hostName)
{
  public string Key { get; set; } = key;
  public Game Game { get; set; } = new Game();
  public string HostName { get; set; } = hostName;
  public string? GuestName { get; set; }

  public async Task HandleHost(WebSocket ws)
  {
    Game.SetHost(ws);

    while (ws.CloseStatus == null)
    {
      bool up = await GetNextMessage(ws);
      Game.Host!.MovePaddle(up);
      await Game.SendUpdateToClients();
    }
  }
  public async Task HandleGuest(WebSocket ws)
  {
    Game.SetGuest(ws);

    while (ws.CloseStatus == null)
    {
      bool up = await GetNextMessage(ws);
      Game.Guest!.MovePaddle(up);
      await Game.SendUpdateToClients();
    }
  }

  private static async Task<bool> GetNextMessage(WebSocket ws)
  {
    Console.WriteLine("waiting for client messagbe");
    var buffer = new byte[1]; // client sends only one bit message
    var recieveResult = await ws.ReceiveAsync(
      new ArraySegment<byte>(buffer), CancellationToken.None);
    Console.WriteLine("received client messagbe");
    Console.WriteLine(buffer[0].ToString("B8"));
    Console.WriteLine(buffer[0].ToString());
    bool up = (buffer[0] & 0b1) == 0b0;
    return up;
  }
}