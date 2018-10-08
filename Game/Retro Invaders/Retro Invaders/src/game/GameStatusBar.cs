using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MPP.core;
using MPP.core2d;

namespace MPP.game
{

    class GameStatusBar : core2d.IBaseView, IEventListener
    {
        SpriteFont font;

        public GameStatusBar(SpriteFont font)
        {
            this.font = font;
            GameState.Instance.RegisterListener(this);
        }

        public void Dispose()
        {
            GameState.Instance.UnregisterListener(this);
        }


        static Dictionary<ResourceManager.StringKey, Vector2> positionMapping = new Dictionary<ResourceManager.StringKey, Vector2>
            {
                { ResourceManager.StringKey.LIVES, new Vector2(0, 0.9f) },
                { ResourceManager.StringKey.LEVEL, new Vector2(0.5f, 0.9f) },
                { ResourceManager.StringKey.SCORE, new Vector2(1f, 0.9f) },
                { ResourceManager.StringKey.UPGRADE_LEVEL, new Vector2(0.5f, 0.95f) }
            };

        private Vector2 GetPosition(ResourceManager.StringKey key)
        {
            return core2d.ScreenUtils.TranslateToScreenCoordinates(positionMapping[key]);
        }

        Color scoreColor = Color.Yellow;

        public void Draw(SpriteBatch spriteBatch)
        {
            // draw lives
            String livesString = ResourceManager.Instance.GetString(ResourceManager.StringKey.LIVES);
            Vector2 livesSize = font.MeasureString(livesString);
            Color livesColor = Color.Yellow;
            if (GameState.Instance.Lives == 1)
            {
                livesColor = Color.Yellow;
            }
            else if (GameState.Instance.Lives == 0)
            {
                livesColor = Color.Red;
            }

            spriteBatch.DrawString(font,
                livesString,
                GetPosition(ResourceManager.StringKey.LIVES),
                livesColor);

            String livesCountString = "" + (GameState.Instance.Lives > 0 ? GameState.Instance.Lives : 0);
        
            Vector2 livesNumberSize = font.MeasureString(livesCountString);
            spriteBatch.DrawString(font,
                livesCountString,
                GetPosition(ResourceManager.StringKey.LIVES) + new Vector2(livesSize.X / 2 - livesNumberSize.X / 2, livesSize.Y),
                livesColor);

            // draw points
            String scoreString = ResourceManager.Instance.GetString(ResourceManager.StringKey.SCORE);
            String scoreNumberString = "" + GameState.Instance.Score.ToString("000000");
            Vector2 scoreSize = font.MeasureString(scoreString);
            Vector2 scoreNumberSize = font.MeasureString(scoreNumberString);
            Vector2 scorePosition = GetPosition(ResourceManager.StringKey.SCORE) - new Vector2(scoreNumberSize.X, 0);
            spriteBatch.DrawString(font,
                scoreString,
                scorePosition,
                scoreColor);
            spriteBatch.DrawString(font,
                scoreNumberString,
                scorePosition + new Vector2(scoreSize.X / 2 - scoreNumberSize.X / 2, scoreSize.Y),
                scoreColor);

            // draw level
            String levelString = ResourceManager.Instance.GetString(ResourceManager.StringKey.LEVEL) + GameState.Instance.Level;
            Vector2 levelSize = font.MeasureString(levelString);
            spriteBatch.DrawString(font,
                levelString,
                GetPosition(ResourceManager.StringKey.LEVEL) - new Vector2(levelSize.X / 2f, 0),
                Color.Yellow);
            
            // draw ship level
            String shipLevelString = ResourceManager.Instance.GetString(ResourceManager.StringKey.UPGRADE_LEVEL) + " " + (GameState.Instance.BonusLevel + 1);
            Vector2 shipLevelSize = font.MeasureString(shipLevelString);
            spriteBatch.DrawString(font,
                shipLevelString,
                GetPosition(ResourceManager.StringKey.UPGRADE_LEVEL) - new Vector2(shipLevelSize.X / 2f, 0),
                Color.LimeGreen);


        }

        public void OnNotification(IEventPublisher publisher, EventArgs eventArgs)
        {
            if (eventArgs as GameState.ScoreCountChanged != null)
            {
                Bind pctl = new Bind(
                    delegate { return scoreColor; },
                    delegate(object value) { scoreColor = (Color)value; }
                );

                scoreColor = Color.White;
                CoreAnimation.Instance.AnimateColor("scoreAnimation", pctl, Color.Yellow, 0.5f);

            }
        }
    }
}
