using System;

namespace DiscordModuleDependency
{
    public class RemoteClient
    {
        public ActionType Action { get; set; }
        public object[] Parameters { get; set; } = Array.Empty<object>();

        public RemoteClient() { }

        public RemoteClient(ActionType action, params object[] parameters)
        {
            Action = action;
            Parameters = parameters;
        }
    }
}
