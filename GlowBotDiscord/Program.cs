using System.Reflection;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;

using GlowBotDiscord.API;
using GlowBotDiscord.Data;
using GlowBotDiscord.Data.Entities;
using GlowBotDiscord.SlashCommands;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace GlowBotDiscord;

internal class Program
{
    const string DbFileName = "database.json";
    const string ConfigFileName = "config.json";
    const string LogFileName = "log.txt";


    static void Main( string[ ] args )
    {
        try
        {
            MainAsync( ).GetAwaiter( ).GetResult( );
        }
        catch ( Exception ex )
        {
            Log( $"Crash: {ex}", ConsoleColor.Red );
            throw;
        }
    }

    public static void Log( string msg, ConsoleColor color )
    {
        ConsoleColor oldColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine( msg );
        Console.ForegroundColor = oldColor;

        File.AppendAllText( BinPath + LogFileName, $"[{DateTime.Now.ToShortTimeString( )}] {msg}{Environment.NewLine}" );
    }

    async private static Task MainAsync( )
    {
        string json = string.Empty;
        BinPath = $"{Path.GetDirectoryName( Assembly.GetEntryAssembly( )?.Location )}/configs/";


        if ( !Directory.Exists( BinPath ) )
        {
            Directory.CreateDirectory( BinPath );
        }

        // Load Config
        if ( File.Exists( BinPath + ConfigFileName ) )
        {
            using (StreamReader sr = new StreamReader( BinPath + ConfigFileName ))
            using (JsonTextReader tr = new JsonTextReader( sr ))
            {
                ConfigData = _jsonSerializer.Deserialize<ConfigData>( tr ) ?? new ConfigData( );
            }
        }
        else
        {
            ConfigData = new ConfigData( );
        }

        string botToken = ConfigData.BOT_TOKEN;

        // Load Database
        if ( File.Exists( BinPath + DbFileName ) )
        {
            using (StreamReader sr = new StreamReader( BinPath + DbFileName ))
            using (JsonTextReader tr = new JsonTextReader( sr ))
            {
                Database = _jsonSerializer.Deserialize<Database>( tr ) ?? new Database( )
                {
                    Guilds = new List<GuildData>( ),
                };
            }
        }
        else
        {
            Database = new Database( );
        }
        await SaveDatabase( );

        DiscordConfiguration conf = new DiscordConfiguration( )
        {
            TokenType = TokenType.Bot,
            Token = botToken,
            MinimumLogLevel = LogLevel.Error,
            ReconnectIndefinitely = true,
            Intents = DiscordIntents.AllUnprivileged | DiscordIntents.Guilds | DiscordIntents.GuildMembers | DiscordIntents.GuildMessages,
        };
        _discord = new DiscordClient( conf );

        _discord.GuildAvailable += OnGuildAvailable;
        _discord.VoiceStateUpdated += OnVoiceStateUpdated;
        _discord.GuildMemberAdded += OnGuildMemberJoin;
        _discord.GuildMemberRemoved += OnGuildMemberLeave;
        _discord.MessageCreated += OnMessageCreated;

        await _discord.ConnectAsync( );

        SlashCommandsExtension slashCmds = _discord.UseSlashCommands( );
        slashCmds.RegisterCommands<SlashUtility>( ConfigData.GUILD_MASTER_ID );
        slashCmds.RegisterCommands<SlashProgram>( ConfigData.GUILD_MASTER_ID );
        slashCmds.RegisterCommands<SlashProfile>( ConfigData.GUILD_MASTER_ID );
        slashCmds.RegisterCommands<SlashMoney>( ConfigData.GUILD_MASTER_ID );

        DateTime lastDbSave = DateTime.Now;


        while ( !IsShutdown )
        {
            await Task.Delay( 2000 );
            if ( ( DateTime.Now - lastDbSave ).TotalMinutes >= 30 )
            {
                lastDbSave = DateTime.Now;
                await SaveDatabase( );
            }
        }
        Log( $"Shutting down...", ConsoleColor.Yellow );

        await _discord.DisconnectAsync( );
        await Task.Delay( 500 );
        _discord.Dispose( );
        await Task.Delay( 1000 );

        await SaveDatabase( );

        Log( $"Shutdown Complete!", ConsoleColor.Red );
    }

