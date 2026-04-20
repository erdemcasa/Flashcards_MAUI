namespace FlashCard
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(DeckDetailPage), typeof(DeckDetailPage));
        }
    }
}
