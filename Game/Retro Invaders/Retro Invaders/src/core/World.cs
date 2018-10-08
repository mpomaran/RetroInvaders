using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MPP.core
{
    class World : IEventPublisher
    {
        #region Life cycle
        static private World instance;

        private World()
        {
        }

        static public World Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new World();
                }
                return instance;
            }
        }
        #endregion

        #region Public interface

        GameTime currentTime;

        public GameTime GetCurrentTime()
        {
            if (currentTime == null)
            {
                currentTime = new GameTime();
            }
            return currentTime;
        }

        public void OnTimeUpdate(GameTime time)
        {
            currentTime = time;
            TimeUpdatedEvent timeEvent = new TimeUpdatedEvent(currentTime);
            NotifyListeners(timeEvent);
        }

        #endregion

        #region Event publishing

        public class TimeUpdatedEvent : EventArgs
        {
            public GameTime time;

            public TimeUpdatedEvent(GameTime newTime)
            {
                time = newTime;
            }
        }

        private delegate void WorldEventHandler(IEventPublisher publisher, EventArgs args);
        private event WorldEventHandler worldEventHandler;

        public void RegisterListener<T>(T listener) where T : IEventListener
        {
            worldEventHandler += new WorldEventHandler(listener.OnNotification);
        }

        public void UnregisterListener<T>(T listener) where T : IEventListener
        {
            worldEventHandler -= new WorldEventHandler(listener.OnNotification);
        }

        public void NotifyListeners(EventArgs args)
        {
            if (null != worldEventHandler)
            {
                worldEventHandler(this, args);
            }
        }
#endregion
    }
}
