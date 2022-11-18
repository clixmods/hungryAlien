using System;
using UnityEngine;

namespace Player
{
    public class AbsorberColor : MonoBehaviour
    {
        private float _colorLerpMultiplier = 5;
        private Light _light;
        public Color color;

        private void Start()
        {
            _light = GetComponent<Light>();
        }

        private void Update()
        {
            _light.color =  Color.Lerp(_light.color, color, _colorLerpMultiplier * Time.deltaTime);
        }
    }
}