using Mirror;
using System.Collections;
using UnityEngine;

namespace Crowbar.Item
{
    public abstract class ItemObject : WorldObject, IUse, IPickInfo
    {      
        public int damage;
        public float cooldownAttack;
        public float handedAngle;
        public bool handedForceAngle;

        public bool isAttacking;
        public bool onCooldown;

        public NetworkIdentity handedCharacter;

        public SpriteRenderer rendererItem;
        public Collider2D colliderItem;        

        public Color PickColor = Color.green;

        private Color m_colorMain;

        public void Pick()
        {
            if (rendererItem != null)
            {
                m_colorMain = rendererItem.color;
                rendererItem.color = PickColor;
            }
        }

        public void UnPick()
        {
            if (rendererItem != null)
                rendererItem.color = m_colorMain;
        }

        [ClientRpc]
        public void RpcGrabItem(NetworkIdentity usingCharacter)
        {
            Character character = usingCharacter.GetComponent<Character>();
            SyncPosition syncPosition = GetComponent<SyncPosition>();

            transform.parent = character.hand.handParentPoint;
            transform.localPosition = Vector3.zero;
            syncPosition.syncPosition = false;
            syncPosition.syncRotation = false;
            character.hand.itemObject = this;
            handedCharacter = usingCharacter;

            if (handedForceAngle)
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                transform.localEulerAngles = new Vector3(0, 0, handedAngle);
            }
        }

        [ClientRpc]
        public void RpcDropItem(NetworkIdentity usingCharacter)
        {
            if (usingCharacter != null)
            {
                Character character = usingCharacter.GetComponent<Character>();

                character.hand.itemObject = null;
            }
            
            SyncPosition syncPosition = GetComponent<SyncPosition>();

            syncPosition.syncPosition = true;
            syncPosition.syncRotation = true;
            transform.parent = null;         
            handedCharacter = null;
        }

        [Server]
        public virtual void Drop(NetworkIdentity usingCharacter)
        {
            Character character = usingCharacter.GetComponent<Character>();

            canParenting = true;
            transform.parent = character.transform.parent;
            character.hand.itemObject = null;
            handedCharacter = null;
            colliderItem.isTrigger = false;
            GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;

            RpcDropItem(usingCharacter);
        }

        [Server]
        public virtual void Grab(NetworkIdentity usingCharacter)
        {
            Character character = usingCharacter.GetComponent<Character>();

            if (character.hand.itemObject == null && !character.isBusy) 
            {
                canParenting = false;
                transform.parent = character.hand.handParentPoint;
                transform.localPosition = Vector3.zero;
                character.hand.itemObject = this;
                handedCharacter = usingCharacter;
                colliderItem.isTrigger = true;
                GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;

                if (handedForceAngle)
                {
                    transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                    transform.localEulerAngles = new Vector3(0, 0, handedAngle);
                }

                RpcGrabItem(usingCharacter);
            }
        }

        [Server]
        public void Use(NetworkIdentity usingCharacter)
        {
            if (handedCharacter == null)
                Grab(usingCharacter);
        }

        public void Attack()
        {
            StartCoroutine(Cooldown());
        }

        protected void CheckToDestroy()
        {
            Collider2D[] players = Physics2D.OverlapCircleAll(transform.position, 400f, LayerMask.GetMask("Player"));

            if (players.Length == 0)
                NetworkServer.Destroy(gameObject);
        }

        private IEnumerator Cooldown()
        {
            onCooldown = true;

            yield return new WaitForSeconds(cooldownAttack);

            onCooldown = false;
        }
    }
}
