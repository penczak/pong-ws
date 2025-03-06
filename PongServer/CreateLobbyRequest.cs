namespace PongServer
{
  public class CreateLobbyRequest
  {
    public string LobbyName { get; set; }
    public string PlayerName { get; set; }

    public CreateLobbyRequest(string lobbyName, string playerName)
    {
      LobbyName = lobbyName;
      PlayerName = playerName;
    }
  }
}