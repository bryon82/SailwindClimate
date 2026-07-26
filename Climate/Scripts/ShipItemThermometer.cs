using System.Linq;
using UnityEngine;

namespace Climate
{
    public class ShipItemThermometer : ShipItem
    {
        private readonly float _minAngle = -45f;
        private readonly float _maxAngle = 225f;        
        private readonly float _smoothingK = -2f;        
        private readonly float _sampleInterval = 1f;

        private Transform _needle;
        private float _sampleTimer;
        private float _smoothedAngle;
        private float _temperature;

        public override void OnLoad()
        {
            _needle = gameObject.GetComponentsInChildren<Transform>(true).Where(t => t.name == "Needle").FirstOrDefault();

            SampleTemp();
        }

        public override void ExtraLateUpdate()
        {
            _sampleTimer += Time.deltaTime;
            if (_sampleTimer >= _sampleInterval)
            {
                _sampleTimer = 0f;
                SampleTemp();
            }

            UpdateNeedle();
        }

        private void SampleTemp()
        {
            var coords = FloatingOriginManager.instance.GetGlobeCoords(transform);
            _temperature = TemperatureService.GetNormalizedTemperature(coords, Sun.sun.localTime, GameState.day);
        }

        private void UpdateNeedle()
        {
            if (_needle == null)
                return;

            var targetAngle = Mathf.Lerp(_minAngle, _maxAngle, _temperature);
            _smoothedAngle = Mathf.Lerp(_smoothedAngle, targetAngle, 1f - Mathf.Exp(_smoothingK * Time.deltaTime));
            _needle.localRotation = Quaternion.Euler(_smoothedAngle, -90f, 90f);
        }
    }
}
