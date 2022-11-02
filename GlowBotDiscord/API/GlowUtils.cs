using DSharpPlus.Entities;

using GlowBotDiscord.Data.Entities;

namespace GlowBotDiscord.API
{
    public class GlowUtils
    {
        public static bool CheckPending( DiscordMember member, DiscordGuild guild )
        {
            GuildData guildData = Program.Database.GetGuildData( guild );
            bool hasPending = member.Roles.Any( x => x.Id.Equals( guildData.ServerRole_Pending ) );
            bool hasTrusted = CheckTrusted( member, guild );

            if ( hasPending && hasTrusted )
            {
                List<DiscordRole> roleBuffer = new List<DiscordRole>( member.Roles );
                roleBuffer.Remove( roleBuffer.First( x => x.Id.Equals( guildData.ServerRole_Trusted ) ) );
                member.ModifyAsync( x => x.Roles = roleBuffer ).Wait();
                return false;
            }
            return hasPending;
        }
        public static bool CheckTrusted( DiscordMember member, DiscordGuild guild )
        {
            GuildData guildData = Program.Database.GetGuildData( guild );
            return member.Roles.Any( x => x.Id.Equals( guildData.ServerRole_Trusted ) );
        }
        
        public static bool HasTrustedPermissions( DiscordMember member, DiscordGuild guild )
        {
            GuildData guildData = Program.Database.GetGuildData( guild );
            return ( member.Roles.Any( x => x.Id.Equals( guildData.ServerRole_Trusted ) ) || HasAdminPermissions(member, guild) );
        }
        public static bool HasAdminPermissions( DiscordMember member, DiscordGuild guild )
        {
            GuildData guildData = Program.Database.GetGuildData( guild );
            return ( member.Roles.Any( x => x.Id.Equals( guildData.ServerRole_Admin ) ) || member.IsOwner || HasMasterPermissions( member, guild ) );
        }
        public static bool HasMasterPermissions( DiscordMember member, DiscordGuild guild )
        {
            GuildData guildData = Program.Database.GetGuildData( guild );
            return ( member.Id == Program.ConfigData.USER_MASTER_ID );
        }
    }
}
