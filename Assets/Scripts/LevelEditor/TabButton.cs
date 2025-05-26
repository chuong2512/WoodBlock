using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TabButton : MonoBehaviour,IPointerClickHandler
{
    [SerializeField] private Tab _tab;
    [SerializeField] private Color[] _normalAndSelectedColor = new Color[2];

    private bool _selected;

    public event Action<TabButton> Clicked;

    public bool Selected
    {
        get { return _selected; }
        set
        {
            GetComponent<Image>().color = _normalAndSelectedColor[value ? 1 : 0];
            _selected = value;
        }
    }

    public Tab Tab
    {
        get { return _tab; }
        set { _tab = value; }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Clicked?.Invoke(this);
    }
}

public enum Tab
{
    None,BackgroundDesign,Design}