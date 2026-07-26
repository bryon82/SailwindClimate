using System.Linq;
using UnityEngine;

namespace Climate
{
    public class ShipItemHygrometer : ShipItem
    {
        private readonly float _minAngle = -45f;
        private readonly float _maxAngle = 225f;        
        private readonly float _smoothingK = -2f;        
        private readonly float _sampleInterval = 1f;

        private Transform _needle;
        private float _sampleTimer;
        private float _smoothedAngle;
        private float _humidity;

        public override void OnLoad()
        {
            _needle = gameObject.GetComponentsInChildren<Transform>(true).Where(t => t.name == "Needle").FirstOrDefault();

            SampleHumidity();
        }

        public override void ExtraLateUpdate()
        {
            _sampleTimer += Time.deltaTime;
            if (_sampleTimer >= _sampleInterval)
            {
                _sampleTimer = 0f;
                SampleHumidity();
            }

            UpdateNeedle();
        }

        private void SampleHumidity()
        {
            var coords = FloatingOriginManager.instance.GetGlobeCoords(transform);
            _humidity = HumidityService.GetRelativeHumidity(coords, Sun.sun.localTime, GameState.day);
        }

        private void UpdateNeedle()
        {
            if (_needle == null)
                return;

            var targetAngle = Mathf.Lerp(_minAngle, _maxAngle, _humidity);
            _smoothedAngle = Mathf.Lerp(_smoothedAngle, targetAngle, 1f - Mathf.Exp(_smoothingK * Time.deltaTime));
            _needle.localRotation = Quaternion.Euler(_smoothedAngle, -90f, 90f);
        }
    }
}
