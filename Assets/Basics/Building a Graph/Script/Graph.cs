using System;
using UnityEngine;

namespace BuildingAGraph
{
    public class Graph : MonoBehaviour
    {
        [SerializeField] private Transform _pointPrefab;
        [Range(10, 100)]
        [SerializeField] private int _resolution = 20;

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
            for (int i = 0; i < _points.Length; i++)
            {
                Transform point = _points[i];
                Vector3 position = point.localPosition;

                position.y = Mathf.Sin((Mathf.PI * (position.x + Time.time)));
                point.localPosition = position;
            }
        }
    }
}

