using System.Net.WebSockets;

namespace PongServer
{
  public partial class Game
  {
    // ws for each player
    // player1 paddle pos
    // player2 paddle pos
    // ball pos
    // ball vel? (maybe just for client optimism)
    // score

    // TODO lobbies
    //   (host creates lobby with name, guest can type lobby name to join host)
    PongClient Host { get; set; }
    PongClient Guest { get; set; }

    public Game(WebSocket hostWs, WebSocket guestWs)
    {
      Host = new PongClient(hostWs);
      Guest = new PongClient(guestWs);
    }


  }
}
