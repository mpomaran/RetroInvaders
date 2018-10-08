using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace MPP.core2d
{

    class TextureBox : IBaseView, ITouchable
    {
        private Texture2D[] textures;
        private int currentTexture;
        
        public int Texture {
            get { return currentTexture; }
            set { 
                if (value >= textures.Length) {
                    throw new IndexOutOfRangeException();
                }
                currentTexture = value;
            }
        }

        public Vector Position { get; set; }
        public Vector Size { get; set; }

        public TextureBox()
        {
            textures = null;
        }

        public bool IsHit(int posX, int posY)
        {
            if (posX > Position.X && posY > Position.Y
                && posX < (Position.X + Size.X) && posY < (Position.Y + Size.Y))
            {
                return true;
            }

            return false;
        }
        #region todel
        /*
        public void InitializeWithTextures(ContentManager contentManager, string[] images)
        {
            if (images.Length == 0)
            {
                throw new ArgumentException();
            }

            lock (this)
            {
                const int OFF_SCREEN = 10000;

                textures = new Texture2D[images.Length];

                for (int i = 0; i < images.Length; i++)
                {
                    textures[i] = contentManager.Load<Texture2D>(images[i]);
                }

                Size = new Vector(textures[0].Width, textures[0].Height);
                Position = new Vector(OFF_SCREEN, OFF_SCREEN);
            }
        }
        */
        #endregion

        public void Draw(SpriteBatch spriteBatch)
        {
            if (textures == null)
            {
                return;
            }

            Texture2D texture = textures[0];

            Vector2 origin = new Vector2(0f, 0f);
            Vector2 position = new Vector2(Position.X, Position.Y);
            Vector2 scale = new Vector2(
                (float)Size.X / (float)texture.Width,
                (float)Size.Y / (float)texture.Height);

            spriteBatch.Draw(texture, position, null, Color.White,
                0f, origin, scale, SpriteEffects.None, 0);
        }
    }

    class Button : IBaseModel, IBaseView, ITouchable
    {
        public const string X_PROPERTY_NAME = "x";
        public const string Y_PROPERTY_NAME = "y";
        public const string WIDTH_PROPERTY_NAME = "width";
        public const string HEIGHT_PROPERTY_NAME = "height";
        public const string STATE_PROPERTY_NAME = "state";

        private string name;
        private Texture2D selected;
        private Texture2D notSelected;
        private Texture2D disabled;

        private Texture2D label;

        public int x { get; set; }
        public int y { get; set; }
        public int width { get; set; }
        public int height { get; set; }

        public enum State
        {
            SELECTED,
            NOT_SELECTED,
            DISABLED
        }

        public State state;

        Dictionary<string, object> nameObjectMap;

        public Button(string name)
        {
            this.name = name;

            nameObjectMap = new Dictionary<string, object>();
            label = null;
        }

       public bool IsHit(int posX, int posY)
        {
            if (posX > x && posY > y
                && posX < (x + width) && posY < (y + height))
            {
                return true;
            }

            return false;

        }

       private void OnLoadContentCommon()
       {
           state = State.NOT_SELECTED;
           width = notSelected.Width;
           height = notSelected.Height;
           const int OFF_SCREEN = 1000;
           x = OFF_SCREEN;
           y = OFF_SCREEN;
       }

        public void OnLoadContent (ContentManager contentManager, string selectedImageName, 
            string notSelectedImageName, string disabledImageName)
        {
            selected = ScreenUtils.GetTexture(selectedImageName);
            notSelected = ScreenUtils.GetTexture(notSelectedImageName);
            disabled = ScreenUtils.GetTexture(disabledImageName);

        }

        public void SetLabel(Color selectedColor,
            Color notSelectedColor, Color disabledColor, Color labelColor, 
            String fontName, String label, int width, int height)
        {
            bool shouldInitialize = selected == null;

            selected = ScreenUtils.CreateTexture(width, height, label,
                labelColor, selectedColor, ScreenUtils.GetFont(fontName));

            notSelected = ScreenUtils.CreateTexture(width, height, label,
                labelColor, notSelectedColor, ScreenUtils.GetFont(fontName));

            disabled = ScreenUtils.CreateTexture(width, height, label,
                labelColor, disabledColor, ScreenUtils.GetFont(fontName));

            if (shouldInitialize) {
                OnLoadContentCommon();
            }
        }


        public void SetLabel(SpriteFont font, String labelString, Color color)
        {
            if (labelString == null)
            {
                label = null;
            }

            if (notSelected == null)
            {
                throw new InvalidOperationException("Button textures not loaded yet, cannot put label");
            }
            Vector2 labelSize = font.MeasureString(labelString);
            label = ScreenUtils.CreateTexture(width, height, labelString, Color.Transparent, color, font);
        }

        public string Name
        {
            get { return name; }
        }

        public object GetPropertyByName(string name)
        {
            return this.GetType().GetProperty(name).GetValue(this, null);
        }

        public void SetPropertyByName(String name, object value)
        {
            if (name.Equals(STATE_PROPERTY_NAME))
            {
                state = (State)value;
            }
            else
            {
                this.GetType().GetProperty(name).SetValue(this, value, null);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Texture2D tex = null;
            switch (state) {
                case State.SELECTED:
                    tex = selected;
                    break;
                case State.NOT_SELECTED:
                    tex = notSelected;
                    break;
                case State.DISABLED:
                    tex = disabled;
                    break;
            }

            Vector2 origin = new Vector2(0f, 0f);
            Vector2 position = new Vector2 (x, y);
            Vector2 scale = new Vector2(
                (float)width / (float)tex.Width,
                (float)height / (float)tex.Height);

            spriteBatch.Draw (tex, position, null, Color.White,
                0f, origin, scale, SpriteEffects.None, 0);

            if (label != null)
            {
                spriteBatch.Draw(label, position, null, Color.White,
                    0f, origin, scale, SpriteEffects.None, 0);
            }
        }
    }
}
