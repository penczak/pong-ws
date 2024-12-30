using System.Net.WebSockets;

namespace PongServer
{
  public partial class Game
  {
    public class PongClient
    {
      public PongClient(WebSocket ws)
      {
        WebSocket = ws;
        Score = 0;
        PaddlePos = 0;
      }
      public WebSocket WebSocket { get; set; }
      public int Score { get; set; }
      public int PaddlePos { get; set; }
    }


  }
}
