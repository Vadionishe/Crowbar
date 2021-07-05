using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Crowbar
{
    public class OxygenChecker : MonoBehaviour, ICollisionChecker
    {
        public float valueInhale = 1f;

        private Water currentWater;
        private List<Water> waters;
        private List<Water> collisionWaters;

        public Place place;
        public Character character;

        public bool inWater;

        public bool Inhale()
        {
            if (inWater)
                return false;

            if (place != null)
            {
                if (!place.openAir)
                {
                    if (place.oxygen.oxygen > 0)
                    {
                        place.oxygen.ChangeOxygen(-valueInhale);

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }

            return true;
        }

        /// <summary>
        /// Check collision water
        /// </summary>
        /// <returns>Result collision</returns>
        public bool CheckCollision()
        {
            return inWater;
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
                place = null;
                currentWater = waters.Find(w => w.waterParent == null);
            }
            else
            {
                place = character.transform.parent.GetComponent<Place>();

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
                    inWater = true;

                    return;
                }
            }

            inWater = false;
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
