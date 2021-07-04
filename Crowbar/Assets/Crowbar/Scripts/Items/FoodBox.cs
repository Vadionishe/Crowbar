using UnityEngine;

namespace Crowbar.Item
{
    public class FoodBox : ItemObject
    {
        public float foodValue;

        public int maxValue;
        public int minValue;

        private void Start()
        {
            if (isServer)
                foodValue = Random.Range(minValue, maxValue + 1);
        }
    }
}
