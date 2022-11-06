using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

using GlowBotDiscord.API;
using GlowBotDiscord.Data.Entities;

namespace GlowBotDiscord.SlashCommands
{
    public class SlashUtility : ApplicationCommandModule
    {
        public enum ClearUsersType
        {
            [ChoiceName("Pending")]
            Pending,
            [ChoiceName("Unregistered")]
            Unregistered,
        }

        [SlashCommand( "ClearUsers", "Kicks users of the specified role." )]
        public async Task ClearUsersCommand( InteractionContext ctx, [Option( "Type", "Type of clear" )] ClearUsersType type )
        {
            await ctx.DeferAsync( true );

            GuildData guildData = Program.Database.GetGuildData( ctx.Guild );

            bool permitted = GlowUtils.HasAdminPermissions( ctx.Member, ctx.Guild );

            if ( !permitted )
            {
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder( ).AsEphemeral( ).WithContent( Program.ConfigData.RESPONSE_INSUFFICIENT_PERMISSIONS ) );
                return;
            }
            
            int removedAmt = 0;
        
            var allMembers = await ctx.Guild.GetAllMembersAsync( );
            
            foreach (DiscordMember member in allMembers)
            {
                bool shouldRemove = false;

                if ( type is ClearUsersType.Pending )
                {
                    shouldRemove = member.Roles.Any( x => x.Id == guildData.ServerRole_Pending ) && member.Roles.All( x => x.Id != guildData.ServerRole_Pending );
                }
                else if( type is ClearUsersType.Unregistered)
                {
                    shouldRemove = member.Roles.All( x =>
                        x.Id != guildData.ServerRole_Pending && x.Id != guildData.ServerRole_Pending );
                }

                if ( shouldRemove )
                {
                    await member.RemoveAsync( "Kicked via the ClearUsers command." );
                    removedAmt++;
                }
            }
            if ( removedAmt > 0 )
            {
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder( ).AsEphemeral( ).WithContent( $"Removed {removedAmt} users." ) );
                return;
            }
            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder( ).AsEphemeral( ).WithContent( $"No users were found to remove." ) );
        }
        
        
        [SlashCommand( "Ping", "Pings the bot" )]
        public async Task PingCommand( InteractionContext ctx )
        {
            await ctx.DeferAsync( true );

            GuildData guildData = Program.Database.GetGuildData( ctx.Guild );

            bool permitted = GlowUtils.HasTrustedPermissions( ctx.Member, ctx.Guild );
            if ( !permitted )
            {
                await ctx.DeleteResponseAsync( );
            }
            
            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder( ).AsEphemeral( ).WithContent( $"Pong!" ) );
        }
        
        [SlashCommand( "Shutdown", "Terminates the bot" )]
        public async Task ShutdownCommand( InteractionContext ctx )
        {
            await ctx.DeferAsync( true );

            bool callerAllowed = GlowUtils.HasMasterPermissions( ctx.Member, ctx.Guild );

            if ( !callerAllowed )
            {
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder( ).AsEphemeral( ).WithContent( Program.ConfigData.RESPONSE_INSUFFICIENT_PERMISSIONS ) );
            }
            
            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder( ).AsEphemeral( ).WithContent( $"Shutting Down..." ) );
            Program.Shutdown(  );
        }
    }
}
