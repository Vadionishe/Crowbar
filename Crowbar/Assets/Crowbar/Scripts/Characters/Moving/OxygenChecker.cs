using UnityEngine;

namespace Crowbar
{
    public class OxygenChecker : MonoBehaviour, ICollisionChecker
    {
        public float valueInhale = 1f;

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
            character = GetComponentInParent<Character>();
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            SetPlace(collision, true);
            SetWater(collision, true);
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            SetPlace(collision, false);
            SetWater(collision, false);
        }

        private void SetPlace(Collider2D collision, bool enter)
        {
            Place place = collision.GetComponent<Place>();

            if (place != null)
                this.place = (enter) ? place : null;
        }

        private void SetWater(Collider2D collision, bool enter)
        {
            Water water = collision.GetComponent<Water>();

            if (water != null)
            {
                if (place != null)
                {
                    if (place.water == water)
                        inWater = enter;
                }
                else
                {
                    inWater = enter;
                }
            }
        }           
    }
}
