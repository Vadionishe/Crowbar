using System;
using System.Collections.Generic;

namespace Crowbar.Server
{
    public class Room
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public int MaxPlayer { get; set; }
        public int MinPlayer { get; set; }
        public List<PlayerInstance> players { get; set; }

        private static int counter;

        public Room(string name, string password)
        {
            counter++;

            Id = counter;
            MaxPlayer = 6;
            MinPlayer = 2;
            Name = name;
            Password = password;

            players = new List<PlayerInstance>();
        }
    }
}