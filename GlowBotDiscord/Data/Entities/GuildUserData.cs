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
                Nickname = member.Username,
                JoinDate = DateTime.Now,
                LastTalkedTime = lastTriggeredTime,
                LastCommandTime = lastTriggeredTime,
                LastNewVCTime = lastTriggeredTime,
            };
        }

        public bool AddExperience( float amount )
        {
            //round((4 * (level ^ 3)) / 5)
            Experience += amount;
            
            if ( Experience >= GetExperienceToNextLevel( ) )
            {
                Level++;
                Experience = 0;
                return true;
            }
            return false;
        }
        public float GetExperienceToNextLevel( )
        {
            return MathF.Round( ( 4f * ( Level ^ 3 ) ) / 5f );
        }
        
        public ulong Snowflake { get; set; } = 0;
        public string Nickname { get; set; } = "_NULL_";
        public float Experience { get; set; } = 0;
        public ulong Level { get; set; } = 0;
        public long Currency { get; set; } = 0;
        public ulong Reports { get; set; } = 0;
        public ulong Messages { get; set; } = 0;
        public DateTime JoinDate { get; set; }
        public DateTime LastTalkedTime { get; set; }
        public DateTime LastCommandTime { get; set; }
        public DateTime LastNewVCTime { get; set; }
    }
}
