// /*
// Created by Darsan
// */

using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace LevelEditor
{
    public class DesignPanel : MonoBehaviour
    {
        [SerializeField] private Toggle _removePieceToggle;
        [SerializeField] private Button _createShapeButton;

        private GameBoard Board => UIManager.Instance.Board;

        private void OnEnable()
        {
        }

        private void Update()
        {
//            _createShapeButton.interactable = Board.BoardTiles.Any(tile =>
//                tile.Tile != null && !tile.Tile.IsDecorator && tile.Tile.Shape == null);
        }

        public void OnClickCreateShape()
        {
            Board.CreateShape();
        }

        public void OnToggleRemovePiece()
        {
        }
    }
}