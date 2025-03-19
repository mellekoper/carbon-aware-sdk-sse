namespace GSF.CarbonAware.Models;

public record CongestionData
{
    private readonly DateTimeOffset _time;
    public string? Location { get; init; }
    public DateTimeOffset Time { get => _time; init => _time = value.ToUniversalTime(); }
    public int  Load { get; init; }

    public int Generation { get; init; }
    public int Difference { get; init; }
    public TimeSpan Duration { get; set; }

    public static implicit operator CongestionData(global::CarbonAware.Model.CongestionData CongestionData) {
        return new CongestionData
        {
            Duration = CongestionData.Duration,
            Location = CongestionData.Location,
            Load = CongestionData.Load,
            Generation = CongestionData.Generation,
            Difference = CongestionData.Difference,
            Time = CongestionData.Time
        };
    }
}