using System.Linq;
using UnityEngine;

namespace Climate
{
    public class ShipItemBarometer : ShipItem
    {
        [SerializeField]
        private Transform needle;

        private float minAngle;
        private float maxAngle;
        private float smoothedAngle;
        private float smoothingK;

        public override void OnLoad()
        {
            needle = gameObject.GetComponentsInChildren<Transform>(true).Where(t => t.name == "Needle").FirstOrDefault();
            minAngle = -118f;
            maxAngle = 240f;
            smoothingK = -6f;
        }

        public override void ExtraLateUpdate()
        {
            if (needle == null)
                return;

            var targetAngle = Mathf.Lerp(minAngle, maxAngle, PressureService.GetNormalizedPressure());
            smoothedAngle = Mathf.Lerp(smoothedAngle, targetAngle, 1f - Mathf.Exp(smoothingK * Time.deltaTime));
            needle.localRotation = Quaternion.Euler(smoothedAngle, -90f, 90f);
        }
    }
}
