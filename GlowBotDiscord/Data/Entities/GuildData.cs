using DSharpPlus.Entities;

namespace GlowBot.Data.Entities
{
    public class GuildData
    {
        public static GuildData CreateFrom( DiscordGuild guild )
        {
            return new GuildData( )
            {
                Snowflake = guild.Id,
                Nickname = guild.Name,
                JoinDate = DateTime.Now,
            };
        }
        public ulong Snowflake { get; set; }
        public string Nickname { get; set; }
        public DateTime JoinDate { get; set; }
        
        public ulong ServerOwnerSnowflake { get; set; } = 0;
        
        public ulong ServerRole_Admin { get; set; } = 0;
        public ulong ServerRole_Trusted { get; set; } = 0;
        public ulong ServerRole_Pending { get; set; } = 0;
        
        public ulong ServerVC_NewVC { get; set; } = 0;
        public ulong ServerVC_Stats { get; set; } = 0;
        
        public ulong ServerTC_Leaderboard { get; set; } = 0;
        public ulong ServerTC_Logs { get; set; } = 0;
        
        public ulong ServerReactsMsg_PingSubscriber { get; set; } = 0;

        public List<ulong> MAVVoiceChannels { get; set; } = new List<ulong>( );
        
        // Switches
        public bool SWITCH_AUTO_MANAGE_NOOBS { get; set; } = false;
    }
}
