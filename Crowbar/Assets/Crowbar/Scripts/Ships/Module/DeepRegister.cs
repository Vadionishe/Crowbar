using Crowbar.Server;
using Mirror;
using UnityEngine;
using System;

namespace Crowbar
{
    public class DeepRegister : NetworkBehaviour, IUse, IPickInfo
    {
        public Sprite updateDeepSprite;

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

        public void Use(NetworkIdentity usingCharacter)
        {
            Character character = usingCharacter.GetComponent<Character>();
            CharacterStatistic statistic = usingCharacter.GetComponent<CharacterStatistic>();
            int deepMeter = Convert.ToInt32(statistic.maxDeep);

            Register(character, deepMeter);
        }

        [TargetRpc]
        private void TargetNotificationUpdateRecord(NetworkConnection connection, int deep)
        {
            string textNotification = $"New depth record - {deep} m";

            GameUI.gameUI.ShowNotification(updateDeepSprite, textNotification, 3f);
        }

        [Server]
        private void Register(Character character, int deep)
        {
            string maxDeepString = SQLiteDB.ExecuteRequestWithAnswer($"SELECT MaxDeep FROM Accounts WHERE CharacterName = '{character.nameCharacter}';");

            if (deep > int.Parse(maxDeepString))
            {
                SQLiteDB.ExecuteRequestWithoutAnswer($"UPDATE Accounts SET MaxDeep = {deep} WHERE CharacterName = '{character.nameCharacter}';");
                TargetNotificationUpdateRecord(character.netIdentity.connectionToClient, deep);
            }

            if (deep > 1000)
                AchivmentsHandler.AddProgress(character, 1);

            if (deep > 3000)
                AchivmentsHandler.AddProgress(character, 2);

            if (deep > 10000)
                AchivmentsHandler.AddProgress(character, 3);
        }
    }
}
