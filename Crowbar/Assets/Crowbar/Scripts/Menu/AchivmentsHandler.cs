using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Crowbar.Server;

namespace Crowbar
{
    public class AchivmentsHandler : NetworkBehaviour
    {
        public static AchivmentsHandler achivmentsHandler;

        [Serializable]
        public class Achivment
        {
            public int id;
            public string name;
            public string description;
            public Sprite icon;
        }

        public List<Achivment> achivments;

        [TargetRpc]
        public void TargetAddAchivment(NetworkConnection connection, int idAchivment)
        {
            Achivment achivment = achivments.Find(a => a.id == idAchivment);

            if (achivment != null)
            {
                if (!Account.idAchivments.Contains(idAchivment))
                {
                    Account.idAchivments.Add(idAchivment);
                    GameUI.gameUI.ShowNotification(achivment.icon, achivment.description, 3f);
                }
            }
        }

        static public void AddProgress(Character character, string achivmentName)
        {
            Achivment achivment = achivmentsHandler.achivments.Find(a => a.name == achivmentName);

            if (achivment != null)
                AddProgress(character, achivment.id);
        }

        static public void AddProgress(Character character, int achivmentId)
        {
            Achivment achivment = achivmentsHandler.achivments.Find(a => a.id == achivmentId);

            if (achivment != null)
            {
                string achivmentsForDB = string.Empty;
                string achivments = SQLiteDB.ExecuteRequestWithAnswer($"SELECT AchivmentsId FROM Accounts WHERE CharacterName = '{character.nameCharacter}';");
                string[] achivmentsIdString = achivments.Split(',');
                List<int> achivmentsId = new List<int>();

                foreach (string id in achivmentsIdString)
                    if (int.TryParse(id, out int _idTry))
                        achivmentsId.Add(_idTry);

                if (!achivmentsId.Contains(achivmentId))
                {
                    achivments += $"{achivmentId},";

                    SQLiteDB.ExecuteRequestWithoutAnswer($"UPDATE Accounts SET AchivmentsId = '{achivments}' WHERE CharacterName = '{character.nameCharacter}';");
                    achivmentsHandler.TargetAddAchivment(character.netIdentity.connectionToClient, achivmentId);
                }
            }
        }

        private void Awake()
        {
            achivmentsHandler = this;
        }
    }
}
