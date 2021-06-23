using UnityEngine;

namespace Crowbar.Ship
{
    public class WiggleShip : MonoBehaviour
    {
        public float maxSpeed;
        public float minSpeed;     
        public float speedUp;

        private float curSpeed;
        private float speed;

        private void ResideRotate()
        {
            curSpeed = Random.Range(minSpeed, maxSpeed);

            if (speed < 0)
                speed = -curSpeed;

            if (speed > 0)
                speed = curSpeed;

            speedUp *= -1;
        }

        private void Start()
        {
            curSpeed = Random.Range(minSpeed, maxSpeed);
            speed = curSpeed;
        }

        private void FixedUpdate()
        {
            speed = Mathf.Clamp(speed + speedUp, -curSpeed, curSpeed);

            if (speed >= curSpeed || speed <= -curSpeed)
                ResideRotate();

            transform.Rotate(0, 0, speed);
        }
    }
}
