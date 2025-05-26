using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace LevelEditor
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }
        [SerializeField] private GameBoard _board;
        [SerializeField] private InputField _levelInputField;
        [SerializeField] private Button _loadBtn, _saveBtn, _playBtn;
        [SerializeField] private ModePanel _modePanel;
        [SerializeField] private DesignPanel _designPanel;
        [SerializeField] private PopUpPanel _popUpPanel;

        private Tab _tab = Tab.None;

        public GameBoard Board => _board;

        public bool Dirty => !Board.CanSave || IsCurrentLevelModified();

        public Tab Tab
        {
            get => _tab;
            private set
            {
                if (_tab == value)
                {
                    return;
                }

                var lastTap = _tab;
                _tab = value;
                _designPanel.gameObject.SetActive(value == Tab.Design);

                _board.Creating = value == Tab.Design || value == Tab.BackgroundDesign;

            

                Board.BoardCreating = value == Tab.BackgroundDesign;

             
            }
        }

        private int Level
        {
            get
            {
                int.TryParse(_levelInputField.text, out var result);
                return result;
            }
            set => _levelInputField.text = value.ToString();
        }

        public ILevel LastLoadedLevel { get; private set; }



        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            ModePanelOnTabChanged(_modePanel.SelectedTab);
            _modePanel.TabChanged += ModePanelOnTabChanged;
        }

        private void ModePanelOnTabChanged(Tab tab)
        {
            Tab = tab;
        }

        public void OnClickSave()
        {
            if (Level <= 0)
            {
                return;
            }

            var modifiedLevel = GetModifiedLevel();



            if (LastLoadedLevel != null && modifiedLevel.LevelNo != LastLoadedLevel.LevelNo &&
                ResourceManager.LevelsScriptable.HasLevel(modifiedLevel.LevelNo))
            {
                _popUpPanel.ShowAsConfirmation("Override!", "Are you sure want to override the level",
                    success =>
                    {
                        if (success) ResourceManager.LevelsScriptable.AddOrUpdateLevel(modifiedLevel);
                    });
                return;
            }

            ResourceManager.LevelsScriptable.AddOrUpdateLevel(modifiedLevel);
        }

      
        private ILevel GetModifiedLevel()
        {
            return new Level(Level,_board.CurrentShapes.Select(shape => shape.ToShapeData()).ToArray(),
                new BoardData
                {
                    tiles = _board.BackgroundShapeTiles.Select(tile => tile.Holder.LocalCoordinate).ToList()
                });
        }

        public void OnClickPlay()
        {
            MyGame.GameManager.LoadGame(new LoadGameDetails
            {
                Level = Level,
                GameType = GameType.Editor
            });
        }

        public void Clear()
        {
            _board.ResetBoard();
     
            LastLoadedLevel = null;
        }

        private void Update()
        {
            _loadBtn.interactable = ResourceManager.Levels.Any(level => level.LevelNo == Level);

            var dirty = Dirty;
            _saveBtn.interactable = Level >= 1 && Board.CanSave && dirty;
            _playBtn.interactable = !dirty && Level >= 1;
        }

        public void OnLoadLevel()
        {
            if (Level <= 0)
            {
                return;
            }

            Clear();
            LastLoadedLevel = ResourceManager.Levels.FirstOrDefault(lvl => lvl.LevelNo == Level);



            if (LastLoadedLevel == null)
            {
                Debug.LogError($"No Level Found with {Level}");
                return;
            }

            _board.SetBackgroundShape(LastLoadedLevel.Board);
            LastLoadedLevel.Shapes.ForEach(data => _board.AddShape(data,data.coordinate));

        }


        private bool IsCurrentLevelModified()
        {
            if (Level <= 0 || ResourceManager.Levels.FirstOrDefault(lvl => lvl.LevelNo == Level) == null)
            {
                return Board.CurrentShapes.Any();
            }

            var modifiedLevel = GetModifiedLevel();

            var level = ResourceManager.Levels.First(lvl => lvl.LevelNo == Level);
            var shapeDatas = level.Shapes.ToList();
            var modifiedShapeDatas = modifiedLevel.Shapes.ToList();


            for (var i = 0; i < shapeDatas.Count; i++)
            {
                var index = modifiedShapeDatas.FindIndex(d => shapeDatas[i].IsEqualPieceData(d));

                if (index == -1)
                    return true;

         

                modifiedShapeDatas.RemoveAt(index);
                shapeDatas.RemoveAt(i);
                --i;
            }

            return modifiedShapeDatas.Count != 0;
        }
    }
}