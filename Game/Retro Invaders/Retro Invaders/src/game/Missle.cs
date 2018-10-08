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
    class Missle : IBaseView, IEventListener, IDisposable, ICollisionEnabled
    {
        public float Speed;   // in relative coordinates in seconds squared
        TimeSpan lastUpdate;

        Vector2 position;
        Vector2 prevPosition;

        Texture2D sprite;

        #region Transition handling
        public Vector2 TransitionOffset { get; set; }
        #endregion

        #region Life cycle

        private float direction;

        private bool friendly;

        public Missle(float x, float y, bool friendly)
        {
            prevPosition = position = new Vector2(x, y);
            Speed = -0.5f;
            direction = friendly ? 1.0f : -1.0f;
            this.friendly = friendly;
            World.Instance.RegisterListener(this);
            GameScreenManager.Instance.LoadContentEventListeners += this.OnLoadContentEvent;
        }

        public void SetPos(Vector2 pos)
        {
            position = pos;
        }

        public void Dispose()
        {
            World.Instance.UnregisterListener(this);
        }
        #endregion

        private void UpdateScreenPosition(TimeSpan delayBetweenUpdates)
        {
            // TODO
            prevPosition = position;
            if (friendly)
            {
                position.Y += direction * delayBetweenUpdates.Milliseconds / 1000f * (Speed * (GameState.Instance.BonusLevel * 0.3f + 1));
            }
            else
            {
                position.Y += direction * delayBetweenUpdates.Milliseconds / 1000f * Speed;
            }
        }

        private Vector2 CalculateScreenPosition(Vector2 position)
        {
            float x = (float)ScreenUtils.ScreenWidth * position.X
                * ((float)ScreenUtils.ScreenWidth / ((float)sprite.Width 
                + ScreenUtils.ScreenWidth));
            float y = (float)ScreenUtils.ScreenHeight * position.Y
                * ((float)ScreenUtils.ScreenHeight / ((float)sprite.Height 
                + ScreenUtils.ScreenHeight));

            return new Vector2((int)x, (int)y);
        }

        
        public void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            if (sprite == null)
            {
                return;
            }
            Vector2 pos = CalculateScreenPosition(position);

            spriteBatch.Draw(sprite, pos + TransitionOffset, Constans.MAIN_SPRITE_COLOR);
        }

        public bool IsOffScreen() {
            if (direction < 0)
            {
                return position.Y > 1;
            }
            else
            {
                return position.Y < 0;
            }
        }

        #region World event handling

        public void OnLoadContentEvent(GameScreenManager.LoadContentEventArgs eventArgs) {
            sprite = eventArgs.contentManager.Load<Texture2D>("playerMissle");
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


        public IBoundingShape GetBoundingShape()
        {
            Vector2 screenPos = CalculateScreenPosition(position);
            Vector2 prevPos = CalculateScreenPosition(prevPosition);

            int height;
            if (friendly)
            {
                height = (int)sprite.Height + (int)prevPos.Y - (int)screenPos.Y;
            }
            else
            {
                height = sprite.Height;
            }

            return new BoundingBox(new Rectangle((int)screenPos.X, (int)screenPos.Y, 
                (int)sprite.Width, height));
        }
    }
}
