using System;
using System.Collections.Generic;
using UnityEngine;
using Nogue.Gameplay.Events;

namespace Nogue.Presentation.UI
{
    // Editor-facing minimal tray: assigns a buffer of today's items and lets user pick 1/2/3.
    public sealed class TodayTray : MonoBehaviour
    {
        public Action<int>? OnPickIndex; // 0..K-1
        private List<EventCandidate>? _buffer;

        public void SetItems(List<EventCandidate> items)
        {
            _buffer = items;
        }

        void Update()
        {
            if (_buffer == null || _buffer.Count == 0) return;
            if (Input.GetKeyDown(KeyCode.Alpha1)) OnPickIndex?.Invoke(0);
            if (Input.GetKeyDown(KeyCode.Alpha2) && _buffer.Count > 1) OnPickIndex?.Invoke(1);
            if (Input.GetKeyDown(KeyCode.Alpha3) && _buffer.Count > 2) OnPickIndex?.Invoke(2);
        }
    }
}

