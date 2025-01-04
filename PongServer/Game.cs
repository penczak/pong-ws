using System.Net.WebSockets;

namespace PongServer
{
  public class Game
  {
    // ws for each player
    // player1 paddle pos
    // player2 paddle pos
    // ball pos
    // ball vel? (maybe just for client optimism)
    // score

    public PongClient Host { get; set; }
    public PongClient Guest { get; set; }
    public int BallY { get; set; }
    public int BallX { get; set; }

    public Game(WebSocket hostWs, WebSocket guestWs)
    {
      Host = new PongClient(hostWs, PlayerType.Host);
      Guest = new PongClient(guestWs, PlayerType.Guest);
      BallY = 0;
      BallX = 0;
    }

    public async Task ProcessMoveRequest(PlayerType playerType, bool up)
    {
      switch (playerType)
      {
        case PlayerType.Host:
          Host.MovePaddle(up);
          break;
        case PlayerType.Guest:
          Guest.MovePaddle(up);
          break;
      }

      await SendUpdateToClients();
    }

    public async Task SendUpdateToClients()
    {
      var data = new ArraySegment<byte>(
        GetMessageForClients(
          PongMessageType.Positions,
          Host.PaddlePos,
          Guest.PaddlePos,
          BallY,
          BallX
        )
      );

      await Host.WebSocket.SendAsync(data, WebSocketMessageType.Binary, true, CancellationToken.None);
      //await Guest.WebSocket.SendAsync(data, WebSocketMessageType.Binary, true, CancellationToken.None);
    }

    public static byte[] GetMessageForClients(
      PongMessageType msgType,
      int hostPos,
      int guestPos,
      int ballY,
      int ballX
      )
    {
      var msgTypeBit_0 = ((byte)msgType & 1) << 7;
      var hostPosBits_1_5 = (hostPos & 0b11111) << 2;
      var guestPosBits_6_7 = (guestPos & 0b11000) >> 3;

      var guestPosBits_8_10 = (guestPos & 0b00111) << 5;
      var ballYBits_11_15 = (ballY & 0b111110) >> 1;

      var ballYBits_16 = (ballY & 0b000001) << 7;
      var ballXBits_17_23 = ballX & 0b1111111;

      return new byte[3] {
        (byte)(msgTypeBit_0 | hostPosBits_1_5 | guestPosBits_6_7),
        (byte)(guestPosBits_8_10 | ballYBits_11_15),
        (byte)(ballYBits_16 | ballXBits_17_23)
      };
    }

  }
}
