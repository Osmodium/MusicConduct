using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using MusicConduct.Events;
using MusicConduct.Utility;
using SpotifyAPI.Local;
using SpotifyAPI.Local.Enums;
using SpotifyAPI.Local.Models;

namespace MusicConduct.Controls
{
    public partial class LocalControl : IDisposable
    {
        private readonly SpotifyLocalAPI m_Spotify;
        private Track m_CurrentTrack;
        private MemoryStream m_AlbumBitmapStream;
        private bool m_IsPlaying;
        private bool m_IsPlayingAd;
        private BackgroundWorker m_ConnectBackgroundWorker;
        private LoaderMessageControl m_LoaderMessageControl;
        private readonly double m_FadeAfterMilliseconds = 5000;
        private readonly double m_FadeTo = 0;
        private readonly double m_FadeTime = 1000f;
        private DateTime m_FadeAt;
        private readonly System.Timers.Timer m_FadeTimer;

        public string SpotifyClientVersion { get; internal set; }

        public SpotifyLocalEvents SpotifyLocalEvents { get; }
        public bool IsConnected { get; set; }

        public LocalControl()
        {
            InitializeComponent();
            m_Spotify = new SpotifyLocalAPI();
            m_Spotify.OnPlayStateChange += Spotify_OnPlayStateChange;
            m_Spotify.OnTrackChange += Spotify_OnTrackChange;
            m_Spotify.OnTrackTimeChange += Spotify_OnTrackTimeChange;

            SpotifyLocalEvents = new SpotifyLocalEvents();

            ArtistLinkLabel.MouseLeftButtonUp += (sender, args) => Process.Start(ArtistLinkLabel.Tag.ToString());
            AlbumLinkLabel.MouseLeftButtonUp += (sender, args) => Process.Start(AlbumLinkLabel.Tag.ToString());
            TitleLinkLabel.MouseLeftButtonUp += (sender, args) => Process.Start(TitleLinkLabel.Tag.ToString());

            ArtistLinkLabel.MouseEnter += LinkLabel_MouseEnter;
            ArtistLinkLabel.MouseLeave += LinkLabel_MouseLeave;

            AlbumLinkLabel.MouseEnter += LinkLabel_MouseEnter;
            AlbumLinkLabel.MouseLeave += LinkLabel_MouseLeave;

            TitleLinkLabel.MouseEnter += LinkLabel_MouseEnter;
            TitleLinkLabel.MouseLeave += LinkLabel_MouseLeave;

            m_FadeTimer = new System.Timers.Timer();
            m_FadeTimer.Elapsed += FadeTimerOnElapsed;
            m_FadeTimer.Interval = 100;
            m_FadeTimer.Enabled = false;
            RunBackgroundWorker();
        }

        private void RunBackgroundWorker()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;

            m_LoaderMessageControl = LoaderMessageControl.ShowLoader("Connecting to Spotify...");
            MainGrid.Children.Add(m_LoaderMessageControl);

            m_ConnectBackgroundWorker = new BackgroundWorker { WorkerReportsProgress = true };
            m_ConnectBackgroundWorker.DoWork += (sender, args) =>
            {
                BackgroundWorker bw = (BackgroundWorker)sender;
                try
                {
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
                            if (m_IsPlayingAd)
                                UpdateTrack();
                            Thread.Sleep(10000);
                            continue;
                        }
                        bw.ReportProgress(90);
                        Thread.Sleep(3000);
                        bw.ReportProgress(100);
                    }
                }
                catch (Exception)
                {
                    // TODO log
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
            Retry.Do(() =>
            {
                IsConnected = m_Spotify.Connect();
            },
            TimeSpan.FromMilliseconds(1000), 10);
            if (!IsConnected)
                return;
            SpotifyLocalEvents.OnConnectionChange(new SpotifyLocalEvents.ConnectedChangeEventArgs { IsConnected = true });
            RulesControl.RulesEvents.RulesChanged += (sender, args) => { SkipTrack(); };
            UpdateInfos();
            m_Spotify.ListenForEvents = true;
            if (m_LoaderMessageControl == null)
                return;
            m_LoaderMessageControl.Dispose();
            MainGrid.Children.Remove(m_LoaderMessageControl);
            m_LoaderMessageControl = null;
            FadeWithDelay();
        }

