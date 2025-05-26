using UnityEngine;

namespace LevelEditor
{
    public class PlayPanel : MonoBehaviour
    {
        public void OnClickUndo()
        {
            UIManager.Instance.Board.Undo();
        }
    }
}