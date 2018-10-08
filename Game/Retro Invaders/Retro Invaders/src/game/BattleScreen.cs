using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework;
using MPP.core;
using Microsoft.Xna.Framework.Graphics;
using MPP.core2d;

namespace MPP.game
{
    class BattleScreen : GameScreen, IDisposable, IEventListener
    {
        private Player player;

        private Missle playerMissle;

        private core2d.AnimatedButton startBattleButton;

        private Texture2D fadeOutOverlay;
        private Color fadeoutColor;
        private const string FADE_ANIM_NAME = "fade in out anim name";

        private List<Missle> enemyMissles;
        private List<Missle> unusedMissles;

        private MotherShip motherShip;
        private float motherShipPos;

        private Color backgroundColor;

        private Enemy[,] enemies;
        private GameStatusBar status;

        Texture2D background;
        //        private List<Bunker> bunkers;

        enum TransitionState
        {
            NOT_VISIBLE,
            MOVING_IN,
            ON_SCREEN,
            MOVING_OUT
        };

        TransitionState transitionState;
        TimeSpan transitionStart;

        #region Life cycle
        public void Dispose()
        {
            World.Instance.UnregisterListener(this);
        }

        Random random;

        private void SetupEnemies()
        {
            for (int x = 0; x < Constans.ENEMY_COLS; x++)
            {
                for (int y = 0; y < Constans.ENEMY_ROWS ; y++)
                {
                    if (enemies[x, y] == null)
                    {
                        enemies[x, y] = new Enemy(new Vector2(x, y));
                    }
                    else
                    {
                        enemies[x, y].Init(new Vector2(x, y));
                    }

                    enemies[x, y].TransitionOffset.X = 1000;
                    enemies[x, y].TransitionOffset.Y = 10;
                }
            }
            foreach (Missle m in enemyMissles)
            {
                unusedMissles.Add(m);
            }
            enemyMissles.Clear();
        }

        public BattleScreen(GraphicsDevice device) :
            base(device)
        {
            player = new Player();
            player.RegisterListener(this);
            random = new Random();
            //enemy = new Enemy();
            playerMissle = new Missle(-10f, -10f, true);
            playerMissle.Speed *= 1.5f;     // FIXME: jesli jest za szybki, to przelatuje przez wroga, bo nie jest wykrywana kolizja

            backgroundColor = Constans.BATTLESCREEN_BACKGROUND_COLORS[0];

            enemies = new Enemy[Constans.ENEMY_COLS, Constans.ENEMY_ROWS];

            motherShip = new MotherShip(1.1f, 0.1f);
            motherShip.CurrentState = MotherShip.State.DEAD;
            motherShipPos = 0.0f;

            enemyMissles = new List<Missle>();
            unusedMissles = new List<Missle>();

            for (int i = 0; i < 5; i++)
            {
                unusedMissles.Add(new Missle(-10f, -10f, false));
            };

            SetupEnemies();

            transitionState = TransitionState.NOT_VISIBLE;

            //            startBattleButton = new core2d.Button("start_battle_button");

            World.Instance.RegisterListener(this);
            GameScreenManager.Instance.LoadContentEventListeners += this.OnLoadContentEvent;
            GameScreenManager.Instance.OnActivatedEventListeners += this.OnActivatedEvent;
        }
        #endregion

        private void StartBattle()
        {
            GameState.Instance.BattleTime = 0;
            GameState.Instance.HordlePos = new Vector2(0, 0);
            motherShipPos = (float)random.NextDouble() / 3f;
        }

        private static GameScreenID id = null;

        public static GameScreenID ID
        {
            get
            {
                if (id == null)
                {
                    id = new GameScreenID();
                }
                return id;
            }
        }

        public override GameScreenID GetID()
        {
            return ID;
        }

        public override void ProcessTouchEvents(int x, int y, TouchLocationState tlState)
        {
            if (transitionState != TransitionState.ON_SCREEN)
            {
                return;
            }

            if (GameState.Instance.CurrentGameState != GameState.State.DURING_GAMEPLAY)
            {
                startBattleButton.ProcessTouchEvent(x, y, tlState);
            }
            else
            {
                player.ProcessTouchEvent(new Vector2(x, y));
            }
        }

        private const float TRANSITION_IN_TIME = 2f;

        private string CreateEnemyAnimName(string prefix, int x, int y)
        {
            return prefix + "," + x + "," + y;
        }

