using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class GamePlayPanel : ShowHidable
    {
        

        [SerializeField] private Button _previousBtn, _nxtBtn, _undoBtn;
        [SerializeField] private Text _lvlTxt;
        [SerializeField] private RectTransform _moreContent;
        [SerializeField] private float _moreContentExpandHeight;
 

        private LevelManager LevelManager => LevelManager.Instance;



        private int CurrentLevel
        {
            set
            {
                _lvlTxt.text = $"Level {value}";
                RefreshBtns();
            }
        }

        private GameBoard _board;

        private bool _showingMoreContent;
//        private int _currentSymmetricCount=0;

        public bool DisableButtons { get; set; } = false;

//        private int CurrentSymmetricCount
//        {
//            get => _currentSymmetricCount;
//            set
//            {
//                _symmetricTxt.text = $"{value}/{LevelManager.Level.SymmetricCount}";
//                if(value!=_currentSymmetricCount)
//                {
//                    _symmetricAnim.SetTrigger(_currentSymmetricCount>value?SYMMETRIC_DOWN_HASH: Board.Completed?COMPLETE_HASH : SYMMETRIC_UP_HASH);
//                }
//
//                _currentSymmetricCount = value;
//            }
//        }

        private bool ShowingMoreContent
        {
            get => _showingMoreContent;
            set
            {
                StopCoroutine(nameof(MoreContentSizeAnim));
                StartCoroutine(MoreContentSizeAnim(value ? _moreContentExpandHeight : 0));
                _showingMoreContent = value;
            }
        }

        private GameBoard Board
        {
            get => _board;
            set
            {
                if (value == null || value == _board)
                    throw new NotImplementedException();

                _board = value;
            }
        }


        private void Awake()
        {
            _previousBtn.onClick.AddListener(OnClickPrevious);
            _nxtBtn.onClick.AddListener(OnClickNxt);
        }


        protected override void OnEnable()
        {
            LevelManager.LevelLoaded += LevelManagerOnLevelLoaded;
            LevelManager.LevelCompleted += LevelManagerOnLevelCompleted;
            base.OnEnable();
        }


        protected override void OnDisable()
        {
            LevelManager.LevelLoaded -= LevelManagerOnLevelLoaded;
            LevelManager.LevelCompleted -= LevelManagerOnLevelCompleted;
            base.OnDisable();
        }

        private void LevelManagerOnLevelCompleted(ILevel level)
        {

        }

       


        private void LevelManagerOnLevelLoaded(ILevel level)
        {
            CurrentLevel = level.LevelNo;
        }

        private IEnumerator Start()
        {
            Board = LevelManager.GameBoard;
            LevelManagerOnLevelLoaded(LevelManager.Level);

            yield return new WaitForEndOfFrame();
        }

        private void Update()
        {
//            CurrentSymmetricCount = Board.CurrentSymmetricCount;
            RefreshBtns();
        }


        private void OnClickNxt()
        {
            LevelManager.LoadLevel(LevelManager.Level.LevelNo + 1);
        }

        private void OnClickPrevious()
        {
            LevelManager.LoadLevel(LevelManager.Level.LevelNo - 1);
        }

        public void OnClickRestart()
        {
            LevelManager.LoadLevel(LevelManager.Level.LevelNo);
        }


        public void OnClickMore()
        {
            ShowingMoreContent = !ShowingMoreContent;
        }

        private IEnumerator MoreContentSizeAnim(float height)
        {
            while (Mathf.Abs(_moreContent.rect.height - height) > 1)
            {
                var h = Mathf.MoveTowards(_moreContent.rect.height, height, Time.deltaTime * 2500);
                _moreContent.sizeDelta = new Vector2(_moreContent.rect.width, h);
                yield return null;
            }

            _moreContent.sizeDelta = new Vector2(_moreContent.rect.width, height);
        }


        public void OnClickLevels()
        {
            var levelsPanel = UIManager.Instance.LevelsPanel;
            levelsPanel.Show();
        }

        public void OnClickStore()
        {
            var storePanel = UIManager.Instance.StorePanel;
            storePanel.Show();
        }


        public void OnClickHint()
        {
            if(Board.Completed)
                return;

            if (!ResourceManager.UnlimitedHints)
            {
                if (ResourceManager.Hints <= 0)
                {
                    UIManager.Instance.StorePanel.Show();
                    return;
                }

                ResourceManager.Hints--;
            }
           ShapeManager.Instance.ShowHint();
//            StartCoroutine(Hint());
        }

        public void OnClickUndo()
        {
            if (!ShapeManager.Instance.HasUndo())
            {
                return;
            }


            ShapeManager.Instance.Undo();
//            StartCoroutine(Hint());
        }




        private void RefreshBtns()
        {
            _previousBtn.interactable = !DisableButtons && LevelManager.Level.LevelNo > 1;
            _nxtBtn.interactable = !DisableButtons &&
                                   !(ResourceManager.GetLevel(LevelManager.Level.LevelNo + 1)?.Locked ?? true);
            _undoBtn.interactable = ShapeManager.Instance.HasUndo();
        }

    }
}