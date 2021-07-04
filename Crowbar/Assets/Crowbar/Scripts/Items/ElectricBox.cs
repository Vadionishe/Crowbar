using UnityEngine;

namespace Crowbar.Item
{
    public class ElectricBox : ItemObject
    {
        public float electricValue;

        public int maxValue;
        public int minValue;

        private void Start()
        {
            if (isServer)
                electricValue = Random.Range(minValue, maxValue + 1);
        }
    }
}
