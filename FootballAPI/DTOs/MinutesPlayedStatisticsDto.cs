using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FootballAPI.DTOs
{
    public class MinutesPlayedStatisticsDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int MinutesPlayed { get; set; }
    }
}