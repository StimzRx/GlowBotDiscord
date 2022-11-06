using DSharpPlus.Entities;

using GlowBotDiscord.Data.Entities;

namespace GlowBotDiscord.Data
{
    public class Database
    {
        public List<GuildData> Guilds { get; set; } = new List<GuildData>( );

        public GuildData GetGuildData( DiscordGuild input )
        {
            foreach (GuildData guild in Guilds)
            {
                if ( guild.Snowflake == input.Id )
                {
                    return guild;
                }
            }
            GuildData tmp = GuildData.CreateFrom( input );
            Guilds.Add( tmp );
            return tmp;
        }
        public GuildUserData GetUserData( DiscordMember member )
        {
            GuildData guildData = GetGuildData( member.Guild );
            
            foreach (GuildUserData user in guildData.Users)
            {
                if ( user.Snowflake == member.Id )
                {
                    return user;
                }
            }
            GuildUserData tmp = GuildUserData.CreateFrom( member );
            guildData.Users.Add( tmp );
            return tmp;
        }
    }
}
