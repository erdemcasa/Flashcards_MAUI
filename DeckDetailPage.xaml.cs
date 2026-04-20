using System.Collections.ObjectModel;
using FlashCard.Models;
using FlashCard.Services;

namespace FlashCard
{
    public partial class DeckDetailPage : ContentPage, IQueryAttributable
    {
        private readonly JsonDataService _dataService = new();
        private readonly ObservableCollection<Card> _cards = new();
        private Deck? _currentDeck;
        private List<Deck> _allDecks = new();
        private Card? _editingCard;

        public DeckDetailPage()
        {
            InitializeComponent();
            CardsCollectionView.ItemsSource = _cards;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("Deck", out object? deckObject) && deckObject is Deck deck)
            {
                _currentDeck = deck;
                Title = deck.Name;
                DeckTitleLabel.Text = deck.Name;
                _ = LoadDeckStateAsync();
            }
        }

        private async Task LoadDeckStateAsync()
        {
            _allDecks = await _dataService.LoadDecksAsync();

            if (_currentDeck == null)
            {
                return;
            }

            Deck? persistedDeck = _allDecks.FirstOrDefault(deck => deck.Id == _currentDeck.Id);
            if (persistedDeck != null)
            {
                _currentDeck = persistedDeck;
            }

            RefreshCards();
        }

        private void RefreshCards()
        {
            _cards.Clear();

            if (_currentDeck == null)
            {
                DeckTitleLabel.Text = "Deck introuvable";
                return;
            }

            DeckTitleLabel.Text = _currentDeck.Name;
            Title = _currentDeck.Name;

            foreach (Card card in _currentDeck.Cards.OrderBy(card => card.Id))
            {
                _cards.Add(card);
            }

            InfoLabel.Text = $"{_cards.Count} carte(s) dans ce deck";
        }

        private static int GetNextCardId(IEnumerable<Card> cards)
        {
            return cards.Any() ? cards.Max(card => card.Id) + 1 : 1;
        }

        private async Task PersistAsync(string successMessage)
        {
            if (_currentDeck == null)
            {
                return;
            }

            Deck? persistedDeck = _allDecks.FirstOrDefault(deck => deck.Id == _currentDeck.Id);
            if (persistedDeck == null)
            {
                persistedDeck = _currentDeck;
                _allDecks.Add(persistedDeck);
            }

            persistedDeck.Name = _currentDeck.Name;
            persistedDeck.CreatedDate = _currentDeck.CreatedDate;
            persistedDeck.Cards = _cards.ToList();

            await _dataService.SaveDecksAsync(_allDecks);
            InfoLabel.Text = $"{DateTime.Now:HH:mm:ss} - {successMessage}";
        }

        private async void OnSaveCardClicked(object sender, EventArgs e)
        {
            if (_currentDeck == null)
            {
                return;
            }

            string question = QuestionEntry.Text?.Trim() ?? string.Empty;
            string answer = AnswerEditor.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(question) || string.IsNullOrWhiteSpace(answer))
            {
                await DisplayAlert("Erreur", "Veuillez renseigner la question et la réponse.", "OK");
                return;
            }

            if (_editingCard == null)
            {
                Card newCard = new()
                {
                    Id = GetNextCardId(_cards),
                    Question = question,
                    Answer = answer
                };

                _cards.Add(newCard);
                await PersistAsync("Carte ajoutée");
            }
            else
            {
                _editingCard.Question = question;
                _editingCard.Answer = answer;
                _editingCard.ModifiedDate = DateTime.Now;
                await PersistAsync("Carte modifiée");
            }

            ResetEditor();
            RefreshCards();
        }

        private async void OnEditCardClicked(object sender, EventArgs e)
        {
            if (sender is not Button button || button.CommandParameter is not Card card)
            {
                return;
            }

            _editingCard = card;
            QuestionEntry.Text = card.Question;
            AnswerEditor.Text = card.Answer;
            SaveCardButton.Text = "Mettre à jour la carte";
            await DisplayAlert("Modification", "Modifiez la carte puis appuyez sur 'Mettre à jour la carte'.", "OK");
        }

        private async void OnDeleteCardClicked(object sender, EventArgs e)
        {
            if (sender is not Button button || button.CommandParameter is not Card card)
            {
                return;
            }

            bool confirm = await DisplayAlert(
                "Supprimer la carte",
                $"Supprimer la carte '{card.Question}' ?",
                "Supprimer",
                "Annuler");

            if (!confirm)
            {
                return;
            }

            _cards.Remove(card);
            await PersistAsync("Carte supprimée");
            RefreshCards();
        }

        private void ResetEditor()
        {
            _editingCard = null;
            QuestionEntry.Text = string.Empty;
            AnswerEditor.Text = string.Empty;
            SaveCardButton.Text = "Ajouter la carte";
        }
    }
}