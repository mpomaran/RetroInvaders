using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MPP.core2d;
using MPP.core;
using Microsoft.Xna.Framework.Input.Touch;

namespace MPP.game
{
    class GameOverScreen : GameScreen, IDisposable, IEventListener
    {
        #region Screen identification
        private static GameScreenID id = null;
        public static GameScreenID ID
        {
            get
            {
                if (id == null)
                {
                    id = new GameScreenID();
                }
                return id;
            }
        }

        public override GameScreenID GetID()
        {
            return ID;
        }
        #endregion

        #region members
        private Texture2D fadeOutOverlay;
        private Color fadeoutColor;
        AnimatedButton mainMenuButton;
        Texture2D background;
        SpriteFont font;
        const string FADE_ANIM_NAME = "game over fade anim name";
        #endregion

        #region Lifecycle
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public GameOverScreen(GraphicsDevice device)
            : base(device)
        {
            GameScreenManager.Instance.LoadContentEventListeners += this.OnLoadContentEvent;
        }
        #endregion

        #region Event handling

        private void OnButtonPressed(AnimatedButton button)
        {
            GameScreenManager.Instance.MoveToScreen(MenuScreen.ID);
        }

        private void OnLoadContentEvent(GameScreenManager.LoadContentEventArgs eventArgs)
        {
            fadeOutOverlay = ScreenUtils.GetTexture("fadeout_overlay");
            font = ScreenUtils.GetFont("GameOverFont");

            mainMenuButton = new AnimatedButton("mainMenuButton");
            mainMenuButton.SetLabel(Constans.MENU_SELECTED_BUTTON_BACKGROUND_COLOR, Constans.MENU_NON_SELECTED_BUTTON_BACKGROUND_COLOR, Constans.MENU_NON_SELECTED_BUTTON_BACKGROUND_COLOR,
                Constans.MENU_NON_ACTIVE_TEXT_COLOR, Constans.MENU_FONT_NAME,
                ResourceManager.Instance.GetString(ResourceManager.StringKey.MAIN_MENU_BUTTON_LABEL),
                Constans.STANDARD_BUTTON_WIDTH, Constans.STANDARD_BUTTON_HEIGHT);
            mainMenuButton.ButtonPressedListener += OnButtonPressed;
            mainMenuButton.x = 0;
            mainMenuButton.y = ScreenUtils.ScreenHeight - Constans.STANDARD_BUTTON_HEIGHT - Constans.STANDARD_BUTTON_HEIGHT / 10;

            background = ScreenUtils.GetTexture("gameOverBackground");
        }

        #endregion

        #region GameScreen methods

        private void FadeScreen(Color startColor, Color endColor, float duration, string animName)
        {
            Bind pctl = new Bind(
                delegate { return fadeoutColor; },
                delegate(object value) { fadeoutColor = (Color)value; }
            );

            fadeoutColor = startColor;
            CoreAnimation.Instance.AnimateColor(animName, pctl, endColor, duration);
            core2d.CoreAnimation.Instance.AddListener(animName, this);
        }


        public override void ProcessTouchEvents(int x, int y, TouchLocationState tlState)
        {
            mainMenuButton.ProcessTouchEvent(x, y, tlState);
        }

        public override void MoveIn()
        {
            FadeScreen(Color.White, Color.Transparent, 0.15f, FADE_ANIM_NAME);
        }

        public override void MoveOut()
        {
            FadeScreen(Color.Transparent, Color.White, 0.15f, FADE_ANIM_NAME);
        }

        public override void Draw()
        {
            spriteBatch.Begin();

            spriteBatch.Draw(background, new Rectangle(0, 0, ScreenUtils.ScreenWidth, ScreenUtils.ScreenHeight), Color.White);
            mainMenuButton.Draw(spriteBatch);

            Color scoreColor = Constans.MENU_SELECTED_BUTTON_BACKGROUND_COLOR;
            Color levelColor = Constans.MAIN_SPRITE_COLOR;

            String scoreString;
            String highScoreString;
            String highScoreNumberString;

            if (GameState.Instance.Score == GameState.Instance.HighScore)
            {
                scoreString = ResourceManager.Instance.GetString(ResourceManager.StringKey.NEW_HIGHSCORE_SUMMARY);
                highScoreString = ResourceManager.Instance.GetString(ResourceManager.StringKey.PREVIOUS_HIGHSCORE_SUMMARY);

                int lastHighScore = GameState.Instance.LastHighScore > 1 ? GameState.Instance.LastHighScore : 0;

                highScoreNumberString = "" + lastHighScore.ToString("000000");
            }
            else
            {
                scoreString = ResourceManager.Instance.GetString(ResourceManager.StringKey.SCORE_SUMMARY);
                highScoreString = ResourceManager.Instance.GetString(ResourceManager.StringKey.HIGHSCORE_SUMMARY);
                highScoreNumberString = "" + GameState.Instance.HighScore.ToString("000000");
            }

            String scoreNumberString = "" + GameState.Instance.Score.ToString("000000");
            Vector2 scoreSize = font.MeasureString(scoreString);
            Vector2 scoreNumberSize = font.MeasureString(scoreNumberString);
            Vector2 scorePosition = new Vector2(ScreenUtils.ScreenWidth, 0) / 2f - scoreSize / 2f;
            Vector2 scoreNumberPosition = new Vector2(ScreenUtils.ScreenWidth, 0) / 2f - scoreNumberSize / 2f;
            scorePosition.Y = 300;
            scoreNumberPosition.Y = scorePosition.Y + scoreNumberSize.Y * 1.1f;
            spriteBatch.DrawString(font,
                scoreString,
                scorePosition,
                scoreColor);
            spriteBatch.DrawString(font,
                scoreNumberString,
                scoreNumberPosition,
                levelColor);

            Vector2 highScoreNumberSize = font.MeasureString(highScoreNumberString);
            Vector2 highScoreSize = font.MeasureString(highScoreString);
            Vector2 highScorePosition = new Vector2(ScreenUtils.ScreenWidth, 0) / 2f - highScoreSize / 2f;
            Vector2 highScoreNumberPosition = new Vector2(ScreenUtils.ScreenWidth, 0) / 2f - highScoreNumberSize / 2f;
            highScorePosition.Y = 500;
            highScoreNumberPosition.Y = highScorePosition.Y + highScoreSize.Y * 1.1f;

            spriteBatch.DrawString(font,
                highScoreString,
                highScorePosition,
                scoreColor);
            spriteBatch.DrawString(font,
                highScoreNumberString,
                highScoreNumberPosition,
                levelColor);

            spriteBatch.Draw(fadeOutOverlay, new Rectangle(0, 0, ScreenUtils.ScreenWidth, ScreenUtils.ScreenHeight), fadeoutColor);
            spriteBatch.End();

        }
        #endregion


        public void OnNotification(IEventPublisher publisher, EventArgs eventArgs)
        {
            PropertyAnimation.AnimationEnded animEndedEvent = eventArgs as PropertyAnimation.AnimationEnded;

            if (animEndedEvent != null)
            {
                if (animEndedEvent.AnimName.Equals(FADE_ANIM_NAME))
                {
                    OnScreenTransition(new TransitionCompletedEventArgs());
                }
            }
        }
    }
}
