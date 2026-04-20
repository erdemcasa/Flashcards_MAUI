using System.Text.Json.Serialization;

namespace FlashCard.Models
{
    public class Deck
    {
        private List<Card> _cards = new();

        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public List<Card> Cards
        {
            get => _cards;
            set => _cards = value ?? new List<Card>();
        }

        [JsonIgnore]
        public int CardCount => Cards?.Count ?? 0;

        public Deck()
        {
            CreatedDate = DateTime.Now;
        }

        public override string ToString()
        {
            return $"{Name} ({CardCount} cards)";
        }
    }
}
