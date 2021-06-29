using UnityEngine;
using Crowbar.Ship;

namespace Crowbar 
{
    public class Splash : MonoBehaviour
    {
        public ParticleSystem particle;
        public WaterCheckerShip waterChecker;
        public ShipDoor door;
        public Water water;

        public bool isCollideWaterShip;

        private void Update()
        {
            if (waterChecker.CheckCollision() && door.isOpen)
            {
                if (isCollideWaterShip)
                {
                    if (!particle.isStopped)
                        particle.Stop();
                }
                else
                {
                    if (particle.isStopped)
                        particle.Play();
                }
            }
            else
            {
                if (!particle.isStopped)
                    particle.Stop();
            }    
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.GetComponent<Water>() == water)
                isCollideWaterShip = true;
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.GetComponent<Water>() == water)
                isCollideWaterShip = false;
        }
    }
}
