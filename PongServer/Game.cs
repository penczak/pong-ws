using System.Net.WebSockets;

namespace PongServer;

public class Game
{
  // ws for each player
  // player1 paddle pos
  // player2 paddle pos
  // ball pos
  // ball vel? (maybe just for client optimism)
  // score

  public PongGameClient? Host { get; set; }
  public PongGameClient? Guest { get; set; }
  public int BallY { get; set; } = 0;
  public int BallX { get; set; } = 0;
  public float BallAlpha { get; set; }

  public const int BallMax = 63;
  public const int BallMin = 0;

  public bool IsOpen => Host?.WebSocket.State == WebSocketState.Open
                      && Guest?.WebSocket.State == WebSocketState.Open;

  public Game SetHost(WebSocket hostWs)
  {
    Host = new PongGameClient(hostWs, PlayerType.Host);
    return this;
  }

  public Game SetGuest(WebSocket guestWs)
  {
    Guest = new PongGameClient(guestWs, PlayerType.Guest);
    return this;
  }

  public async Task SendUpdateToClients()
  {
    await Host!.WebSocket.SendAsync(new ArraySegment<byte>(
      GetMessageForClients(
        PongMessageType.Positions,
        Host?.PaddlePos ?? 0, // host pos first
        Guest?.PaddlePos ?? 0,
        BallY,
        BallX
      )
    ), WebSocketMessageType.Binary, true, CancellationToken.None);
    if (Guest?.WebSocket.State == WebSocketState.Open)
    {
      await Guest.WebSocket.SendAsync(new ArraySegment<byte>(
      GetMessageForClients(
        PongMessageType.Positions,
        Guest?.PaddlePos ?? 0, // guest pos first
        Host?.PaddlePos ?? 0,
        BallY,
        -BallX + Game.BallMax // invert X
      )
    ), WebSocketMessageType.Binary, true, CancellationToken.None);
    }
  }

  public static byte[] GetMessageForClients(
    PongMessageType msgType,
    int myPos,
    int oppPos,
    int ballY,
    int ballX
    )
  {
    var msgTypeBit_0 = ((byte)msgType & 1) << 7;
    var hostPosBits_1_5 = (myPos & 0b11111) << 2;
    var guestPosBits_6_7 = (oppPos & 0b11000) >> 3;

    var guestPosBits_8_10 = (oppPos & 0b00111) << 5;
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
