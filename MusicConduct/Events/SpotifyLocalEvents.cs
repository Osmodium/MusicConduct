using System;

namespace MusicConduct.Events
{
    public class SpotifyLocalEvents
    {
        #region ConnectionChangeEvent
        public event EventHandler<ConnectedChangeEventArgs> ConnectionChanged;

        public virtual void OnConnectionChange(ConnectedChangeEventArgs e)
        {
            EventHandler<ConnectedChangeEventArgs> handler = ConnectionChanged;
            handler?.Invoke(this, e);
        }

        public class ConnectedChangeEventArgs : EventArgs
        {
            public bool IsConnected;
        }
        #endregion

        #region TrackChangeEvent
        public event EventHandler<TrackChangeEventArgs> TrackChanged;

        public virtual void OnTrackChange(TrackChangeEventArgs e)
        {
            EventHandler<TrackChangeEventArgs> handler = TrackChanged;
            handler?.Invoke(this, e);
        }

        public class TrackChangeEventArgs : EventArgs
        {
            public string Title;
            public string Artist;
            public string Album;
        }
        #endregion
    }
}
