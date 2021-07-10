using UnityEngine;

namespace Crowbar.Item
{
    public class FoodBox : ItemObject
    {
        public float foodValue;

        public int maxValue;
        public int minValue;

        public void SetResource(float value)
        {
            foodValue = value;
        }

        public override void Initialize()
        {
            foodValue = Random.Range(minValue, maxValue + 1);
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
