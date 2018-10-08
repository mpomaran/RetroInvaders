using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input.Touch;
using MPP.core;

namespace MPP.core2d
{
    class AnimatedButton : Button
    {
        #region Members
        private bool isDuringTransition;
        #endregion

        public AnimatedButton(string name) : base (name)
        {
            CoreAnimation.Instance.AnimationEndedListener += OnAnimationEnded;
            isDuringTransition = false;
        }

        #region Processing user input
        public void ProcessTouchEvent(int x, int y, TouchLocationState eventType)
        {
            if (isDuringTransition)
                return;

            bool isHit = IsHit(x, y);

            if (eventType == TouchLocationState.Released)
            {
                if (this.state.Equals(Button.State.SELECTED))
                {
                    Bind buttonPropertyControl = new Bind(
                       delegate { return state; },
                       delegate(object value) { state = (Button.State)value; }
                    );

                    CoreAnimation.Instance.ToggleProperty(Name, buttonPropertyControl,
                        Button.State.SELECTED, Button.State.NOT_SELECTED, 0f, 0.6f, 0.18f);

                    OnButtonPressed();
                }
                else
                {
                    state = Button.State.NOT_SELECTED;
                }
            }
            else
            {
                state = isHit ? Button.State.SELECTED : Button.State.NOT_SELECTED;
            }

        }
        #endregion

        #region Event handling
        public delegate void ButtonPressedEventHandler(AnimatedButton button);
        public event ButtonPressedEventHandler ButtonPressedListener;

        private void OnButtonPressed()
        {
            if (ButtonPressedListener != null)
            {
                ButtonPressedListener(this);
            }
        }

        public delegate void ButtonTransitionEventHandler(AnimatedButton button);
        public event ButtonTransitionEventHandler ButtonTransitionListener;

        private void OnTransitionCompleted()
        {
            if (ButtonTransitionListener != null)
            {
                ButtonTransitionListener(this);
            }
        }

        public void OnAnimationEnded(String name)
        {
            if (name.Equals("transition of " + Name))
            {
                isDuringTransition = false;
                OnTransitionCompleted();
            }
        }

        #endregion

        public void StartTransition(int x0, int y0, int x1, int y1, double delay, double duration)
        {
            isDuringTransition = true;
            x = x0;
            y = y0;

            Bind pctl = new Bind(
                delegate { return x; },
                delegate(object value) { x = (int)value; }
            );

            core2d.CoreAnimation.Instance.Animate(
                "transition of " + Name,
                pctl,
                x0,
                x1,
                (float)delay,
                (float)duration);
        }

        public new void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }
    }
}