        private void CreateTransitionAnimation(bool moveIn, bool instant)
        {
            int offsetStart, offsetEnd;
            string animNamePrefix = "enemy_in";
            //            string animNameToRemove;

            if (moveIn)
            {
                offsetStart = 1000;
                offsetEnd = 0;
                animNamePrefix = "enemy_in";
                //                animNameToRemove = "enemy_out";
            }
            else
            {
                offsetStart = 0;
                offsetEnd = 1000;
                //                animNameToRemove = "enemy_in";
                animNamePrefix = "enemy_out";
            }

            for (int x = 0; x < Constans.ENEMY_COLS; x++)
            {
                for (int y = 0; y < Constans.ENEMY_ROWS; y++)
                {
                    Enemy enemy = enemies[x, y];

                    Bind pctl = new Bind(
                                    delegate { return enemy.TransitionOffset.X; },
                                    delegate(object value) { enemy.TransitionOffset.X = (int)value; }
                                );

                    core2d.CoreAnimation.Instance.Animate(
                                    CreateEnemyAnimName(animNamePrefix, x, y),
                                    pctl,
                                    offsetStart,
                                    offsetEnd,
                                    0.05f * x + 0.05f * y,
                                    instant == true ? 0.0f : TRANSITION_IN_TIME);


                }
            }

            core2d.CoreAnimation.Instance.AddListener(
                CreateEnemyAnimName(animNamePrefix,
                    Constans.ENEMY_COLS - 1, Constans.ENEMY_ROWS - 1),
                this);

            Bind colorCntrl = new Bind(
                delegate { return backgroundColor; },
                delegate(object value) { backgroundColor = (Color)value; }
                );

            int colorIndex = (GameState.Instance.Level - 1) % Constans.BATTLESCREEN_BACKGROUND_COLORS.Length;

            core2d.CoreAnimation.Instance.AnimateColor("color transition", colorCntrl, Constans.BATTLESCREEN_BACKGROUND_COLORS[colorIndex], instant == true ? 0.0f : TRANSITION_IN_TIME * 2f);
        }

        private void FadeScreen(Color startColor, Color endColor, float duration, string animName)
        {
            Bind pctl = new Bind(
                delegate { return fadeoutColor; },
                delegate(object value) { fadeoutColor = (Color)value; }
            );

            fadeoutColor = startColor;
            CoreAnimation.Instance.AnimateColor(animName, pctl, endColor, duration);

        }

        public override void MoveIn()
        {
            // TODO
            //TouchPanel.EnabledGestures = GestureType.FreeDrag;
            //TransitionCompletedEvent e = new TransitionCompletedEvent(); ;
            //NotifyListeners(e);
            transitionStart = World.Instance.GetCurrentTime().TotalGameTime;
            transitionState = TransitionState.MOVING_IN;

            RestartGame();

            SoundPlayer.Instance.Resume();

            FadeScreen(Color.White, Color.Transparent, 0.3f, "fade anim name");

            CreateTransitionAnimation(true, GameState.Instance.IsGameInProgress);
            CalculateEnemyPath(0f);
        }

        public void SaveEnemyState()
        {
            bool[,] enemyState = new bool[Constans.ENEMY_COLS, Constans.ENEMY_ROWS];

            for (int x = 0; x < Constans.ENEMY_COLS; x++)
            {
                for (int y = 0; y < Constans.ENEMY_ROWS; y++)
                {
                    enemyState[x, y] = enemies[x, y].CurrentState == Enemy.State.ALIVE;
                }
            }
            GameState.Instance.IsEnemyAlive = enemyState;
        }

        public void LoadEnemyState()
        {
            if (GameState.Instance.IsEnemyAlive == null)
            {
                // it means there was nothing to deserialize
                // nop
                return;
            }

            for (int x = 0; x < Constans.ENEMY_COLS; x++)
            {
                for (int y = 0; y < Constans.ENEMY_ROWS; y++)
                {
                    if (GameState.Instance.IsEnemyAlive[x, y] == false)
                    {
                        enemies[x, y].CurrentState = Enemy.State.DEAD;
                    }
                }
            }
        }
        public override void MoveOut()
        {
            // TODO
            transitionStart = World.Instance.GetCurrentTime().TotalGameTime;
            transitionState = TransitionState.MOVING_OUT;
            FadeScreen(Color.Transparent, Color.White, 0.15f, FADE_ANIM_NAME);
            core2d.CoreAnimation.Instance.AddListener(FADE_ANIM_NAME, this);

            SoundPlayer.Instance.Suspend();

            //CreateTransitionAnimation(false);
            //TransitionCompletedEvent e = new TransitionCompletedEvent(); ;
            //NotifyListeners(e);
        }

