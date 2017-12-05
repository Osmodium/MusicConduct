using System;
using MusicConduct.Models;

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

    public class RuleEvents
    {
        #region CancelRuleCreationEvent
        public event EventHandler<CancelRuleCreationEventArgs> CancelRuleCreation;

        public virtual void OnCancelRuleCreation(CancelRuleCreationEventArgs e)
        {
            EventHandler<CancelRuleCreationEventArgs> handler = CancelRuleCreation;
            handler?.Invoke(this, e);
        }

        public class CancelRuleCreationEventArgs : EventArgs
        {}
        #endregion

        #region RuleCreationEvent
        public event EventHandler<RuleCreationEventArgs> RuleCreation;

        public virtual void OnRuleCreation(RuleCreationEventArgs e)
        {
            EventHandler<RuleCreationEventArgs> handler = RuleCreation;
            handler?.Invoke(this, e);
        }

        public class RuleCreationEventArgs : EventArgs
        {
            public Rule NewRule;
        }
        #endregion
    }
}
