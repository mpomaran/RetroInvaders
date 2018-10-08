using System;
using System.Collections.Generic;
using System.Linq;
using MPP.core2d;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using MPP.core;
using MPP.game;
using Microsoft.Advertising.Mobile.Xna;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Advertising.Mobile.UI;

namespace MPP
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        DrawableAd bannerAd;

        private static readonly string ApplicationId = "57577568-b26b-4fc4-acf9-860bef4f11e7";
        private static readonly string AdUnitId = "10058159"; //other test values: Image480_80, Image300_50, TextAd 
       
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.SupportedOrientations = DisplayOrientation.Portrait;
            ScreenUtils.ScreenHeight = graphics.PreferredBackBufferHeight = 800;
            ScreenUtils.ScreenWidth = graphics.PreferredBackBufferWidth = 480;

            Content.RootDirectory = "Content";

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            // Extend battery life under lock.
            InactiveSleepTime = TimeSpan.FromSeconds(10);

        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // Initialize the AdGameComponent and add it to the game’s Component object
            AdGameComponent.Initialize(this, ApplicationId);

            AdGameComponent.Current.CountryOrRegion = "US";

            Components.Add(AdGameComponent.Current);

            // Create an actual ad for display.
            CreateAd();

            GameScreenManager.Instance.AddScreen(new BattleScreen(GraphicsDevice));
            GameScreenManager.Instance.AddScreen(new MenuScreen(GraphicsDevice));
            GameScreenManager.Instance.AddScreen(new GameOverScreen(GraphicsDevice));
            SoundPlayer.Instance.Initialize() ;
            GameState.Instance.Load();
            base.Initialize();

        }

        protected override void OnActivated(object sender, EventArgs args)
        {
            GameState.Instance.Load();
            GameScreenManager.Instance.OnActivated(args);
            base.OnActivated(sender, args);
        }

        protected override void OnDeactivated(object sender, EventArgs args)
        {
            SoundPlayer.Instance.StopAll();
            GameState.Instance.Save();

            if (GameScreenManager.Instance.GetActiveScreenID().Equals(BattleScreen.ID))
            {
                GameScreenManager.Instance.MoveToScreen(MenuScreen.ID);
            }

            base.OnDeactivated(sender, args);
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            GameState.Instance.Save();
            SoundPlayer.Instance.StopAll();
            base.OnExiting(sender, args);
        }

        enum ContentLoadingState
        {
            FIRST_RUN,
            SPLASH_CONTENT_LOADED,
            SPLASH_CONTENT_SHOWN,
            LOADING_CONTENT,
            CONTENT_LOADED
        };

        ContentLoadingState contentLoadingState = ContentLoadingState.FIRST_RUN;

        Texture2D splashScreen = null;

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);           

            if (contentLoadingState == ContentLoadingState.FIRST_RUN)
            {
                splashScreen = Content.Load<Texture2D>("SplashScreenImage");
                contentLoadingState = ContentLoadingState.SPLASH_CONTENT_LOADED;
                return;
            }

            ScreenUtils.OnLoadContent(GraphicsDevice, Content);
            GameScreenManager.Instance.OnLoadContent(new GameScreenManager.LoadContentEventArgs(Content));

            GameScreenManager.Instance.MoveToScreen(MenuScreen.ID);
            // Create a new SpriteBatch, which can be used to draw textures.

            contentLoadingState = ContentLoadingState.CONTENT_LOADED;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // NOP
        }

        private void ProcessTouchEvents()
        {
            GameScreenManager.Instance.ProcessTouchEvents();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (contentLoadingState == ContentLoadingState.SPLASH_CONTENT_SHOWN)
            {
                contentLoadingState = ContentLoadingState.LOADING_CONTENT;
                LoadContent();
            }
            else if (contentLoadingState != ContentLoadingState.CONTENT_LOADED)
            {
                base.Update(gameTime);
                return;
            }

            try
            {
                World.Instance.OnTimeUpdate(gameTime);
                ProcessTouchEvents();

                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                {
                    GameState.Instance.Save();

                    if (GameScreenManager.Instance.GetActiveScreenID().Equals(MenuScreen.ID))
                    {
                        this.Exit();
                    }
                    else if (GameScreenManager.Instance.GetActiveScreenID().Equals(GameOverScreen.ID))
                    {
                        GameScreenManager.Instance.MoveToScreen(MenuScreen.ID);
                    }
                    else if (GameScreenManager.Instance.GetActiveScreenID().Equals(BattleScreen.ID))
                    {
                        GameScreenManager.Instance.MoveToScreen(MenuScreen.ID);
                    }
                }
            }
            catch (Exception e)
            {
                // NOP
            }

//            AdGameComponent.Current.Update(gameTime);

            try
            {
                base.Update(gameTime);
            }
            catch (Exception e)
            {
                Debug.WriteLine("An exception in update caught: " + e + " " + e.Message + " " + e.StackTrace);
            }
        }

        protected override bool BeginDraw()
        {
            if (contentLoadingState != ContentLoadingState.SPLASH_CONTENT_LOADED && contentLoadingState != ContentLoadingState.CONTENT_LOADED) {
                return false;
            } else {
                return base.BeginDraw();
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            try
            {
                GraphicsDevice.BlendState = BlendState.AlphaBlend;
                GraphicsDevice.Clear(new Color(0, 0, 00));

                if (contentLoadingState == ContentLoadingState.SPLASH_CONTENT_LOADED)
                {
                    contentLoadingState = ContentLoadingState.SPLASH_CONTENT_SHOWN;
                    spriteBatch.Begin();
                    spriteBatch.Draw(splashScreen, new Rectangle(0, 0, ScreenUtils.ScreenWidth, ScreenUtils.ScreenHeight), Color.White);
                    spriteBatch.End();
                }
                else if (contentLoadingState == ContentLoadingState.CONTENT_LOADED)
                {
                    core2d.CoreAnimation.Instance.Update(gameTime.TotalGameTime.TotalSeconds);
                    GameScreenManager.Instance.Draw();
                }
                base.Draw(gameTime);
            }
            catch (Exception e)
            {
                Debug.WriteLine("An exception in draw caught: " + e + " " + e.Message + " " + e.StackTrace);
            }

        }

        /// <summary> 
        /// Create a DrawableAd with desired properties. 
        /// </summary> 
        private void CreateAd()
        {
            // Create a banner ad for the game. 
            int width = 480;
            int height = 80;
            int x = 0;
            int y = 0;

            bannerAd = AdGameComponent.Current.CreateAd(AdUnitId, new Rectangle(x, y, width, height), true);
            AdGameComponent.Current.UpdateOrder = 0;

            // Add handlers for events (optional). 
            bannerAd.ErrorOccurred += new EventHandler<Microsoft.Advertising.AdErrorEventArgs>(bannerAd_ErrorOccurred);
            bannerAd.AdRefreshed += new EventHandler(bannerAd_AdRefreshed);

//            bannerAd.Visible = true;

            // Set some visual properties (optional). 
            bannerAd.BorderEnabled = true; // default is true 
//            bannerAd.BorderColor = Color.White; // default is White 
            bannerAd.DropShadowEnabled = false; // default is true 

            AdGameComponent.Current.Enabled = true;
            AdGameComponent.Current.Visible = true;

            Debug.WriteLine("Ad created");
        }

        /// <summary> 
        /// This is called whenever a new ad is received by the ad client. 
        /// </summary> 
        /// <param name="sender"></param> 
        /// <param name="e"></param> 
        private void bannerAd_AdRefreshed(object sender, EventArgs e)
        {
            Debug.WriteLine("Ad received successfully");
        }

        /// <summary> 
        /// This is called when an error occurs during the retrieval of an ad. 
        /// </summary> 
        /// <param name="sender"></param> 
        /// <param name="e">Contains the Error that occurred.</param> 
        private void bannerAd_ErrorOccurred(object sender, Microsoft.Advertising.AdErrorEventArgs e)
        {
            Debug.WriteLine("An error: " + e.Error.Message + " code: " + e.ErrorCode + " error: " + e.Error);
        } 

    }
}
