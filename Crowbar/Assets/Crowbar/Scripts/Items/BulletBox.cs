using UnityEngine;

namespace Crowbar.Item
{
    public class BulletBox : ItemObject
    {
        public int bulletValue;

        public int maxValue;
        public int minValue;

        public void SetResource(int value)
        {
            bulletValue = value;
        }

        public override void Initialize()
        {
            bulletValue = Random.Range(minValue, maxValue + 1);
        }

        private void Start()
        {
            if (isServer)
            {
                InvokeRepeating(nameof(CheckToDestroy), 30f, 30f);
                InvokeRepeating(nameof(CheckToSleep), 2f, 2f);
            }
            else
            {
                GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
                colliderItem.isTrigger = true;
            }
        }
    }
}
