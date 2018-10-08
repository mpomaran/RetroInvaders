using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MPP.core2d
{
 
    public interface INameable {
        string Name { get; }
    }

    public interface IBaseModel : INameable
    {
        Object GetPropertyByName(String name);
        void SetPropertyByName(String name, object value);
    }
    public interface ITouchable
    {
        bool IsHit(int x, int y);
    }

    public interface IBaseView
    {
        void Draw(SpriteBatch spriteBatch);
    }

    /**
     * This code is used to encapsulate getter/setter
     * and is being used as a helper during the animation
     */
    public class Bind
    {
        private Func<object> getter;
        private Action<object> setter;

        public Bind(Func<object> getter,
            Action<object> setter)
        {
            this.getter = getter;
            this.setter = setter;
        }

        public object Get()
        {
            return getter();
        }

        public void Set(object value)
        {
            setter(value);
        }
    }

    public delegate object FrameUpdater(float time);

    public class ScreenUtils
    {
        private static GraphicsDevice device;
        private static ContentManager contentManager;

        #region Screen properties

        public static int ScreenHeight;
        public static int ScreenWidth;

        #endregion


        public static Vector2 TranslateToScreenCoordinates(Vector2 coordinates)
        {
            return new Vector2(coordinates.X * ScreenWidth,
                coordinates.Y * ScreenHeight);
        }

        public static void OnLoadContent(GraphicsDevice graphicsDevice, ContentManager manager)
        {
            device = graphicsDevice;
            contentManager = manager;
        }

        public static SpriteFont GetFont(String fontName)
        {
            AssertClassIsInitialized();

            return contentManager.Load<SpriteFont>(fontName);
        }

        public static Texture2D GetTexture(String textureName)
        {
            AssertClassIsInitialized();
            return contentManager.Load<Texture2D>(textureName);
        }

        private static void AssertClassIsInitialized()
        {
            if (device == null || contentManager == null)
            {
                throw new InvalidOperationException("Screen utils not initialized. Call OnLoadContent first");
            }
        }

        public static Texture2D CreateTexture(int width, int height)
        {
            AssertClassIsInitialized();

            return new Texture2D(device, width, height);
        }

        public static Texture2D CreateTexture(int width, int height, SurfaceFormat format)
        {
            AssertClassIsInitialized();

            return new Texture2D(device, width, height, false, format);
        }

        public static Texture2D CreateTexture(int width, int height, string label, Color textColor, 
            Color backgroundColor, SpriteFont font)
        {
            AssertClassIsInitialized();

            PresentationParameters pp = device.PresentationParameters;

            RenderTarget2D target = new RenderTarget2D(device, width, height, false, SurfaceFormat.Color, DepthFormat.None);
            device.SetRenderTarget(target);

            SpriteBatch batch = new SpriteBatch(device);
            batch.Begin();

            device.BlendState = BlendState.AlphaBlend;
            device.Clear(backgroundColor);

            Vector2 labelSize = font.MeasureString(label);

            batch.DrawString(font,
                label,
                (new Vector2(width, height) - labelSize) / 2,
                textColor);

            batch.End();
            device.SetRenderTarget(null);

            return (Texture2D)target;
        }

        public static SpriteBatch CreateSpriteBatch()
        {
            AssertClassIsInitialized();
            return new SpriteBatch(device);
        }
    }
}