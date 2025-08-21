namespace Chat.Tests;

public enum ServerType {
    Monolith,
    Microservice
}

public class EndpointMapping {
    public string GetRoom;
    public string GetMessages;
    public string SendMessage;

    public EndpointMapping(ServerType serverType) {
        switch (serverType) {
            case ServerType.Monolith:
                this.GetRoom = "/getroom";
                this.GetMessages = "/getmessages";
                this.SendMessage = "/send";
                break;
            case ServerType.Microservice:
                GetRoom = "/room";
                GetMessages = "/history";
                SendMessage = "/send";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(serverType), serverType, null);
        }
    }
}