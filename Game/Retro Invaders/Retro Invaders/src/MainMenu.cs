using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using MPP.core2d;

namespace MPP
{
    class MainMenu
    {
        private Texture2D background;
        private Rectangle FULL_SCREEN;

        private core2d.Button[] buttons;

        public void Initialize(ContentManager contentManager, int screenWidth, int screenHeight)
        {
            FULL_SCREEN = new Rectangle(0,0, screenWidth, screenHeight);
            background = contentManager.Load<Texture2D>("mainMenuBackground");
            buttons = new core2d.Button[4];

            for (int i = 0; i < buttons.Length; i++)
            {
                core2d.Button button = new core2d.Button("start_a_new_game_button" + i);
                button.OnLoadContent(contentManager,
                    "start_a_new_active",
                    "start_a_new_inactive",
                    "start_a_new_inactive");
                button.x = 1000;
                button.y = 250+i*110;

                Bind pctl = new Bind(
                    delegate { return button.x; },
                    delegate(object value) { button.x = (int)value; }
                );

                core2d.CoreAnimation.Instance.Animate(
                    "creating" + i,
                    pctl,
                    1000,
                    0,
                    0.05f * i,
                    0.5f);

                buttons[i] = button;
            }
        }

        public void Update()
        {
            TouchCollection touchCollection = TouchPanel.GetState();
            foreach (TouchLocation tl in touchCollection)
            {
                if ((tl.State == TouchLocationState.Pressed)
                        || tl.State == TouchLocationState.Moved
                        || tl.State == TouchLocationState.Released)
                {
                    foreach (Button button in buttons)
                    {
                        bool isHit = button.IsHit((int)tl.Position.X, (int)tl.Position.Y);
                        if (tl.State == TouchLocationState.Released)
                        {
                            if (button.state.Equals(Button.State.SELECTED))
                            {                                Button btton = button;
                                Bind buttonPropertyControl = new Bind(
                                   delegate { return btton.state; },
                                   delegate(object value) { btton.state = (Button.State)value; }
                                );

                                CoreAnimation.Instance.ToggleProperty(button.Name, buttonPropertyControl,
                                    Button.State.SELECTED, Button.State.NOT_SELECTED, 0f, 0.6f, 0.18f);

                                for (int i = 0; i < buttons.Length; i++)
                                {
                                    core2d.Button b = buttons[i];
                                    float delay;
                                    if (b.Equals(button)) {
                                        delay = 0.3f;
                                    } else {
                                        delay = 0.0f;
                                    }

                                    Bind pctl = new Bind(
                                        delegate { return b.x; },
                                        delegate (object value) { b.x = (int)value; }
                                    );

                                    core2d.CoreAnimation.Instance.Animate(
                                        "button_animation_appeareance_1" + i,
                                        pctl,
                                        0,
                                        500,
                                        0.05f * i + delay,
                                        0.5f);

                                    core2d.CoreAnimation.Instance.Animate(
                                        "button_animation_appeareance_2" + i,
                                        pctl,
                                        1000,
                                        0,
                                        1f + 0.05f * i + delay,
                                        0.5f);
                                }
                            }
                            else
                            {
                                button.state = Button.State.NOT_SELECTED;
                            }
                        }
                        else
                        {
                            button.state = isHit ? Button.State.SELECTED : Button.State.NOT_SELECTED;
                        }
                    }
                }
            }

        }

        private void DrawBackground(SpriteBatch batch) 
        {
            batch.Draw(background, FULL_SCREEN, Color.White);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            DrawBackground(spriteBatch);
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].Draw(spriteBatch);
            }
        }
    }
}
