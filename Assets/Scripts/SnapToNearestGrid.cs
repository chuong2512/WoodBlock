// /*
// Created by Darsan
// */

using System.Collections;
using System.Linq;
using UnityEngine;

namespace Game
{
    public class SnapToNearestGrid : MonoBehaviour
    {
        [SerializeField] private Edge _edge;

        public RectTransform RectTransform => (RectTransform) transform;

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => LevelManager.Instance.GameBoard.Initialized);

            var corners = new Vector3[4];
            RectTransform.GetWorldCorners(corners);

            var gameBoard = LevelManager.Instance.GameBoard;

            if (_edge == Edge.Bottom)
            {
                // ReSharper disable once PossibleNullReferenceException
                var worldPoint = Camera.main.ScreenToWorldPoint(corners[0]);
//                Debug.Log(worldPoint);
                var boardTile = gameBoard.BoardTiles.OrderBy(tile => ((Vector2)tile.transform.position - (Vector2)worldPoint).sqrMagnitude).First();

                var nearestPositionY = boardTile.transform.position.y > worldPoint.y
                    ? boardTile.transform.position.y - gameBoard.TileSize/2
                    : boardTile.transform.position.y + gameBoard.TileSize/2;

                var correctedPosition = Camera.main.WorldToScreenPoint(worldPoint+Vector3.up* (nearestPositionY - worldPoint.y));
                
                RectTransform.sizeDelta = new Vector2(RectTransform.sizeDelta.x, RectTransform.rect.height + ScreenWidthToCanvas(-correctedPosition.y + corners[0].y));
            }
            else
            {
                Debug.LogError("Not Implemented");
            }

        }

        public float ScreenWidthToCanvas(float width)
        {
            var trans = (RectTransform)transform.GetComponentInParent<Canvas>().transform;

            return width * trans.rect.height / Screen.height;
        }

        public enum Edge
        {
            Top,
            Right,
            Bottom,
            Left
        }
    }
}