using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace MPP.game
{
    public interface IBoundingShape {
        bool CollidesWith (IBoundingShape shape);
    }

    public class BoundingBox : IBoundingShape
    {

        private Rectangle box;

        public BoundingBox(Rectangle box)
        {
            if (box == null)
            {
                throw new ArgumentNullException();
            }

            this.box = box;
        }

        public bool CollidesWith(IBoundingShape shape)
        {
            // TODO: przerobic
            if (shape.GetType() == typeof(BoundingBox))
            {
                BoundingBox bb = (BoundingBox)shape;

                if (bb.box.Left < box.Right && bb.box.Right > box.Left)
                {
                    if (bb.box.Top > box.Bottom)
                    {
                        return false;
                    }

                    if (bb.box.Top > box.Top)
                    {
                        return true;
                    }
                    else
                    {
                        int h = bb.box.Bottom - bb.box.Top;
                        if (bb.box.Top + h < box.Bottom && bb.box.Top > box.Top)
                        {
                            return true;
                        }
                        else if (bb.box.Top + h > box.Bottom && bb.box.Top < box.Top)
                        {
                            return true;
                        }
                    }

                    return false;
                }
                else
                {
                    return false;
                }

            }
            else
            {
                // TODO: idiom do dupy
                throw new NotImplementedException();
            }
        }
    }

    public interface ICollisionEnabled
    {
        IBoundingShape GetBoundingShape();
    }
}
