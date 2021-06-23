using UnityEngine;

namespace Crowbar.Ship
{
    public class MotorRotate : MonoBehaviour
    {
        public enum Side : int 
        { 
            Up = 0,
            Down = 1,
            Right = 2,
            Left = 3
        }

        public bool isRotate;
        public Side side;

        public float speedX = 50f;
        public float speedY = 50f;
        public float speedZ = 50f;

        private Vector3 standRotate;

        public void SetStandRotate()
        {
            transform.localEulerAngles = standRotate;
        }

        private void Start()
        {
            standRotate = transform.localEulerAngles;
        }

        private void Update()
        {
            if (isRotate)
                transform.Rotate(new Vector3(speedX, speedY, speedZ));
        }
    }
}
