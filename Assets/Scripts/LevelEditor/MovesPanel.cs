using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MovesPanel : MonoBehaviour
{
    [SerializeField] private MoveTileUI _moveTilePrefab;
    [SerializeField] private RectTransform _content;
    [SerializeField] private Board _board;

    private readonly List<MoveTileUI> _tiles = new List<MoveTileUI>();
    private bool _dirty;

//    public bool Interaction { get; set; }

    public MoveTileUI SelectedTile
    {
        get { return _tiles.FirstOrDefault(ui => ui.Selected); }
        private set
        {
            foreach (var moveTileUI in _tiles)
            {
                moveTileUI.Selected = moveTileUI == value;
            }
        }
    }

    public bool Dirty
    {
        get { return _dirty || _tiles.Any(ui => ui.Dirty); }
        set
        {
            _dirty = value;
            if (!value)
                _tiles.ForEach(ui => ui.Dirty = false);
        }
    }


    public IEnumerable<Vector2Int> Moves
    {
        get { return _tiles.Select(ui => ui.MViewModel.coordinate); }
        set
        {
            Clear();
            foreach (var direction in value)
            {
                var moveTileUI = AddTile();
                var viewModel = moveTileUI.MViewModel;
                viewModel.coordinate = direction;
                moveTileUI.MViewModel = viewModel;
            }
        }
    }


    public void OnClickAdd()
    {
        AddTile();
        Dirty = true;
    }

    private void OnEnable()
    {
        _board.BoardTileClicked += BoardOnBoardTileClicked;
    }

    private void OnDisable()
    {
        _board.BoardTileClicked -= BoardOnBoardTileClicked;
    }

    private void BoardOnBoardTileClicked(BoardTile tile)
    {
        if (tile.Tile == null || SelectedTile == null)
        {
            return;
        }

        SelectedTile.Coordinate = tile.LocalCoordinate;
    }

    private MoveTileUI AddTile()
    {
        var moveTileUI = Instantiate(_moveTilePrefab, _content);
        var model = moveTileUI.MViewModel;
        model.index = _tiles.Count;
        moveTileUI.MViewModel = model;
        moveTileUI.DeleteClicked += MoveTileUIOnDeleteClicked;
        moveTileUI.Clicked += MoveTileUIOnClicked;
        _tiles.Add(moveTileUI);

        var siblingIndex = moveTileUI.transform.GetSiblingIndex();
        if (siblingIndex > 0)
        {
            if (moveTileUI.transform.parent.GetChild(siblingIndex - 1).GetComponent<MoveTileUI>() == null)
            {
                moveTileUI.transform.SetSiblingIndex(siblingIndex - 1);
            }
        }

        return moveTileUI;
    }

    private void MoveTileUIOnClicked(MoveTileUI tile)
    {
        SelectedTile = tile;
    }

    public void Clear()
    {
        _tiles.ForEach(ui => Destroy(ui.gameObject));
        _tiles.Clear();
    }

    private void MoveTileUIOnDeleteClicked(MoveTileUI tileUi)
    {
        if (tileUi == SelectedTile)
        {
            SelectedTile = null;
        }

        var index = _tiles.IndexOf(tileUi);
        Destroy(tileUi.gameObject);
        _tiles.RemoveAt(index);
        foreach (var moveTileUI in _tiles.GetRange(index, _tiles.Count - index))
        {
            var model = moveTileUI.MViewModel;
            model.index--;
            moveTileUI.MViewModel = model;
        }
    }

    public void OnClickAutoCalculate()
    {
        Clear();
    }
}