namespace MusicNope
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MusicNope_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LocalSpotifyControl.Dispose();
        }
    }
}
