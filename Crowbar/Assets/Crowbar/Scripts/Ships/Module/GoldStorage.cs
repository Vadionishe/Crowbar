using UnityEngine;
using Mirror;
using Crowbar.Item;

namespace Crowbar.Ship
{
    public class GoldStorage : NetworkBehaviour, IShipModule, IPickInfo
    {
        public TextMesh textMesh;

        [SyncVar]
        public int maxGold;
        [SyncVar(hook = nameof(SyncValue))]
        public int gold;
        public int grabGold = 100;

        public GoldBox goldPrefab;

        public Color PickColor = Color.green;

        private Color m_colorMain;

        public void Pick()
        {
            m_colorMain = GetComponent<SpriteRenderer>().color;
            GetComponent<SpriteRenderer>().color = PickColor;
        }

        public void UnPick()
        {
            GetComponent<SpriteRenderer>().color = m_colorMain;
        }

        [Server]
        public void ChangeGold(int value)
        {
            gold = Mathf.Clamp(gold + value, 0, maxGold);
        }

        public virtual void Use(NetworkIdentity usingCharacter)
        {
            Character character = usingCharacter.GetComponent<Character>();
            ItemObject itemObject = character.hand.itemObject;

            if (itemObject != null)
            {
                if (itemObject as GoldBox)
                {
                    ChangeGold((itemObject as GoldBox).goldValue);
                    itemObject.Drop(usingCharacter);
                    NetworkServer.Destroy(itemObject.gameObject);
                }
            }
            else
            {
                if (gold > 0)
                {
                    GoldBox goldItem = Instantiate(goldPrefab, transform.position, Quaternion.identity, null);

                    NetworkServer.Spawn(goldItem.gameObject);
                    goldItem.Use(usingCharacter);

                    if (gold >= grabGold)
                    {
                        goldItem.goldValue = grabGold;
                        gold -= grabGold;
                    }
                    else
                    {
                        goldItem.goldValue = gold;
                        gold = 0;
                    }
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
                SyncValue(0, gold);
        }
    }
}
