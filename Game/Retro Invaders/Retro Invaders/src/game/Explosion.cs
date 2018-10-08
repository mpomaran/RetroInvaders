using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPP.core2d;
using MPP.core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MPP.game
{
    class Explosion : IBaseView, IEventListener
    {
        private int spriteWidth;
        PrimitiveLine line;
        Texture2D circle;
        float explosionTime;

        bool shouldPlaySound;

        public enum State
        {
            EXPLODING, EXPLODED
        };
        public State CurrentState;

        private Vector2 position;

        public Explosion()
        {
            explosionTime = 1000;
            CurrentState = State.EXPLODED;
            shouldPlaySound = false;
            World.Instance.RegisterListener(this);
            GameScreenManager.Instance.LoadContentEventListeners += this.OnLoadContentEvent;
        }

        public void Explode(int radius, Vector2 position, float delay)
        {
            this.spriteWidth = radius;
            this.position = position;
            this.explosionTime = -delay;
            shouldPlaySound = true;
            CurrentState = State.EXPLODING;
        }

        public void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            if (explosionTime < 0)
                return;

            if (shouldPlaySound)
            {
                SoundPlayer.Instance.PlaySound("explosion1");
                shouldPlaySound = false;
            }

            if (explosionTime > 0.3f || CurrentState.Equals(State.EXPLODED))
            {
                CurrentState = State.EXPLODED;
                return;
            }

            float circleRadius;
            float f = 1f - explosionTime * 3.33f;
            float k;
            if (explosionTime < 0.1f)
            {
                circleRadius = (spriteWidth / 2f) * 1.3f - explosionTime * explosionTime * 50f + 5 + 50 * explosionTime;
                k = 1;
            }
            else
            {
                circleRadius = (spriteWidth / 2f) * 1.3f - explosionTime * explosionTime * 200f;
                k = f + 0.3f;
            }
            // 0.1 = 5
            float blastRadius = (spriteWidth / 2f) * 1.3f + 50f * (float)(Math.Sqrt(explosionTime * 4f));
            Color blastColor = new Color(Constans.MAIN_PLAYER_EXPLOSION_COLOR.R / 255f * f * 1.5f,
                Constans.MAIN_PLAYER_EXPLOSION_COLOR.G / 255f * f * 1.5f,
                Constans.MAIN_PLAYER_EXPLOSION_COLOR.B / 255f * f * 1.5f, f * 1.5f);
            line.ClearVectors();
            line.CreateCircle(blastRadius, 100);

            Vector2 corner = ScreenUtils.TranslateToScreenCoordinates(position);

            float blastX = corner.X /*- spriteWidth / 2f*/;
            float blastY = corner.Y/* - spriteWidth / 2f*/;
            line.Colour = blastColor;
            line.Render(spriteBatch, new Vector2((int)blastX, (int)blastY));

            if (circleRadius >= 0)
            {

                corner.X -= circleRadius;
                corner.Y -= circleRadius;

                Color color = new Color(Constans.MAIN_PLAYER_EXPLOSION_COLOR.R / 255f * k, 
                Constans.MAIN_PLAYER_EXPLOSION_COLOR.G / 255f * k, 
                Constans.MAIN_PLAYER_EXPLOSION_COLOR.B / 255f * k, k);
                spriteBatch.Draw(circle,
                    new Rectangle((int)corner.X, (int)corner.Y, (int)circleRadius * 2, (int)circleRadius * 2),
                    color); 
            }
        }

        #region World event handling

        private void OnLoadContentEvent(GameScreenManager.LoadContentEventArgs eventArgs)
        {
            circle = eventArgs.contentManager.Load<Texture2D>("circle");
            line = new PrimitiveLine();
            line.CreateCircle(200, 1000);
        }

        private void OnTimeUpdatedEvent(World.TimeUpdatedEvent eventArgs)
        {
            explosionTime += eventArgs.time.ElapsedGameTime.Milliseconds / 1000f;
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
    }
}
