using RemoteAdmin;
using System;
using System.Reflection;

namespace Discord_Module.API.Pathes
{
    public static class ProcessQueryWrapper
    {
        private static readonly MethodInfo ProcessQueryMethod = typeof(CommandProcessor).GetMethod("ProcessQuery", BindingFlags.NonPublic | BindingFlags.Static);
        public static string ProcessQuery(string query, CommandSender sender)
        {
            if (ProcessQueryMethod == null)
                throw new InvalidOperationException("Not find CommandProcessor.ProcessQuery");

            return (string)ProcessQueryMethod.Invoke(null, new object[] { query, sender });
        }
    }
}
