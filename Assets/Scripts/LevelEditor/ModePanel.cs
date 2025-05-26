using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ModePanel : MonoBehaviour
{
    public event Action<Tab> TabChanged; 

    [SerializeField]private List<TabButton> _tabButtons = new List<TabButton>();

    public Tab SelectedTab => SelectedTabButton.Tab;

    private TabButton SelectedTabButton
    {
        get { return _tabButtons.FirstOrDefault(button => button.Selected);}
        set
        {
            if (SelectedTabButton == value)
            {
                return;
            }
            foreach (var tabButton in _tabButtons)
            {
                tabButton.Selected = value == tabButton;
            }
            TabChanged?.Invoke(value.Tab);
        }
    }

    private void Awake()
    {
        _tabButtons.ForEach(button => button.Clicked +=ButtonOnClicked);
        SelectedTabButton = _tabButtons.FirstOrDefault();
    }

    private void ButtonOnClicked(TabButton tabButton)
    {
        SelectedTabButton = tabButton;
    }
}