using UnityEngine;

namespace Crowbar.Item
{
    public class BulletBox : ItemObject
    {
        public int bulletValue;

        public int maxValue;
        public int minValue;

        private void Start()
        {
            if (isServer)
            {
                bulletValue = Random.Range(minValue, maxValue + 1);

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
