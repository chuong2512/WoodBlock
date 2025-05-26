using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LevelEditor
{
    public class PieceTileUI : MonoBehaviour,IPointerClickHandler
    {
        public event Action<PieceTileUI> Clicked;

        [SerializeField] private GameObject _selectedEffect;
        [SerializeField] private Image _img;
        [SerializeField] private bool _flip;

        private bool _selected;

        public bool Selected
        {
            get { return _selected; }
            set
            {
                _selectedEffect?.SetActive(value);
                _selected = value;
            }
        }


        public bool Flip
        {
            get { return _flip; }
            set { _flip = value; }
        }


        public void OnPointerClick(PointerEventData eventData)
        {
            Clicked?.Invoke(this);
        }

        private void Awake()
        {
            Flip = Flip;
        }
    }
}