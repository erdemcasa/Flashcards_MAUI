using System.Collections.ObjectModel;
using FlashCard.Models;
using FlashCard.Services;

namespace FlashCard
{
    public partial class DecksPage : ContentPage
    {
        private readonly JsonDataService _dataService = new();
        private readonly ObservableCollection<Deck> _decks = new();

        public DecksPage()
        {
            InitializeComponent();
            DecksCollectionView.ItemsSource = _decks;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadDecksAsync();
        }

        private async Task LoadDecksAsync()
        {
            List<Deck> loadedDecks = await _dataService.LoadDecksAsync();
            _decks.Clear();

            foreach (Deck deck in loadedDecks.OrderBy(deck => deck.Name))
            {
                _decks.Add(deck);
            }

            UpdateInfo($"{_decks.Count} deck(s) chargé(s)");
        }

        private static int GetNextDeckId(IEnumerable<Deck> decks)
        {
            return decks.Any() ? decks.Max(deck => deck.Id) + 1 : 1;
        }

        private void UpdateInfo(string message)
        {
            InfoLabel.Text = $"{DateTime.Now:HH:mm:ss} - {message}";
        }

        private async void OnAddDeckClicked(object sender, EventArgs e)
        {
            string name = NewDeckEntry.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(name))
            {
                await DisplayAlert("Erreur", "Veuillez entrer un nom de deck.", "OK");
                return;
            }

            if (_decks.Any(deck => deck.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                await DisplayAlert("Erreur", "Un deck avec ce nom existe déjà.", "OK");
                return;
            }

            Deck newDeck = new()
            {
                Id = GetNextDeckId(_decks),
                Name = name,
                Cards = new List<Card>()
            };

            _decks.Add(newDeck);
            await _dataService.SaveDecksAsync(_decks.ToList());

            NewDeckEntry.Text = string.Empty;
            UpdateInfo($"Deck ajouté: {name}");
        }

        private async void OnOpenDeckClicked(object sender, EventArgs e)
        {
            if (sender is not Button button || button.CommandParameter is not Deck deck)
            {
                return;
            }

            await Shell.Current.GoToAsync(nameof(DeckDetailPage), new Dictionary<string, object>
            {
                ["Deck"] = deck
            });
        }

        private async void OnEditDeckClicked(object sender, EventArgs e)
        {
            if (sender is not Button button || button.CommandParameter is not Deck deck)
            {
                return;
            }

            string? newName = await DisplayPromptAsync(
                "Renommer le deck",
                "Nouveau nom :",
                initialValue: deck.Name,
                placeholder: "Nom du deck"
            );

            if (string.IsNullOrWhiteSpace(newName))
            {
                return;
            }

            string trimmedName = newName.Trim();

            if (_decks.Any(existing => existing.Id != deck.Id && existing.Name.Equals(trimmedName, StringComparison.OrdinalIgnoreCase)))
            {
                await DisplayAlert("Erreur", "Un deck avec ce nom existe déjà.", "OK");
                return;
            }

            deck.Name = trimmedName;
            await _dataService.SaveDecksAsync(_decks.ToList());
            await LoadDecksAsync();
            UpdateInfo($"Deck renommé: {trimmedName}");
        }

        private async void OnDeleteDeckClicked(object sender, EventArgs e)
        {
            if (sender is not Button button || button.CommandParameter is not Deck deck)
            {
                return;
            }

            bool confirm = await DisplayAlert(
                "Supprimer le deck",
                $"Supprimer '{deck.Name}' et ses {deck.CardCount} carte(s) ?",
                "Supprimer",
                "Annuler");

            if (!confirm)
            {
                return;
            }

            _decks.Remove(deck);
            await _dataService.SaveDecksAsync(_decks.ToList());
            UpdateInfo($"Deck supprimé: {deck.Name}");
        }
    }
}