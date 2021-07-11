using UnityEngine;

namespace Crowbar.Ship
{
    public class MotorEffect : MonoBehaviour
    {
        public bool isWater;

        public ParticleSystem particle;
        public MotorRotate motorRotate;

        private void Update()
        {
            if (motorRotate.isRotate && isWater)
                if (!particle.isPlaying)
                    particle.Play();

            if (!motorRotate.isRotate || !isWater)
                if (particle.isPlaying)
                    particle.Stop();
        }

        private void OnTriggerStay2D(Collider2D collision)
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