    async private static Task SaveDatabase( )
    {
        if ( File.Exists( BinPath + DbFileName ) )
        {
            File.Move( BinPath + DbFileName, BinPath + DbFileName+".bak", true );
        }
        
        await using (StreamWriter sr = new StreamWriter( BinPath + DbFileName ))
        using ( JsonTextWriter tw = new JsonTextWriter( sr ) )
        {
            tw.Formatting = Formatting.Indented;
            _jsonSerializer.Serialize( tw, Database );
            await tw.FlushAsync( );
        }
        
        // Save Config
        if ( File.Exists( BinPath + ConfigFileName ) )
        {
            File.Move( BinPath + ConfigFileName, BinPath + ConfigFileName+".bak", true );
        }
        
        await using (StreamWriter sr = new StreamWriter( BinPath + ConfigFileName ))
        using ( JsonWriter tw = new JsonTextWriter( sr ) )
        {
            tw.Formatting = Formatting.Indented;
            _jsonSerializer.Serialize( tw, ConfigData );
            await tw.FlushAsync( );
        }
        
        Log($"Saved database @ [{DateTime.Now.ToShortTimeString(  )}]", ConsoleColor.Yellow );
    }
    async private static Task OnVoiceStateUpdated( DiscordClient sender, VoiceStateUpdateEventArgs e )
    {
        GuildData guildData = Database.GetGuildData( e.Guild );

        if ( e.After?.Channel?.Id is not null )
        {
            GuildUserData userData = Database.GetUserData( (DiscordMember)e.User );
        }

        if ( e.After?.Channel?.Id is not null && e.After.Channel.Id == guildData.ServerVC_NewVC )
        {
            DiscordMember member = (DiscordMember)e.User;
            GuildUserData userData = Database.GetUserData( member );

            if ( ( DateTime.Now - userData.LastNewVCTime ).TotalSeconds < 120 )
            {
                await member.ModifyAsync( x => x.VoiceChannel = null );
            }
            else
            {
                string channelName = string.Empty;

                bool usePrefix = Random.Shared.Next( 0, 5 ) == 1;
                bool useSuffix = Random.Shared.Next( 0, 20 ) == 1;
            
                if ( usePrefix )
                    channelName += ConfigData.PREFIX_DEFAULTS[Random.Shared.Next(0, ConfigData.PREFIX_DEFAULTS.Length - 1)] + " ";
                channelName += ConfigData.NAME_DEFAULTS[ Random.Shared.Next( 0, ConfigData.NAME_DEFAULTS.Length - 1 ) ];
                if ( useSuffix )
                    channelName += " " + ConfigData.SUFFIX_DEFAULTS[ Random.Shared.Next( 0, ConfigData.SUFFIX_DEFAULTS.Length - 1 ) ];
            
                DiscordChannel channel = await e.Guild.CreateChannelAsync( channelName, ChannelType.Voice, position: 10 );
                await member.ModifyAsync( x => x.VoiceChannel = channel );

                guildData.MAVVoiceChannels.Add( channel.Id );

                userData.LastNewVCTime = DateTime.Now;

                Log( $"Created VC for '{userData.Nickname}' called '{channelName}'!", ConsoleColor.Cyan );
            }
        }

        if ( e.Before?.Channel?.Id is null )
        {
            return;
        }

        if ( guildData.MAVVoiceChannels.Contains( e.Before.Channel.Id ) )
        {
            if ( e.Before.Channel.Users.Count <= 0 )
            {
                await e.Before.Channel.DeleteAsync( );
                guildData.MAVVoiceChannels.Remove( e.Before.Channel.Id );
            }
        }
    }
    
