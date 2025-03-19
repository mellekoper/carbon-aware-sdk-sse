using GSF.CarbonAware.Models;

namespace CarbonAware.CLI.Model
{
    public class CongestionDataDTO
    {
        public string Location { get; set; } = string.Empty;
        ///<example> 01-01-2022 </example>   
        public DateTimeOffset? Time { get; set; }
        ///<example> 140.5 </example>
        public int Load { get; set; }
        public int Generation { get; set; }
        public double Difference { get; set; }

        ///<example>1.12:24:02 </example>
        public TimeSpan? Duration { get; set; }

        public static explicit operator CongestionDataDTO(CongestionData congestion)
        {
            CongestionDataDTO congestionDTO = new CongestionDataDTO();
            congestionDTO.Location = congestion.Location;
            congestionDTO.Time = congestion.Time;
            congestionDTO.Duration = congestion.Duration;
            congestionDTO.Load = congestion.Load;
            congestionDTO.Generation = congestion.Generation;
            congestionDTO.Difference = congestion.Difference;
            return congestionDTO;
        }
    }
}
