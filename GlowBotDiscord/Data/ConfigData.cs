namespace GlowBot.Data
{
    public class ConfigData
    {
        public ulong GUILD_MASTER_ID { get; set; } = 0;
        public ulong USER_MASTER_ID { get; set; } = 0;

        public string RESPONSE_INSUFFICIENT_PERMISSIONS { get; set; } = "Insufficent Permissions.";
        public string RESPONSE_COMMAND_COOLDOWN { get; set; } = "You're in timeout.";
        public string RESPONSE_INVALID_CHANNEL_TYPE { get; set; } = "Invalid channel type.";
        public string BOT_TOKEN { get; set; } = "CHANGE_ME";

        public string[ ] NAME_DEFAULTS = new string[ ]
        {
            "Alpha",
            "Beta",
            "Delta",
            "Flare",
            "Gold",
            "Venom",
            "Pizza",
            "Optic",
            "Glow",
            "Klaw",
            "Puddle",
            "Thor",
            "Sauce",
            "Blaze",
            "Ultra",
            "Hook",
            "Robot",
            "Blade",
            "Power",
            "Nova",
            "Silence",
            "Star",
        };
        public string[] PREFIX_DEFAULTS = new string[]
        {
            "Super",
            "Golden",
            "Temple of",
            "Club",
            "The",
            "Simply",
            "Crispy",
            "Smooth",
            "Loud",
            "Chilled",
            "Polar",
        };
        public string[] SUFFIX_DEFAULTS = new string[]
        {
            "of Death",
            "INC.",
            "LLC",
            "[TM]",
        };
    }
}
