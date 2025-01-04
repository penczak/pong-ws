using PongServer;

namespace PongServer.Tests
{
  public class ClientMessageTests
  {
    [Fact]
    public void AllZeros()
    {
      var result = Game.GetMessageForClients(PongMessageType.Positions, 0, 0, 0, 0);
      Assert.Equal(3, result.Length);
      Assert.Equal(0, result[0]);
      Assert.Equal(0, result[1]);
      Assert.Equal(0, result[2]);
    }

    [Fact]
    public void MessageType_Positions()
    {
      var result = Game.GetMessageForClients(PongMessageType.Positions, 0, 0, 0, 0);
      Assert.Equal(0, result[0] & 0b10000000);
    }
    [Fact]
    public void MessageType_RoundEnd()
    {
      var result = Game.GetMessageForClients(PongMessageType.RoundEnd, 0, 0, 0, 0);
      Assert.Equal(0b10000000, result[0] & 0b10000000);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(16)]
    [InlineData(31)]
    public void HostPos(int hostPos)
    {
      var result = Game.GetMessageForClients(PongMessageType.Positions, hostPos, 0, 0, 0);
      Assert.Equal(hostPos, (result[0] & 0b01111100) >> 2);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(16)]
    [InlineData(31)]
    [InlineData(63)]
    public void BallY(int ballY)
    {
      var result = Game.GetMessageForClients(PongMessageType.Positions, 0, 0, ballY, 0);
      Assert.Equal(ballY >> 1, result[1] & 0b00011111);
      Assert.Equal(ballY & 1, (result[2] & 0b10000000) >> 7);
    }
  }
}