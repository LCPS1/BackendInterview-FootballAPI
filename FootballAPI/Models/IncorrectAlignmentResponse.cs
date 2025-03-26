using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FootballAPI.Models
{
    public class IncorrectAlignmentResponse
    {
        public bool HasIncorrectAlignment { get; set; }
        public string? Message { get; set; }
        public List<string> Details { get; set; } = new List<string>();
    }
}