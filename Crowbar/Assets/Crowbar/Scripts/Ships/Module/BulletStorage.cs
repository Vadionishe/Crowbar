using UnityEngine;
using Mirror;
using Crowbar.Item;

namespace Crowbar.Ship
{
    public class BulletStorage : NetworkBehaviour, IShipModule, IPickInfo
    {
        public TextMesh textMesh;

        [SyncVar]
        public int maxBullet;
        [SyncVar(hook = nameof(SyncValue))]
        public int bullet;

        public Bullet bulletPrefab;

        public Color PickColor = Color.green;

        private Color m_colorMain = Color.white;

        public void Pick()
        {
            if (GetComponent<SpriteRenderer>())
                GetComponent<SpriteRenderer>().color = PickColor;
        }

        public void UnPick()
        {
            if (GetComponent<SpriteRenderer>())
                GetComponent<SpriteRenderer>().color = m_colorMain;
        }

        [Server]
        public void ChangeBullet(int value)
        {
            bullet = Mathf.Clamp(bullet + value, 0, maxBullet);
        }

        public void Use(NetworkIdentity usingCharacter)
        {
            Character character = usingCharacter.GetComponent<Character>();
            ItemObject itemObject = character.hand.itemObject;

            if (itemObject != null)
            {
                if (bullet < maxBullet)
                {
                    if (itemObject as BulletBox)
                    {
                        ChangeBullet((itemObject as BulletBox).bulletValue);
                        itemObject.Drop(usingCharacter, 0, Vector2.zero);
                        NetworkServer.Destroy(itemObject.gameObject);
                    }
                    else if (itemObject as Bullet)
                    {
                        ChangeBullet(1);
                        itemObject.Drop(usingCharacter, 0, Vector2.zero);
                        NetworkServer.Destroy(itemObject.gameObject);
                    }
                }
            }
            else
            {
                if (bullet > 0)
                {
                    Bullet bulletItem = Instantiate(bulletPrefab, transform.position, Quaternion.identity, null);

                    NetworkServer.Spawn(bulletItem.gameObject);
                    bulletItem.Use(usingCharacter);

                    bullet--;
                }            
            }
        }

        [Client]
        private void SyncValue(int oldValue, int newValue)
        {
            textMesh.text = $"{newValue.ToString("0")}";
        }

        private void Start()
        {
            if (!isServer)
                SyncValue(0, bullet);
        }
    }
}
