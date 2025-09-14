using System;
using System.Collections.Generic;
using UnityEngine;
using Nogue.Gameplay.Director;

namespace Nogue.Presentation.UI
{
    // Editor-facing minimal tray: assigns a buffer of today's items and lets user pick 1/2/3.
    public sealed class TodayTray : MonoBehaviour
    {
        public Action<int>? OnPickIndex; // 0..K-1
        private List<IEventCandidate>? _buffer;
        private List<string>? _reasons;

        public void SetItems(List<IEventCandidate> items, List<string>? reasons = null)
        {
            _buffer = items;
            _reasons = reasons;
        }

        void Update()
        {
            if (_buffer == null || _buffer.Count == 0) return;
            if (Input.GetKeyDown(KeyCode.Alpha1)) OnPickIndex?.Invoke(0);
            if (Input.GetKeyDown(KeyCode.Alpha2) && _buffer.Count > 1) OnPickIndex?.Invoke(1);
            if (Input.GetKeyDown(KeyCode.Alpha3) && _buffer.Count > 2) OnPickIndex?.Invoke(2);
        }

#if UNITY_EDITOR
        private const float Width = 560f;
        private Vector2 _scroll;
        void OnGUI()
        {
            if (_buffer == null || _buffer.Count == 0) return;
            GUILayout.BeginArea(new Rect(10, 10, Width, 200), GUI.skin.window);
            GUILayout.Label("Today's picks (Editor Debug)");
            _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(160));
            for (int i = 0; i < _buffer.Count; i++)
            {
                var c = _buffer[i];
                string title = $"{i + 1}. {c.Id} [{c.Type}]" + (c.IsContractCritical ? " [CRITICAL]" : "");
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(title, GUILayout.Width(320))) OnPickIndex?.Invoke(i);
                string reason = (_reasons != null && i < _reasons.Count) ? _reasons[i] : string.Empty;
                GUILayout.Label(reason, GUILayout.Width(Width - 330));
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
#endif
    }
}
