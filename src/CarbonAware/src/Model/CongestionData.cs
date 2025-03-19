namespace CarbonAware.Model;

[Serializable]
public record CongestionData
{
    ///<example> eastus </example>
    public string Location { get; set; } = string.Empty;
    ///<example> 01-01-2022 </example>   
    public DateTimeOffset Time { get; set; }
    ///<example> 140.5 </example>
    public int Load { get; set; }
    public int Generation { get; set; }
    public int Difference { get; set; }

    ///<example>1.12:24:02 </example>
    public TimeSpan Duration { get; set; }

    public bool TimeBetween(DateTimeOffset fromNotInclusive, DateTimeOffset? endInclusive)
    {
        if (endInclusive == null) return false;

        return Time > fromNotInclusive && Time <= endInclusive;
    }

}
