namespace StopsApi;

public class Stop
{
    public Guid Id {get; set;}
    public string Address {get; set;} = string.Empty;
    public double Latitude {get; set;}
    public double Longitude {get; set;}
    public StopStatus Status {get; set;}
}
