using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Basics
{
    public class FrameRateCounter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI display;

        public enum EDisplayMode { FPS, MS }
        [SerializeField] private EDisplayMode displayMode = EDisplayMode.FPS;

        [SerializeField, Range(0.1f, 2f)] private float _sampleDuration = 1f;

        private int _frames;
        private float _duration, _bestDuration = float.MaxValue, _worstDuration;

        private void Update()
        {
            float frameDuration = Time.unscaledDeltaTime;

            _frames += 1;
            _duration += frameDuration;

            if (frameDuration < _bestDuration)
            {
                _bestDuration = frameDuration;
            }
            if (frameDuration > _worstDuration)
            {
                _worstDuration = frameDuration;
            }

            if (_duration >= _sampleDuration)
            {
                if (displayMode == EDisplayMode.FPS)
                {
                    display.SetText(
                        "FPS\n{0:0}\n{1:0}\n{2:0}",
                        1f / _bestDuration,
                        _frames / _duration,
                        1f / _worstDuration
                        );
                }
                else
                {
                    display.SetText(
                        "MS\n{0:1}\n{1:1}\n{2:1}",
                        1000f * _bestDuration,
                        1000f * _duration / _frames,
                        1000f * _worstDuration
                        );
                }
                _frames = 0;
                _duration = 0;

                _bestDuration = float.MaxValue;
                _worstDuration = 0f;
            }

        }
    }

}