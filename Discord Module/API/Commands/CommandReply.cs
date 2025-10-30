namespace Discord_Module.API.Commands
{
    public class CommandReply
    {
        public CommandReply(CommandSender sender, string response, bool isSucceeded)
        {
            Sender = sender;
            Response = response;
            IsSucceeded = isSucceeded;
        }
        public CommandSender Sender { get; }
        public string Response { get; }
        public bool IsSucceeded { get; }
        public void Answer() => Sender?.RaReply(Response, IsSucceeded, true, string.Empty);
    }
}
