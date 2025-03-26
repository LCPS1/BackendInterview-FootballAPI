using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FootballAPI.DTOs
{
    public class CardStatisticsDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CardCount { get; set; }
        public string Type { get; set; } // "Player" or "Manager"
    }
}