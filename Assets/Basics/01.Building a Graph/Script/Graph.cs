using System;
using UnityEngine;

namespace BuildingAGraph
{
    public class Graph : MonoBehaviour
    {
        [SerializeField] private Transform _pointPrefab;
        [Range(10, 100)]
        [SerializeField] private int _resolution = 20;

        [SerializeField]
        private FunctionLibrary.EFunctionName _function;

        private Transform[] _points;

        private void Awake()
        {
            float step = 2f / _resolution;
            var position = Vector3.zero;
            var scale = Vector3.one * step;
            _points = new Transform[_resolution];

            for (int i = 0; i < _points.Length; i++)
            {
                Transform point = Instantiate(_pointPrefab);
                _points[i] = point;
                position.x = (i + 0.5f) * step - 1f;

                // position.y = position.x * position.x * position.x;

                point.localPosition = position;
                point.localScale = scale;

                point.SetParent(transform, false);
            }
        }

        private void Update()
        {
            FunctionLibrary.Function f = FunctionLibrary.GetFunction(_function);
            float time = Time.time;
            for (int i = 0; i < _points.Length; i++)
            {
                Transform point = _points[i];
                Vector3 position = point.localPosition;
                /*if (_function == 0)
                {
                    position.y = FunctionLibrary.Wave(position.x, time);
                }
                else if(_function == 1) 
                {
                    position.y = FunctionLibrary.MultiWave(position.x, time);
                }
                else
                {
                    position.y = FunctionLibrary.Ripple(position.x, time);
                }*/

                position.y = f(position.x, position.z, time);
                point.localPosition = position;
            }
        }
    }
}

