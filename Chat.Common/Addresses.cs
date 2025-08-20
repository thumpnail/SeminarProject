namespace Chat.Common;

public class Addresses {
    // Microservice
    public const string CHAT_MESSAGING_SERVICE = "http://localhost:5000";
    public const string CHAT_HISTORY_SERVICE = "http://localhost:5001";
    public const string CHAT_DB_SERVICE = "http://localhost:5002";
    // Monolith
    public const string CHAT_MONOLITH_SERVICE = "http://localhost:5010";
}