        private void UpdateInfos()
        {
            StatusResponse status = m_Spotify.GetStatus();
            if (status == null)
                return;

            UpdatePlayingStatus(status.Playing);

            SpotifyClientVersion = $"{status.ClientVersion} ({status.Version})";

            if (status.Track != null)
                UpdateTrack(status.Track);
        }

        private Track GetTrack()
        {
            StatusResponse status = m_Spotify.GetStatus();
            return status.Track;
        }

        private void SkipTrack()
        {
            SkipTrack(GetTrack());
        }

        private void SkipTrack(Track track)
        {
            if (RulesControl.ShouldSkipTrack(track))
                m_Spotify.Skip();
        }

        private void UpdateTrack()
        {
            StatusResponse status = m_Spotify.GetStatus();
            if (status?.Track == null)
                return;
            UpdateTrack(status.Track);
        }

        private void UpdateTrack(Track track)
        {
            m_CurrentTrack = track;

            // Handle ads
            if (track.IsAd())
            {
                SpotifyLocalEvents.OnTrackChange(new SpotifyLocalEvents.TrackChangeEventArgs { Title = "Ad", IsAd = true});
                AlbumImage.Source = new BitmapImage(new Uri("/MusicConduct;component/Resources/Images/placeholder.png", UriKind.Relative));
                TrackDetailsGrid.Visibility = Visibility.Hidden;
                m_IsPlayingAd = true;
                return; //Don't process further, maybe null values
            }

            m_IsPlayingAd = false;

            SkipTrack(track);

            // TODO skip repeated tracks
            
            TitleLinkLabel.Content = track.TrackResource.Name;
            TitleLinkLabel.Tag = track.TrackResource.Uri;

            AlbumLinkLabel.Content = track.AlbumResource.Name;
            AlbumLinkLabel.Tag = track.AlbumResource.Uri;

            ArtistLinkLabel.Content = track.ArtistResource.Name;
            ArtistLinkLabel.Tag = track.ArtistResource.Uri;

            TimeProgressBar.Maximum = track.Length;

            TrackDetailsGrid.Visibility = Visibility.Visible;

            SpotifyLocalEvents.OnTrackChange(new SpotifyLocalEvents.TrackChangeEventArgs { Title = track.TrackResource.Name, Album = track.AlbumResource.Name, Artist = track.ArtistResource.Name});

            UpdateAlbumArt(track);
        }

        private async void UpdateAlbumArt(Track track)
        {
            byte[] bitmapBytes = await track.GetAlbumArtAsByteArrayAsync(AlbumArtSize.Size640);
            m_AlbumBitmapStream = new MemoryStream(bitmapBytes) { Position = 0 };
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = m_AlbumBitmapStream;
            bitmapImage.EndInit();
            AlbumImage.Source = bitmapImage;
        }

        private void UpdatePlayingStatus(bool playing)
        {
            m_IsPlaying = playing;
            Uri imageUri = m_IsPlaying ? new Uri("../Resources/Images/Pause.png", UriKind.Relative) : new Uri("../Resources/Images/Play.png", UriKind.Relative);
            PlayPauseImage.Source = new BitmapImage(imageUri);
        }

