using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework;
using MPP.core;
using Microsoft.Xna.Framework.Graphics;
using MPP.core2d;

namespace MPP.game
{
    class HelpScreen : GameScreen, IDisposable
    {
        #region Class members

        private Texture2D background;
        private core2d.AnimatedButton okButton;


        private const string OK_BUTTON_LABEL = "OK";

        #endregion

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

        #region Life cycle

        public void Dispose()
        {
        }

        public HelpScreen(GraphicsDevice device) :
            base(device)
        {
            GameScreenManager.Instance.LoadContentEventListeners += this.OnLoadContentEvent;
        }
        #endregion

        #region Transitions

        public override void MoveIn()
        {
            okButton.StartTransition((int)((double)ScreenUtils.ScreenWidth * 2.08), 650, 0, 650, 0, 0.5f);
        }

        public override void MoveOut()
        {
            int x0 = 0;
            int x1 = (int)((float)ScreenUtils.ScreenWidth * 1.04f);
            int y = okButton.y;
            float duration = 0.5f;

            okButton.StartTransition(x0, y, x1, y, 0f, duration);
            
            
        }

        #endregion

        #region Event handling

        public void OnButtonTransition(AnimatedButton button)
        {
            OnScreenTransition(new TransitionCompletedEventArgs());
        }

        public void OnButtonPressed(AnimatedButton button)
        {
                GameScreenManager.Instance.MoveToScreen(MenuScreen.ID);
        }

        public void OnLoadContentEvent(MPP.core2d.GameScreenManager.LoadContentEventArgs eventArgs)
        {
            background = ScreenUtils.GetTexture(ResourceManager.Instance.GetString(ResourceManager.StringKey.HELP_SCREEN_BACKGROUND));

            okButton = new AnimatedButton(HelpScreen.OK_BUTTON_LABEL);
            okButton.ButtonPressedListener += this.OnButtonPressed;
            okButton.ButtonTransitionListener += this.OnButtonTransition;
        }

        public override void ProcessTouchEvents(int x, int y, TouchLocationState tlState)
        {

            okButton.ProcessTouchEvent(x, y, tlState);
        }

        private void DrawBackground(SpriteBatch batch)
        {
            Rectangle FULL_SCREEN = new Rectangle(0, 0, ScreenUtils.ScreenWidth,
                ScreenUtils.ScreenHeight);

            float alpha;



            try
            {
                alpha =
                    (float)(ScreenUtils.ScreenWidth - okButton.x)
                    / (float)ScreenUtils.ScreenWidth;
            }
            catch (System.Exception)
            {
                alpha = 1.0f;
            }

            Color color = new Color(alpha, alpha, alpha, alpha);

            batch.Draw(background, FULL_SCREEN, color);
        }

        public override void Draw()
        {
            spriteBatch.Begin();
            DrawBackground(spriteBatch);
            okButton.Draw(spriteBatch);
        }
        #endregion

    }
}
