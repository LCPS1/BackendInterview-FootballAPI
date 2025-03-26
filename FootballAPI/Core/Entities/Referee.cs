using FootballAPI.Core.Interfaces;

namespace FootballAPI.Core.Entities
{
    public class Referee : IEntity
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int MinutesPlayed { get; set; }
    }
}
