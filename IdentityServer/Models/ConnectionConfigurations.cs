namespace IdentityServer.Models;

public class ConnectionConfigurations
{
    public string ConnectionString { get; set; }
    public string DatabaseName { get; set; }
    public string UserTypeCollectionName { get; set; }
    public string UserAuthCollectionName { get; set; }
}
