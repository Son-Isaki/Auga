﻿using UnityEngine;
using UnityEngine.UI;

namespace AugaUnity
{
    [RequireComponent(typeof(Text))]
    public class PlayerNameText : MonoBehaviour
    {
        private Text _text;

        public virtual void Start()
        {
            _text = GetComponent<Text>();
            if (_text != null)
            {
                _text.text = Game.instance?.GetPlayerProfile()?.GetName() ?? "";
            }
        }

        public virtual void Update()
        {
            if (_text != null && string.IsNullOrEmpty(_text.text))
            {
                _text.text = Game.instance?.GetPlayerProfile()?.GetName() ?? "";
            }
        }
    }
}
