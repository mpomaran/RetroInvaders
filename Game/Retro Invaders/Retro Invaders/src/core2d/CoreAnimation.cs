using System;
using System.Collections.Generic;
using System.Text;
using MPP.core;
using Microsoft.Xna.Framework;

namespace MPP.core2d
{
    class CoreAnimation : IEventListener
    {
        static private CoreAnimation instance;
        private Dictionary<string, PropertyAnimation> animations;
        private Dictionary<string, PropertyAnimation> animationsToAdd;

        private List<string> toDelete;

        private Double lastUpdateTime;

        private CoreAnimation()
        {
            animations = new Dictionary<string, PropertyAnimation>();
            toDelete = new List<string>();
            animationsToAdd = new Dictionary<string, PropertyAnimation>();


            lastUpdateTime = 0;
        }

        static public CoreAnimation Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CoreAnimation();
                }
                return instance;
            }
        }

        public void AddListener(string name, IEventListener listener)
        {
            if (animations.ContainsKey(name)) {
                animations[name].RegisterListener(listener);
            } else if (animationsToAdd.ContainsKey(name)) {
                animationsToAdd[name].RegisterListener(listener);
            } else {
                throw new IndexOutOfRangeException ("Unknown animation " + name);
            }
        }

        public void AddAnimation(string name, PropertyAnimation anim)
        {
            lock (animationsToAdd)
            {
                anim.RegisterListener(this);
                animationsToAdd[name] = anim;
            }
        }

        private void RemoveAnimation(string name)
        {
            lock (animations)
            {
                PropertyAnimation anim = animations[name];
                anim.Stop();
                anim.UnregisterListener(this);
                animations.Remove(name);
            }
        }

        public Double GetCurrentGameTime()
        {
            return lastUpdateTime;
        }

        public void Update(Double currentTime)
        {
            lastUpdateTime = currentTime;

            lock (animations)
            {
                foreach (KeyValuePair<string, PropertyAnimation> animation in animations)
                    {
                        PropertyAnimation node = animation.Value;
                        node.Update(currentTime);
                    }

                lock (animationsToAdd)
                {
                    foreach (KeyValuePair<string, PropertyAnimation> animation in animationsToAdd)
                    {
                        animations[animation.Key] = animation.Value;
                    }

                    animationsToAdd.Clear();
                }
            }

            lock (toDelete)
            {
                foreach (string key in toDelete)
                {
                    RemoveAnimation(key);
                    OnAnimationEnded(key);
                }
                toDelete.Clear();
            }
        }

        public void ToggleProperty(
            string animationName,
            Bind pctl,
            object startValue,
            object endValue,
            float start,
            float length,
            float period)
        {
            BlinkPath path = new BlinkPath(startValue, endValue, period, length);

            PropertyAnimation anim = new PropertyAnimation(animationName, path.ComputeFrame, pctl);
            anim.Start = GetCurrentGameTime() + start;
            anim.End = GetCurrentGameTime() + length + start;
            AddAnimation(animationName, anim);
        }

        public void Animate(
            string animationName,
            Bind pctl,
            IBaseModel model,
            string propertyName,
            object endValue,
            float start,
            float length)
        {
            if (model.GetPropertyByName(propertyName).GetType().Equals(typeof(int)))
            {
                KineticPath path = new KineticPath(
                    (int)pctl.Get(),
                    (int)endValue,
                    length);

                PropertyAnimation anim = new PropertyAnimation(animationName, path.ComputeFrame, pctl);
                anim.Start = GetCurrentGameTime() + start;
                anim.End = GetCurrentGameTime() + length + start;
                AddAnimation(animationName, anim);
            }
            else
            {
                throw new NotImplementedException("Only integers can be animated now!!!");
            }
        }
        public void Animate(
            string animationName,
            Bind pctl,
            object startValue,
            object endValue,
            float start,
            float length)
        {
                KineticPath path = new KineticPath(
                    (int)startValue,
                    (int)endValue,
                    length);

                PropertyAnimation anim = new PropertyAnimation(animationName, path.ComputeFrame, pctl);
                anim.Start = GetCurrentGameTime() + start;
                anim.End = GetCurrentGameTime() + length + start;
                AddAnimation(animationName, anim);
        }

        public void AnimateColor(
            string animationName,
            Bind pctl,
            object endValue,
            float length)
        {
            ColorAnimationPath path = new ColorAnimationPath(
                (Color)pctl.Get(),
                (Color)endValue,
                length);

            PropertyAnimation anim = new PropertyAnimation(animationName, path.ComputeFrame, pctl);
            anim.Start = CoreAnimation.Instance.GetCurrentGameTime();
            anim.End = CoreAnimation.Instance.GetCurrentGameTime() + length;
            CoreAnimation.Instance.AddAnimation(animationName, anim);
        }



        #region Listener implementation

        public delegate void AnimationEndedEventHandler(String name);
        public event AnimationEndedEventHandler AnimationEndedListener;

        private void OnAnimationEnded(String name)
        {
            if (AnimationEndedListener != null)
            {
                AnimationEndedListener(name);
            }
        }

        public void OnNotification(IEventPublisher publisher, EventArgs eventArgs)
        {
            if (publisher.GetType() != typeof(PropertyAnimation)) {
                throw new NotImplementedException();
            }

            if (eventArgs.GetType() != typeof(PropertyAnimation.AnimationEnded)) {
                throw new NotImplementedException();
            }

            lock (toDelete)
            {
                toDelete.Add(((PropertyAnimation)publisher).Name);
            }
        }
        #endregion
    }
}
