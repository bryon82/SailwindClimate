using System.Linq;
using UnityEngine;

namespace Climate
{
    public class ShipItemThermometer : ShipItem
    {
        private Transform needle;
        
        private float minAngle;
        private float maxAngle;
        private float smoothingK;
        private float sampleInterval;

        private float temperature;
        private float smoothedAngle;
        private float sampleTimer;

        public override void OnLoad()
        {
            needle = gameObject.GetComponentsInChildren<Transform>(true).Where(t => t.name == "Needle").FirstOrDefault();
            minAngle = -45f;
            maxAngle = 225f;
            smoothingK = -2f;
            sampleInterval = 1f;

            SampleTemp();
        }

        public override void ExtraLateUpdate()
        {
            sampleTimer += Time.deltaTime;
            if (sampleTimer >= sampleInterval)
            {
                sampleTimer = 0f;
                SampleTemp();
            }

            UpdateNeedle();
        }

        private void SampleTemp()
        {
            var coords = FloatingOriginManager.instance.GetGlobeCoords(transform);
            temperature = TemperatureService.GetNormalizedTemperature(coords);
        }

        private void UpdateNeedle()
        {
            if (needle == null)
                return;

            var targetAngle = Mathf.Lerp(minAngle, maxAngle, temperature);
            smoothedAngle = Mathf.Lerp(smoothedAngle, targetAngle, 1f - Mathf.Exp(smoothingK * Time.deltaTime));
            needle.localRotation = Quaternion.Euler(smoothedAngle, -90f, 90f);
        }
    }
}
