using FlashCard.Models;
using FlashCard.Services;

namespace FlashCard
{
    public partial class MainPage : ContentPage
    {
        private readonly JsonDataService _dataService = new();

        public MainPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            List<Deck> decks = await _dataService.LoadDecksAsync();
            StatsLabel.Text = $"{decks.Count} deck(s) • {decks.Sum(deck => deck.CardCount)} carte(s)";
            RecentDecksCollectionView.ItemsSource = decks.OrderByDescending(deck => deck.CreatedDate).Take(5).ToList();
        }

        private async void OnOpenDecksClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//DecksPage");
        }

        private async void OnCreateDeckClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//DecksPage");
        }
    }
}
