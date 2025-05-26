using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using UnityEngine;

public class LevelsPanel : ShowHidable
{
    [SerializeField] private RectTransform _content;
    [SerializeField] private LevelTileUI _levelTileUIPrefab;

    private readonly List<LevelTileUI> _tiles = new List<LevelTileUI>();

    public void Back()
    {
        Hide();
    }

    public override void Show(bool animate = true, Action completed = null)
    {
        var levels = ResourceManager.Levels.ToList();
        while (_tiles.Count<levels.Count)
        {
            var levelTileUI = Instantiate(_levelTileUIPrefab,_content);
            levelTileUI.Clicked+=LevelTileUIOnClicked;
            _tiles.Add(levelTileUI);
        }

        if(_tiles.Count>levels.Count)
            throw new NotImplementedException();

        for (var i = 0; i < _tiles.Count; i++)
        {
            _tiles[i].Level = levels[i];
        }
        base.Show(animate, completed);
    }

    private void LevelTileUIOnClicked(LevelTileUI tile)
    {
        if(!tile.Level.Locked)
        {
            LevelManager.Instance.LoadLevel(tile.Level.LevelNo);
            Hide();
        }
        
    }
}