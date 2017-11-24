using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using MusicNope.Controls;
using SpotifyAPI.Local;
using SpotifyAPI.Local.Enums;
using SpotifyAPI.Local.Models;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace MusicNope
{
    public partial class LocalControl : IDisposable
    {
        private readonly SpotifyLocalAPI m_Spotify;
        private Track m_CurrentTrack;
        private MemoryStream m_AlbumBitmapStream;
        private bool m_IsPlaying;
        private BackgroundWorker m_ConnectBackgroundWorker;
        private LoaderMessageControl m_LoaderMessageControl;

        public bool IsConnected { get; private set; }

        public LocalControl()
        {
            InitializeComponent();
            m_Spotify = new SpotifyLocalAPI();
            m_Spotify.OnPlayStateChange += _spotify_OnPlayStateChange;
            m_Spotify.OnTrackChange += _spotify_OnTrackChange;
            m_Spotify.OnTrackTimeChange += _spotify_OnTrackTimeChange;
            //m_Spotify.OnVolumeChange += _spotify_OnVolumeChange;

            ArtistLinkLabel.MouseLeftButtonUp += (sender, args) => Process.Start(ArtistLinkLabel.Tag.ToString());
            AlbumLinkLabel.MouseLeftButtonUp += (sender, args) => Process.Start(AlbumLinkLabel.Tag.ToString());
            TitleLinkLabel.MouseLeftButtonUp += (sender, args) => Process.Start(TitleLinkLabel.Tag.ToString());

            ArtistLinkLabel.MouseEnter += LinkLabel_MouseEnter;
            ArtistLinkLabel.MouseLeave += LinkLabel_MouseLeave;

            AlbumLinkLabel.MouseEnter += LinkLabel_MouseEnter;
            AlbumLinkLabel.MouseLeave += LinkLabel_MouseLeave;

            TitleLinkLabel.MouseEnter += LinkLabel_MouseEnter;
            TitleLinkLabel.MouseLeave += LinkLabel_MouseLeave;
            RunBackgroundWorker();
        }

        public void RunBackgroundWorker()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;

            m_LoaderMessageControl = LoaderMessageControl.ShowLoader("Connecting to Spotify...");
            MainGrid.Children.Add(m_LoaderMessageControl);

            m_ConnectBackgroundWorker = new BackgroundWorker { WorkerReportsProgress = true };
            m_ConnectBackgroundWorker.DoWork += (sender, args) =>
            {
                BackgroundWorker bw = (BackgroundWorker)sender;
                while (true)
                {
                    if (!SpotifyLocalAPI.IsSpotifyRunning())
                    {
                        bw.ReportProgress(0);
                        Thread.Sleep(1000);
                        continue;
                    }

                    if (!SpotifyLocalAPI.IsSpotifyWebHelperRunning())
                    {
                        bw.ReportProgress(50);
                        Thread.Sleep(1000);
                        continue;
                    }

                    if (IsConnected)
                    {
                        Thread.Sleep(10000);
                        continue;
                    }
                    bw.ReportProgress(90);
                    Thread.Sleep(3000);
                    bw.ReportProgress(100);
                }
            };
            m_ConnectBackgroundWorker.ProgressChanged += (sender, args) =>
            {
                switch (args.ProgressPercentage)
                {
                    case 90:
                    {
                        m_LoaderMessageControl?.SetMessage("Connecting to Spotify...");
                        break;
                    }
                    case 100:
                    {
                        if (IsConnected)
                            break;
                        Connect();
                        break;
                    }
                    default:
                    {
                        IsConnected = false;

                        if (m_LoaderMessageControl != null)
                        {
                            m_LoaderMessageControl.SetMessage("Spotify isn't running...");
                            break;
                        }
                        m_LoaderMessageControl = LoaderMessageControl.ShowLoader("Spotify isn't running...");
                        MainGrid.Children.Add(m_LoaderMessageControl);
                        break;
                    }
                }
            };
            m_ConnectBackgroundWorker.RunWorkerCompleted += (sender, args) =>
            {
                // Never being used since the worker is always checking for Spotify...
            };
            m_ConnectBackgroundWorker.RunWorkerAsync();
        }

        private void Connect()
        {
            IsConnected = m_Spotify.Connect();
            if (!IsConnected)
                return;
            UpdateInfos();
            m_Spotify.ListenForEvents = true;
            if (m_LoaderMessageControl == null)
                return;
            m_LoaderMessageControl.Dispose();
            MainGrid.Children.Remove(m_LoaderMessageControl);
            m_LoaderMessageControl = null;
        }

        public void UpdateInfos()
        {
            StatusResponse status = m_Spotify.GetStatus();
            if (status == null)
                return;

            UpdatePlayingStatus(status.Playing);
            //clientVersionLabel.Content = $"Client Version: {status.ClientVersion}";
            //versionLabel.Content = $"Version: {status.Version}";
            //repeatShuffleLabel.Content = (status.Repeat ? "Repeat" : "") + " " +  (status.Shuffle ? "Shuffle" : "");

            if (status.Track != null) //Update track infos
                UpdateTrack(status.Track);
        }

        public void UpdateTrack(Track track)
        {
            m_CurrentTrack = track;

            //advertLabel.Content = track.IsAd() ? "ADVERT" : "";
            TimeProgressBar.Maximum = track.Length;

            if (track.IsAd())
                return; //Don't process further, maybe null values

            // TODO ALLL THE RULES THINGS

            // track.TrackType //! Contains "normal" og "explicit" 

            //for (int i = 0; i < DenyArtistsList.Items.Count; i++)
            //{
            //    System.Windows.Controls.CheckBox artist = (System.Windows.Controls.CheckBox) DenyArtistsList.Items[i];

            //    if (artist.IsChecked.HasValue && artist.IsChecked.Value && artist.Content.ToString().Equals(track.ArtistResource.Name, StringComparison.InvariantCultureIgnoreCase))
            //    {
            //        m_Spotify.Skip();
            //        return;
            //    }
            //}

            //for (int i = 0; i < DenyAlbumsList.Items.Count; i++)
            //{
            //    System.Windows.Controls.CheckBox album = (System.Windows.Controls.CheckBox)DenyAlbumsList.Items[i];
            //    if (!album.IsChecked.HasValue || !album.IsChecked.Value || !album.Content.ToString().Equals(track.AlbumResource.Name, StringComparison.InvariantCultureIgnoreCase))
            //        continue;
            //    m_Spotify.Skip();
            //    return;
            //}

            //for (int i = 0; i < DenySongsList.Items.Count; i++)
            //{
            //    System.Windows.Controls.CheckBox song = (System.Windows.Controls.CheckBox)DenySongsList.Items[i];
            //    if (!song.IsChecked.HasValue || !song.IsChecked.Value || !song.Content.ToString().Equals(track.TrackResource.Name, StringComparison.InvariantCultureIgnoreCase))
            //        continue;
            //    m_Spotify.Skip();
            //    return;
            //}

            TitleLinkLabel.Content = track.TrackResource.Name;
            TitleLinkLabel.Tag = track.TrackResource.Uri;

            AlbumLinkLabel.Content = track.AlbumResource.Name;
            AlbumLinkLabel.Tag = track.AlbumResource.Uri;

            ArtistLinkLabel.Content = track.ArtistResource.Name;
            ArtistLinkLabel.Tag = track.ArtistResource.Uri;

            UpdateAlbumArt(track);
        }

        public async void UpdateAlbumArt(Track track)
        {
            byte[] bitmapBytes = await track.GetAlbumArtAsByteArrayAsync(AlbumArtSize.Size640);
            m_AlbumBitmapStream = new MemoryStream(bitmapBytes) { Position = 0 };
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = m_AlbumBitmapStream;
            bitmapImage.EndInit();
            AlbumImage.Source = bitmapImage;
        }

        public void UpdatePlayingStatus(bool playing)
        {
            m_IsPlaying = playing;
            Uri imageUri = m_IsPlaying ? new Uri("../Resources/Images/Pause.png", UriKind.Relative) : new Uri("../Resources/Images/Play.png", UriKind.Relative);
            PlayPauseImage.Source = new BitmapImage(imageUri);
        }

        //private void _spotify_OnVolumeChange(object sender, VolumeChangeEventArgs e)
        //{
        //    if (!CheckAccess())
        //    {
        //        Dispatcher.Invoke(() => _spotify_OnVolumeChange(sender, e));
        //        return;
        //    }
        //    volumeLabel.Content = (e.NewVolume * 100).ToString(CultureInfo.InvariantCulture);
        //}

        private void _spotify_OnTrackTimeChange(object sender, TrackTimeChangeEventArgs e)
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(() => _spotify_OnTrackTimeChange(sender, e));
                return;
            }

            if (m_CurrentTrack == null)
                return;

            CurrentTimeLabel.Content = $@"{FormatTime(e.TrackTime)}";
            EndTimeLabel.Content = $@"{FormatTime(m_CurrentTrack.Length)}";
            if (e.TrackTime < m_CurrentTrack.Length)
                TimeProgressBar.Value = (int)e.TrackTime;
        }

        private void _spotify_OnTrackChange(object sender, TrackChangeEventArgs e)
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(() => _spotify_OnTrackChange(sender, e));
                return;
            }
            UpdateTrack(e.NewTrack);
        }

        private void _spotify_OnPlayStateChange(object sender, PlayStateEventArgs e)
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(() => _spotify_OnPlayStateChange(sender, e));
                return;
            }
            UpdatePlayingStatus(e.Playing);
        }

        //private async void playUrlBtn_Click(object sender, EventArgs e)
        //{
        //    await _spotify.PlayURL(playTextBox.Text, contextTextBox.Text);
        //}

        //private async void playBtn_Click(object sender, EventArgs e)
        //{
        //    await _spotify.Play();
        //}

        //private async void pauseBtn_Click(object sender, EventArgs e)
        //{
        //    await _spotify.Pause();
        //}

        //private void prevBtn_Click(object sender, EventArgs e)
        //{
        //    _spotify.Previous();
        //}

        //private void skipBtn_Click(object sender, EventArgs e)
        //{
        //    _spotify.Skip();
        //}

        private static string FormatTime(double sec)
        {
            TimeSpan span = TimeSpan.FromSeconds(sec);
            string secs = span.Seconds.ToString(), mins = span.Minutes.ToString();
            if (secs.Length < 2)
                secs = "0" + secs;
            return mins + ":" + secs;
        }

        private readonly Brush m_LinkLabelForegroundDefault = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        private readonly Brush m_LinkLabelForegroundHighlight = new SolidColorBrush(Color.FromRgb(35, 255, 200));

        private void LinkLabel_MouseEnter(object sender, MouseEventArgs e)
        {
            System.Windows.Controls.Label linkLabel = (System.Windows.Controls.Label)sender;
            linkLabel.Foreground = m_LinkLabelForegroundHighlight;
        }

        private void LinkLabel_MouseLeave(object sender, MouseEventArgs e)
        {
            System.Windows.Controls.Label linkLabel = (System.Windows.Controls.Label)sender;
            linkLabel.Foreground = m_LinkLabelForegroundDefault;
        }

        public void Dispose()
        {
            m_Spotify?.Dispose();
            m_AlbumBitmapStream?.Dispose();
            m_ConnectBackgroundWorker?.Dispose();
        }

        private void PlayPauseImage_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (m_IsPlaying)
                m_Spotify?.Pause();
            else
                m_Spotify?.Play();
        }

        private void ExpandRetractCanvas_MouseEnter(object sender, MouseEventArgs e)
        {
            ExpandRetractImage.Opacity = 1;
        }

        private void ExpandRetractCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            ExpandRetractImage.Opacity = 0.2;
        }

        private void ExpandRetractCanvas_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            double to = Math.Abs(ControlGrid.ActualHeight - 120) < 0.1 ? MainGrid.ActualHeight : 120;
            ExpandRetractRules(to);
        }

        private void ExpandRetractRules(double to)
        {
            double from = ControlGrid.ActualHeight;
            AnimationTimeline animationTimeline = new DoubleAnimation(from, to, new Duration(new TimeSpan(0, 0, 0, 0, 250)));
            ControlGrid.BeginAnimation(HeightProperty, animationTimeline);
            ExpandRetractImage.RenderTransform = ControlGrid.ActualHeight < MainGrid.ActualHeight ? new RotateTransform(180) : new RotateTransform(0);
        }

        public void ExpandRules()
        {
            ExpandRetractRules(MainGrid.ActualHeight);
        }

        public void RetractRules()
        {
            ExpandRetractRules(120);
        }
    }
}
