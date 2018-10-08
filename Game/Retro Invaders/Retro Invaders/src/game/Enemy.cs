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
    class Enemy : IEventListener, IDisposable, ICollisionEnabled
    {
        Vector2 position;
        Texture2D sprite;

        PrimitiveLine line;
        Texture2D circle;
        float explosionTime;

        int row;

        public enum State
        {
            ALIVE, EXPLODING, DEAD
        };

        public State CurrentState;

#region Life cycle
        public void Init(Vector2 formation)
        {
            position = new Vector2(0.0f, 0.0f);
            CurrentState = State.ALIVE;
            FormationPosition = formation;
            TransitionOffset = new Vector(0, 0);
        }

        public Enemy(Vector2 formation)
        {
            this.row = (int)formation.Y;
            Init(formation);
            World.Instance.RegisterListener(this);
            GameScreenManager.Instance.LoadContentEventListeners += this.OnLoadContentEvent;

        }

        public void UpdatePos(float x, float y)
        {
            position = new Vector2(x, y);
        }

        public Vector2 GetPos()
        {
            Vector2 result = CalculateScreenPosition();
            result.X += sprite.Width / 2;
            result.Y += sprite.Height;
            result.X /= (float)ScreenUtils.ScreenWidth;
            result.Y /= (float)ScreenUtils.ScreenHeight;

            return result;
        }

        public void Dispose()
        {
            World.Instance.UnregisterListener(this);
        }
        #endregion

        #region Transition handling
        public Vector TransitionOffset { 
            get; 
            set; 
        }
        #endregion

        public void Explode()
        {
            if (CurrentState != State.ALIVE)
            {
                return;
            }

            CurrentState = State.EXPLODING;
            explosionTime = 0f;
        }

        private TimeSpan launchTime;
        public void Launch(TimeSpan time)
        {
            launchTime = time;
            CurrentState = State.ALIVE;
        }

        public Vector2 FormationPosition { get; set; } 

        private Vector2 CalculateScreenPosition()
        {
            float x = (float)ScreenUtils.ScreenWidth * position.X + FormationPosition.X * 1.3f * sprite.Width;
            float y = (float)ScreenUtils.ScreenHeight * (1f / 10f + position.Y) 
                + FormationPosition.Y * 1.3f * sprite.Width;
            x += TransitionOffset.X;
            y += TransitionOffset.Y;

            return new Vector2((int)x, (int)y);
        }

        public void DrawExplosion(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            if (explosionTime > 0.3f)
            {
                CurrentState = State.DEAD;
            }

            float circleRadius;
            float f = 1f - explosionTime * 3.33f;
            float k;
            if (explosionTime < 0.1f)
            {
                circleRadius = (sprite.Width / 2f) * 1.3f - explosionTime * explosionTime * 50f + 5 + 50 * explosionTime ;
                k = 1;
            }
            else
            {
                circleRadius = (sprite.Width / 2f) * 1.3f - explosionTime * explosionTime * 200f;
                k = f+0.3f;
            }
            // 0.1 = 5
            float blastRadius = (sprite.Width / 2f) * 1.3f + 50f * (float)(Math.Sqrt(explosionTime * 4f));

            Color blastColor = new Color(Constans.MAIN_EXPLOSION_COLOR.R / 255f * f * 1.5f,
                Constans.MAIN_EXPLOSION_COLOR.G / 255f * f * 1.5f,
                Constans.MAIN_EXPLOSION_COLOR.B / 255f * f * 1.5f, f * 1.5f);
            line.ClearVectors();
            line.CreateCircle(blastRadius, 20);

            Vector2 corner = CalculateScreenPosition();

            float blastX = corner.X + sprite.Width / 2f;
            float blastY = corner.Y + sprite.Height / 2f;
            line.Colour = blastColor;
            line.Render(spriteBatch, new Vector2((int)blastX, (int)blastY));

            if (circleRadius >= 0)
            {
                
                corner.X += sprite.Width / 2f;
                corner.Y += sprite.Height / 2f;

                corner.X -= circleRadius;
                corner.Y -= circleRadius;

                Color color = new Color(Constans.MAIN_EXPLOSION_COLOR.R / 255f * k , 
                    Constans.MAIN_EXPLOSION_COLOR.G * k / 255f, 
                    Constans.MAIN_EXPLOSION_COLOR.B * k / 255f, k);

                spriteBatch.Draw(circle, 
                    new Rectangle((int)corner.X, (int)corner.Y, (int)circleRadius*2, (int)circleRadius*2), 
                    color);
            }
        }

        public void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, int row)
        {
            if (sprite == null)
            {
                return;
            }

            if (CurrentState == State.ALIVE)
            {
                float k = 0.5f + row / 10f;
                int r = (int)((float)Constans.MAIN_SPRITE_COLOR.R * k);
                int g = (int)((float)Constans.MAIN_SPRITE_COLOR.G * k);
                int b = (int)((float)Constans.MAIN_SPRITE_COLOR.B * k);
                Color color = new Color(r, g, b, Constans.MAIN_SPRITE_COLOR.A);
                spriteBatch.Draw(sprite, CalculateScreenPosition(), color);
            }
            else if (CurrentState == State.EXPLODING)
            {
                DrawExplosion(spriteBatch);
            }

//            line.Render(spriteBatch, CalculateScreenPosition());

        }

        public IBoundingShape GetBoundingShape()
        {
            Vector2 screenPos = CalculateScreenPosition();
            return new BoundingBox(new Rectangle((int)screenPos.X, (int)screenPos.Y,
                (int)sprite.Width, (int)sprite.Height));
        }


        #region World event handling

        public void OnLoadContentEvent(GameScreenManager.LoadContentEventArgs eventArgs) {

            List<string> enemies = new List<string> { "alien1", "alien2", "alien3" };
            int index = row % enemies.Count;

            sprite = eventArgs.contentManager.Load<Texture2D>(enemies[index]);
            circle = eventArgs.contentManager.Load<Texture2D>("circle");
            line = new PrimitiveLine();
            line.CreateCircle(200, 40);
        }

        private void OnTimeUpdatedEvent(World.TimeUpdatedEvent eventArgs)
        {
            if (launchTime == null)
            {
                return;
            }

            TimeSpan time = eventArgs.time.TotalGameTime - launchTime;

            if (CurrentState == State.ALIVE)
            {
         //       position.X = ((float)time.Seconds % 10f) * (1f / 10f) + 0.1f * time.Milliseconds / 1000f;
            }
            else
            {
                explosionTime += eventArgs.time.ElapsedGameTime.Milliseconds / 1000f;
            }
            /*
            for (int i = 0; i < 100; i++)
            {
                line.ClearVectors();
                float c = 1f - time.TotalGameTime.Milliseconds / 1000f;
                line.Colour = new Color(c, c, c, c);
                line.CreateCircle(time.TotalGameTime.Milliseconds / 3f, 40);
            }
             */
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
