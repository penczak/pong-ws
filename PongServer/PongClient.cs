using System.Net.WebSockets;

namespace PongServer
{
  public class PongClient
  {
    public PongClient(WebSocket ws, PlayerType playerType)
    {
      WebSocket = ws;
      PlayerType = playerType;
      Score = 0;
      PaddlePos = 0;
    }
    public WebSocket WebSocket { get; set; }
    public PlayerType PlayerType { get; set; }
    public int Score { get; set; }
    public int PaddlePos { get; set; }

    public const int PaddleMax = 31;
    public const int PaddleMin = 0;

    public void MovePaddle(bool up)
    {
      PaddlePos += up ? 1 : -1;
      ClampPaddlePos();
    }

    public void ClampPaddlePos()
    {
      if (PaddlePos > PongClient.PaddleMax)
      {
        PaddlePos = PongClient.PaddleMax;
      }
      if (PaddlePos < PongClient.PaddleMin)
      {
        PaddlePos = PongClient.PaddleMin;
      }
    }
  }
}
