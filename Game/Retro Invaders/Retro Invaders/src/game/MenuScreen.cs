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
    class MenuScreen : GameScreen, IDisposable
    {
        #region Class members

        private Texture2D background;
        private core2d.AnimatedButton[] buttons;

        AnimatedButton lastButtonPressed;
        int transitionButtonCountdown;

        private const string START_A_NEW_GAME_BUTTON_LABEL = "START_A_NEW_GAME_BUTTON_LABEL";
        private const string HELP_BUTTON_LABEL = "HELP_BUTTON_LABEL";
        private const string SOUND_SETTINGS_BUTTON_LABEL = "SOUND_SETTINGS_BUTTON_LABEL";

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

        public MenuScreen(GraphicsDevice device) :
            base(device)
        {
            GameScreenManager.Instance.LoadContentEventListeners += this.OnLoadContentEvent;
            transitionButtonCountdown = 0;
        }

        #endregion

        #region Transitions

        public override void MoveIn()
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                int x0 = (int)((double)ScreenUtils.ScreenWidth * 2.08);
                int x1 = 0;
                int y = (int)((double)ScreenUtils.ScreenHeight / 2)
                    + i * (int)((double)ScreenUtils.ScreenHeight / 7.27);
                double duration = 0.5;
                double delay = 0.05 * i;

                buttons[i].StartTransition(x0, y, x1, 0, delay, duration);
                transitionButtonCountdown++;
            }

            SetNewGameButtonLabel();
            SetSoundButtonLabel();
        }

        public override void MoveOut()
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                core2d.AnimatedButton button = buttons[i];
                float delay;
                if (button.Equals(lastButtonPressed))
                {
                    delay = 0.3f;
                }
                else
                {
                    delay = 0.0f;
                }

                delay += 0.05f * i;

                int x0 = 0;
                int x1 = (int)((float)ScreenUtils.ScreenWidth * 1.04f);
                int y = button.y;
                float duration = 0.5f;

                button.StartTransition(x0, y, x1, y, delay, duration);
                transitionButtonCountdown++;
            }
        }

        #endregion

        #region Event handling

        public void OnButtonTransition(AnimatedButton button)
        {
            transitionButtonCountdown--;

            if (transitionButtonCountdown == 0)
            {
                OnScreenTransition(new TransitionCompletedEventArgs());
            }
        }

        public void OnButtonPressed(AnimatedButton button)
        {
            lastButtonPressed = button;

            if (button.Name.Equals(MenuScreen.SOUND_SETTINGS_BUTTON_LABEL))
            {
                SoundPlayer.Instance.IsSoundEnabled = !SoundPlayer.Instance.IsSoundEnabled;
                SetSoundButtonLabel();
            }
            else if (button.Name.Equals(MenuScreen.HELP_BUTTON_LABEL))
            {
                // NOP
            }
            else if (button.Name.Equals(MenuScreen.START_A_NEW_GAME_BUTTON_LABEL))
            {
                GameScreenManager.Instance.MoveToScreen(BattleScreen.ID);
                BattleScreen.shouldBeRestarted = true;
            }
            else
            {
                throw new NotSupportedException("Button " + button.Name + " not implemented");
            }
        }

        private void SetSoundButtonLabel()
        {
            string soundButtonLabel = SoundPlayer.Instance.IsSoundEnabled ?
                ResourceManager.Instance.GetString(ResourceManager.StringKey.SOUND_ON)
                : ResourceManager.Instance.GetString(ResourceManager.StringKey.SOUND_OFF);

            buttons[1].SetLabel(Constans.MENU_SELECTED_BUTTON_BACKGROUND_COLOR, 
                Constans.MENU_NON_SELECTED_BUTTON_BACKGROUND_COLOR, 
                Constans.MENU_NON_SELECTED_BUTTON_BACKGROUND_COLOR,
                Constans.MENU_NON_ACTIVE_TEXT_COLOR, Constans.MENU_FONT_NAME,
                soundButtonLabel,
                Constans.STANDARD_BUTTON_WIDTH, Constans.STANDARD_BUTTON_HEIGHT);

            CreateRenderBatch();
        }

        private void SetNewGameButtonLabel()
        {
            string startButtonLabel = GameState.Instance.IsGameInProgress ?
                ResourceManager.Instance.GetString(ResourceManager.StringKey.RESUME_THE_GAME)
                : ResourceManager.Instance.GetString(ResourceManager.StringKey.START_A_NEW_GAME);

            buttons[0].SetLabel(Constans.MENU_SELECTED_BUTTON_BACKGROUND_COLOR,
                Constans.MENU_NON_SELECTED_BUTTON_BACKGROUND_COLOR,
                Constans.MENU_NON_SELECTED_BUTTON_BACKGROUND_COLOR,
                Constans.MENU_NON_ACTIVE_TEXT_COLOR, Constans.MENU_FONT_NAME,
                startButtonLabel,
                Constans.STANDARD_BUTTON_WIDTH, Constans.STANDARD_BUTTON_HEIGHT);

            CreateRenderBatch();
        }

        public void OnLoadContentEvent(MPP.core2d.GameScreenManager.LoadContentEventArgs eventArgs)
        {
            background = ScreenUtils.GetTexture("mainMenuBackground");

            buttons = new core2d.AnimatedButton[2];

            buttons[0] = new AnimatedButton(MenuScreen.START_A_NEW_GAME_BUTTON_LABEL);
            SetNewGameButtonLabel();


            buttons[1] = new AnimatedButton(MenuScreen.SOUND_SETTINGS_BUTTON_LABEL);
            SetSoundButtonLabel();
            /*
            buttons[2] = new AnimatedButton(MenuScreen.HELP_BUTTON_LABEL);
            buttons[2].SetLabel(Constans.MENU_SELECTED_BUTTON_BACKGROUND_COLOR, Constans.MENU_NON_SELECTED_BUTTON_BACKGROUND_COLOR, Constans.MENU_NON_SELECTED_BUTTON_BACKGROUND_COLOR,
                Constans.MENU_NON_ACTIVE_TEXT_COLOR, Constans.MENU_FONT_NAME,
                ResourceManager.Instance.GetString(ResourceManager.StringKey.HELP),
                Constans.STANDARD_BUTTON_WIDTH, Constans.STANDARD_BUTTON_HEIGHT);
            */
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].ButtonPressedListener += this.OnButtonPressed;
                buttons[i].ButtonTransitionListener += this.OnButtonTransition;
                transitionButtonCountdown = 0;
            }
        }

        public override void ProcessTouchEvents(int x, int y, TouchLocationState tlState)
        {
            foreach (AnimatedButton button in buttons)
            {
                button.ProcessTouchEvent(x, y, tlState);
            }
        }

        private void DrawBackground(SpriteBatch batch)
        {
            Rectangle FULL_SCREEN = new Rectangle(0, 0, ScreenUtils.ScreenWidth,
                ScreenUtils.ScreenHeight);

            float alpha;



            try
            {
                AnimatedButton referenceButton = lastButtonPressed == null ? buttons[buttons.Length - 1] : lastButtonPressed;
                alpha =
                    (float)(ScreenUtils.ScreenWidth - referenceButton.x)
                    / (float)ScreenUtils.ScreenWidth;
            }
            catch (System.Exception)
            {
                alpha = 1.0f;
            }

            alpha = 1.0f;

            Color color = new Color(alpha, alpha, alpha, alpha);

            batch.Draw(background, FULL_SCREEN, color);
        }

        public override void Draw()
        {
            spriteBatch.Begin();
            DrawBackground(spriteBatch);
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].Draw(spriteBatch);
            }
            spriteBatch.End();
        }
        #endregion

    }
}
