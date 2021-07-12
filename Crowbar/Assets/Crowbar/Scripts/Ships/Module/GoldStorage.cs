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

        private Color PickColor = Color.green;
        private Color m_colorMain = Color.white;

        public void Pick()
        {
            if (GetComponent<SpriteRenderer>() != null)
                GetComponent<SpriteRenderer>().color = PickColor;

            foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>())
                if (renderer != null)
                    renderer.color = PickColor;
        }

        public void UnPick()
        {
            if (GetComponent<SpriteRenderer>() != null)
                GetComponent<SpriteRenderer>().color = m_colorMain;

            foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>())
                if (renderer != null)
                    renderer.color = m_colorMain;
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
                    itemObject.Drop(usingCharacter, 0, Vector2.zero, Vector2.zero);
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
                        goldItem.SetResource(grabGold);
                        gold -= grabGold;
                    }
                    else
                    {
                        goldItem.SetResource(gold);
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
