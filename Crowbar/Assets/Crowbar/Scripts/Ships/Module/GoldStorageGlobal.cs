using Crowbar.Ship;
using Crowbar.Item;
using Crowbar.Server;
using Mirror;
using System.Linq;

namespace Crowbar
{
    public class GoldStorageGlobal : GoldStorage
    {
        public override void Use(NetworkIdentity usingCharacter)
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

                    AddGoldToAccounts((itemObject as GoldBox).goldValue);
                }
            }            
        }

        [Server]
        private void AddGoldToAccounts(int gold)
        {
            int countCharacters = FindObjectsOfType<Character>().Where(c => !c.GetComponent<CharacterStats>().isDied).Count();

            foreach (Character character in FindObjectsOfType<Character>())
            {
                if (!character.GetComponent<CharacterStats>().isDied)
                {
                    int goldCharacter = gold / countCharacters;

                    SQLiteDB.ExecuteRequestWithoutAnswer($"UPDATE Accounts SET Gold = Gold + {goldCharacter} WHERE CharacterName = '{character.nameCharacter}';");
                }
            }
        } 
    }
}
