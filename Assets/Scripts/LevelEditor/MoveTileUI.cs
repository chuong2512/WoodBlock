using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class MoveTileUI : MonoBehaviour
{
    public event Action<MoveTileUI> DeleteClicked;
    public event Action<MoveTileUI> Clicked; 

    [SerializeField] private Text _indexTxt;
    [SerializeField] private Text _coordinateTxt;
    [SerializeField] private GameObject _selectedEffect;

    private bool _selected;
    private Vector2Int _coordinate;

    public bool Dirty { get;  set; }

    public ViewModel MViewModel
    {
        get
        {
            int result;
            int.TryParse(_indexTxt.text, out result);
            return new ViewModel
            {
                index = result,
                coordinate = Coordinate
            };
        }
        set
        {
            _indexTxt.text = value.index.ToString();
            Coordinate = value.coordinate;
        }
    }

    public Vector2Int Coordinate
    {
        get { return _coordinate; }
        set
        {
            _coordinate = value;
            _coordinateTxt.text = $"({value.x},{value.y})";
        }
    }

    public bool Selected
    {
        get { return _selected; }
        set
        {
            if (_selectedEffect!=null)
            {
                _selectedEffect.SetActive(value);
            }
            _selected = value;
        }
    }

    public void OnClicked()
    {
        Clicked?.Invoke(this);
    }

    public void OnClickDelete()
    {
        Dirty = true;
        DeleteClicked?.Invoke(this);
    }

    public struct ViewModel
    {
        public Vector2Int coordinate;
        public int index;
    }
}