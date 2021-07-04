using System.Collections.Generic;
using System.Linq;
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

        private Water currentWater;
        private List<Water> waters;
        private List<Water> collisionWaters;

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
            collisionWaters = new List<Water>();

            character = GetComponentInParent<Character>();
            waters = FindObjectsOfType<Water>().ToList();
        }

        private void Update()
        {
            if (character.transform.parent == null)
            {
                currentWater = waters.Find(w => w.waterParent == null);
            }
            else
            {
                Place place = character.transform.parent.GetComponent<Place>();

                if (place.openAir)
                {
                    currentWater = waters.Find(w => w.waterParent == null);
                }
                else
                {
                    currentWater = place.water;
                }
            }

            foreach (Water water in collisionWaters)
            {
                if (water == currentWater)
                {
                    isWater = true;

                    return;
                }
            }

            isWater = false;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            Water water = collision.GetComponent<Water>();

            if (water != null)
                if (!collisionWaters.Contains(water))
                    collisionWaters.Add(water);
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            Water water = collision.GetComponent<Water>();

            if (water != null)
                if (collisionWaters.Contains(water))
                    collisionWaters.Remove(water);
        }
    }
}
