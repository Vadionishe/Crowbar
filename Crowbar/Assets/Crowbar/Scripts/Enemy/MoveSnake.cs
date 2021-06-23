using UnityEngine;

namespace Crowbar.Enemy
{
    public class MoveSnake : MoveEnemy
    {
        public WaterSnake waterSnake;

        private void Update()
        {
            if (!waterSnake.isDied && isServer)
            {
                transform.position += transform.right * speed * Time.deltaTime;
            }
        }
    }
}
