using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class Board : MonoBehaviour,IInitializable
{
    public event Action<BoardTile> BoardTileClicked;

    [SerializeField] protected BoardTile boardTilePrefab;
    [SerializeField] protected BoardTile backgroundTilePrefab;
    [SerializeField] protected float _width;
    [SerializeField] protected int _rows;
    [SerializeField] protected float _spacing=0.05f;
    [SerializeField] protected bool debugCoordinate;
    [SerializeField] protected Piece piecePrefab;
    [SerializeField] protected bool hideBoardTiles = true;


    private readonly List<BoardTile> _boardTiles = new List<BoardTile>();

    public IEnumerable<BoardTile> BoardTiles => _boardTiles;

    public float TileSize { get; private set; }

    public BoxGrid BoxGrid { get; private set; }
    public bool Intractable { get; set; } = true;

    public IEnumerable<Shape> CurrentShapes => backgroundShapeTiles.Where(tile => tile.Tile != null && ((Piece) tile.Tile).Shape!=null)
        .Select(tile => ((Piece)tile.Tile).Shape).Distinct();

    protected readonly List<Piece> decoratePieces = new List<Piece>();
    private Vector2Int? _lastUpdatedMatchCoordinate;

    public BoardTile this[Vector2Int coordinate]
    {
        get
        {
            var coordinateFromBase = coordinate+new Vector2Int(_rows/2,_rows/2);
            return _boardTiles[coordinateFromBase.x*_rows+coordinateFromBase.y];
        }
    }


    protected readonly List<BoardTile> backgroundShapeTiles = new List<BoardTile>();

    public IEnumerable<BoardTile> BackgroundShapeTiles => backgroundShapeTiles;
    public BoardTile GetBackgroundTile(Vector2Int coordinate)=>this[coordinate].Tile as BoardTile;


    public bool Initialized { get; protected set; }
    


    public void Init()
    {
        if (Initialized)
        {
            return;
        }
        UpdateBoard();

        Initialized = true;
    }

    protected virtual void Awake()
    {
        Init();
    }


    public void SetBackgroundShape(IEnumerable<Vector2Int> coordinates)
    {
        if (backgroundShapeTiles.Count>0)
        {
            throw new InvalidOperationException();
        }

        foreach (var coordinate in coordinates)
        {
            var boardTile = Instantiate(backgroundTilePrefab,this[coordinate].transform.position,Quaternion.identity);
            AddBgTile(boardTile,coordinate);
        }
    }

    public void AddPiece(Piece piece, Vector2Int to)
    {
        if (piece.Holder != null)
        {
            throw new InvalidOperationException();
        }

        var boardTile = GetBackgroundTile(to);

        if(boardTile==null)
            return;

        if (boardTile.Tile != null)
        {
        }

        piece.transform.position = boardTile.transform.position;
        boardTile.Tile = piece;
        piece.Inverted = boardTile.Inverted;
        
        if (piece.Shape == null)
            piece.LocalCoordinate = to;
    }

    public void AddBgTile(BoardTile tile, Vector2Int to)
    {
        if (tile.Holder != null)
        {
            throw new InvalidOperationException();
        }

        var boardTile = this[to];
        if (boardTile.Tile != null)
        {
        }

        tile.transform.parent = transform;
        tile.Inverted = boardTile.Inverted;
        tile.transform.position = boardTile.transform.position;
        boardTile.Tile = tile;
        
        tile.LocalCoordinate = to;
        backgroundShapeTiles.Add(tile);
    }

    public void RemoveBgTile(BoardTile tile)
    {
        if (tile.Holder == null)
        {
            throw new InvalidOperationException();
        }

        var boardTile = (BoardTile) tile.Holder;
        boardTile.Tile = null;
        
        if (tile.Tile is Piece piece)
        {
            RemovePiece(piece);
            Destroy(piece.gameObject);
        }

        backgroundShapeTiles.Remove(tile);
        Destroy(tile.gameObject);
    }   

    public virtual void RemovePiece(Piece piece)
    {
        if (piece.Holder == null)
        {
            return;
        }

        ((BoardTile) piece.Holder).Tile = null;
    }


    public void AddShape(Shape shape, Vector2Int coordinate)
    {
        if (CanPlaceShape(shape, coordinate))
        {
            Debug.LogError("Overlap Shape");
            return;
        }
        shape.transform.parent = transform;
        shape.Coordinate = coordinate;
        shape.transform.position = this[coordinate].transform.position;
       
        shape.ForEach(piece =>
            AddPiece(piece, BoxGrid.RelativeCoordinate(coordinate, piece.LocalCoordinate, Vector2Int.zero)));
        
        OnAddShape(shape);
        ClearMatchEffect();
    }


    protected virtual void OnAddShape(Shape shape)
    {

    }



   

    protected Vector2Int? GetDragMatchCoordinate(Shape shape)
    {
        return _lastUpdatedMatchCoordinate ?? GetNearestMatchCoordinateForShape(shape);
    }


    public Vector2Int? HandleNearestMatchCoordinate(Shape shape)
    {
        var matchCoordinateForShape = GetNearestMatchCoordinateForShape(shape, out var distance);
        if (distance < TileSize * 0.9f / 2)
        {
            UpdateMatchEffect(shape, matchCoordinateForShape);
        }
        else
        {
            UpdateMatchEffect(shape, null);
        }

        return matchCoordinateForShape;
    }

    public void ClearMatchEffect()
    {
        decoratePieces.ForEach(piece => piece.gameObject.SetActive(false));
    }

    public virtual void UpdateMatchEffect(Shape shape, Vector2Int? matchCoordinate)
    {
        _lastUpdatedMatchCoordinate = matchCoordinate;
        var shapePieceCount = shape.Count();
        if (decoratePieces.Count < shapePieceCount)
        {
            var count = shapePieceCount - decoratePieces.Count;
            for (var i = 0; i < count; i++)
            {
                var piece = Instantiate(piecePrefab);
                piece.IsDecorator = true;
                decoratePieces.Add(piece);
            }
        }

        if (matchCoordinate == null)
        {
            decoratePieces.ForEach(piece => piece.gameObject.SetActive(false));
        }
        else
        {
            for (var i = 0; i < decoratePieces.Count; i++)
            {
                if (i < shapePieceCount)
                {
                    decoratePieces[i].gameObject.SetActive(true);
                    decoratePieces[i].Size = TileSize;
                    decoratePieces[i].PieceColor = shape.PieceColor;
                    var coordinate = BoxGrid.RelativeCoordinate(matchCoordinate.Value,
                        shape.ElementAt(i).LocalCoordinate,
                        Vector2Int.zero);
                    decoratePieces[i].transform.position =
                        this[coordinate].transform.position;
                    decoratePieces[i].LocalCoordinate = coordinate;
                    decoratePieces[i].Inverted = this[coordinate].Inverted;
                }
                else
                {
                    decoratePieces[i].gameObject.SetActive(false);
                }
            }
        }
    }

    public Vector2Int? GetNearestMatchCoordinateForShape(Shape shape)
    {
        return GetNearestMatchCoordinateForShape(shape, out _);
    }

    public Vector2Int? GetNearestMatchCoordinateForShape(Shape shape,out float distance)
    {
        var piece = shape.First();

        var nearestCoordinate = GetNearestCoordinate(piece.transform.position);

        if (nearestCoordinate.x > _rows/2 || nearestCoordinate.x < -_rows/2 || nearestCoordinate.y > _rows/2 ||
            nearestCoordinate.y < -_rows/2)
        {
            distance = 0;
            return null;
        }


        if (this[nearestCoordinate].Inverted != piece.Inverted)
        {
            distance = -1;
            return null;
        }

        var baseCoordinate = BoxGrid.RelativeCoordinate(nearestCoordinate,
            BoxGrid.InverseCoordinate(Vector2Int.zero, piece.LocalCoordinate), Vector2Int.zero);
        var coordinate = CanPlaceShape(shape, baseCoordinate) ? null : (Vector2Int?)baseCoordinate;

        distance =  coordinate!=null?  (this[nearestCoordinate].transform.position - piece.transform.position).magnitude:-1f;
        return coordinate;

    }

    public Vector2Int GetNearestCoordinate(Vector2 position)
    {
        return BoxGrid.GetNearestCoordinateForRelativePosition(position - (Vector2) transform.position);
    }


    public void RemoveShape(Shape shape)
    {
        if (backgroundShapeTiles.All(tile => (tile.Tile as Piece)?.Shape != shape))
        {
            Debug.LogError("Shape not Found!");
            return;
        }


        var lastCoordinate = shape.Coordinate;
        shape.Coordinate = Vector2Int.one * -100;

        shape.ForEach(RemovePiece);
        OnRemoveShape(shape,lastCoordinate);
    }

    protected virtual void OnRemoveShape(Shape shape, Vector2Int lastCoordinate)
    {

    }


    public bool CanPlaceShape(Shape shape, Vector2Int coordinate)
    {
        return shape.Any(piece =>
        {
            var c = BoxGrid.RelativeCoordinate(coordinate, piece.LocalCoordinate, Vector2Int.zero);
            return this[c].Tile == null  || ((BoardTile)this[c].Tile).Tile!=null;
        });
    }

    public void MovePiece(Piece piece, Vector2Int to)
    {
        var holder = (BoardTile) piece.Holder;
        if (holder != null)
            holder.Tile = null;
        this[to].Tile = piece;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position,new Vector3(_width,_width,0.1f));
    }

    [ContextMenu("Update Board")]
    public void UpdateBoard()
    {
        ResetBoard();
        TileSize = (_width - (_rows - 1) * _spacing) / _rows;
        BoxGrid = new BoxGrid(TileSize,_spacing);
        foreach (var boardTile in transform.GetComponentsInChildren<BoardTile>())
        {
            if (Application.isPlaying)
                Destroy(boardTile.gameObject);
            else
            {
                DestroyImmediate(boardTile.gameObject);
            }
        }

        _boardTiles.Clear();

        for (var i = -_rows / 2; i <= _rows / 2; i++)
        {
            for (var j = -_rows / 2; j <= _rows / 2; j++)
            {
                BoardTile tile=null;

                if (Application.isPlaying)
                {
                    tile = Instantiate(boardTilePrefab,
                        (Vector2)transform.position + BoxGrid.GetRelativePositionForCoordinate(new Vector2Int(i, j)),
                        Quaternion.identity);
                }
                else
                {
#if UNITY_EDITOR
                    tile = (BoardTile)PrefabUtility.InstantiatePrefab(boardTilePrefab);
                    
                    tile.transform.position = (Vector2)transform.position +
                                              BoxGrid.GetRelativePositionForCoordinate(new Vector2Int(i, j));
#endif
                }
                tile.Size = TileSize;
                tile.transform.parent = transform;
                tile.LocalCoordinate = new Vector2Int(i, j);
                tile.Inverted = Mathf.RoundToInt(Mathf.Abs(i + j)) % 2 == 1;
                tile.HideRenderer = hideBoardTiles;
                tile.Clicked += BoardTileOnClicked;
                _boardTiles.Add(tile);
            }
        }

        BoardTile.DebugCoordinate = debugCoordinate;
    }

    protected void Refresh()
    {

    }

    private void BoardTileOnClicked(Tile tile)
    {
        OnBoardTileClicked((BoardTile) tile);
        BoardTileClicked?.Invoke((BoardTile) tile);
    }

    protected virtual void OnBoardTileClicked(BoardTile tile)
    {
    }

    public virtual void ResetBoard()
    {
        decoratePieces.ForEach(piece => piece.gameObject.SetActive(false));
        CurrentShapes.ToList().ForEach(shape =>
        {
            Destroy(shape.gameObject);
        });
        backgroundShapeTiles.ForEach(tile =>Destroy(tile.gameObject));
        backgroundShapeTiles.Clear();
        foreach (var boardTile in BoardTiles)
        {
            if(boardTile.Tile is BoardTile tile)
            {
                if (tile.Tile != null)
                {
                    Destroy(tile.Tile.gameObject);
                }
                Destroy(tile.gameObject);
            }

            boardTile.Tile = null;
        }
    }
}