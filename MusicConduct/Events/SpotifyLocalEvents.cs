using System;
using MusicConduct.Models;

namespace MusicConduct.Events
{
    /// <summary>
    /// Gathered all events here since I always feel like they are messing up the overview of a class.
    /// Also from here they are not class specific if we ever need to hook the same event up to a new class.
    /// </summary>
    public class SpotifyLocalEvents
    {
        #region ConnectionChangeEvent
        public class ConnectedChangeEventArgs : EventArgs
        {
            public bool IsConnected;
        }

        public event EventHandler<ConnectedChangeEventArgs> ConnectionChanged;

        public virtual void OnConnectionChange(ConnectedChangeEventArgs e)
        {
            EventHandler<ConnectedChangeEventArgs> handler = ConnectionChanged;
            handler?.Invoke(this, e);
        }
        #endregion

        #region TrackChangeEvent
        public class TrackChangeEventArgs : EventArgs
        {
            public string Title;
            public string Artist;
            public string Album;
        }

        public event EventHandler<TrackChangeEventArgs> TrackChanged;

        public virtual void OnTrackChange(TrackChangeEventArgs e)
        {
            EventHandler<TrackChangeEventArgs> handler = TrackChanged;
            handler?.Invoke(this, e);
        }
        #endregion
    }

    public class RuleEvents
    {
        #region CancelRuleCreationEvent
        public class CancelRuleCreationEventArgs : EventArgs { }

        public event EventHandler<CancelRuleCreationEventArgs> CancelRuleCreation;

        public virtual void OnCancelRuleCreation(CancelRuleCreationEventArgs e)
        {
            EventHandler<CancelRuleCreationEventArgs> handler = CancelRuleCreation;
            handler?.Invoke(this, e);
        }
        #endregion

        #region RuleCreationEvent
        public class RuleCreationEventArgs : EventArgs
        {
            public Rule NewRule;
        }

        public event EventHandler<RuleCreationEventArgs> RuleCreation;

        public virtual void OnRuleCreation(RuleCreationEventArgs e)
        {
            EventHandler<RuleCreationEventArgs> handler = RuleCreation;
            handler?.Invoke(this, e);
        }
        #endregion

        #region RulesChanged
        public class RulesChangedEventArgs : EventArgs { }

        public event EventHandler<RulesChangedEventArgs> RulesChanged;

        public virtual void OnRulesChanged(RulesChangedEventArgs e)
        {
            EventHandler<RulesChangedEventArgs> handler = RulesChanged;
            handler?.Invoke(this, e);
        }
        #endregion
    }
}
