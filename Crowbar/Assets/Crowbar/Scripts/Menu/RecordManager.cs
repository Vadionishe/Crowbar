using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Crowbar.Server;

namespace Crowbar
{
    public class RecordManager : MonoBehaviour
    {
        public TextMeshProUGUI textRecords;

        public int countRecords = 10;

        public void GetRecords()
        {
            ClientMenu.localPlayerInstance.GetRecords(countRecords);
        }

        public void SetRecords(List<string> records)
        {
            textRecords.text = "DEPTH RECORDS\n\n";

            for (int i = 0; i < records.Count; i++)
                textRecords.text += $"{i + 1}. {records[i]}\n";
        }
    }
}
