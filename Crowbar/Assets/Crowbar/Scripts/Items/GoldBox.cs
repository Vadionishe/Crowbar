using UnityEngine;

namespace Crowbar.Item
{
    public class GoldBox : ItemObject
    {
        public int goldValue;

        public int maxValue;
        public int minValue;

        public void SetResource(int value)
        {
            goldValue = value;
        }

        public override void Initialize()
        {
            goldValue = Random.Range(minValue, maxValue + 1);
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

        private void Update()
        {
            if (isServer)
                Fall();
        }
    }
}
