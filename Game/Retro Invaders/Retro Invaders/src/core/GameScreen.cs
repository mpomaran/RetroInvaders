using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MPP.core2d;
using MPP.core;

namespace MPP
{
    class GameScreenID
    {
        private int id;
        private static int counter = 0;

        public GameScreenID()
        {
            id = counter;
            counter++;
        }

        public GameScreenID(GameScreenID x)
        {
            this.id = x.id;
        }

        public static bool operator ==(GameScreenID a, GameScreenID b) {
            if ((object)a == null && (object)b == null)
            {
                return true;
            }

            if ((object)a == null || (object)b == null)
            {
                return false;
            }

            return a.id == b.id;
        }

        public static bool operator !=(GameScreenID a, GameScreenID b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(GameScreenID))
            {
                return false;
            }
            else
            {
                return (GameScreenID)obj == this;
            }
        }

        public override int GetHashCode()
        {
            return id;
        }
    }

    abstract class GameScreen : IEventPublisher
    {
        abstract public GameScreenID GetID();

        protected SpriteBatch spriteBatch;

        public GameScreen(GraphicsDevice device)
        {
            spriteBatch = new SpriteBatch(device);
        }

        #region Common interface
        abstract public void ProcessTouchEvents(int x, int y, TouchLocationState state);
        abstract public void MoveIn();
        abstract public void MoveOut();
        abstract public void Draw();
        #endregion

        #region Notifications

        public class TransitionCompletedEventArgs : EventArgs
        {
            public bool IsVisible;
        };

        public delegate void ScreenTransitionEventHandler(TransitionCompletedEventArgs args);
        public event ScreenTransitionEventHandler ScreenTransitionListener;
        public void OnScreenTransition(TransitionCompletedEventArgs args)
        {
            ScreenTransitionListener(args);
        }

        private delegate void ScreenEventHandler(IEventPublisher publisher, EventArgs args);
        private event ScreenEventHandler screenEventHandler;

        public void RegisterListener<T>(T listener) where T : IEventListener
        {
            screenEventHandler += new ScreenEventHandler(listener.OnNotification);
        }

        public void UnregisterListener<T>(T listener) where T : IEventListener
        {
            screenEventHandler -= new ScreenEventHandler(listener.OnNotification);
        }

        public void NotifyListeners(EventArgs args)
        {
            if (null != screenEventHandler)
            {
                screenEventHandler(this, args);
            }
        }

        protected void CreateRenderBatch() 
        {
            spriteBatch = ScreenUtils.CreateSpriteBatch();
        }


        #endregion
    }
}
