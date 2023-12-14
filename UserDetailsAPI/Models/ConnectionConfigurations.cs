namespace UserDetailsAPI.Models;

public class ConnectionConfigurations
{
    public string ConnectionString { get; set; }
    public string DatabaseName { get; set; }
    public string UserDetailsCollectionName { get; set; }
    public string ProfilePicturesCollectionName { get; set; }
}