    async private static Task OnGuildAvailable( DiscordClient sender, GuildCreateEventArgs e )
    {
        GuildData guildData = Database.GetGuildData( e.Guild );
        Log( $"Guild '{guildData.Nickname}' has come online!", ConsoleColor.Green );

        List<ulong> chanIdBuffer = new List<ulong>(guildData.MAVVoiceChannels);
        foreach(ulong chanId in chanIdBuffer)
        {
            DiscordChannel discordChannel = e.Guild.GetChannel(chanId);
            if(discordChannel!=null)
            {
                if(discordChannel.Users.Count <= 0)
                {
                    await discordChannel.DeleteAsync();
                    guildData.MAVVoiceChannels.Remove(chanId);
                }
            }
        }
    }
    async private static Task OnGuildMemberLeave( DiscordClient sender, GuildMemberRemoveEventArgs e )
    {
        GuildData guildData = Database.GetGuildData( e.Guild );
        GuildUserData userData = Database.GetUserData( e.Member );
        if ( guildData.ServerTC_Logs != 0 )
        {
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder( );
            embedBuilder.WithTitle( $"User Left Guild" );
            embedBuilder.WithColor( DiscordColor.Gold );
            embedBuilder.WithThumbnail( e.Member.AvatarUrl );
            embedBuilder.AddField( "DB Name", userData.Nickname, true );
            embedBuilder.AddField( "Snowflake", e.Member.Id.ToString(), true );
            await e.Guild.GetChannel( guildData.ServerTC_Logs ).SendMessageAsync( new DiscordMessageBuilder( ).WithEmbed( embedBuilder.Build(  ) ) );
        }
        Log( $"[User Leave] {userData.Nickname}", ConsoleColor.Yellow );
    }
    async private static Task OnGuildMemberJoin( DiscordClient sender, GuildMemberAddEventArgs e )
    {
        GuildData guildData = Database.GetGuildData( e.Guild );
        GuildUserData userData = Database.GetUserData( e.Member );
        if ( guildData.ServerTC_Logs != 0 )
        {
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder( );
            embedBuilder.WithTitle( $"User Joined Guild" );
            embedBuilder.WithColor( DiscordColor.Gold );
            embedBuilder.WithThumbnail( e.Member.AvatarUrl );
            embedBuilder.AddField( "DB Name", userData.Nickname, true );
            embedBuilder.AddField( "Snowflake", e.Member.Id.ToString(), true );
            await e.Guild.GetChannel( guildData.ServerTC_Logs ).SendMessageAsync( new DiscordMessageBuilder( ).WithEmbed( embedBuilder.Build(  ) ) );
        }
        Log( $"[User Join] {userData.Nickname}", ConsoleColor.Yellow );

        if ( guildData.SWITCH_AUTO_MANAGE_NOOBS )
        {
            await e.Member.GrantRoleAsync( e.Guild.GetRole( guildData.ServerRole_Pending ) );
        }
    }
    async private static Task OnMessageCreated( DiscordClient sender, MessageCreateEventArgs e )
    {
        if ( e.Guild == null )
        {
            return;
        }
        
        
        GuildData guildData = Database.GetGuildData( e.Guild );
        if ( e.Author.IsBot )
        {
            return;
        }
        
        DiscordMember member = (DiscordMember)e.Author;
        
        GuildUserData userData = Database.GetUserData( member );
        
        userData.MessageHistory.Add( new MsgHistory(  )
        {
            Message = e.Message.ToString(  ),
            MentionCount = ( e.MentionedRoles.Count + e.MentionedUsers.Count ),
            Time = DateTime.UtcNow,
            HandledMentionSpam = false,
        });
        if ( userData.MessageHistory.Count > 4 )
        {
            userData.MessageHistory.RemoveRange( 0, userData.MessageHistory.Count - 10 );
        }

        if ( !GlowUtils.HasAdminPermissions( member, member.Guild ) )
        {
            List<MsgHistory> foundMessages = new List<MsgHistory>( );
            int foundMentions = 0;
            foreach (MsgHistory history in userData.MessageHistory)
            {
                if ( history.MentionCount > 0 && ( DateTime.UtcNow - history.Time ).TotalMinutes <= 8 && !history.HandledMentionSpam )
                {
                    foundMessages.Add( history );
                    foundMentions += history.MentionCount;
                }
            }
            if ( foundMentions >= 10 )
            {
                foreach (MsgHistory msg in foundMessages)
                {
                    msg.HandledMentionSpam = true;
                }
                
                await member.TimeoutAsync( new DateTimeOffset( DateTime.Now + TimeSpan.FromMinutes( 15 ) ) );

                await e.Message.DeleteAsync( );
                
                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder( );
                embedBuilder.WithTitle( $"User Timed Out - Mention Spam" );
                embedBuilder.WithColor( DiscordColor.Red );
                embedBuilder.WithThumbnail( member.AvatarUrl );
                embedBuilder.AddField( "Mentions", foundMentions.ToString(), true );
                embedBuilder.AddField( "Time", $"15 minutes", true );
            
                await member.Guild.GetChannel( guildData.ServerTC_Logs ).SendMessageAsync( new DiscordMessageBuilder(  ).AddEmbed( embedBuilder.Build(  ) ) );
            }
        }

        if ( e.Channel.Id != guildData.ServerTC_General )
            return;
        userData.Messages += 1;
        
        if ( ( DateTime.Now - userData.LastTalkedTime ).TotalMinutes >= 30 )
        {
            userData.LastTalkedTime = DateTime.Now;

            int returnAddMoney = userData.AddExperience( 0.75f );
            if( returnAddMoney > 0 )
            {
                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder( );
                embedBuilder.WithTitle( $"User Level Up" );
                embedBuilder.WithColor( DiscordColor.Gold );
                embedBuilder.WithThumbnail( member.AvatarUrl );
                embedBuilder.AddField( "Level", userData.Level.ToString(), true );
                embedBuilder.AddField( "Tokens", $"+{returnAddMoney}", true );
                await e.Channel.SendMessageAsync( new DiscordMessageBuilder( ).AddEmbed( embedBuilder.Build( ) ) );
            }
        }
    }

    public static void Shutdown( )
    {
        IsShutdown = true;
    }

    public static Database Database { get; protected set; }
    private static JsonSerializer _jsonSerializer = new JsonSerializer( );

    public static string BinPath { get; private set; }
    
    private static DiscordClient? _discord;
    public static ConfigData ConfigData { get; private set; }

    public static bool IsShutdown { get; private set; } = false;
}
