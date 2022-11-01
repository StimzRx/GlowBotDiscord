using DSharpPlus.Entities;

using GlowBot.Data.Entities;

namespace GlowBot.Data
{
    public class Database
    {
        public List<GuildData> Guilds { get; set; } = new List<GuildData>( );
        public List<GuildUserData> Users { get; set; } = new List<GuildUserData>( );

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
            foreach (GuildUserData user in Users)
            {
                if ( user.Snowflake == member.Id )
                {
                    return user;
                }
            }
            GuildUserData tmp = GuildUserData.CreateFrom( member );
            Users.Add( tmp );
            return tmp;
        }
    }
}
