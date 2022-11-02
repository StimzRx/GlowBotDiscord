using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

using GlowBotDiscord.API;
using GlowBotDiscord.Data.Entities;

namespace GlowBotDiscord.SlashCommands
{
    public class SlashProgram : ApplicationCommandModule
    {
        public enum ProgramSwitchType
        {
            [ChoiceName("Auto Manage New")]
            AUTO_MANAGE_NEW,
        }

        [SlashCommand( "ProgramSwitch", "Programs a switch with given value" )]
        public async Task ProgramSwitchCommand( InteractionContext ctx, [Option( "Switch", "Switch to set" )] ProgramSwitchType type, [Option("Value", "New value")]bool value )
        {
            await ctx.CreateResponseAsync( InteractionResponseType.DeferredChannelMessageWithSource );

            bool permitted = GlowUtils.HasAdminPermissions( ctx.Member, ctx.Guild );
            if ( !permitted )
            {
                await ctx.FollowUpAsync( new DiscordFollowupMessageBuilder( ).AsEphemeral( ).WithContent( Program.ConfigData.RESPONSE_INSUFFICIENT_PERMISSIONS ) );
                return;
            }
            
            GuildData guildData = Program.Database.GetGuildData( ctx.Guild );

            if ( type == ProgramSwitchType.AUTO_MANAGE_NEW )
            {
                if ( value && guildData.ServerRole_Pending == 0 )
                {
                    await ctx.FollowUpAsync( new DiscordFollowupMessageBuilder( ).AsEphemeral( ).WithContent( $"Failed: ServerRole_Pending is unset." ));
                    return;
                }
                guildData.SWITCH_AUTO_MANAGE_NOOBS = value;
            }
            else
            {
                await ctx.FollowUpAsync( new DiscordFollowupMessageBuilder( ).AsEphemeral( ).WithContent( $"Failed: Given switch type isn't supported!" ) );
                return;
            }
            
            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder( ).AsEphemeral( ).WithContent( $"Switch was changed to {( value ? "YES" : "NO" )}!" ) );
        }
        public enum ProgramChannelType
        {
            [ChoiceName("CustomVC")]
            CUSTOM_VC,
            [ChoiceName("Stats")]
            STATS,
            [ChoiceName("Logs")]
            LOGS,
            [ChoiceName("General")]
            GENERAL,
        }
        [SlashCommand( "ProgramChannel", "Programs guild in the Database" )]
        public async Task ProgramRoleCommand( InteractionContext ctx, [Option("Type", "Type of programming to do")] ProgramChannelType type, [Option("Target", "The target")]DiscordChannel channel  )
        {
            await ctx.CreateResponseAsync( InteractionResponseType.DeferredChannelMessageWithSource );

            GuildData guildData = Program.Database.GetGuildData( ctx.Guild );

            bool permitted = GlowUtils.HasAdminPermissions( ctx.Member, ctx.Guild );

            if ( !permitted )
            {
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder( ).AsEphemeral( ).WithContent( Program.ConfigData!.RESPONSE_INSUFFICIENT_PERMISSIONS ) );
                return;
            }
            if ( type is ProgramChannelType.CUSTOM_VC )
            {
                if ( channel.Type is ChannelType.Text )
                {
                    await ctx.FollowUpAsync( new DiscordFollowupMessageBuilder(  ).AsEphemeral(  ).WithContent( $"Failed: Invalid channel type. Expecting VOICE channel!" ) );
                    return;
                }
                guildData.ServerVC_NewVC = channel.Id;
                    
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder( ).AsEphemeral( ).WithContent( $"NewVC's Snowflake was updated in the database." ) );
            }
            else if( type is ProgramChannelType.LOGS )
            {
                if ( channel.Type is ChannelType.Voice )
                {
                    await ctx.FollowUpAsync( new DiscordFollowupMessageBuilder(  ).AsEphemeral(  ).WithContent( $"Failed: Invalid channel type. Expecting TEXT channel!" ) );
                    return;
                }
                guildData.ServerTC_Logs = channel.Id;
                
                await ctx.FollowUpAsync( new DiscordFollowupMessageBuilder(  ).AsEphemeral( ).WithContent( $"Log channel Snowflake was updated in the database." ) );
            }
            else if ( type is ProgramChannelType.GENERAL )
            {
                if ( channel.Type is ChannelType.Voice )
                {
                    await ctx.FollowUpAsync( new DiscordFollowupMessageBuilder(  ).AsEphemeral(  ).WithContent( $"Failed: Invalid channel type. Expecting TEXT channel!" ) );
                    return;
                }
                guildData.ServerTC_General = channel.Id;
                
                await ctx.FollowUpAsync( new DiscordFollowupMessageBuilder(  ).AsEphemeral( ).WithContent( $"Log channel Snowflake was updated in the database." ) );
            }
            else
            {
                if ( channel.Type is ChannelType.Text )
                {
                    await ctx.FollowUpAsync( new DiscordFollowupMessageBuilder(  ).AsEphemeral(  ).WithContent( $"Failed: Invalid channel type. Expecting VOICE channel!" ) );
                    return;
                }
                guildData.ServerVC_Stats = channel.Id;
                    
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder( ).AsEphemeral( ).WithContent( $"Stats channel Snowflake was updated in the database." ) );
            }
        }
        public enum ProgramRoleType
        {
            [ChoiceName("Trusted Role")]
            TRUSTED,
            [ChoiceName("Pending Role")]
            PENDING,
            [ChoiceName("Admin Role")]
            ADMIN,
        }
        [SlashCommand( "ProgramRole", "Programs guild in the Database" )]
        public async Task ProgramRoleCommand( InteractionContext ctx, [Option("Type", "Type of programming to do")] ProgramRoleType type, [Option("Target", "The target")]DiscordRole tarRole  )
        {
            await ctx.CreateResponseAsync( InteractionResponseType.DeferredChannelMessageWithSource );

            GuildData guildData = Program.Database.GetGuildData( ctx.Guild );

            bool permitted = GlowUtils.HasAdminPermissions( ctx.Member, ctx.Guild );

            if ( !permitted )
            {
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder( ).AsEphemeral( ).WithContent( Program.ConfigData!.RESPONSE_INSUFFICIENT_PERMISSIONS ) );
                return;
            }
            
            switch (type)
            {
                case ProgramRoleType.ADMIN:
                    guildData.ServerRole_Admin = tarRole.Id;
                    break;
                case ProgramRoleType.TRUSTED:
                    guildData.ServerRole_Trusted = tarRole.Id;
                    break;
                case ProgramRoleType.PENDING:
                    guildData.ServerRole_Pending = tarRole.Id;
                    break;
            }
                
            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder( ).AsEphemeral( ).WithContent( $"Role Id was updated in the database." ) );
        }
    }
}
