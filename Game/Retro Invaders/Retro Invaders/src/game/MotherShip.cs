using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPP.core2d;
using Microsoft.Xna.Framework;
using MPP.core;
using Microsoft.Xna.Framework.Graphics;

namespace MPP.game
{
    class MotherShip : IBaseView, IEventListener, IDisposable, ICollisionEnabled
    {
        public float Speed;   // in relative coordinates in seconds squared
        TimeSpan lastUpdate;

        Vector2 position;

        Color mothershipColor;

        Texture2D sprite;

        #region Life cycle

        public MotherShip(float x, float y)
        {
            position = new Vector2(x, y);
            Speed = 0.15f;
            World.Instance.RegisterListener(this);
            GameScreenManager.Instance.LoadContentEventListeners += this.OnLoadContentEvent;
        }

        public void SetPos(Vector2 pos)
        {
            position = pos;
            mothershipColor = Color.Crimson;
        }

        public void Dispose()
        {
            World.Instance.UnregisterListener(this);
        }
        #endregion

        private void UpdateScreenPosition(TimeSpan delayBetweenUpdates)
        {
            position.X += delayBetweenUpdates.Milliseconds / 1000f * Speed;
        }

        private Vector2 CalculateScreenPosition()
        {
            float x = (float)ScreenUtils.ScreenWidth * position.X
                * ((float)ScreenUtils.ScreenWidth / ((float)sprite.Width
                + ScreenUtils.ScreenWidth));
            float y = (float)ScreenUtils.ScreenHeight * position.Y
                * ((float)ScreenUtils.ScreenHeight / ((float)sprite.Height
                + ScreenUtils.ScreenHeight));

            return new Vector2((int)x, (int)y);
        }


        public enum State
        {
            ALIVE, EXPLODING, DEAD
        };

        public State CurrentState;

        public void Explode()
        {
            if (CurrentState != State.ALIVE)
            {
                return;
            }

            CurrentState = State.EXPLODING;

            Bind colorCntrl = new Bind(
                delegate { return mothershipColor; },
                delegate(object value) { mothershipColor = (Color)value; }
                );

            core2d.CoreAnimation.Instance.AnimateColor("mothership color transition", colorCntrl, Color.Transparent, 0.3f);
            SoundPlayer.Instance.StopSound("mothershipSound");
        }

        public void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            if (sprite == null)
            {
                return;
            }
            Vector2 pos = CalculateScreenPosition();

            if (CurrentState != State.DEAD)
            {
                spriteBatch.Draw(sprite, pos, mothershipColor);
            }
        }

        public bool IsOffScreen()
        {
            return position.X > 1;
        }

        #region World event handling

        public void OnLoadContentEvent(GameScreenManager.LoadContentEventArgs eventArgs)
        {
            sprite = eventArgs.contentManager.Load<Texture2D>("motherShip");
        }

        private void OnTimeUpdatedEvent(World.TimeUpdatedEvent eventArgs)
        {
            if (CurrentState != State.ALIVE || IsOffScreen())
            {
                return;
            }


            GameTime time = eventArgs.time;

            if (GameScreenManager.Instance.GetActiveScreenID() != BattleScreen.ID || GameState.Instance.CurrentGameState != GameState.State.DURING_GAMEPLAY)
            {
                lastUpdate = time.TotalGameTime;
                return;
            }

                if (lastUpdate == null)
            {
                lastUpdate = time.TotalGameTime;
            }
            else
            {
                TimeSpan span = time.TotalGameTime - lastUpdate;
                UpdateScreenPosition(span);
                lastUpdate = time.TotalGameTime;

                if (IsOffScreen())
                {
                    SoundPlayer.Instance.StopSound("mothershipSound");
                }
            }
        }

        public void OnNotification(IEventPublisher publisher, EventArgs eventArgs)
        {
            World.TimeUpdatedEvent timeEvent = eventArgs as World.TimeUpdatedEvent;

            if (timeEvent != null)
            {
                OnTimeUpdatedEvent(timeEvent);
                return;
            }
        }
        #endregion


        public IBoundingShape GetBoundingShape()
        {
            Vector2 screenPos = CalculateScreenPosition();
            return new BoundingBox(new Rectangle((int)screenPos.X, (int)screenPos.Y,
                (int)sprite.Width, (int)sprite.Height));
        }
    }
}
