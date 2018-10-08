using System;
using System.Collections.Generic;
using System.Text;
using MPP.core;
using System.IO.IsolatedStorage;
using System.IO;


namespace MPP.game
{
    public class GameState : IEventPublisher
    {

        #region Life cycle
        static private GameState instance;

        private GameState()
        {
            LastHighScore = 0;
            score = 0;
        }

        static public GameState Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameState();
                }
                return instance;
            }
        }
        #endregion

        #region Meat

        public enum State
        {
            NOT_STARTED,
            DURING_GAMEPLAY
        }

        public State CurrentGameState
        {
            get;
            set;
        }

        public class ScoreCountChanged : EventArgs { }
        public class LivesCountChanged : EventArgs { }

        public float BattleTime;

        public Microsoft.Xna.Framework.Vector2 HordlePos;

        private int highScore;
        public int LastHighScore
        {
            get { return highScore; }
            private set { highScore = value; }
        }


        private int score;
        public int Score
        {
            get { return score; }
            set
            {
                const int NEW_LIFE = 10000;

                int previousScore = score;

                score = value;

                if (score >= HighScore)
                {
                    if (LastHighScore == 0)
                    {
                        LastHighScore = HighScore;
                    }

                    HighScore = score;
                }

                if (previousScore / NEW_LIFE != score / NEW_LIFE)
                {
                    Lives += 1;
                }

                if (score == 0)
                {
                    LastHighScore = 0;
                }

                NotifyListeners(new ScoreCountChanged());
            }
        }

        public int HighScore { get; set; }

        public int BonusLevel { get; set; }

        public bool IsGameInProgress { get; set; }
        public bool IsSoundEnabled { get; set; }

        public int Level { get; set; }
        public int Lives { get; set; }

        public bool[,] IsEnemyAlive;

        private delegate void GameStateHandler(IEventPublisher publisher, EventArgs args);
        private event GameStateHandler gameStateEventHandler;

        public void RegisterListener<T>(T listener) where T : IEventListener
        {
            gameStateEventHandler += new GameStateHandler(listener.OnNotification);
        }

        public void UnregisterListener<T>(T listener) where T : IEventListener
        {
            gameStateEventHandler -= new GameStateHandler(listener.OnNotification);
        }

        public void NotifyListeners(EventArgs args)
        {
            if (null != gameStateEventHandler)
            {
                gameStateEventHandler(this, args);
            }
        }

        #endregion

        #region Serialization

        private int CalculateCRC(byte[] buffer, int length)
        {
            int result = 0;

            if (buffer != null)
            {
                for (int i = 0; i < buffer.Length && i < length; i++)
                {
                    result += buffer[i];
                }
            }

            return result;
        }

        private const string GAME_STATE_FOLDER = "Data";
        private const string GAME_STATE_FILE = "gamestate.dat";

        public void Load()
        {
            IsGameInProgress = false;

            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication() ) {
                string filePath = System.IO.Path.Combine(GAME_STATE_FOLDER, GAME_STATE_FILE);
                if (storage.FileExists(filePath)) {
                    try
                    {
                        using (IsolatedStorageFileStream rawStream = storage.OpenFile(filePath, System.IO.FileMode.Open))
                        {
                            BinaryReader reader = new BinaryReader(rawStream);

                            int crc = reader.ReadInt32();
                            int length = reader.ReadInt32();

                            byte[] buffer = reader.ReadBytes(length);

                            if (crc == CalculateCRC(buffer, length)) {
                                this.FromByteArray(buffer, 0);
                            }

                            reader.Close();

                        }
                    } catch {
                        storage.DeleteFile(filePath);
                        IsGameInProgress = false;
                        HighScore = 0;
                    }
                }
            }
        }

        public void Save()
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!storage.DirectoryExists(GAME_STATE_FOLDER))
                    storage.CreateDirectory(GAME_STATE_FOLDER);

                using (IsolatedStorageFileStream stream = storage.CreateFile(System.IO.Path.Combine(GAME_STATE_FOLDER, GAME_STATE_FILE)))
                {
                    BinaryWriter writer = new BinaryWriter(stream);

                    byte[] buffer = new byte[1024];

                    int length = this.ToByteArray(buffer);

                    writer.Write(CalculateCRC(buffer, length));
                    writer.Write(length);
                    if (buffer != null)
                    {
                        writer.Write(buffer, 0, length);
                    }

                    writer.Close();
                }
            }
        }

        private int ToByteArray(byte[] buffer)
        {
            int result = 0;

            byte[] temp = BitConverter.GetBytes((Boolean)IsGameInProgress);
            temp.CopyTo(buffer, result);
            result += temp.Length;

            temp = BitConverter.GetBytes((Boolean)SoundPlayer.Instance.IsSoundEnabled);
            temp.CopyTo(buffer, result);
            result += temp.Length;

            temp = BitConverter.GetBytes((Int32)Score);
            temp.CopyTo(buffer, result);
            result += temp.Length;

            temp = BitConverter.GetBytes((Int32)HighScore);
            temp.CopyTo(buffer, result);
            result += temp.Length;

            temp = BitConverter.GetBytes((Int32)Level);
            temp.CopyTo(buffer, result);
            result += temp.Length;

            temp = BitConverter.GetBytes((Int32)BonusLevel);
            temp.CopyTo(buffer, result);
            result += temp.Length;

            temp = BitConverter.GetBytes((Int32)Lives);
            temp.CopyTo(buffer, result);
            result += temp.Length;

            temp = BitConverter.GetBytes((Double)BattleTime);
            temp.CopyTo(buffer, result);
            result += temp.Length;

            temp = BitConverter.GetBytes((Double)HordlePos.X);
            temp.CopyTo(buffer, result);
            result += temp.Length;
            temp = BitConverter.GetBytes((Double)HordlePos.Y);
            temp.CopyTo(buffer, result);
            result += temp.Length;

            bool afterStart = true;
            if (IsEnemyAlive == null)
            {
                afterStart = false;    
            }

            temp = BitConverter.GetBytes((Boolean)afterStart);
            temp.CopyTo(buffer, result);
            result += temp.Length;

            if (afterStart == false)
                return result;

            temp = BitConverter.GetBytes((Int32)IsEnemyAlive.GetLength(0));
            temp.CopyTo(buffer, result);
            result += temp.Length;
            temp = BitConverter.GetBytes((Int32)IsEnemyAlive.GetLength(1));
            temp.CopyTo(buffer, result);
            result += temp.Length;

            for (int x = 0; x < IsEnemyAlive.GetLength(0); x++) {
                for (int y = 0; y < IsEnemyAlive.GetLength(1); y++) {
                    temp = BitConverter.GetBytes((Boolean)IsEnemyAlive[x,y]);
                    temp.CopyTo(buffer, result);
                    result += temp.Length;
                }
            }
            
            
            return result;
        }

        private void FromByteArray(byte[] buffer, int index)
        {
            IsGameInProgress = BitConverter.ToBoolean(buffer, index);
            index += BitConverter.GetBytes((Boolean)IsGameInProgress).Length;

            SoundPlayer.Instance.IsSoundEnabled = BitConverter.ToBoolean(buffer, index);
            index += BitConverter.GetBytes((Boolean)SoundPlayer.Instance.IsSoundEnabled).Length;

            Score = BitConverter.ToInt32(buffer, index);
            index += BitConverter.GetBytes((Int32)Score).Length;

            HighScore = BitConverter.ToInt32(buffer, index);
            index += BitConverter.GetBytes((Int32)HighScore).Length;

            Level = BitConverter.ToInt32(buffer, index);
            index += BitConverter.GetBytes((Int32)Level).Length;

            BonusLevel = BitConverter.ToInt32(buffer, index);
            index += BitConverter.GetBytes((Int32)BonusLevel).Length;

            Lives = BitConverter.ToInt32(buffer, index);
            index += BitConverter.GetBytes((Int32)Lives).Length;

            BattleTime = (float)BitConverter.ToDouble(buffer, index);
            index += BitConverter.GetBytes((Double)BattleTime).Length;

            HordlePos.X = (float)BitConverter.ToDouble(buffer, index);
            index += BitConverter.GetBytes((Double)HordlePos.X).Length;
            HordlePos.Y = (float)BitConverter.ToDouble(buffer, index);
            index += BitConverter.GetBytes((Double)HordlePos.Y).Length;


            bool afterStart = BitConverter.ToBoolean(buffer, index);
            index += BitConverter.GetBytes((Boolean)afterStart).Length;

            if (afterStart == false)
            {
                return;
            }

            int cols = BitConverter.ToInt32(buffer, index);
            index += BitConverter.GetBytes((Int32)cols).Length;
            int rows = BitConverter.ToInt32(buffer, index);
            index += BitConverter.GetBytes((Int32)cols).Length;

            IsEnemyAlive = new bool[cols, rows];

            for (int x = 0; x < IsEnemyAlive.GetLength(0); x++)
            {
                for (int y = 0; y < IsEnemyAlive.GetLength(1); y++)
                {
                    bool temp = BitConverter.ToBoolean(buffer, index);
                    index += BitConverter.GetBytes((Boolean)SoundPlayer.Instance.IsSoundEnabled).Length;
                    IsEnemyAlive[x,y] = temp;
                }
            }
        }

        #endregion
    }
}
