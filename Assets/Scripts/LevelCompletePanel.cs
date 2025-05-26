using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class LevelCompletePanel : ShowHidable
    {
        [SerializeField] private Text _text;
        [SerializeField]private List<string> _wishWords = new List<string>();

        public override void Show(bool animate = true, Action completed = null)
        {
            _text.text = _wishWords.GetRandom();
            base.Show(animate, completed);
        }
    }
}