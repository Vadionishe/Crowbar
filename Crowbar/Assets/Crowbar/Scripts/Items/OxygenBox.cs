using UnityEngine;

namespace Crowbar.Item
{
    public class OxygenBox : ItemObject
    {
        public float oxygenValue;

        public float maxValue;
        public float minValue;

        public void SetResource(float value)
        {
            oxygenValue = value;
        }

        public override void Initialize()
        {
            oxygenValue = Random.Range(minValue, maxValue + 1);
        }

        private void Start()
        {
            if (isServer)
            {
                InvokeRepeating(nameof(CheckToDestroy), 30f, 30f);
            }
            else
            {
                GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
                colliderItem.isTrigger = true;
            }
        }
    }
}
