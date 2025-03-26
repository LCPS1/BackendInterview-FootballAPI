using FootballAPI.Core.Interfaces;

namespace FootballAPI.Core.Entities
{
    public class Player : IEntity
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int YellowCard { get; set; }
        public int RedCard { get; set; }
        public int MinutesPlayed { get; set; }

        // These properties would be created as shadow properties otherwise
        public int? HouseMatchId { get; set; }
        public int? AwayMatchId { get; set; }
    }
}
