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
    class Player : IBaseView, IEventListener, IDisposable, ICollisionEnabled, IEventPublisher
    {
        static float Y_POS = 0.8f;
        private const int PLAYER_EXPLOSIONS_COUNT = 100;
        TimeSpan lastUpdate;

        float currentPosition;                  // current position in relative coordinates <0,1>
        float finalPosition;

        Texture2D sprite;

        #region Transition handling
        public Vector2 TransitionOffset { get; set; }
        #endregion

        List<Explosion> playerExplosions;


        private void InitPlayerExplosions()
        {
            if (playerExplosions != null)
            {
                return;
            }
            playerExplosions = new List<Explosion>();
            for (int i = 0; i < PLAYER_EXPLOSIONS_COUNT; i++)
            {
                playerExplosions.Add(new Explosion());
            }
        }

        private void DrawExplosions(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            if (playerExplosions == null)
            {
                return;
            }

            if (CurrentState == State.EXPLODING)
            {
                bool shouldSwitchState = true;
                foreach (Explosion e in playerExplosions)
                {
                    if (e.CurrentState != Explosion.State.EXPLODED)
                    {
                        e.Draw(spriteBatch);
                        shouldSwitchState = false;
                    } 
                }

                if (shouldSwitchState)
                {
                    CurrentState = State.DEAD;
                    NotifyListeners(new PlayerDied());
                }

            }
        }

        public void Explode()
        {
            Random rnd = new Random();

            bool isFirst = true;

            CurrentState = State.EXPLODING;

            foreach (Explosion e in playerExplosions) {
                if (isFirst)
                {
                    int radius = sprite.Width * 10;

                    e.Explode(
                        radius,
                        new Vector2(currentPosition, Y_POS),
                        0);
                    isFirst = false;
                }
                else
                {
                    Vector2 randomVector = new Vector2((float)rnd.NextDouble() / 2f - 0.25f, (float)rnd.NextDouble() / 2f);
                    e.Explode(
                        (int)(sprite.Width * 8f * (float)rnd.NextDouble()),
                        new Vector2(currentPosition - (float)sprite.Width / ScreenUtils.ScreenWidth, Y_POS) + randomVector,
                        (float)rnd.NextDouble() * 3f);
                }
            }
        }

        public Vector2 GetPosition()
        {
            float k = ((float)ScreenUtils.ScreenWidth
                / ((float)sprite.Width + (float)ScreenUtils.ScreenWidth));

            Vector2 result = new Vector2(currentPosition * k + (sprite.Width / 2f) 
                / (float)ScreenUtils.ScreenWidth,
                Y_POS);

            return result;
        }

        public enum State
        {
            ALIVE, EXPLODING, DEAD
        };

        public State CurrentState;

        public IBoundingShape GetBoundingShape()
        {
            Vector2 screenPos = CalculateScreenPosition();
            return new BoundingBox(new Rectangle((int)screenPos.X, (int)screenPos.Y,
                (int)sprite.Width, (int)sprite.Height));
        }

        public void SetStartingPosition()
        {
            currentPosition = 0.5f;
        }


        #region Life cycle
        public Player()
        {
            SetStartingPosition();
            finalPosition = currentPosition;
            CurrentState = State.ALIVE;
            InitPlayerExplosions();
            World.Instance.RegisterListener(this);
            GameScreenManager.Instance.LoadContentEventListeners += this.OnLoadContentEvent;
        }

        public void Dispose()
        {
            World.Instance.UnregisterListener(this);
        }
        #endregion

        public void ProcessTouchEvent(Vector2 pos)
        {
            finalPosition = (float)pos.X / (float)(ScreenUtils.ScreenWidth);
        }

        private void UpdateScreenPosition(TimeSpan delayBetweenUpdates)
        {
            float secondsPassed = delayBetweenUpdates.Milliseconds / 1000f;
            if (finalPosition == currentPosition || secondsPassed <= 0f)
            {
                // NOP
                return;
            }
            float distance = finalPosition - currentPosition;
            float direction = distance < 0f ? -1f : 1f;
            distance = Math.Abs(distance);

            float speed = 1.5f;

            if (secondsPassed * speed > distance)
            {
                currentPosition = finalPosition;
            }
            else
            {
                currentPosition += secondsPassed * speed * direction;
            }
        }

        private Vector2 CalculateScreenPosition()
        {
            float x = (float)ScreenUtils.ScreenWidth * currentPosition
                * ((float)ScreenUtils.ScreenWidth / ((float)sprite.Width + ScreenUtils.ScreenWidth));
            float y = (float)ScreenUtils.ScreenHeight * Y_POS;

            return new Vector2((int)x, (int)y);
        }

        public void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            if (sprite == null)
            {
                return;
            }
            Vector2 pos = CalculateScreenPosition();
            if (CurrentState == State.ALIVE)
            {
                spriteBatch.Draw(sprite, pos + TransitionOffset, Constans.MAIN_SPRITE_COLOR);
            }

            DrawExplosions(spriteBatch);
        }

        #region World event handling

        private void OnLoadContentEvent(GameScreenManager.LoadContentEventArgs eventArgs) {
            sprite = eventArgs.contentManager.Load<Texture2D>("ship");
        }

        private void OnTimeUpdatedEvent(World.TimeUpdatedEvent eventArgs)
        {
            GameTime time = eventArgs.time;

            if (lastUpdate == null)
            {
                lastUpdate = time.TotalGameTime;
            }
            else
            {
                TimeSpan span = time.TotalGameTime - lastUpdate;
                UpdateScreenPosition(span);
                lastUpdate = time.TotalGameTime;
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

        #region Event publishing

        public class PlayerDied : EventArgs { }

        private delegate void PlayerEventHandler(IEventPublisher publisher, EventArgs args);
        private event PlayerEventHandler playerEventHandler;

        public void RegisterListener<T>(T listener) where T : IEventListener
        {
            playerEventHandler += new PlayerEventHandler(listener.OnNotification);
        }

        public void UnregisterListener<T>(T listener) where T : IEventListener
        {
            playerEventHandler -= new PlayerEventHandler(listener.OnNotification);
        }

        public void NotifyListeners(EventArgs args)
        {
            if (null != playerEventHandler)
            {
                playerEventHandler(this, args);
            }
        }
        #endregion
    }
}
