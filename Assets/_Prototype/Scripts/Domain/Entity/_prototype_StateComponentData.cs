using System;
using System.Collections.Generic;
using UnityEngine;

namespace TDG0407._prototype
{
    [Serializable]
    public class _prototype_StateComponentData : _prototype_EntityComponentData
    {
        [SerializeField]
        private List<_prototype_StateEntry> _states = new();

        public IReadOnlyList<_prototype_StateEntry> States => _states;

        public event Action<string, string> OnStateChanged;

        [NonSerialized]
        private _prototype_EntityView _owner;
        public _prototype_EntityView Owner => _owner;

        public _prototype_StateComponentData() { }

        public _prototype_StateComponentData(List<_prototype_StateEntry> initialStates)
        {
            if (initialStates != null)
            {
                foreach (var entry in initialStates)
                {
                    if (entry != null && !string.IsNullOrEmpty(entry.key))
                    {
                        _states.Add(new _prototype_StateEntry(entry.key, entry.value));
                    }
                }
            }
        }

        public override void Initialize(_prototype_EntityView owner)
        {
            _owner = owner;
        }

        public string GetState(string key, string defaultValue = "")
        {
            if (string.IsNullOrEmpty(key)) return defaultValue;
            var entry = _states.Find(s => s.key == key);
            return entry != null ? entry.value : defaultValue;
        }

        public bool GetBoolState(string key, bool defaultValue = false)
        {
            string val = GetState(key);
            if (string.IsNullOrEmpty(val)) return defaultValue;
            if (bool.TryParse(val, out bool result)) return result;
            if (val.Equals("1", StringComparison.OrdinalIgnoreCase) || val.Equals("on", StringComparison.OrdinalIgnoreCase) || val.Equals("true", StringComparison.OrdinalIgnoreCase)) return true;
            if (val.Equals("0", StringComparison.OrdinalIgnoreCase) || val.Equals("off", StringComparison.OrdinalIgnoreCase) || val.Equals("false", StringComparison.OrdinalIgnoreCase)) return false;
            return defaultValue;
        }

        public void SetState(string key, string value)
        {
            if (string.IsNullOrEmpty(key)) return;

            var entry = _states.Find(s => s.key == key);
            if (entry != null)
            {
                if (entry.value == value) return;
                entry.value = value;
            }
            else
            {
                _states.Add(new _prototype_StateEntry(key, value));
            }

            OnStateChanged?.Invoke(key, value);

            var targetView = _owner;
            if (targetView == null && _prototype_GridManager.Instance != null)
            {
                foreach (var ev in _prototype_GridManager.Instance.GetAllEntityViews())
                {
                    if (ev != null && ev.EntityData?.components != null && ev.EntityData.components.Contains(this))
                    {
                        targetView = ev;
                        _owner = ev;
                        break;
                    }
                }
            }

            if (targetView != null)
            {
                Color textColor = value.Equals("On", StringComparison.OrdinalIgnoreCase) || value.Equals("True", StringComparison.OrdinalIgnoreCase)
                    ? new Color(0.2f, 0.9f, 0.3f)
                    : new Color(0.9f, 0.6f, 0.2f);
                _prototype_FloatingText.SpawnOnEntity(targetView, $"{key}: {value}!", textColor);
            }

            if (_prototype_PlayerUIView.Instance != null)
            {
                _prototype_PlayerUIView.Instance.UpdatePlayerCardDeck();
            }
        }

        public void SetBoolState(string key, bool value)
        {
            SetState(key, value ? "True" : "False");
        }

        public void ToggleBoolState(string key)
        {
            bool current = GetBoolState(key, false);
            SetBoolState(key, !current);
        }

        public override _prototype_EntityComponentData Clone()
        {
            var clone = new _prototype_StateComponentData();
            foreach (var s in _states)
            {
                clone._states.Add(new _prototype_StateEntry(s.key, s.value));
            }
            return clone;
        }
    }
}
