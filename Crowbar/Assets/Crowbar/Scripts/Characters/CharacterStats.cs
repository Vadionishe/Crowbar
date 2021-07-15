using UnityEngine;
using Mirror;
using UnityEngine.Events;
using Crowbar.Item;

namespace Crowbar
{
    public class CharacterStats : NetworkBehaviour, ICharacterComponent
    {
        [Tooltip("Enable on server")]
        public bool enableOnServer = true;

        public bool isDied;

        public float maxHealth;
        public float health;
        public float maxOxygen;
        public float oxygen;
        public float maxFood;
        public float food;

        public float timeChangeStats = 1f;

        public float valueFoodDown = 0.05f;
        public float valueOxygenDown = 5f;
        public float valueOxygenUp = 20f;
        public float valueHealthUp = 0.1f;
        public float valueHealthDown = 0.5f;

        public OxygenChecker oxygenChecker;
        public MoveComponent moveComponent;
        public JumpComponent jumpComponent;
        public UsingComponent usingComponent;
        public Character character;

        public ParticleSystem particleBlood;
        public AudioSource audioSource;
        public AudioClip damageSound;
        public AudioClip needOxygenSound;

        public class DiedEvent : UnityEvent<bool> { }
        public DiedEvent onDied;

        [Command]
        public void CmdDied(bool isDied)
        {
            ChangeDied(isDied);
        }

        public void SetComponentActive(bool isActive)
        {
            enabled = isActive;
        }

        public bool IsEnableComponentOnServer()
        {
            return enableOnServer;
        }

        [Server]
        public void ChangeDied(bool died)
        {
            isDied = died;

            moveComponent.canMove = !died;
            jumpComponent.canJump = !died;
            usingComponent.enabled = !died;
            character.handController.enabled = !died;

            if (character.hand.itemObject != null)
                character.hand.itemObject.Drop(netIdentity, 0, Vector2.zero, transform.localPosition);

            TargetDied(netIdentity.connectionToClient, isDied);
            onDied.Invoke(died);
        }

        [Server]
        public void ChangeFood(float value)
        {
            food = Mathf.Clamp(food + value, 0, maxFood);

            TargetSetFood(netIdentity.connectionToClient, food);
        }

        [Server]
        public void ChangeOxygen(float value)
        {
            oxygen = Mathf.Clamp(oxygen + value, 0, maxOxygen);

            if (oxygen < 20f && !isDied)
                RpcNeedOxygen();

            TargetSetOxygen(netIdentity.connectionToClient, oxygen);
        }

        [Server]
        public void ChangeHealth(float value)
        {
            health = Mathf.Clamp(health + value, 0, maxHealth);

            if (!isDied && health <= 0)
                ChangeDied(true);

            TargetSetHealth(netIdentity.connectionToClient, health);
        }

        [TargetRpc]
        public void TargetDied(NetworkConnection connection, bool died)
        {
            isDied = died;

            moveComponent.canMove = !died;
            jumpComponent.canJump = !died;
            usingComponent.enabled = !died;
            character.handController.enabled = !died;
            character.avatarController.canFlip = !died;
            character.handController.gameObject.SetActive(!died);

            moveComponent.SetVelocityForced(0, 0);
            moveComponent.animator.SetBool("isDeath", died);

            onDied.Invoke(died);
        }

        [TargetRpc]
        public void TargetSetFood(NetworkConnection connection, float value)
        {
            food = value;
        }

        [TargetRpc]
        public void TargetSetOxygen(NetworkConnection connection, float value)
        {
            oxygen = value;
        }

        [TargetRpc]
        public void TargetSetHealth(NetworkConnection connection, float value)
        {
            health = value;
        }

        [ClientRpc]
        public void RpcDamage()
        {
            audioSource.volume = Settings.volume;
            audioSource.PlayOneShot(damageSound);
            particleBlood.Play();
        }

        [ClientRpc]
        public void RpcNeedOxygen()
        {
            audioSource.volume = Settings.volume;
            audioSource.PlayOneShot(needOxygenSound);
        }

        [Server]
        private void ChangeStats()
        {
            if (!isDied)
            {
                ChangeFood(-valueFoodDown);
                ChangeOxygen((oxygenChecker.Inhale()) ? valueOxygenUp : -valueOxygenDown);
                ChangeHealth((food <= 0 || oxygen <= 0) ? -valueHealthDown : valueHealthUp);
            }
        }

        private void Start()
        {
            onDied = new DiedEvent();

            if (isServer)
                InvokeRepeating(nameof(ChangeStats), timeChangeStats, timeChangeStats);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (isServer) 
            {
                Hammer hammer = collision.GetComponent<Hammer>();
                ShipBullet bullet = collision.GetComponent<ShipBullet>();

                if (hammer != null && hammer.handedCharacter != null)
                {
                    if (hammer.handedCharacter != netIdentity && hammer.isAttacking && !hammer.onCooldown)
                    {
                        if (!isDied)
                        {
                            ChangeHealth(-4f);
                            RpcDamage();
                            hammer.SetCooldown();
                        }
                    }
                }

                if (bullet != null)
                {
                    if (!isDied && transform.parent == null)
                    {
                        ChangeHealth(-1000);
                        RpcDamage();
                        NetworkServer.Destroy(bullet.gameObject);
                    }
                }
            }
        }
    }
}
