using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

using GlowBotDiscord.API;
using GlowBotDiscord.Data.Entities;

namespace GlowBotDiscord.SlashCommands
{
    public class SlashMoney : ApplicationCommandModule
    {
        [SlashCommand( "Balance", "Checks your balance" )]
        async public Task BalanceCommand( InteractionContext ctx )
        {
            await ctx.DeferAsync( true );
            
            bool permitted = GlowUtils.HasTrustedPermissions( ctx.Member, ctx.Guild );
            if ( !permitted )
            {
                await ctx.FollowUpAsync( new DiscordFollowupMessageBuilder(  ).AsEphemeral( ).WithContent( Program.ConfigData.RESPONSE_INSUFFICIENT_PERMISSIONS ) );
                return;
            }
            GuildUserData userData = Program.Database.GetUserData( ctx.Member );
            
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder( );
            embedBuilder.WithTitle( $"Balance" );
            embedBuilder.WithColor( DiscordColor.Gold );
            embedBuilder.WithThumbnail( ctx.Member.AvatarUrl );
            embedBuilder.AddField( "Name", userData.Nickname, true );
            embedBuilder.AddField( "Tokens", userData.Currency.ToString( ), true );
            await ctx.FollowUpAsync( new DiscordFollowupMessageBuilder( ).AsEphemeral(  ).AddEmbed( embedBuilder.Build( ) ) );
        }
    }
}
