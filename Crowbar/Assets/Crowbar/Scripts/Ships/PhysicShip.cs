using Mirror;
using UnityEngine;

namespace Crowbar.Ship
{
    public class PhysicShip : MonoBehaviour
    {
        public UnderwaterShip underwaterShip;
        public CheckWaterShip waterChecker;

        public float maxSpeed = 5f;
        public float speedUp = 0.2f;
        public float gravity = 10f;
        public float gravityWater = 0.1f;
        public float electricDown = 0.01f;
        public float fuelDown = 0.05f;

        public ElectricStorage electricStorage;
        public FuelStorage fuelStorage;

        private Vector2 directionMove;

        private Rigidbody2D m_rigidBody;

        public void Move(Vector2 direction)
        {
            if (electricStorage.electric >= electricDown && fuelStorage.fuel >= fuelDown)
            {
                if (m_rigidBody.velocity.magnitude < maxSpeed)
                {
                    electricStorage.ChangeElectric(-electricDown);
                    fuelStorage.ChangeFuel(-fuelDown);
                    m_rigidBody.AddForce(direction * speedUp);
                }

                directionMove = direction;
            }            
        }

        private void Start()
        {
            m_rigidBody = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (!NetworkServer.active)
                return;

            underwaterShip.SetMotorStateServer(MotorRotate.Side.Left, directionMove.x > 0);
            underwaterShip.SetMotorStateServer(MotorRotate.Side.Right, directionMove.x < 0);
            underwaterShip.SetMotorStateServer(MotorRotate.Side.Down, directionMove.y > 0);
            underwaterShip.SetMotorStateServer(MotorRotate.Side.Up, directionMove.y < 0);

            m_rigidBody.gravityScale = (waterChecker.CheckCollision()) ? gravityWater : gravity;
        }
    }
}

