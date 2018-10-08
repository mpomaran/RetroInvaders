using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;
using MPP.core2d;

namespace MPP.core
{
    class SoundPlayer
    {
        #region Life cycle
        static private SoundPlayer instance;

        private List<string> effectNames = null;
        private List<string> loopedEffectNames = null;

        private List<string> currentlyPlaying = null;

        private List<SoundEffectInstance> instances = null;

        private Dictionary<string, SoundEffectInstance> loopedSoundEffects;

        private Dictionary<string, SoundEffect> soundEffects;

        private SoundPlayer()
        {
            effectNames = new List<string> () {
                "explosion1", 
                "laser",
                "photonShot",
                "mothershipSound" };

            loopedEffectNames = new List<string>() {
                "mothershipSound"
            };
            
            IsSoundEnabled = true;

            soundEffects = new Dictionary<string, SoundEffect>();
            currentlyPlaying = new List<string>();
            loopedSoundEffects = new Dictionary<string, SoundEffectInstance>();
            instances = new List<SoundEffectInstance>();

            GameScreenManager.Instance.LoadContentEventListeners += this.OnLoadContentEvent;
        }

        void PurgeEmptyEffectsFromTheList() {
            bool done = false;

            while (!done) { 
                foreach (SoundEffectInstance soundInstance in instances) {
                    if (soundInstance.State == SoundState.Stopped) {
                        instances.Remove(soundInstance);
                        break;
                    }
                }
                done = true;
            }
        }

        public void Initialize()
        {
            // NOP
        }

        static public SoundPlayer Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SoundPlayer();
                }
                return instance;
            }
        }
        #endregion

        public void StopSound(string name)
        {
            SoundEffectInstance loopedEffect;
            if (loopedSoundEffects.TryGetValue(name, out loopedEffect))
            {
                loopedEffect.Stop();
                currentlyPlaying.Remove(name);
            }
        }

        public void PlaySound(string name)
        {
            if (IsSoundEnabled == false)
            {
                return;
            }

            else
            {
                try
                {
                    SoundEffect effect;
                    if (soundEffects.TryGetValue(name, out effect))
                    {
                        PurgeEmptyEffectsFromTheList();

                        if (instances.Count > 15)
                        {
                            // no capacity
                            return;
                        }

                        SoundEffectInstance instance = effect.CreateInstance();
                        instances.Add(instance);
                        instance.Play();
                    }
                    else
                    {
                        SoundEffectInstance loopedEffect;
                        if (loopedSoundEffects.TryGetValue(name, out loopedEffect))
                        {
                            loopedEffect.Play();
                            currentlyPlaying.Add(name);
                        }
                        else
                        {
                            throw new ArgumentException("Cannot find sound " + name);
                        }
                    }
                }
                catch (InstancePlayLimitException e) { }
            }
        }

        public void StopAll()
        {
            foreach (string name in currentlyPlaying)
            {
                StopSound(name);
            }


            foreach (SoundEffectInstance soundInstance in instances)
            {
                if (soundInstance.State != SoundState.Stopped)
                {
                    soundInstance.Stop();
                }
            }        
        
        }

        public void Resume()
        {
            foreach (string name in currentlyPlaying)
            {
                SoundEffectInstance loopedEffect;
                if (loopedSoundEffects.TryGetValue(name, out loopedEffect))
                {
                    loopedEffect.Resume();
                }
            }
        }


        public void Suspend()
        {
            foreach (string name in currentlyPlaying)
            {
                SoundEffectInstance loopedEffect;
                if (loopedSoundEffects.TryGetValue(name, out loopedEffect))
                {
                    loopedEffect.Pause();
                }
            }
        }

        #region Playback properties

        public bool IsSoundEnabled;

        #endregion

        #region World event handling

        public void OnLoadContentEvent(MPP.core2d.GameScreenManager.LoadContentEventArgs eventArgs)
        {
            foreach (string name in effectNames) {
                SoundEffect effect = eventArgs.contentManager.Load<SoundEffect>(name);
                soundEffects[name] = effect;
            }

            foreach (string name in loopedEffectNames)
            {
                loopedSoundEffects[name] = soundEffects[name].CreateInstance();
                loopedSoundEffects[name].IsLooped = true;
                soundEffects.Remove(name);
            }

        }

        #endregion
    

    }
}
