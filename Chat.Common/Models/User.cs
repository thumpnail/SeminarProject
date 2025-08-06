namespace Chat.Common.Models;

public class User {
    public required Identifier Id { get; set; }
    public required string Username { get; set; }
    // todo: This is cleartext, should be hashed in production
    public required string Password { get; set; }
    public required List<Identifier> ChatIds { get; set; }
}