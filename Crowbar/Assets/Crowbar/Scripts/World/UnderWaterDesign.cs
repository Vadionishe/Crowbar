using UnityEngine;

namespace Crowbar
{
    public class UnderWaterDesign : MonoBehaviour
    {
        public float maxY = -70f;

        private void Update()
        {
            if (GameUI.localStats != null)
            {
                Vector3 targetPosition = GameUI.localStats.transform.position;

                targetPosition.z = 0;

                if (targetPosition.y > maxY)
                    targetPosition.y = maxY;

                transform.position = targetPosition;
            }
        }
    }
}
