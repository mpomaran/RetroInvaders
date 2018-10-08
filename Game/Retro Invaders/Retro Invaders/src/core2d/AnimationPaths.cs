using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace MPP.core2d
{
    public class ColorAnimationPath
    {
        Color startValue;
        Color endValue;
        double length;

        public ColorAnimationPath(Color startValue, Color endValue, double length)
        {
            this.length = length;
            this.startValue = startValue;
            this.endValue = endValue;
        }

        public object ComputeFrame(float currTime)
        {
            Color result = startValue;

            if (length == 0)
            {
                result = endValue;
            }
            else
            {
                float time = (float)(currTime / length);
                result = new Color(startValue.ToVector4() + ((endValue.ToVector4() - startValue.ToVector4()) * time));
            }

            return result;
        }
    }

    public class LinearPath
    {
        int startValue;
        int endValue;
        double length;

        public LinearPath(int startValue, int endValue, double length)
        {
            this.length = length;
            this.startValue = startValue;
            this.endValue = endValue;
        }

        public object ComputeFrame(float currTime)
        {
            int result = startValue;

            if (length == 0)
            {
                result = endValue;
            }
            else
            {
                double time = currTime / length;
                result = startValue + (int)((double)(endValue - startValue) * time);
            }

            return result;
        }
    }

    public class KineticPath
    {
        int startValue;
        int endValue;
        double length;

        public KineticPath(int startValue, int endValue, double length)
        {
            this.length = length;
            this.startValue = startValue;
            this.endValue = endValue;
        }

        public object ComputeFrame(float currTime)
        {
            int result = startValue;

            if (length == 0)
            {
                result = endValue;
            }
            else
            {
                double time = currTime / length;
                double k = (Math.Pow(Math.E, -time) - Math.Pow(Math.E, -1f))
                    / (1f - Math.Pow(Math.E, -1f)) * (Math.Cos(Math.Pow(time, 1) * Math.PI));
                result = startValue + (int)((double)(endValue - startValue) * (1f - k));
            }

            if (currTime >= length)
                result = endValue;

            //(e^(-x)-e^(-1)) / (1-e^(-1)) * abs((cos(x^3*3.14)))

            return result;
        }
    }

    public class BlinkPath
    {
        object startValue;
        object endValue;
        double blinkPeriod;
        double length;

        public BlinkPath(object startValue, object endValue, double blinkPeriod, double length)
        {
            this.blinkPeriod = blinkPeriod;
            this.startValue = startValue;
            this.endValue = endValue;
            this.length = length;
        }

        public object ComputeFrame(float currTime)
        {
            if (currTime >= length)
            {
                return endValue;
            }

            if (blinkPeriod <= 0)
            {
                blinkPeriod = 1;
            }
            double k = currTime / blinkPeriod;
            k -= Math.Floor(k);
            if (k >= 0.5)
            {
                return endValue;
            }
            else
            {
                return startValue;
            }
        }
    }

}
