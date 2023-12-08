namespace EventsAPI.Models;

public class ConnectionConfigurations
{
    public string ConnectionString { get; set; }
    public string DatabaseName { get; set; }
    public string EventsTypeCollectionName { get; set; }
    public string EventsCollectionName { get; set; }
}
