using UnityEngine;

namespace Crowbar.Item
{
    public class FuelBox : ItemObject
    {
        public float fuelValue;

        public int maxValue;
        public int minValue;

        public void SetResource(float value)
        {
            fuelValue = value;
        }

        public override void Initialize()
        {
            fuelValue = Random.Range(minValue, maxValue + 1);
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
