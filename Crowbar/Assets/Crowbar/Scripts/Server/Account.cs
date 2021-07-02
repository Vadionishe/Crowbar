
using System.Collections.Generic;

namespace Crowbar.Server
{
    public static class Account
    {
        public static bool IsAuthentication { get; set; }   
        public static bool IsGuest { get; set; }   
        public static string Login { get; set; }   
        public static string Password { get; set; }   
        public static string Name { get; set; }   
        public static int Gold { get; set; }

        public static List<int> idAchivments { get; set; }
        public static List<int> idHats { get; set; }
        public static int idCurrentHat { get; set; }

        public static void MapAccount(string data)
        {
            Reset();

            string[] parceData = data.Split(':');
            string[] achivmentsIdString = parceData[5].Split(',');
            string[] hatsIdString = parceData[7].Split(',');
            List<int> achivmentsId = new List<int>();
            List<int> hatsId = new List<int>();

            foreach (string id in achivmentsIdString)
                if (int.TryParse(id, out int _idTry))
                    achivmentsId.Add(_idTry);

            foreach (string id in hatsIdString)
                if (int.TryParse(id, out int _idTry))
                    hatsId.Add(_idTry);

            IsAuthentication = true;
            IsGuest = false;
            Login = parceData[1];
            Password = parceData[2];
            Name = parceData[3];
            Gold = int.Parse(parceData[4]);
            idAchivments = achivmentsId;
            idHats = hatsId;
            idCurrentHat = int.Parse(parceData[6]);
        }

        public static void Reset()
        {
            IsAuthentication = false;
            IsGuest = false;

            Login = string.Empty;
            Password = string.Empty;
            Name = string.Empty;

            Gold = 0;
            idCurrentHat = 0; 

            idAchivments = new List<int>();
            idHats = new List<int>();
        }
    }
}
