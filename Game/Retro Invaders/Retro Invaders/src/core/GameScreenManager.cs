using Microsoft.Xna.Framework.Input.Touch;
using System.Collections.Generic;
using System.Reflection;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using MPP.core;

namespace MPP.core2d
{
    class GameScreenManager
    {
        #region Lifecycle
        static private GameScreenManager instance;
        static public GameScreenManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameScreenManager();
                }
                return instance;
            }
        }

        private GameScreenManager()
        {
            screens = new Dictionary<GameScreenID, GameScreen>();
        }
        #endregion

        #region Screen management

        private Dictionary<GameScreenID, GameScreen> screens;
        private GameScreenID activeScreen;
        private GameScreenID nextScreen;

        public void AddScreen(GameScreen screen)
        {
            if (screens.ContainsKey(screen.GetID()))
            {
                throw new System.ArgumentException("Duplicate screen ID is not allowed");
            }

            screens[screen.GetID()] = screen;
            screen.ScreenTransitionListener += OnGameScreenNotification;
        }

        public GameScreenID GetActiveScreenID()
        {
            if (nextScreen != null)
                return nextScreen;
            else
                return activeScreen;
        }

        public void MoveToScreen(GameScreenID id)
        {
            nextScreen = id;

            if (activeScreen != null)
            {
                screens[activeScreen].MoveOut();
            }
            else
            {
                OnGameScreenNotification(new GameScreen.TransitionCompletedEventArgs());
            }
        }
        #endregion

        #region Input/output

        public void ProcessTouchEvents()
        {
            if (activeScreen == null)
            {
                return;
            }

            // Process touch events
            TouchCollection touchCollection = TouchPanel.GetState();
            foreach (TouchLocation tl in touchCollection)
            {
                if ((tl.State == TouchLocationState.Pressed)
                        || (tl.State == TouchLocationState.Moved)
                        || (tl.State == TouchLocationState.Released))
                {
                    screens[activeScreen].ProcessTouchEvents((int)tl.Position.X, (int)tl.Position.Y, tl.State);
                }
            }
        }

        public void Draw()
        {
            if (activeScreen != null)
            {
                screens[activeScreen].Draw();
            }
        }

        #endregion

        #region Global graphics events
        public class LoadContentEventArgs : EventArgs
        {
            public ContentManager contentManager;
            public LoadContentEventArgs(ContentManager cmgr)
            {
                contentManager = cmgr;
            }
        }

        public delegate void LoadContentEventHandler(LoadContentEventArgs args);
        public event LoadContentEventHandler LoadContentEventListeners;

        public void OnLoadContent(LoadContentEventArgs args)
        {
            LoadContentEventListeners(args);
        }

        public delegate void OnActivatedEventHandler(EventArgs args);
        public event OnActivatedEventHandler OnActivatedEventListeners;

        public void OnActivated(EventArgs args)
        {
            OnActivatedEventListeners(args);
        }


        #endregion

        #region Notification processing

        private void OnGameScreenNotification(GameScreen.TransitionCompletedEventArgs eventArgs)
        {
                if (nextScreen == null)
                {
                    return;
                }
                    activeScreen = nextScreen;
                    if (activeScreen != null)
                    {
                        screens[activeScreen].MoveIn();
                    }
                    nextScreen = null;
        }
        #endregion
    }
}