        private void CheckCollisions()
        {
            if (motherShip.CurrentState == MotherShip.State.ALIVE)
            {
                if (motherShip.GetBoundingShape().CollidesWith(playerMissle.GetBoundingShape()))
                {
                    SoundPlayer.Instance.PlaySound("explosion1");
                    motherShip.Explode();
                    GameState.Instance.BonusLevel += 1;
                    GameState.Instance.Score += 1000 * GameState.Instance.Level;
                    SaveEnemyState();

                    FireMissle();
                }
            }

            for (int x = 0; x < Constans.ENEMY_COLS; x++)
            {
                for (int y = Constans.ENEMY_ROWS - 1; y >= 0; y--)
                {
                    Enemy enemy = enemies[x, y];
                    if (enemy.CurrentState == Enemy.State.ALIVE)
                    {
                        if (enemy.GetBoundingShape().CollidesWith(playerMissle.GetBoundingShape()))
                        {
                            SoundPlayer.Instance.PlaySound("explosion1");
                            enemy.Explode();
                            SaveEnemyState();
                            GameState.Instance.Score += GameState.Instance.Level;

                            FireMissle();
                        }

                        if (enemy.GetPos().Y > player.GetPosition().Y && player.CurrentState == Player.State.ALIVE)
                        {
                            GameState.Instance.Lives = 0;
                            player.Explode();
                        }
                    }
                }
            }


            if (player.CurrentState == Player.State.ALIVE)
            {
                foreach (Missle missle in enemyMissles)
                {
                    if (player.GetBoundingShape().CollidesWith(missle.GetBoundingShape()))
                    {
                        missle.SetPos(new Vector2(-10f, -10f));
                        player.Explode();
                    }
                }
            }
        }

        public static bool shouldBeRestarted;

        public void RestartGame()
        {
            if (shouldBeRestarted)
            {
                if (GameState.Instance.IsGameInProgress == false)
                {
                    GameState.Instance.CurrentGameState = GameState.State.NOT_STARTED;
                    GameState.Instance.Score = 0;
                    GameState.Instance.Level = 1;
                    GameState.Instance.Lives = 2;

                    GameState.Instance.BonusLevel = 0;

                    SetupEnemies();
                    StartBattle();
                    playerMissle.SetPos(new Vector2(-10, -10));
                    player.CurrentState = Player.State.ALIVE;
                    player.SetStartingPosition();
                }

                shouldBeRestarted = false;
            }
        }

        private void OnNextLevel()
        {
            GameState.Instance.CurrentGameState = GameState.State.NOT_STARTED;
            GameState.Instance.Level = GameState.Instance.Level + 1;
            motherShip.CurrentState = MotherShip.State.DEAD;
            SoundPlayer.Instance.StopSound("mothershipSound");
            SetupEnemies();
            StartBattle();
            playerMissle.SetPos(new Vector2(-10, -10));
            CreateTransitionAnimation(true, false);
        }

        private bool AreEnemiesDead()
        {
            bool allAreDead = true;

            foreach (Enemy e in enemies)
            {
                if (e.CurrentState != Enemy.State.DEAD)
                {
                    allAreDead = false;
                }
            }

            return allAreDead;
        }
        
        public override void Draw()
        {
            if (transitionState == TransitionState.ON_SCREEN)
            {
                CheckCollisions();
            }

            spriteBatch.Begin();

            spriteBatch.Draw(background, new Rectangle(0, 0, 480, 800), backgroundColor);

            motherShip.Draw(spriteBatch);

            player.Draw(spriteBatch);

            for (int column = 0; column < Constans.ENEMY_COLS; column++)
                for (int row = 0; row < Constans.ENEMY_ROWS; row++)
                    enemies[column, row].Draw(spriteBatch, row);

            playerMissle.Draw(spriteBatch);

            foreach (Missle m in enemyMissles)
            {
                m.Draw(spriteBatch);
            }

            status.Draw(spriteBatch);

            spriteBatch.Draw(fadeOutOverlay, new Rectangle(0, 0, ScreenUtils.ScreenWidth, ScreenUtils.ScreenHeight), fadeoutColor);

            startBattleButton.Draw(spriteBatch);

            spriteBatch.End();

            if (AreEnemiesDead())
            {
                OnNextLevel();
                return;
            }
        }

