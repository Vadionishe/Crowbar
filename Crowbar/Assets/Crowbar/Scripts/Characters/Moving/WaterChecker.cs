using UnityEngine;

namespace Crowbar 
{
    /// <summary>
    /// Component for check collision water
    /// </summary>
    public class WaterChecker : MonoBehaviour, ICollisionChecker
    {
        [Header("Collision properties")]
        [Tooltip("Water tag")]
        public string tagWater = "Water";

        private Character character;
        private bool isWater;

        /// <summary>
        /// Check collision water
        /// </summary>
        /// <returns>Result collision</returns>
        public bool CheckCollision()
        {
            return isWater;
        }

        private void Start()
        {
            character = GetComponentInParent<Character>();
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            Water water = collision.GetComponent<Water>();

            if (water != null)
            {
                if (character.transform.parent == null)
                {
                    if (water.waterParent == null)
                    {
                        isWater = true;
                    }
                }
                else
                {
                    Place characterPlace = character.transform.parent.GetComponent<Place>();

                    if (water.waterParent == null && characterPlace.openAir)
                    {
                        isWater = true;
                    }
                    else if (water.waterParent == characterPlace.transform)
                    {
                        isWater = true;
                    }
                    else
                    {
                        isWater = false;
                    }
                }              
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            Water water = collision.GetComponent<Water>();

            if (water != null)
            {
                if (character.transform.parent == null)
                {
                    if (water.waterParent == null)
                    {
                        isWater = false;
                    }
                }
                else
                {
                    Place characterPlace = character.transform.parent.GetComponent<Place>();

                    if (water.waterParent == null && characterPlace.openAir)
                    {
                        isWater = false;
                    }

                    if (water.waterParent == characterPlace.transform)
                    {
                        isWater = false;
                    }
                }
            }
        }
    }
}
