using FootballAPI.Core.Interfaces;

namespace FootballAPI.Core.Entities
{
    public class Manager : IEntity
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int YellowCard { get; set; }
        public int RedCard { get; set; }
    }
}
