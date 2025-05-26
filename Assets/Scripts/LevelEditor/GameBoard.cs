// /*
// Created by Darsan 
// */

using System;
using System.Linq;
using UnityEngine;

namespace LevelEditor
{
    public class GameBoard : Board
    {
        [SerializeField] private Shape _shapePrefab;

        public bool Creating { get; set; }
        public bool BoardCreating { get; set; } = true;

        public bool RemoveShapePieceOnClickWhileCreating { get; set; } = false;

        public bool CanSave => backgroundShapeTiles.All(tile => tile.Tile != null && ((Piece)tile.Tile).Shape);// || tile.Tile.Shape != null);

        protected override void Awake()
        {
            base.Awake();
            Refresh();
        }

        public void AddPiece(PieceData data)
        {
            var piece = Instantiate(piecePrefab);
            AddPiece(piece, data.coordinate);
        }

        public bool Undo()
        {
            return true;
        }


        protected void ShapeOnPointerDown(Shape shape)
        {
            if (Input.GetKey(KeyCode.LeftControl))
                return;


        }


        public Shape CreateShape()
        {
            var boardTiles = backgroundShapeTiles.Where(tile => tile.Tile != null && ((Piece) tile.Tile).Shape == null).ToList();

            if (boardTiles.Count == 0)
                return null;

            var coordinate = boardTiles.First().LocalCoordinate;

            var pieceDatas = boardTiles.Select(tile => new PieceData
            {
                coordinate = BoxGrid.RelativeCoordinate(Vector2Int.zero, tile.LocalCoordinate, coordinate)
            }).ToList();

            boardTiles.ForEach(tile =>
            {
                Destroy(tile.Tile.gameObject);
                tile.Tile = null;
            });

            var shape = AddShape(new ShapeData
            {
                color = PieceColorExtensions.All().Except(CurrentShapes.Select(s => s.PieceColor)).GetRandom(),
                pieces = pieceDatas,
                id = UUID.GetId(),
                baseInverted = boardTiles.First().Inverted

            }, coordinate);


            return shape;
        }



        public Shape AddShape(ShapeData data, Vector2Int coordinate)
        {
            var shape = Instantiate(_shapePrefab);
            shape.PointerDown += ShapeOnPointerDown;
            shape.Init(new Shape.InitParams
            {
                ShapeData = data,
                Spacing = _spacing,
                TileSize = TileSize,
                Id = data.id
            });
            AddShape(shape, coordinate);

            return shape;
        }

        protected override void OnAddShape(Shape shape)
        {
            base.OnAddShape(shape);
            shape.ForEach(piece => piece.Clicked += PieceOnClicked);
        }

        protected override void OnRemoveShape(Shape shape, Vector2Int lastCoordinate)
        {
            base.OnRemoveShape(shape, lastCoordinate);
            shape.ForEach(piece => piece.Clicked -= PieceOnClicked);
        }

        private void PieceOnClicked(Tile tile)
        {
            if (!Intractable || !Input.GetKey(KeyCode.LeftControl))
                return;
            var piece = (Piece) tile;
            var holder = (BoardTile) piece.Holder;
            piece.Shape.RemovePiece(piece);
            holder.Tile = null;
            var newPiece = Instantiate(piecePrefab, tile.transform.position, Quaternion.identity);
            newPiece.Clicked += t => OnBoardTileClicked(this[t.LocalCoordinate]);
            AddPiece(newPiece, holder.LocalCoordinate);
        }


        protected override void OnBoardTileClicked(BoardTile tile)
        {
            if (!Intractable)
            {
                return;
            }

            if (Creating && !BoardCreating)
            {
                var tilePiece = tile.Tile as Piece;
                if (tilePiece?.Shape != null)
                    return;

                if (tilePiece != null)
                {
                    RemovePiece(tilePiece);
                    Destroy(tilePiece.gameObject);
                }
                else
                {
                    if (tile.Holder!=null)
                    {
                        var piece = Instantiate(piecePrefab, tile.transform.position, Quaternion.identity);
                        piece.Clicked += t => OnBoardTileClicked(GetBackgroundTile(tile.LocalCoordinate));
                        AddPiece(piece, tile.LocalCoordinate);
                    }
                   
                }
            }
            else if(Creating)
            {

                if (backgroundShapeTiles.Contains(tile))
                { 
                    RemoveBgTile(tile);
                    Destroy(tile.gameObject);
                }
                else
                {
                    var bgTile = Instantiate(backgroundTilePrefab, tile.transform.position, Quaternion.identity);
                    bgTile.Clicked += t => OnBoardTileClicked(bgTile);
                    AddBgTile(bgTile, tile.LocalCoordinate);
                }
            }

            base.OnBoardTileClicked(tile);
        }
    }

}