using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPP.core;

namespace MPP.core2d
{
    public class PropertyAnimation : IEventPublisher, INameable
    {
        #region Private stuff
        private FrameUpdater path;
        private Bind pctrl;
        private bool lastFrameCalled;
        #endregion

        public PropertyAnimation(string name, FrameUpdater path, Bind pctrl)
        {
            this.path = path;
            this.pctrl = pctrl;
            lastFrameCalled = false;
            Name = name;
        }

        #region Getters/setters

        private string name;
        public string Name
        {
            get { return name; }
            private set { name = value; }
        }

        public bool CanBeAutodeleted
        {
            get { return true; }
        }

        public bool Enabled
        {
            get;
            set;
        }
        public Double Start
        {
            get;
            set;
        }

        public Double End
        {
            get;
            set;
        }

        public bool EndlessLoop
        {
            get { return End < 0f; }
        }
        #endregion
        
        #region Animation processing

        public void Stop()
        {
            lastFrameCalled = true;
            End = 0;
        }


        private bool ShouldUpdate(Double time)
        {
            if (Start < 0f)
            {
                Start = time;
            }

            if (!EndlessLoop && time > End && lastFrameCalled)
            {
                return false;
            }

            if (time >= Start)
            {
                if (EndlessLoop || time <= End || !lastFrameCalled)
                {
                    return true;
                }
            }

            return false;
        }

        private bool ShouldDelete(Double time)
        {
            if (CanBeAutodeleted == false)
                return false;

            return HasEnded(time) && lastFrameCalled;
        }

        private bool HasEnded(Double time)
        {
            if (!EndlessLoop && time > End)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Update(Double time)
        {
            if (ShouldUpdate(time) == false)
            {
                return;
            }

            if (time > End)
            {
                time = End;
            }

            float currentTime = (float)time;

            if (End > 0f)
            {
                currentTime = (float)(time - Start);
            }

            object frame = path.Invoke(currentTime);

            pctrl.Set(frame);

            if (End == time)
            {
                lastFrameCalled = true;
                NotifyListeners(new AnimationEnded(name));
            }
        }

#endregion

        #region Event publishing

        private delegate void AnimationEventHandler(IEventPublisher publisher, EventArgs args);
        private event AnimationEventHandler animationEventHandler;
        public class AnimationEnded : EventArgs
        {
            public AnimationEnded(String name)
            {
                AnimName = name;
            }

            public String AnimName { get; private set; }
        }

        public void RegisterListener<T>(T listener) where T : IEventListener
        {
            animationEventHandler += new AnimationEventHandler(listener.OnNotification);
        }

        public void UnregisterListener<T>(T listener) where T : IEventListener
        {
            animationEventHandler -= new AnimationEventHandler(listener.OnNotification);
        }

        public void NotifyListeners(EventArgs args)
        {
            if (null != animationEventHandler)
            {
                animationEventHandler(this, args);
            }
        }
        #endregion

    }
}
