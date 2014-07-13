namespace Platform.ClientInteraction.Actions
{
    public class YesSendQuestion : ClientActionBase
    {
        public YesSendQuestion(string title, string message)
        {
            ClientHandler = "YesSendQuestion";
            Title = title;
            Message = message;
        }

        public string Message { get; set; }
        public string Title { get; set; }
    }
}
