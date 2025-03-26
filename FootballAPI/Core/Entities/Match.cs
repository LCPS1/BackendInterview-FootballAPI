using FootballAPI.Core.Interfaces;

namespace FootballAPI.Core.Entities
{
    public class Match : IEntity
    {
        public int Id { get; set; }

        public int HouseManagerId { get; set; }
        public Manager? HouseManager { get; set; }

        public int AwayManagerId { get; set; }
        public Manager? AwayManager { get; set; }

        public int RefereeId { get; set; }
        public Referee? Referee { get; set; }
        public ICollection<Player> HousePlayers { get; set; } = new List<Player>(); 
        public ICollection<Player> AwayPlayers { get; set; } = new List<Player>(); 

         // Added properties for alignment check functionality
        public DateTime ScheduledStart { get; set; } // Represents the scheduled start time of the match.
        public MatchStatus Status { get; set; } = MatchStatus.Scheduled; // Represents the status of the match (Scheduled, InProgress, Completed, Cancelled).
    }
}
