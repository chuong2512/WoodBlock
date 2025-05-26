using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelTileUI : MonoBehaviour,IPointerClickHandler
{
    public event Action<LevelTileUI> Clicked; 

    [SerializeField] private Text _lvlTxt;
    [SerializeField] private GameObject _lockEffect;


    private ILevel _level;

    public ILevel Level
    {
        get => _level;
        set
        {
            _lvlTxt.text = value.LevelNo.ToString();
            _lockEffect.gameObject.SetActive(value.Locked);
         
            _level = value;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Clicked?.Invoke(this);
    }
}