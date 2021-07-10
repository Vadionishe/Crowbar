using UnityEngine;

namespace Crowbar
{
    public class Sky : MonoBehaviour
    {
        public GameObject target;
        public bool canTargetY;
        public float targetZ;

        private void Update()
        {
            Vector3 position = target.transform.position;

            position.z = targetZ;

            if (!canTargetY)
                position.y = transform.position.y;

            transform.position = position;
        }
    }
}
