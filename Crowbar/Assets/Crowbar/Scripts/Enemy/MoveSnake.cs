using UnityEngine;

namespace Crowbar.Enemy
{
    public class MoveSnake : MoveEnemy
    {
        public WaterSnake waterSnake;
        public AudioSource audioSource;

        private void Start()
        {
            audioSource.volume = Settings.volume;

            audioSource.Play();
        }

        private void Update()
        {
            if (!waterSnake.isDied && isServer)
            {
                transform.position += transform.right * speed * Time.deltaTime;
            }
        }
    }
}
