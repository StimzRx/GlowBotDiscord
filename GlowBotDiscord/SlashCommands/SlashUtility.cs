using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;

using GlowBotDiscord.API;
using GlowBotDiscord.Data.Entities;

namespace GlowBotDiscord.SlashCommands
{
    public class SlashUtility : ApplicationCommandModule
    {
        public async Task BanUserCommand(InteractionContext ctx, [Option("User", "User to ban")] DiscordUser discordUser, [Option("Reason", "Reason for ban")] string reason)
        {
            await ctx.DeferAsync(  );

            GuildData guildData = Program.Database.GetGuildData( ctx.Guild );

            bool permitted = GlowUtils.HasAdminPermissions( ctx.Member, ctx.Guild );

            if ( !permitted )
            {
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder( ).WithContent( Program.ConfigData.RESPONSE_INSUFFICIENT_PERMISSIONS ) );
                return;
            }
            DiscordMember targetMember = (DiscordMember)discordUser;
            
            GuildUserData targetUserData = Program.Database.GetUserData( targetMember );
            GuildUserData sourceUserData = Program.Database.GetUserData( ctx.Member );
            
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder( );
            embedBuilder.WithTitle( $"Banned User" );
            embedBuilder.WithColor( DiscordColor.Red );
            embedBuilder.WithThumbnail( ctx.Member.AvatarUrl );
            embedBuilder.AddField( "Name", targetUserData.Nickname );
            embedBuilder.AddField( "Reason", reason );
            embedBuilder.WithFooter( $"Banned by '{sourceUserData.Nickname}'({ctx.Member.Id})" );

            try
            {
                await targetMember.CreateDmChannelAsync( ).Result.SendMessageAsync( embedBuilder.Build( ) );
            }
            catch ( UnauthorizedException ex ) { }
            
            await targetMember.BanAsync( 0, $"[GlowBan]:"+reason );

            await ctx.Guild.GetChannel( guildData.ServerTC_General ).SendMessageAsync( embedBuilder.Build( ) );
        }
        
        
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