        #region Player behavior

        #endregion

        #region World event handling

        private void ShowStartBattleButton()
        {
            startBattleButton.x = -1000;
            startBattleButton.y = 450;

            GameState.Instance.CurrentGameState = GameState.State.NOT_STARTED;

            startBattleButton.StartTransition(-1000, startBattleButton.y, 0, startBattleButton.y, 0f, 0.5f);
        }

        private void OnActivatedEvent(EventArgs eventArgs)
        {
            LoadEnemyState();
        }

        private void OnLoadContentEvent(GameScreenManager.LoadContentEventArgs eventArgs)
        {
            background = ScreenUtils.GetTexture("battle_background");
            status = new GameStatusBar(ScreenUtils.GetFont("statusbar_font"));

            int width = ScreenUtils.ScreenWidth;
            int height = ScreenUtils.ScreenHeight / 8;

            startBattleButton = new AnimatedButton("start_battle_button");
            startBattleButton.SetLabel(Constans.MENU_SELECTED_BUTTON_BACKGROUND_COLOR, Constans.MENU_NON_SELECTED_BUTTON_BACKGROUND_COLOR, Constans.MENU_NON_SELECTED_BUTTON_BACKGROUND_COLOR,
                Constans.MENU_NON_ACTIVE_TEXT_COLOR, Constans.MENU_FONT_NAME,
                ResourceManager.Instance.GetString(ResourceManager.StringKey.START_BATTLE),
                width, height);
            startBattleButton.ButtonPressedListener += OnButtonPressed;
            startBattleButton.ButtonTransitionListener += OnButtonMovedIn;

            startBattleButton.x = -1000;

            fadeOutOverlay = ScreenUtils.GetTexture("fadeout_overlay");

            StartBattle();
        }

        private void OnButtonPressed(AnimatedButton button)
        {
            startBattleButton.x = -1000;
            GameState.Instance.CurrentGameState = GameState.State.DURING_GAMEPLAY;
            if (GameState.Instance.IsEnemyAlive == null)
            {
                SaveEnemyState();
            }

            GameState.Instance.IsGameInProgress = true;
        }

        private void FireMissle()
        {
            if (player.CurrentState == Player.State.ALIVE)
            {
                playerMissle.SetPos(player.GetPosition());
                SoundPlayer.Instance.PlaySound("photonShot");
            }
            else
            {
                playerMissle.SetPos(new Vector2(-10f, -10f));
            }
        }


        private void CalculateEnemyPath(float timeDiff)
        {

            float ENEMY_SPEED = 0.01f + (float)GameState.Instance.Level / 30f;
            /* float ENEMY_SPEED = 0.01f + (float)GameState.Instance.Level; */

            float maxXMovement = 0.09f * 3f;
            float maxYMovement = 0.04f;

            float movementCycleDistance = maxXMovement * 2f + maxYMovement * 2f;

            float coveredDistance = GameState.Instance.BattleTime * ENEMY_SPEED;
            int currentCycle = (int)Math.Floor((float)coveredDistance / (float)movementCycleDistance);
            float movementPhase = (float)coveredDistance
                - currentCycle * movementCycleDistance;

            Vector2 dx;

            if (movementPhase < 0)
            {
                throw new NotImplementedException();
            }

            if (movementPhase < maxXMovement)
            {
                dx = new Vector2(1, 0);
            }
            else if (movementPhase > maxXMovement && movementPhase < (maxXMovement + maxYMovement))
            {
                dx = new Vector2(0, 1);
            }
            else if (
                (movementPhase > maxXMovement + maxYMovement)
                && (movementPhase < (2 * maxXMovement + maxYMovement)))
            {
                dx = new Vector2(-1, 0);
            }
            else
            {
                dx = new Vector2(0, 1);
            }

            GameState.Instance.HordlePos += dx * ENEMY_SPEED * timeDiff;

            foreach (Enemy enemy in enemies)
            {
                enemy.UpdatePos(GameState.Instance.HordlePos.X, GameState.Instance.HordlePos.Y);
            }
        }

