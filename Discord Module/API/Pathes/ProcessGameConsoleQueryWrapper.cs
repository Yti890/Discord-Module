using RemoteAdmin;
using System;
using System.Reflection;

namespace Discord_Module.API.Pathes
{
    public static class ProcessGameConsoleQueryWrapper
    {
        private static readonly MethodInfo ProcessGameConsoleQueryMth = typeof(QueryProcessor).GetMethod("ProcessGameConsoleQuery", BindingFlags.NonPublic);
        public static string ProcessGameConsoleQuery(string query, CommandSender sender)
        {
            if (ProcessGameConsoleQueryMth == null)
                throw new InvalidOperationException("Not find CommandProcessor.ProcessQuery");

            return (string)ProcessGameConsoleQueryMth.Invoke(null, new object[] { query, sender });
        }
    }
}