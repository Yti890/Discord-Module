using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Module.API.Other
{
    public class DiscordUser
    {
        public DiscordUser(string id, string name, string command)
        {
            Id = id;
            Name = name;
            Command = command;
        }
        public string Command { get; }
        public string Id { get; }
        public string Name { get; }
    }
}