        private void FireEnemyMissle()
        {

            int activeEnemyCount = 0;

            foreach (Enemy enemy in enemies)
            {
                if (enemy.CurrentState == Enemy.State.ALIVE)
                {
                    activeEnemyCount++;
                }
            }

            int drawnEnemy = random.Next(activeEnemyCount);
            foreach (Enemy enemy in enemies)
            {
                if (enemy.CurrentState == Enemy.State.ALIVE)
                {
                    drawnEnemy--;
                    if (drawnEnemy <= 0)
                    {
                        if (unusedMissles.Count > 0 && enemyMissles.Count < activeEnemyCount)
                        {
                            Missle enemyMissle = unusedMissles[0];
                            unusedMissles.RemoveAt(0);
                            enemyMissles.Add(enemyMissle);
                            enemyMissle.SetPos(enemy.GetPos());
                            SoundPlayer.Instance.PlaySound("laser");
                        }
                        return;
                    }
                }
            }
        }


        private void OnTimeUpdatedEvent(World.TimeUpdatedEvent eventArgs)
        {
            GameTime time = eventArgs.time;

            if (playerMissle.IsOffScreen() && transitionState == TransitionState.ON_SCREEN && GameState.Instance.CurrentGameState == GameState.State.DURING_GAMEPLAY)
            {
                FireMissle();
            }

            List<Missle> toRemove = new List<Missle>();
            foreach (Missle m in enemyMissles)
            {
                if (m.IsOffScreen() && transitionState == TransitionState.ON_SCREEN)
                {
                    unusedMissles.Add(m);
                    toRemove.Add(m);
                }
            }
            foreach (Missle m in toRemove)
            {
                enemyMissles.Remove(m);
            }

            if (transitionState == TransitionState.ON_SCREEN && GameState.Instance.CurrentGameState == GameState.State.DURING_GAMEPLAY)
            {
                if (player.CurrentState.Equals(Player.State.ALIVE))
                {
                    GameState.Instance.BattleTime += time.ElapsedGameTime.Milliseconds / 1000f;
                    CalculateEnemyPath(time.ElapsedGameTime.Milliseconds / 1000f);

                    if (random.NextDouble() < time.ElapsedGameTime.Milliseconds / 1000f && random.NextDouble() < (double)GameState.Instance.Level / 5f)
                    {
                        FireEnemyMissle();
                    }

                    if (motherShip.CurrentState == MotherShip.State.DEAD && GameState.Instance.HordlePos.Y > 0.12f + motherShipPos)
                    {
                        motherShip.SetPos(new Vector2(-0.4f, 0.15f));
                        motherShip.CurrentState = MotherShip.State.ALIVE;
                        SoundPlayer.Instance.PlaySound("mothershipSound");
                    }
                }

            }
        }

        private void OnButtonMovedIn(AnimatedButton button)
        {
            if (transitionState == TransitionState.MOVING_IN)
            {
                transitionState = TransitionState.ON_SCREEN;
                OnScreenTransition(new TransitionCompletedEventArgs());
            }
        }

        private void OnAnimEnded(PropertyAnimation.AnimationEnded animEndedEvent)
        {
            if (animEndedEvent.AnimName.Equals(CreateEnemyAnimName("enemy_in", Constans.ENEMY_COLS - 1, Constans.ENEMY_ROWS - 1)))
            {
                ShowStartBattleButton();
            }
            else if (animEndedEvent.AnimName.Equals(FADE_ANIM_NAME) && transitionState != TransitionState.MOVING_IN)
            {
                OnScreenTransition(new TransitionCompletedEventArgs());
            }

        }

        private void OnPlayerDied()
        {
            GameState.Instance.Lives = GameState.Instance.Lives - 1;

            if (GameState.Instance.Lives < 0)
            {
                SoundPlayer.Instance.StopAll();
                GameState.Instance.IsGameInProgress = false;
                GameScreenManager.Instance.MoveToScreen(GameOverScreen.ID);
            }
            else
            {
                player.CurrentState = Player.State.ALIVE;
                ShowStartBattleButton();
            }
        }

        public void OnNotification(IEventPublisher publisher, EventArgs eventArgs)
        {
            World.TimeUpdatedEvent timeEvent = eventArgs as World.TimeUpdatedEvent;
            PropertyAnimation.AnimationEnded animEndedEvent = eventArgs as PropertyAnimation.AnimationEnded;

            if (timeEvent != null)
            {
                OnTimeUpdatedEvent(timeEvent);
                return;
            }
            else if (animEndedEvent != null)
            {
                OnAnimEnded(animEndedEvent);
            }
            else if (eventArgs as Player.PlayerDied != null)
            {
                OnPlayerDied();
            }

            return;
        }
        #endregion
    }
}
