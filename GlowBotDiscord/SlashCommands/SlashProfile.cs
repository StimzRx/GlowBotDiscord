using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

using GlowBotDiscord.API;
using GlowBotDiscord.Data.Entities;

namespace GlowBotDiscord.SlashCommands
{
    public class SlashProfile : ApplicationCommandModule
    {
        [SlashCommand( "Profile", "Checks your user profile" )]
        async public Task ProfileCommand( InteractionContext ctx )
        {
            await ctx.CreateResponseAsync( InteractionResponseType.DeferredChannelMessageWithSource );
            
            bool permitted = GlowUtils.HasTrustedPermissions( ctx.Member, ctx.Guild );
            if ( !permitted )
            {
                await ctx.FollowUpAsync( new DiscordFollowupMessageBuilder(  ).WithContent( Program.ConfigData.RESPONSE_INSUFFICIENT_PERMISSIONS ) );
                return;
            }
            GuildUserData userData = Program.Database.GetUserData( ctx.Member );
            
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder( );
            embedBuilder.WithTitle( $"Profile" );
            embedBuilder.WithColor( DiscordColor.White );
            embedBuilder.WithThumbnail( ctx.Member.AvatarUrl );
            embedBuilder.AddField( "Name", userData.Nickname, true );
            embedBuilder.AddField( "Level", userData.Level.ToString( ), true );
            embedBuilder.AddField( "Experience", $"{userData.Experience}/{userData.GetExperienceToNextLevel()}", false );
            embedBuilder.AddField( "Tokens", userData.Currency.ToString( ), true );
            await ctx.FollowUpAsync( new DiscordFollowupMessageBuilder( ).AddEmbed( embedBuilder.Build( ) ) );
        }
        [SlashCommand( "Debug", "Admin Debug Command" )]
        async public Task DebugMeCommand( InteractionContext ctx )
        {
            await ctx.DeferAsync( true );
            
            bool permitted = GlowUtils.HasAdminPermissions( ctx.Member, ctx.Guild );
            if ( !permitted )
            {
                await ctx.FollowUpAsync( new DiscordFollowupMessageBuilder(  ).AsEphemeral(  ).WithContent( Program.ConfigData.RESPONSE_INSUFFICIENT_PERMISSIONS ).AsEphemeral( ) );
                return;
            }
            GuildUserData userData = Program.Database.GetUserData( ctx.Member );
            
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder( );
            embedBuilder.WithTitle( $"Debug User" );
            embedBuilder.WithColor( DiscordColor.Red );
            embedBuilder.WithThumbnail( ctx.Member.AvatarUrl );
            embedBuilder.AddField( "Name", userData.Nickname, true );
            embedBuilder.AddField( "Snowflake", ctx.Member.Id.ToString(), true );
            embedBuilder.AddField( "Chats", userData.Messages.ToString(), true );
            embedBuilder.AddField( "XP", $"{userData.Experience}/{userData.GetExperienceToNextLevel(  )}", true );
            await ctx.FollowUpAsync( new DiscordFollowupMessageBuilder( ).AsEphemeral(  ).AddEmbed( embedBuilder.Build( ) ).AsEphemeral( ) );
        }
        [SlashCommand( "DebugUser", "Admin Debug Others" )]
        async public Task DebugUserCommand( InteractionContext ctx, [Option( "User", "Target User" )] DiscordUser member )
        {
            await ctx.DeferAsync( true );
            
            bool permitted = GlowUtils.HasAdminPermissions( ctx.Member, ctx.Guild );
            if ( !permitted )
            {
                await ctx.FollowUpAsync( new DiscordFollowupMessageBuilder(  ).AsEphemeral(  ).WithContent( Program.ConfigData.RESPONSE_INSUFFICIENT_PERMISSIONS ).AsEphemeral( ) );
                return;
            }
            
            GuildUserData userData = Program.Database.GetUserData( (DiscordMember)member );
            
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder( );
            embedBuilder.WithTitle( $"Debug User" );
            embedBuilder.WithColor( DiscordColor.Red );
            embedBuilder.WithThumbnail( member.AvatarUrl );
            embedBuilder.AddField( "Name", userData.Nickname, true );
            embedBuilder.AddField( "Snowflake", member.Id.ToString(), true );
            embedBuilder.AddField( "Chats", userData.Messages.ToString(), true );
            embedBuilder.AddField( "XP", $"{userData.Experience}/{userData.GetExperienceToNextLevel(  )}", true );
            await ctx.FollowUpAsync( new DiscordFollowupMessageBuilder( ).AddEmbed( embedBuilder.Build( ) ).AsEphemeral( ) );
        }
    }
}