        private void Spotify_OnTrackTimeChange(object sender, TrackTimeChangeEventArgs e)
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(() => Spotify_OnTrackTimeChange(sender, e));
                return;
            }

            if (m_CurrentTrack == null)
                return;

            CurrentTimeLabel.Content = $@"{FormatTime(e.TrackTime)}";
            EndTimeLabel.Content = $@"{FormatTime(m_CurrentTrack.Length)}";
            if (e.TrackTime < m_CurrentTrack.Length)
                TimeProgressBar.Value = (int)e.TrackTime;
        }

        private void Spotify_OnTrackChange(object sender, TrackChangeEventArgs e)
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(() => Spotify_OnTrackChange(sender, e));
                return;
            }
            UpdateTrack(e.NewTrack);
        }

        private void Spotify_OnPlayStateChange(object sender, PlayStateEventArgs e)
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(() => Spotify_OnPlayStateChange(sender, e));
                return;
            }
            UpdatePlayingStatus(e.Playing);
        }

        private static string FormatTime(double sec)
        {
            TimeSpan span = TimeSpan.FromSeconds(sec);
            string secs = span.Seconds.ToString(), mins = span.Minutes.ToString();
            if (secs.Length < 2)
                secs = "0" + secs;
            return mins + ":" + secs;
        }

        private void LinkLabel_MouseEnter(object sender, MouseEventArgs e)
        {
            Label linkLabel = (Label)sender;
            linkLabel.Foreground = Constants.LinkLabelForegroundHighlight;
        }

        private void LinkLabel_MouseLeave(object sender, MouseEventArgs e)
        {
            Label linkLabel = (Label)sender;
            linkLabel.Foreground = Constants.LinkLabelForegroundWhite;
        }

        private void PlayPauseImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (m_IsPlaying)
                m_Spotify?.Pause();
            else
                m_Spotify?.Play();
        }

        private void ExpandRetractCanvas_MouseEnter(object sender, MouseEventArgs e)
        {
            ExpandRetractRect.Opacity = 1;
            ExpandRetractRect.Fill = Constants.LinkLabelForegroundHighlight;
        }

        private void ExpandRetractCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            ExpandRetractRect.Opacity = 0.2;
            ExpandRetractRect.Fill = Constants.LinkLabelForegroundWhite;
        }

        private void ExpandRetractCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            double to = Math.Abs(ControlGrid.ActualHeight - 120) < 0.1 ? MainGrid.ActualHeight : 120;
            ExpandRetractRules(to);
        }

        public void ExpandRules()
        {
            ExpandRetractRules(MainGrid.ActualHeight);
        }

        public void RetractRules()
        {
            ExpandRetractRules(120);
        }

        private void ExpandRetractRules(double to)
        {
            AnimationTimeline animationTimeline = new DoubleAnimation(to, new Duration(TimeSpan.FromMilliseconds(250)));
            ControlGrid.BeginAnimation(HeightProperty, animationTimeline);
            ExpandRetractRect.RenderTransform = ControlGrid.ActualHeight < MainGrid.ActualHeight ? new RotateTransform(180) : new RotateTransform(0);
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            Fade(1);
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            FadeWithDelay();
        }

        private void Fade(double to, double milliSeconds = 100)
        {
            Dispatcher.Invoke(() =>
            {
                DoubleAnimation opacityAnimation = new DoubleAnimation(to, TimeSpan.FromMilliseconds(milliSeconds));
                ControlGrid.BeginAnimation(OpacityProperty, opacityAnimation);
            });
        }

        private void FadeWithDelay()
        {
            if (IsExpanded)
                return;
            m_FadeAt = DateTime.Now.AddMilliseconds(m_FadeAfterMilliseconds);
            m_FadeTimer.Enabled = true;
        }

        private void FadeTimerOnElapsed(object o, ElapsedEventArgs elapsedEventArgs)
        {
            if (m_FadeAt > DateTime.Now) 
                return;
            if (!IsExpanded)
                Fade(m_FadeTo, m_FadeTime);
            m_FadeTimer.Enabled = false;
        }

        public bool IsExpanded => Math.Abs(ControlGrid.ActualHeight - MainGrid.ActualHeight) < 0.1;

        public void Dispose()
        {
            m_Spotify?.Dispose();
            m_AlbumBitmapStream?.Dispose();
            m_ConnectBackgroundWorker?.Dispose();
            RulesControl?.Dispose();
        }    
    }
}
