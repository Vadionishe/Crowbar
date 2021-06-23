using UnityEngine;

namespace Crowbar.Ship
{
    public class CheckWaterShip : MonoBehaviour, ICollisionChecker
    {
        private bool isWater;

        public bool CheckCollision()
        {
            return isWater;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            Water water = collision.GetComponent<Water>();

            if (water != null)
                if (water.waterParent == null)
                    isWater = true;
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            Water water = collision.GetComponent<Water>();

            if (water != null)
                if (water.waterParent == null)
                    isWater = false;
        }
    }
}
