using System;
using System.Linq;
using UnityEngine;

namespace Game
{
    public partial class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }
        public static event Action<ILevel> LevelLoaded;
        public static event Action<ILevel> LevelCompleted;
        //        public static event Action GameOver;
        //        public static event Action GameStarted;

        [SerializeField] private GameBoard _gameBoard;

        [SerializeField] private ShapeManager _shapeManager;

        //        [SerializeField] private LevelCreator _levelCreator;
        private int _score;

        public GameType GameType { get; private set; }

        public GameBoard GameBoard => _gameBoard;

        public State CurrentState { get; private set; }
        public ILevel Level { get; private set; }

        private void Awake()
        {
            Instance = this;
            GameType = MyGame.GameManager.LoadGameDetails.GameType;
            _shapeManager.Active = true;

        }

        


        private void GameBoardOnGameResulted()
        {
                CurrentState = State.GameOver;
                _shapeManager.Active = false;
                ResourceManager.CompleteLevel(Level.LevelNo);
            LevelCompleted?.Invoke(Level);
        }

        public void LoadLevel(int lvl)
        {
            Level = ResourceManager.GetLevel(lvl);
           _gameBoard.ResetBoard();
            _gameBoard.SetBackgroundShape(Level.Board);
            _shapeManager.SetUp(Level.Shapes.ToArray());
            _shapeManager.Active = true;
            CurrentState = State.Playing;
            MyGame.GameManager.TOTAL_GAME_COUNT++;
            LevelLoaded?.Invoke(Level);
        }



        private void OnEnable()
        {
            _gameBoard.GameResulted += GameBoardOnGameResulted;
        }


        private void OnDisable()
        {
            _gameBoard.GameResulted -= GameBoardOnGameResulted;
        }


        private void Start()
        {
            LoadLevel(GameType == GameType.Editor
                ? MyGame.GameManager.LoadGameDetails.Level
                : ResourceManager.TargetLevel);
        }


        private void Update()
        {
            if (CurrentState == State.GameOver)
                return;
        }


        public enum State
        {
            None,
            Playing,
            GameOver
        }
    }

}