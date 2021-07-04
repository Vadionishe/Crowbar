using UnityEngine;

namespace Crowbar.Item
{
    public class FuelBox : ItemObject
    {
        public float fuelValue;

        public int maxValue;
        public int minValue;

        private void Start()
        {
            if (isServer)
                fuelValue = Random.Range(minValue, maxValue + 1);
        }
    }
}
