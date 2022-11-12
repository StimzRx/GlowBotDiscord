using DSharpPlus.Entities;

namespace GlowBotDiscord.Data.Entities
{
    public class GuildUserData
    {
        public static GuildUserData CreateFrom( DiscordMember member )
        {
            DateTime lastTriggeredTime = DateTime.Now.Subtract( TimeSpan.FromMinutes( 5 ) );
            return new GuildUserData( )
            {
                Snowflake = member.Id,
                GuildSnowflake = member.Guild.Id,
                Nickname = member.Username,
                JoinDate = DateTime.Now,
                LastTalkedTime = lastTriggeredTime,
                LastCommandTime = lastTriggeredTime,
                LastNewVCTime = lastTriggeredTime,
            };
        }

        public int AddExperience( float amount )
        {
            Experience += amount;
            
            if ( Experience >= GetExperienceToNextLevel( ) )
            {
                Level++;
                Experience = 0;
                int addMoney = Random.Shared.Next( 5, 25 );
                Currency += addMoney;
                return addMoney;
            }
            return -1;
        }
        public float GetExperienceToNextLevel( )
        {
            return ( ( 4f * ( MathF.Pow( Level, 3f ) ) ) / 5f ) + 5f;
        }
        
        public ulong Snowflake { get; set; } = 0;
        public ulong GuildSnowflake { get; set; } = 0;
        public uint Version { get; set; } = 1;
        public string Nickname { get; set; } = "_NULL_";
        public float Experience { get; set; } = 0;
        public int Level { get; set; } = 0;
        public long Currency { get; set; } = 0;
        public ulong Reports { get; set; } = 0;
        public ulong Messages { get; set; } = 0;
        public DateTime JoinDate { get; set; }
        public DateTime LastTalkedTime { get; set; }
        public DateTime LastCommandTime { get; set; }
        public DateTime LastNewVCTime { get; set; }

        public List<MsgHistory> MessageHistory { get; set; } = new List<MsgHistory>( );
    }
}
