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

        public AudioSource audioSource;

        public bool isCollideWaterShip;

        private void Start()
        {
            audioSource.volume = Settings.volume;
        }

        private void Update()
        {
            if (waterChecker.CheckCollision() && door.isOpen)
            {
                if (isCollideWaterShip)
                {
                    if (!particle.isStopped)
                    {
                        particle.Stop();
                        audioSource.Stop();
                    }
                }
                else
                {
                    if (particle.isStopped)
                    {
                        particle.Play();
                        audioSource.Play();
                    }
                }
            }
            else
            {
                if (!particle.isStopped)
                {
                    particle.Stop();
                    audioSource.Stop();
                }
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
