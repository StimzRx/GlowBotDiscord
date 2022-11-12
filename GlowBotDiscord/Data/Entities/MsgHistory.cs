namespace GlowBotDiscord.Data.Entities
{
    public class MsgHistory
    {
        public string Message { get; set; }
        public int MentionCount { get; set; } = 0;
        public DateTime Time { get; set; } = DateTime.UtcNow;
        public bool HandledMentionSpam { get; set; } = false;
    }
}
