/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

// The General class has general purpose functions and main routines.
// It also has some public objects that are often referenced by many
// other classes in the engine.

#region Namespace usage

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Bloodmasters.Client.Effects;
using Bloodmasters.Client.Graphics;
using Bloodmasters.Client.Interface;
using Bloodmasters.Client.LevelMap;
using Bloodmasters.Client.Lights;
using Bloodmasters.Client.Net;
using Bloodmasters.Client.Resources;
using Bloodmasters.Client.Weapons;
using Bloodmasters.LevelMap;
using Bloodmasters.Net;
using Bloodmasters.Server;
using SharpDX;
using SharpDX.Direct3D9;
using Bullet = Bloodmasters.Client.Weapons.Bullet;
using CharSet = Bloodmasters.Client.Graphics.CharSet;
using Direct3D = Bloodmasters.Client.Graphics.Direct3D;
using Flag = Bloodmasters.Client.Items.Flag;
using GameServer = Bloodmasters.Server.GameServer;
using Grenade = Bloodmasters.Client.Projectiles.Grenade;
using IonBall = Bloodmasters.Client.Projectiles.IonBall;
using Item = Bloodmasters.Client.Items.Item;
using PlasmaBall = Bloodmasters.Client.Projectiles.PlasmaBall;
using Projectile = Bloodmasters.Client.Projectiles.Projectile;
using Rocket = Bloodmasters.Client.Projectiles.Rocket;
using Sprite = Bloodmasters.Client.Graphics.Sprite;
using WGrenadeLauncher = Bloodmasters.Client.Weapons.WGrenadeLauncher;
using WLightChaingun = Bloodmasters.Client.Weapons.WLightChaingun;
using WMinigun = Bloodmasters.Client.Weapons.WMinigun;
using WPlasmaCannon = Bloodmasters.Client.Weapons.WPlasmaCannon;
using WRocketLauncher = Bloodmasters.Client.Weapons.WRocketLauncher;

#endregion

namespace Bloodmasters.Client;

using CharSet = Graphics.CharSet;
using Flag = Items.Flag;
using Item = Items.Item;
using Projectile = Projectiles.Projectile;

internal sealed class General : SharedGeneral
{
    // TODO[#45]: Only used in server. Remove later.
    internal static bool logtofile = false;

    // API declarations
    [DllImport("user32.dll")] public static extern int LockWindowUpdate(IntPtr hwnd);

    #region ================== Constants

    // Networking
    public const int CONNECT_TIMEOUT = 30000;
    public const int LOADING_TIMEOUT = 10000; //40000;
    public const int CONNECT_RESEND_INTERVAL = 1000;
    public const int DISCONNECT_TIMEOUT = 1000;
    public const string UNKNOWN_PLAYER_NAME = "(unknown player)";
    public const int AUTO_SCREENSHOT_DELAY = 300;

    #endregion

    #region ================== Variables

    // Application paths and name
    public static string appname = "";

    // Filenames
    private static string configfilename = Paths.DefaultConfigFileName;
    public static string logfilename = ""; // TODO[#45]: Only used in server code. Remove later.

    // Configuration
    public static Configuration config;

    // Windows
    public static FormGame gamewindow = null;
    public static FormServer serverwindow = null;

    // Status
    public static bool clientrunning = false;
    public static bool serverrunning = false;
    public static string serveraddress = "";
    public static string serverpassword = "";
    public static int serverport = 0;
    public static string disconnectreason = "";
    public static bool connecting;

    // Game Client
    public static Map map;
    public static Arena arena;
    public static HUD hud;
    public static Scoreboard scoreboard;
    public static GConsole console;
    public static ChatBox chatbox;
    public static Jukebox jukebox;
    public static GameMenu gamemenu;
    public static WeaponDisplay weapondisplay;

    // Settings
    public static bool scrollweapons;
    public static bool autoswitchweapon;
    public static int movemethod;
    public static WEAPON[] bestweapons;

    // Game info
    public static string playername;
    public static int scorelimit;
    public static GAMETYPE gametype;
    public static Client[] clients = null;
    public static Client localclient = null;
    public static GAMESTATE gamestate;
    public static int gamestateend;
    public static int[] teamscore = new int[3];
    public static string servertitle;
    public static string serverwebsite;
    public static string mapname;
    public static string maptitle;
    public static int maxclients;
    public static int localclientid;
    public static bool teamgame;
    public static bool joinsmallestteam;
    public static int callvotetimeout;
    public static int callvotes;

    // Game Server
    public static GameServer server;
    public static string serverconfig;

    // Networking
    public static Gateway gateway;
    public static Connection conn;
    private static Thread networkproc;

    // Auto-screenshot
    public static int screenshottime;
    public static bool autoscreenshot;

    // Randomizer
    public static Random random = new Random();

    // Font charsets
    public static CharSet charset_shaded;

    // Font textures
    public static TextureResource font_shaded;

    // Console texture
    public static TextureResource console_edge;

    // Background
    public static SurfaceResource background;

    #endregion

    #region ================== Initialize / Terminate

    // This is the very first initialize of the engine
    private static bool Initialize()
    {
        // Enable OS visual styles
        Application.EnableVisualStyles();
        Application.DoEvents();		// This must be here to work around a .NET bug

        // Setup application name
        appname = Assembly.GetExecutingAssembly().GetName().Name;

        // Setup filenames
        configfilename = Path.Combine(Paths.Instance.ConfigDirPath, configfilename);
        logfilename = Path.Combine(Paths.Instance.LogDirPath, appname + ".log");

        // Ensure a Music directory exists
        string musicdir = Path.Combine(Paths.Instance.BundledResourceDir, "Music");
        if(!Directory.Exists(musicdir)) Directory.CreateDirectory(musicdir);

        // Open all archives with archivemanager
        ArchiveManager.Initialize(Paths.Instance.BundledResourceDir);
        ArchiveManager.OpenArchive(Path.Combine(Paths.Instance.BundledResourceDir, "Sprites"));

        // Get the high resolution clock frequency
        timefrequency = TimeProvider.System.TimestampFrequency;
        if(timefrequency == 0)
        {
            // No high resolution clock available
            timefrequency = -1;
        }
        else
        {
            // Set time scale
            timescale = (1d / (double)timefrequency) * 1000d;
        }

        // Initialize DirectX
        try { Graphics.Direct3D.InitDX(); }
        catch(Exception)
        {
            // DirectX not installed?
            MessageBox.Show(null, "Unable to initialize DirectX. Please ensure that you have the latest version of DirectX installed.", "Bloodmasters", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        // Return success
        return true;
    }

    // This is the loading of standard
    // resources and setting up defaults
    private static bool LoadStandardComponents()
    {
        // Setup clock
        currenttime = SharedGeneral.GetCurrentTime();
        previoustime = SharedGeneral.GetCurrentTime();
        realtime = SharedGeneral.GetCurrentTime() + 1;
        accumulator = 0;

        // Initialize for client?
        if(clientrunning)
        {
            // Initialize color codes
            TextResource.Initialize();

            // Load font charsets
            charset_shaded = new CharSet(ArchiveManager.ExtractFile("general.zip/charset_shaded.cfg"));
            charset_shaded.SetColorCode(Consts.COLOR_CODE_SIGN);

            // Fonts
            font_shaded = Graphics.Direct3D.LoadTexture(ArchiveManager.ExtractFile("general.zip/font_shaded.tga"), false, true);

            // Window
            WindowBorder.texture = SharpDX.Direct3D9.Direct3D.LoadTexture(ArchiveManager.ExtractFile("general.zip/window.tga"), false, false);

            // Load the background
            background = SharpDX.Direct3D9.Direct3D.LoadSurfaceResource(ArchiveManager.ExtractFile("general.zip/background.bmp"), Pool.SystemMemory);

            // Make the console
            console = new GConsole();

            // Make the chat box
            chatbox = new ChatBox();

            // Initialize the mouse cursor
            MouseCursor.Initialize();

            // Make the HUD
            hud = new HUD();
            hud.ShowFPS = config.ReadSetting("showfps", false);
            hud.ShowScreenFlashes = config.ReadSetting("screenflashes", true);

            // Make weapon display
            weapondisplay = new WeaponDisplay();

            // Make the menu
            gamemenu = new GameMenu();

            // Make the scoreboard
            scoreboard = new Scoreboard();

            // Make the jukebox
            if (config.ReadSetting("music", true)) jukebox = new Jukebox();
        }

        // Return success
        return true;
    }

    // This unloads all resources and the game
    private static void Terminate()
    {
        // If device was initialized
        if(SharpDX.Direct3D9.Direct3D.d3dd != null)
        {
            // Clear screen
            try { SharpDX.Direct3D9.Direct3D.ClearScreen(); }
            catch(Exception) { }

            // Erase device references
            SharpDX.Direct3D9.Direct3D.d3dd.SetTexture(0, null);
            SharpDX.Direct3D9.Direct3D.d3dd.SetTexture(1, null);
            SharpDX.Direct3D9.Direct3D.d3dd.SetStreamSource(0, null, 0, 0);
            SharpDX.Direct3D9.Direct3D.d3dd.Indices = null;
            SharpDX.Direct3D9.Direct3D.d3dd.EvictManagedResources();
        }

        // Disconnect immediately
        Disconnect(true);

        // Unload the map
        UnloadMap();

        // Unload generic resources
        UnloadGenericResources();

        // Terminate game components
        if(hud != null) hud.Dispose(); hud = null;
        if(console != null) console.Dispose(); console = null;
        if(chatbox != null) chatbox.Dispose(); chatbox = null;
        if(jukebox != null) jukebox.Dispose(); jukebox = null;
        if(weapondisplay != null) weapondisplay.Dispose(); weapondisplay = null;
        if(scoreboard != null) scoreboard.Dispose(); scoreboard = null;

        // Dispose any general resources
        if(font_shaded != null) font_shaded.Dispose();
        if(background != null) background.Destroy();

        // Terminate the mouse cursors
        MouseCursor.Terminate();

        // Terminate Direct3D
        SharpDX.Direct3D9.Direct3D.Terminate();

        // Terminate server stuff
        if(server != null) server.Dispose();
        server = null;

        // Check if the game window was created
        if(gamewindow != null)
        {
            // Close the window
            gamewindow.Close();
            gamewindow.Dispose();
            gamewindow = null;
        }

        // Check if the server window was created
        if(serverwindow != null)
        {
            // Close the window
            serverwindow.Close();
            serverwindow.Dispose();
            serverwindow = null;
        }
    }

    // This termines the entire application
    public static void Exit()
    {
        // Close archives
        ArchiveManager.Dispose();

        // Delete the temporary directory
        if(!string.IsNullOrEmpty(Paths.Instance.TempDir))
            try { Directory.Delete(Paths.Instance.TempDir, true); } catch(Exception) { }

        // End of program
        Application.Exit();
    }

    #endregion

    #region ================== Configuration

    // Loads the settings configuration
    private static bool LoadConfiguration()
    {
        // Check if settings configuration file exists
        if(File.Exists(configfilename))
        {
            // Load configuration
            config = new Configuration(configfilename, true);

            // Check for errors
            if(config.ErrorResult != 0)
            {
                // Error in configuration
                MessageBox.Show("Error in configuration file " + Path.GetFileName(configfilename) + " on line " + config.ErrorLine + ":\n" + config.ErrorDescription,
                    Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Success
            return true;
        }
        else
        {
            // Cannot find configuration
            MessageBox.Show("Unable to load the configuration file " + Path.GetFileName(configfilename) + ".", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }

    // This parses command line arguments
    public static bool ApplyCmdConfiguration(string[] args)
    {
        IDictionary weaponorder;
        int i;

        string allargs = string.Join(" ", args);

        // Check if settings configuration file exists
        if(!File.Exists(allargs))
        {
            // Try in local path
            if(!File.Exists(Path.Combine(Paths.Instance.ConfigDirPath, allargs)))
            {
                // Cannot find configuration
                MessageBox.Show("Unable to load the configuration file " + Path.GetFileName(allargs) + ".", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            else
            {
                // Its in the local path
                allargs = Path.Combine(Paths.Instance.ConfigDirPath, allargs);
            }
        }

        Configuration cargs = new Configuration();
        cargs.LoadConfiguration(allargs, true);

        // Check if no errors
        if(cargs.ErrorResult == 0)
        {
            // Apply settings to configuration
            config = config + cargs;

            // Apply configuration settings
            SharpDX.Direct3D9.Direct3D.DisplayWidth = config.ReadSetting("displaywidth", 800);
            SharpDX.Direct3D9.Direct3D.DisplayHeight = config.ReadSetting("displayheight", 600);
            SharpDX.Direct3D9.Direct3D.DisplayFormat = config.ReadSetting("displayformat", (int)Format.X8R8G8B8);
            SharpDX.Direct3D9.Direct3D.DisplayRefreshRate = config.ReadSetting("displayrate", 70);
            SharpDX.Direct3D9.Direct3D.DisplayWindowed = config.ReadSetting("displaywindowed", false);
            SharpDX.Direct3D9.Direct3D.DisplaySyncRefresh = config.ReadSetting("displaysync", true);
            SharpDX.Direct3D9.Direct3D.DisplayFSAA = config.ReadSetting("displayfsaa", 0);
            SharpDX.Direct3D9.Direct3D.DisplayGamma = config.ReadSetting("displaygamma", 0);
            SharpDX.Direct3D9.Direct3D.hightextures = config.ReadSetting("hightextures", true);
            General.playername = config.ReadSetting("playername", "Newbie");
            General.scrollweapons = config.ReadSetting("scrollweapons", true);
            General.autoswitchweapon = config.ReadSetting("autoswitchweapon", true);
            Decal.showdecals = config.ReadSetting("showdecals", true);
            Actor.showgibbing = config.ReadSetting("showgibs", true);
            StaticLight.highlightmaps = config.ReadSetting("highlightmaps", false);
            DynamicLight.dynamiclights = config.ReadSetting("dynamiclights", true);
            Decal.decaltimeout = config.ReadSetting("decaltimeout", 5000);
            Laser.opacity = config.ReadSetting("laserintensity", 2);
            Client.timenudge = config.ReadSetting("timenudge", 0);
            Client.showcollisions = config.ReadSetting("showcollisions", false);
            Client.teamcolorednames = config.ReadSetting("teamcolorednames", false);
            General.autoscreenshot = config.ReadSetting("autoscreenshot", false);
            General.movemethod = config.ReadSetting("movemethod", 0);

            // Make array for best weapons
            weaponorder = config.ReadSetting("weaponorder", new Hashtable());
            bestweapons = new WEAPON[weaponorder.Count];
            i = 0;

            // Go for all best weapons in rpeferred order
            foreach(DictionaryEntry de in weaponorder)
            {
                // Add the weapon to array
                int w = int.Parse((string)de.Key);
                bestweapons[i++] = (WEAPON)w;
            }

            // Success
            return true;
        }
        else
        {
            // Error in configuration
            MessageBox.Show("Error in configuration file " + Path.GetFileName(allargs) + " on line " + cargs.ErrorLine + ":\n" + cargs.ErrorDescription,
                Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }

    #endregion

    #region ================== Logging

    // This writes text to the log file
    public static void WriteLogLine(string text)
    {
        // Open or create the log file
        StreamWriter log = File.AppendText(logfilename);

        // Write the text to the file
        log.WriteLine(text);

        // Close the file
        log.Close();
        log = null;
    }

    // This writes an exception to the log file
    public static void WriteErrorLine(Exception error)
    {
        // Open or create the log file
        StreamWriter log = File.AppendText(logfilename);

        // Write the error to the file
        log.WriteLine();
        log.WriteLine(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
        log.WriteLine(error.Source + " throws " + error.GetType().Name + ":");
        log.WriteLine(error.Message);
        log.WriteLine(error.StackTrace);
        log.WriteLine();

        // Close the file
        log.Close();
        log = null;
    }

    // This dumps an exception to the error file
    public static void OutputError(Exception error)
    {
        // Make error configuration
        Configuration errcfg = new Configuration();
        errcfg.WriteSetting("datetime", DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
        errcfg.WriteSetting("message", error.Message);
        errcfg.WriteSetting("exception", error.GetType().Name);
        errcfg.WriteSetting("details", error.Source + " throws " + error.GetType().Name + ":\r\n" + error.Message + "\r\n" + error.StackTrace);

        // Output configuration to error stream
        Console.Error.WriteLine(errcfg.OutputConfiguration());
    }

    // This dumps a custom error to the error file
    public static void OutputCustomError(string error)
    {
        // Make error configuration
        Configuration errcfg = new Configuration();
        errcfg.WriteSetting("datetime", DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
        errcfg.WriteSetting("message", error);

        // Output configuration to error stream
        Console.Error.WriteLine(errcfg.OutputConfiguration());
    }

    #endregion

    #region ================== Connect / Login

    // This logs in to the server
    // Returns the StartGameInfo message on success, null on failure
    private static bool Login(int connectid, out string reason)
    {
        int timeout = SharedGeneral.GetCurrentTime() + CONNECT_TIMEOUT;
        NetMessage msg, rconmsg, rep = null;

        // Send a player login request
        msg = conn.CreateMessage(MsgCmd.PlayerLogin, true);
        if(msg != null)
        {
            msg.AddData((int)connectid);
            msg.AddData((string)serverpassword);
            msg.AddData((string)playername);
            msg.AddData((int)General.config.ReadSetting("snapsspeed", 20));
            msg.AddData((bool)General.autoswitchweapon);
            msg.Send();
        }

        // Wait for StartGameInfo answer
        while(timeout > SharedGeneral.GetCurrentTime())
        {
            // Wait for a message
            rep = WaitForMessage(timeout);
            if(rep != null)
            {
                // Check for StartGameInfo message
                if(rep.Command == MsgCmd.StartGameInfo)
                {
                    // Send rcon login password if also the server
                    if(serverrunning)
                    {
                        // Send rcon arguments to server
                        rconmsg = conn.CreateMessage(MsgCmd.Command, true);
                        rconmsg.AddData("login " + server.RConPassword);
                        rconmsg.Send();
                    }

                    // Apply the server information
                    servertitle = rep.GetString();
                    serverwebsite = rep.GetString();
                    playername = rep.GetString();
                    mapname = rep.GetString();
                    maptitle = rep.GetString();
                    gametype = (GAMETYPE)rep.GetByte();
                    scorelimit = rep.GetInt();
                    maxclients = rep.GetShort();
                    localclientid = rep.GetByte();
                    teamgame = rep.GetBool();
                    joinsmallestteam = rep.GetBool();

                    // Return success
                    reason = "";
                    return true;
                }

                // Check if connection refused
                if(rep.Command == MsgCmd.ConnectRefused)
                {
                    // Return reason
                    reason = rep.GetString();
                    return false;
                }
            }
        }

        // Timed out
        reason = "Connection request timed out";
        return false;
    }

    // This waits for a messages and returns it
    // or returns null when timeout reached
    private static NetMessage WaitForMessage(int timeouttime)
    {
        NetMessage msg = null;

        // Wait for reply
        while((gateway != null) && (timeouttime > SharedGeneral.GetCurrentTime()))
        {
            // Process networking
            if(serverrunning) server.Process();
            gateway.Process();

            // Get message and return it
            msg = gateway.GetNextMessage();
            if(msg != null) return msg;

            // Allow events
            Application.DoEvents();
            Thread.Sleep(10);
        }

        // Nothing
        return null;
    }

    // This waits for all reliable messages to be confirmed
    // or until timeout reached
    private static void WaitForConfirms(int timeouttime)
    {
        // Wait for confirms
        while((gateway != null) && (conn != null) && (timeouttime > SharedGeneral.GetCurrentTime()) && !conn.Disposed && (conn.QueueLength > 0))
        {
            // Process networking
            if(serverrunning) server.Process();
            gateway.Process();

            // Allow events
            Application.DoEvents();
            Thread.Sleep(10);
        }
    }

    // This establishes a connection with the server
    // Returns the connection id on success, otherwise returns 0
    public static int Connect(out string reason)
    {
        NetMessage msg, rep;
        int timeout = SharedGeneral.GetCurrentTime() + CONNECT_TIMEOUT;
        int resend;
        int connectid = 0;
        int port = 0;
        int simping, simloss;

        // Initial client state settings
        gametype = GAMETYPE.DM;
        disconnectreason = "";

        // Set specific port number if configured
        if(config.ReadSetting("fixedclientport", false))
            port = config.ReadSetting("clientport", 0);

        // Get lag/loss simulation settings
        simping = config.ReadSetting("simulateping", 0);
        simloss = config.ReadSetting("simulateloss", 0);

        // Make the gateway and connection
        gateway = new ClientGateway(port, simping, simloss);
        IPEndPoint target = new IPEndPoint(IPAddress.Parse(serveraddress), serverport);
        conn = gateway.CreateConnection(target);
        conn.SetTimeout(CONNECT_TIMEOUT);
        conn.ShowDataMeasures = config.ReadSetting("shownetstats", false);

        // Send connection request
        msg = conn.CreateMessage(MsgCmd.ConnectRequest, false);
        msg.AddData(Gateway.PROTOCOL_VERSION);
        msg.Send();

        // If no other reason is given, the reason is timeout
        reason = "Connection request timed out";

        // Keep waiting until timeout
        while((timeout > SharedGeneral.GetCurrentTime()) && (gateway != null))
        {
            // Wait for an answer
            resend = SharedGeneral.GetCurrentTime() + CONNECT_RESEND_INTERVAL;
            rep = WaitForMessage(resend);

            // Message received?
            if(rep != null)
            {
                // Check if this is a correct reply
                if(rep.Command == MsgCmd.ConnectConfirm)
                {
                    // Connected
                    reason = "";
                    connectid = rep.GetInt();
                    break;
                }
                // Check if connection refused
                else if(rep.Command == MsgCmd.ConnectRefused)
                {
                    // Connection refused
                    reason = rep.GetString();
                    break;
                }
            }
            else
            {
                // Resend the request
                if(gateway != null) msg.Send();
            }
        }

        // Disconnect if attempt failed
        if(connectid == 0) Disconnect(false);

        // Return result
        return connectid;
    }

    // This disconnects from the server
    public static void Disconnect(bool notifyserver)
    {
        // Should we notify the server we are disconnecting?
        if(notifyserver && (gateway != null) && (conn != null) && !conn.Disposed)
        {
            // Clear reliable messages
            conn.ResetQueue(false);

            // Send reliable disconnect message
            NetMessage msg = conn.CreateMessage(MsgCmd.Disconnect, true);
            msg.Send();

            // Process networking until timeout or confirmed
            WaitForConfirms(SharedGeneral.GetCurrentTime() + DISCONNECT_TIMEOUT);
        }

        // Dispose networking
        if(gateway != null) gateway.Dispose();
        gateway = null;
        conn = null;
    }

    #endregion

    #region ================== Clients

    // This ensures a client is set at the given id
    // and makes an unknown client if the slot is null
    public static void EnsureClient(int id)
    {
        // Check if slot is null
        if(clients[id] == null)
        {
            // Make an unknown client
            clients[id] = new Client(id, true, TEAM.NONE, UNKNOWN_PLAYER_NAME, false);
        }
    }

    // This removes all playing actors
    public static void RemovePlayingActors()
    {
        // Go for all clients
        foreach(Client c in clients)
        {
            // Remove actor
            if(c != null) c.DestroyActor(true);
        }
    }

    // This stops all playing actors
    public static void StopPlayingActors()
    {
        // Go for all clients
        foreach(Client c in clients)
        {
            // Stop actor
            if(c != null) c.StopActor();
        }
    }

    // This tests a ray for collision with a client
    public static bool FindRayPlayerCollision(Vector3D start, Vector3D end, Actor exclude, ref Vector3D point, ref object obj, ref float u)
    {
        float uray, delta2dlensq;
        Vector3D intp, delta;
        Vector2D delta2d;
        bool found = false;

        // Get trajectory length
        delta = end - start;
        delta2d = (Vector2D)delta;
        delta2dlensq = delta2d.LengthSq();

        // Go for all players
        foreach(Client c in General.clients)
        {
            // This client in the game and not the excluded client
            if((c != null) && (c.Actor != exclude) && (c.Actor != null))
            {
                // Calculate intersection offset
                uray = ((c.Actor.Position.x - start.x) * (end.x - start.x) + (c.Actor.Position.y - start.y) * (end.y - start.y)) / delta2dlensq;
                if((uray > 0f) && (uray < 1f) && (uray < u))
                {
                    // Calculate intersection point
                    intp = start + (delta * uray);

                    // Check if within Z heights
                    if((intp.z > c.Actor.Position.z) && (intp.z < (c.Actor.Position.z + Consts.PLAYER_HEIGHT)))
                    {
                        // Calculate 2D distance from collision to player
                        Vector2D dist = (Vector2D)intp - (Vector2D)c.Actor.Position;

                        // Check if close enough to collide
                        if(dist.Length() < Consts.PLAYER_RADIUS)
                        {
                            // Keep collision point
                            u = uray;
                            obj = c;
                            point = intp;
                            found = true;
                        }
                    }
                }
            }
        }

        // Return result
        return found;
    }

    #endregion

    #region ================== Networking

    // This processes incoming messages
    public static void ProcessNetworking()
    {
        NetMessage msg;

        // Process gateway
        if(gateway != null) gateway.Process();

        // Still connected?
        if((conn != null) && !conn.Disposed)
        {
            // Get next message
            while((gateway != null) && ((msg = gateway.GetNextMessage()) != null))
            {
                // Handle message
                HandleNetworkMessage(msg);
            }
        }
        else
        {
            // Connection timed out!
            gamewindow.Close();
            disconnectreason = "Connection to the game server timed out.";
        }
    }

    // Handle a single message
    public static void HandleNetworkMessage(NetMessage msg)
    {
        // Message coming from the server?
        if(msg.Connection == General.conn)
        {
            // Handle message command
            switch(msg.Command)
            {
                case MsgCmd.ShowMessage: hShowMessage(msg); break;
                case MsgCmd.SayMessage: hSayMessage(msg); break;
                case MsgCmd.Disconnect: hDisconnect(msg); break;
                case MsgCmd.ConnectRefused: hDisconnect(msg); break;
                case MsgCmd.ClientUpdate: hClientUpdate(msg); break;
                case MsgCmd.ClientDisposed: hClientDisposed(msg); break;
                case MsgCmd.ChangeTeam: hChangeTeam(msg); break;
                case MsgCmd.SpawnActor: hSpawnActor(msg); break;
                case MsgCmd.Snapshot: hSnapshots(msg); break;
                case MsgCmd.SectorMovement: hSectorMovement(msg); break;
                case MsgCmd.ClientCorrection: hClientCorrection(msg); break;
                case MsgCmd.ItemPickup: hItemPickup(msg); break;
                case MsgCmd.StatusUpdate: hStatusUpdate(msg); break;
                case MsgCmd.ClientDead: hClientDead(msg); break;
                case MsgCmd.TakeDamage: hTakeDamage(msg); break;
                case MsgCmd.GameStateChange: hGameStateChange(msg); break;
                case MsgCmd.TeleportClient: hTeleportClient(msg); break;
                case MsgCmd.SwitchWeapon: hSwitchWeapon(msg); break;
                case MsgCmd.SpawnProjectile: hSpawnProjectile(msg); break;
                case MsgCmd.UpdateProjectile: hUpdateProjectile(msg); break;
                case MsgCmd.TeleportProjectile: hTeleportProjectile(msg); break;
                case MsgCmd.DestroyProjectile: hDestroyProjectile(msg); break;
                case MsgCmd.ShieldHit: hShieldHit(msg); break;
                case MsgCmd.DamageGiven: hDamageGiven(msg); break;
                case MsgCmd.PowerupCountUpdate: hPowerupCountUpdate(msg); break;
                case MsgCmd.FireIntensity: hFireIntensity(msg); break;
                case MsgCmd.ScoreFlag: hScoreFlag(msg); break;
                case MsgCmd.ReturnFlag: hReturnFlag(msg); break;
                case MsgCmd.MapChange: hMapChange(msg); break;
                case MsgCmd.GameSnapshot: hGameSnapshot(msg); break;
                case MsgCmd.PlayerNameChange: hPlayerNameChange(msg); break;
                case MsgCmd.CallvoteStatus: hCallvoteStatus(msg); break;
                case MsgCmd.CallvoteEnd: hCallvoteEnd(msg); break;

                default:
                    //if(General.console != null) General.console.AddMessage("Unknown command message " + (int)msg.Cmd);
                    break;
            }
        }
    }

    // Call vote status
    private static void hCallvoteStatus(NetMessage msg)
    {
        // Read information
        string desc = msg.GetString();
        int votes = msg.GetInt();
        int timeleft = msg.GetInt();

        // Set the time left
        General.callvotetimeout = SharedGeneral.currenttime + timeleft;

        // Set the number of votes
        General.callvotes = votes;

        // Set the description
        General.hud.CallVoteDescription = desc;

        // Make bleep sound
        SoundSystem.PlaySound("messagebeep.wav");
    }

    // Call vote ended
    private static void hCallvoteEnd(NetMessage msg)
    {
        // Remove the call vote
        General.callvotetimeout = 0;
        General.callvotes = 0;
    }

    // Player Name Change
    private static void hPlayerNameChange(NetMessage msg)
    {
        Client player = null;

        // Read information
        int id = msg.GetInt();
        string newname = msg.GetString();

        // Ensure a client exists on this slot
        if((id < 255) && (id > -1)) EnsureClient(id);

        // Get player object
        player = clients[id];

        // Client renamed
        General.console.AddMessage(player.Name + "^7 renamed to " + newname);

        // Local player?
        if(player == General.localclient)
        {
            // Change name locally
            General.playername = newname;
        }

        // Change player name and update scoreboard
        player.SetName(newname);
        General.scoreboard.Update();
    }

    // Game snapshot
    private static void hGameSnapshot(NetMessage msg)
    {
        // Apply team scores
        teamscore[1] = msg.GetInt();
        teamscore[2] = msg.GetInt();
    }

    // Map has changed
    private static void hMapChange(NetMessage msg)
    {
        string reason;

        // Apply new map information
        mapname = msg.GetString();
        maptitle = msg.GetString();

        // Spectate
        General.localclient.DestroyActor(false);
        General.localclient.Team = TEAM.NONE;
        General.localclient.IsSpectator = true;
        hud.ShowModeMessage();

        // Start network processor
        // This will answer pings while map is being loaded
        StartNetworkProcessor();

        // Unload the map
        UnloadMap();

        // Load the new map
        reason = LoadMap();

        // Stop network processor
        StopNetworkProcessor();

        // Check game loading result
        if(reason != "")
        {
            General.OutputCustomError(reason);
            return;
        }
    }

    // Flag scored
    private static void hScoreFlag(NetMessage msg)
    {
        // Read information
        int id = msg.GetByte();
        string itemkey = msg.GetString();

        // Ensure a client exists on this slot
        EnsureClient(id);

        // Find the item
        Item i = arena.GetItemByKey(itemkey);
        if(i is Flag)
        {
            // Score the flag
            ((Flag)i).Return(clients[id], true);
        }
        else
        {
            // Problem! Item does not exist!
            if(General.console != null) General.console.AddMessage("Unknown item " + itemkey + " to score!");
        }
    }

    // Flag returned
    private static void hReturnFlag(NetMessage msg)
    {
        // Read information
        int id = msg.GetByte();
        string itemkey = msg.GetString();

        // Ensure a client exists on this slot
        if(id < 255) EnsureClient(id);

        // Find the item
        Item i = arena.GetItemByKey(itemkey);
        if(i is Flag)
        {
            // Score the flag
            if(id < 255) ((Flag)i).Return(clients[id], false);
            else ((Flag)i).Return(null, false);
        }
        else
        {
            // Problem! Item does not exist!
            if(General.console != null) General.console.AddMessage("Unknown item " + itemkey + " to return!");
        }
    }

    // Fire intensity update
    private static void hFireIntensity(NetMessage msg)
    {
        // Read update information
        int id = msg.GetByte();
        int intensity = msg.GetShort();

        // Ensure a client exists on this slot
        EnsureClient(id);

        // Update intensity
        if(intensity < 1000) intensity = 1000;
        if(clients[id].Actor != null) clients[id].Actor.SetOnFire(intensity);
    }

    // Powerup countdown update
    private static void hPowerupCountUpdate(NetMessage msg)
    {
        // Read update information
        int attime = msg.GetInt();
        int countdown = msg.GetInt();
        bool powerupfired = msg.GetBool();

        // Local client available?
        if(General.localclient != null)
        {
            // Calculate and apply current countdown time
            General.localclient.SetPowerupCountdown(countdown + (SharedGeneral.currenttime - attime), powerupfired);
        }
    }

    // Damage given to player
    private static void hDamageGiven(NetMessage msg)
    {
        // Play hit sound
        SoundSystem.PlaySound("hitplayer.wav");
    }

    // Shield Hit
    private static void hShieldHit(NetMessage msg)
    {
        // Read hit information
        int id = msg.GetByte();
        float angle = msg.GetFloat();
        float fadeout = msg.GetFloat();

        // Ensure a client exists on this slot
        EnsureClient(id);

        // Client has an actor?
        if(clients[id].Actor != null)
        {
            // Spawn shit hit effect
            new ShieldEffect(clients[id].Actor, angle, fadeout);
        }
    }

    // Spawn a projectile
    private static void hSpawnProjectile(NetMessage msg)
    {
        Projectile p = null;

        // Read projectile information
        string pid = msg.GetString();
        PROJECTILE type = (PROJECTILE)(int)msg.GetByte();
        float px = msg.GetFloat();
        float py = msg.GetFloat();
        float pz = msg.GetFloat();
        float vx = msg.GetFloat();
        float vy = msg.GetFloat();
        float vz = msg.GetFloat();
        int source = msg.GetByte();
        TEAM team = (TEAM)(int)msg.GetByte();

        // Ensure a client exists on this slot
        //EnsureClient(source);

        // Create vectors
        Vector3D pos = new Vector3D(px, py, pz);
        Vector3D vel = new Vector3D(vx, vy, vz);

        // Create projectile
        p = General.arena.GetProjectile(pid);
        if(p == null) p = General.arena.CreateProjectile(type, pid, pos, vel);
        else p.Update(pos, vel);

        // Apply settings
        p.SourceID = source;
        p.Team = team;
    }

    // Update a projectile
    private static void hUpdateProjectile(NetMessage msg)
    {
        Projectile p = null;

        // Read projectile information
        string pid = msg.GetString();
        PROJECTILE type = (PROJECTILE)(int)msg.GetByte();
        float px = msg.GetFloat();
        float py = msg.GetFloat();
        float pz = msg.GetFloat();
        float vx = msg.GetFloat();
        float vy = msg.GetFloat();
        float vz = msg.GetFloat();
        int source = msg.GetByte();
        TEAM team = (TEAM)(int)msg.GetByte();

        // Ensure a client exists on this slot
        //EnsureClient(source);

        // Create vectors
        Vector3D pos = new Vector3D(px, py, pz);
        Vector3D vel = new Vector3D(vx, vy, vz);

        // Find the projectile
        p = General.arena.GetProjectile(pid);
        if(p == null) p = General.arena.CreateProjectile(type, pid, pos, new Vector3D());

        // Apply settings
        p.SourceID = source;
        p.Team = team;

        // Update it
        if(p != null) p.Update(pos, vel);
    }

    // Destroy a projectile
    private static void hTeleportProjectile(NetMessage msg)
    {
        Projectile p = null;

        // Read projectile information
        string pid = msg.GetString();
        PROJECTILE type = (PROJECTILE)(int)msg.GetByte();
        float ox = msg.GetFloat();
        float oy = msg.GetFloat();
        float oz = msg.GetFloat();
        float px = msg.GetFloat();
        float py = msg.GetFloat();
        float pz = msg.GetFloat();
        float vx = msg.GetFloat();
        float vy = msg.GetFloat();
        float vz = msg.GetFloat();

        // Create vectors
        Vector3D oldpos = new Vector3D(ox, oy, oz);
        Vector3D pos = new Vector3D(px, py, pz);
        Vector3D vel = new Vector3D(vx, vy, vz);

        // Find the projectile
        p = General.arena.GetProjectile(pid);
        if(p == null) General.arena.CreateProjectile(type, pid, oldpos, new Vector3D());

        // Teleport it
        if(p != null) p.TeleportTo(oldpos, pos, vel);
    }

    // Destroy a projectile
    private static void hDestroyProjectile(NetMessage msg)
    {
        Projectile p = null;
        Client hitplayer = null;

        // Read projectile information
        string pid = msg.GetString();
        PROJECTILE type = (PROJECTILE)(int)msg.GetByte();
        bool silent = msg.GetBool();
        byte hitplayerid = msg.GetByte();
        float px = msg.GetFloat();
        float py = msg.GetFloat();
        float pz = msg.GetFloat();

        // Get the player being hit
        if(hitplayerid < 255)
        {
            // Ensure clients exist
            EnsureClient(hitplayerid);

            // Get player object
            hitplayer = clients[hitplayerid];
        }

        // Create vectors
        Vector3D pos = new Vector3D(px, py, pz);

        // Find the projectile
        p = General.arena.GetProjectile(pid);
        if(p == null) General.arena.CreateProjectile(type, pid, pos, new Vector3D());

        // Destroy it
        if(p != null) p.Destroy(pos, silent, hitplayer);
    }

    // Client switches weapon
    private static void hSwitchWeapon(NetMessage msg)
    {
        // Get the weapon id from message
        WEAPON weaponid = (WEAPON)msg.GetInt();
        bool silent = msg.GetBool();

        // Switch weapon
        General.localclient.SwitchWeapon(weaponid, silent);
    }

    // Client teleports
    private static void hTeleportClient(NetMessage msg)
    {
        // Get the ID from message
        int id = msg.GetByte();

        // Ensure a client exists on this slot
        EnsureClient(id);

        // Spawn actor from message
        clients[id].Teleport(msg);
    }

    // Handle game state change
    private static void hGameStateChange(NetMessage msg)
    {
        // Get the new gamestate
        GAMESTATE newstate = (GAMESTATE)msg.GetByte();
        int gamestatelen = msg.GetInt();

        // Time until gamestate timeout
        gamestateend = SharedGeneral.currenttime + gamestatelen;

        // When round or game is finished
        if((newstate == GAMESTATE.ROUNDFINISH) ||
           (newstate == GAMESTATE.GAMEFINISH))
        {
            // Stop all playing actors and release weapons
            StopPlayingActors();
            General.localclient.ReleaseAllWeapons();
            RemovePlayingActors();

            // Set auto-screenshot time
            screenshottime = realtime + AUTO_SCREENSHOT_DELAY;
        }

        // Changing to new game?
        if(((gamestate == GAMESTATE.ROUNDFINISH) ||
            (gamestate == GAMESTATE.GAMEFINISH)) &&
           (newstate == GAMESTATE.WAITING)) RemovePlayingActors();

        // Countdown ended?
        if ((gamestate == GAMESTATE.COUNTDOWN) &&
            (newstate == GAMESTATE.PLAYING))
        {
            // Show FIGHT!
            hud.ShowBigMessage("FIGHT!", 1000);
            SoundSystem.PlaySound("voc_fight.wav");

            // Remove all actors
            arena.RespawnAllItems();
            RemovePlayingActors();

            // Reset team scores
            teamscore[1] = 0;
            teamscore[2] = 0;
        }

        // Or new game just started playing?
        if((gamestate == GAMESTATE.SPAWNING) &&
           (newstate == GAMESTATE.PLAYING))
        {
            // Just reset team scores
            teamscore[1] = 0;
            teamscore[2] = 0;
        }

        // Apply the new gamestate
        gamestate = newstate;

        // Update the HUD
        General.hud.ShowModeMessage();
    }

    // Handle client death
    private static void hClientDead(NetMessage msg)
    {
        int sourcescore = 0;
        int targetscore = 0;

        // Get the arguments
        int targetid = msg.GetByte();
        int sourceid = msg.GetByte();
        string message = msg.GetString();
        DEATHMETHOD method = (DEATHMETHOD)msg.GetByte();
        float posx = msg.GetFloat();
        float posy = msg.GetFloat();
        float posz = msg.GetFloat();
        float velx = msg.GetFloat();
        float vely = msg.GetFloat();
        float velz = msg.GetFloat();
        float pushx = msg.GetFloat();
        float pushy = msg.GetFloat();

        // Ensure the clients exist
        if(sourceid < 255) EnsureClient(sourceid);
        if(targetid < 255) EnsureClient(targetid);

        // Set client position/velocity
        if(clients[targetid].Actor != null)
        {
            // Apply position/velocity
            Actor act = clients[targetid].Actor;
            act.State.pos = new Vector3D(posx, posy, posz);
            act.State.vel = new Vector3D(velx, vely, velz);
            act.PushVector = new Vector2D(pushx, pushy);
        }

        // Kill this client
        clients[targetid].Kill(method);

        // Show the message
        General.console.AddMessage(message, true);

        // Am I the source?
        if(sourceid == General.localclient.ID)
        {
            // Did I kill myself?
            if(targetid == sourceid)
            {
                // I am an idiot
                //General.hud.ShowSmallMessage("^7You killed yourself", HUD.MSG_DEATH_TIMEOUT);

                // Adjust score
                if((General.gametype == GAMETYPE.DM) ||
                   (General.gametype == GAMETYPE.TDM)) sourcescore = -1;

                // Make red flash
                if(method == DEATHMETHOD.NORMAL) General.hud.FlashScreen(0.6f);
                if(method == DEATHMETHOD.GIBBED) General.hud.FlashScreen(2f);
            }
            else
            {
                // I killed someone! yay!
                General.hud.ShowSmallMessage("^7You killed " + clients[targetid].Name, HUD.MSG_DEATH_TIMEOUT);

                // Not on the same team?
                if((clients[targetid].Team != clients[sourceid].Team) || !teamgame)
                {
                    // Count a frag for me
                    clients[sourceid].Frags++;

                    // Adjust score
                    if((General.gametype == GAMETYPE.DM) ||
                       (General.gametype == GAMETYPE.TDM)) sourcescore = 1;
                }
            }
        }
        // Am I the target?
        else if(targetid == General.localclient.ID)
        {
            // Did someone kill me?
            if(sourceid < 255)
            {
                // I got owned
                General.hud.ShowSmallMessage("^7You were killed by " + clients[sourceid].Name, HUD.MSG_DEATH_TIMEOUT);

                // Not on the same team?
                if((clients[targetid].Team != clients[sourceid].Team) || !teamgame)
                {
                    // Count a frag for him
                    clients[sourceid].Frags++;

                    // Adjust score
                    if((General.gametype == GAMETYPE.DM) ||
                       (General.gametype == GAMETYPE.TDM)) sourcescore = 1;
                }
            }
            else
            {
                // Some force of nature killed me
                //General.hud.ShowSmallMessage("^7You committed suicide!", HUD.MSG_DEATH_TIMEOUT);

                // Adjust score
                if((General.gametype == GAMETYPE.DM) ||
                   (General.gametype == GAMETYPE.TDM)) targetscore = -1;
            }

            // Make red flash
            if(method == DEATHMETHOD.NORMAL) General.hud.FlashScreen(0.6f);
            if(method == DEATHMETHOD.GIBBED) General.hud.FlashScreen(2f);
        }
        else
        {
            // This kill is not related to me
            if((General.gametype == GAMETYPE.DM) ||
               (General.gametype == GAMETYPE.TDM))
            {
                // Add to score when not died by force of nature and not killing myself
                if((sourceid < 255) && (targetid != sourceid)) sourcescore = 1; else targetscore = -1;
            }
        }

        // Count the death
        clients[targetid].Deaths++;

        // Scavenger mode: loser loses 10 points!
        if((General.gametype == GAMETYPE.SC) ||
           (General.gametype == GAMETYPE.TSC)) targetscore = -10;

        // Change source score
        if(sourceid < 255)
        {
            clients[sourceid].Score += sourcescore;
            if(General.teamgame) General.teamscore[(int)clients[sourceid].Team] += sourcescore;
        }

        // Change target score
        if(targetid < 255)
        {
            clients[targetid].Score += targetscore;
            if(General.teamgame) General.teamscore[(int)clients[targetid].Team] += targetscore;
        }

        // Update scoreboard
        General.scoreboard.Update();
    }

    // Client takes damage
    private static void hTakeDamage(NetMessage msg)
    {
        byte targetid = msg.GetByte();

        // Ensure the clients exist
        EnsureClient(targetid);

        // Take damage
        clients[targetid].TakeDamage(msg);
    }

    // Handle full Status Update
    private static void hStatusUpdate(NetMessage msg)
    {
        // Health and armor
        General.localclient.Health = msg.GetByte();
        General.localclient.Armor = msg.GetByte();

        // Ammo
        for(int i = 0; i < (int)AMMO.TOTAL_AMMO_TYPES; i++)
        {
            // Update ammo
            General.localclient.Ammo[i] = msg.GetShort();
        }

        // Update ammo display
        General.weapondisplay.UpdateAmmo();

        // Weapons
        for(int i = 0; i < (int)WEAPON.TOTAL_WEAPONS; i++)
        {
            // Get weapon status from message
            if(msg.GetBool())
            {
                // Add weapon if not yet added
                General.localclient.GiveWeapon((WEAPON)i);
            }
            else
            {
                // Remove weapon if exists
                General.localclient.RemoveWeapon((WEAPON)i);
            }
        }
    }

    // Handle ItemPickup
    private static void hItemPickup(NetMessage msg)
    {
        // Get the arguments
        int clientid = msg.GetByte();
        string itemkey = msg.GetString();
        int delay = msg.GetInt();
        bool attach = msg.GetBool();
        bool silent = msg.GetBool();

        // Find the item
        Item i = arena.GetItemByKey(itemkey);
        if(i != null)
        {
            // Ensure a client exists on this slot
            EnsureClient(clientid);

            // Pickup item
            i.Pickup(clients[clientid], delay, attach, silent);
        }
        else
        {
            // Problem! Item does not exist!
            if(General.console != null) General.console.AddMessage("Unknown item " + itemkey + " to pick up!");
        }
    }

    // Handle ClientCorrection
    private static void hClientCorrection(NetMessage msg)
    {
        // Perform client correction for local client
        General.localclient.ClientCorrection(msg);
    }

    // Handle Sector Movement
    private static void hSectorMovement(NetMessage msg)
    {
        // Continue reading while not at the end of the message
        while(!msg.EndOfMessage)
        {
            // Get the target height and movement speed
            int sectorid = msg.GetInt();
            float targetheight = msg.GetFloat();
            float movespeed = msg.GetFloat();

            // Move the sector
            General.map.Sectors[sectorid].MoveTo(targetheight, movespeed);
        }
    }

    // Handle Snapshots
    private static void hSnapshots(NetMessage msg)
    {
        // Continue until no more data in message
        while(!msg.EndOfMessage)
        {
            // Get the client id
            int id = msg.GetByte();

            // Ensure a client exists here
            EnsureClient(id);

            // Spawn actor from message
            clients[id].GetSnapshotFromMessage(msg);
        }
    }

    // Handle SpawnActor
    private static void hSpawnActor(NetMessage msg)
    {
        // Get the ID from message
        int id = msg.GetByte();

        // Ensure a client exists on this slot
        EnsureClient(id);

        // Spawn actor from message
        clients[id].SpawnActor(msg);
    }

    // Handle ChangeTeam
    private static void hChangeTeam(NetMessage msg)
    {
        // Read data
        TEAM t = (TEAM)msg.GetByte();
        bool s = msg.GetBool();

        // Lose actor
        General.localclient.DestroyActor(false);

        // Apply settings to client
        General.localclient.Team = t;
        General.localclient.IsSpectator = s;
        General.localclient.SetName(General.localclient.Name);
        hud.ShowModeMessage();

        // Update scores on HUD
        hud.UpdateScore();
    }

    // Handle ClientDisposed
    private static void hClientDisposed(NetMessage msg)
    {
        // Get the ID from message
        int id = msg.GetByte();

        // Dispose this client
        if(clients[id] != null) clients[id].Dispose();
    }

    // Handle ClientUpdate
    private static void hClientUpdate(NetMessage msg)
    {
        // Get the ID from message
        int id = msg.GetByte();

        // Ensure a client exists on this slot
        EnsureClient(id);

        // Update client from message
        clients[id].UpdateFromMessage(msg);

        // Update scoreboard
        General.scoreboard.Update();
    }

    // Handle ShowMessage
    private static void hShowMessage(NetMessage msg)
    {
        bool showonscreen;
        string message;

        try
        {
            // Read the parameters
            showonscreen = msg.GetBool();
            message = msg.GetString();
        }
        catch(Exception) { return; }

        // Show the message
        console.AddMessage(message, showonscreen);
    }

    // Handle SayMessage
    private static void hSayMessage(NetMessage msg)
    {
        string message;

        try
        {
            // Read the parameters
            message = msg.GetString();
        }
        catch(Exception) { return; }

        // Show the message
        console.AddMessage(message, true);

        // Make message sound
        SoundSystem.PlaySound("messagebeep.wav");
    }

    // Handle Disconnect
    private static void hDisconnect(NetMessage msg)
    {
        try
        {
            // Read the parameters
            disconnectreason = "Disconnected from server: " + msg.GetString();
        }
        catch(Exception) { return; }

        // End the game
        gamewindow.Close();
    }

    #endregion

    #region ================== Game Loop

    // This is the main game loop
    private static void GeneralLoop()
    {
        // Continue while a client or server is running
        while(clientrunning || serverrunning)
        {
            // Process and render a single frame
            DoOneFrame(true, true, true);

            // Process messages
            Application.DoEvents();
        }
    }

    // This makes adjustments to catch up any lag
    public static void CatchLag()
    {
        // Reset previous time to fix the next delta time
        previoustime = GetCurrentTime();
        accumulator = 0;
    }

    // This processes and renders a single frame
    public static void DoOneFrame(bool process, bool render, bool renderhud)
    {
        int deltatime;

        // Calculate the frame time
        realtime = GetCurrentTime();
        deltatime = realtime - previoustime;
        previoustime = realtime;
        accumulator += deltatime;

        // Do processing?
        if(process)
        {
            // Always process networking
            if(serverrunning) server.gateway.Process();
            if(clientrunning) gateway.Process();

            // Enough delta time for processing the game?
            while(accumulator >= Consts.TIMESTEP)
            {
                // Advance time
                currenttime += Consts.TIMESTEP;

                // Process a server pass
                if(serverrunning) server.Process();

                // Process a client pass
                if(clientrunning)
                {
                    ProcessNetworking();
                    SoundSystem.Process();
                    arena.Process();
                    hud.Process();
                    scoreboard.Process();
                    chatbox.Process();
                    console.Process();
                    weapondisplay.Process();
                    gamemenu.Process();
                    if(jukebox != null) jukebox.Process();
                }

                // Time processed
                accumulator -= Consts.TIMESTEP;
            }
        }

        // Client?
        if(clientrunning)
        {
            // Render the screen
            if(render)
            {
                // Begin rendering procedure if possible
                if(SharpDX.Direct3D9.Direct3D.StartRendering())
                {
                    // Prepare for rendering
                    if(arena != null) arena.PrepareRendering();

                    // Begin scene rendering
                    SharpDX.Direct3D9.Direct3D.d3dd.SetRenderTarget(0, SharpDX.Direct3D9.Direct3D.backbuffer);
                    SharpDX.Direct3D9.Direct3D.d3dd.DepthStencilSurface = SharpDX.Direct3D9.Direct3D.depthbuffer;
                    SharpDX.Direct3D9.Direct3D.d3dd.BeginScene();


                    // Render a game frame
                    if(arena != null)
                        arena.Render();
                    else
                        RenderBackground();

                    // Render the HUD and Console
                    if(renderhud) hud.RenderScreenFlashes();
                    if(!scoreboard.Visible) hud.RenderMessages();
                    if(renderhud) hud.RenderStatus();
                    if(renderhud) hud.RenderFPS();
                    if(renderhud) weapondisplay.Render();
                    if(renderhud) scoreboard.Render();
                    if(renderhud) gamemenu.Render();
                    if(renderhud) console.Render();
                    if(renderhud) chatbox.Render();

                    // Render the mouse cursor
                    if(gamewindow.MouseInside && renderhud) MouseCursor.Render();


                    // Unset textures and streams
                    SharpDX.Direct3D9.Direct3D.d3dd.SetTexture(0, null);
                    SharpDX.Direct3D9.Direct3D.d3dd.SetTexture(1, null);
                    SharpDX.Direct3D9.Direct3D.d3dd.SetStreamSource(0, null, 0, 0);

                    // Done rendering
                    SharpDX.Direct3D9.Direct3D.d3dd.EndScene();

                    // Present the scene
                    SharpDX.Direct3D9.Direct3D.FinishRendering();

                    // Time to make a screenshot?
                    if((screenshottime > 0) && (screenshottime < realtime) && autoscreenshot)
                    {
                        // Make a screenshot
                        screenshottime = 0;
                        console.ProcessInput("/screenshot");
                    }
                }
                // When not possible to render...
                else
                {
                    // Wait a bit
                    Thread.Sleep(10);
                }
            }
        }
        else
        {
            // Wait a bit
            Thread.Sleep(2);
        }
    }

    // This renders the background
    private static void RenderBackground()
    {
        // Determine target position
        Point pos = new Point((int)((float)(SharpDX.Direct3D9.Direct3D.DisplayWidth - background.Width) * 0.5f),
            (int)((float)(SharpDX.Direct3D9.Direct3D.DisplayHeight - background.Height) * 0.5f));

        // Draw background logo on screen
        try { SharpDX.Direct3D9.Direct3D.d3dd.UpdateSurface(background.surface, null, SharpDX.Direct3D9.Direct3D.backbuffer, pos); }
        catch(Exception) { }
    }

    #endregion

    #region ================== Game Loading

    // This loads generic resources
    private static bool LoadGenericResources()
    {
        // Show loading screen (hud message)
        hud.ShowSmallMessage("Loading graphics...", 0);
        hud.ShowBigMessage("", 0);
        DoOneFrame(false, true, false);

        // Load generic images/textures
        General.console_edge = SharpDX.Direct3D9.Direct3D.LoadTexture(ArchiveManager.ExtractFile("general.zip/console.bmp"), false, true);
        Shadow.texture = SharpDX.Direct3D9.Direct3D.LoadTexture(ArchiveManager.ExtractFile("general.zip/objshadow.tga"), false, true);
        StaticLight.lightshadow = SharpDX.Direct3D9.Direct3D.LoadTexture(ArchiveManager.ExtractFile("general.zip/lightshadow.tga"), true);
        Weapons.Bullet.bulletflash = SharpDX.Direct3D9.Direct3D.LoadTexture(ArchiveManager.ExtractFile("sprites/bulletflash.bmp"), true);
        Laser.texture = SharpDX.Direct3D9.Direct3D.LoadTexture(ArchiveManager.ExtractFile("sprites/laser.tga"), true);
        Laser.dottexture = SharpDX.Direct3D9.Direct3D.LoadTexture(ArchiveManager.ExtractFile("sprites/laserdot.tga"), true);
        Weapons.WLightChaingun.flaretex = SharpDX.Direct3D9.Direct3D.LoadTexture(ArchiveManager.ExtractFile("sprites/chain1flare.tga"), true);
        Weapons.WMinigun.flaretex = SharpDX.Direct3D9.Direct3D.LoadTexture(ArchiveManager.ExtractFile("sprites/chain1flare.tga"), true);
        Weapons.WPlasmaCannon.flaretex = SharpDX.Direct3D9.Direct3D.LoadTexture(ArchiveManager.ExtractFile("sprites/plasmaflare.tga"), true);
        Weapons.WRocketLauncher.flaretex = SharpDX.Direct3D9.Direct3D.LoadTexture(ArchiveManager.ExtractFile("sprites/chain1flare.tga"), true);
        Weapons.WGrenadeLauncher.flaretex = SharpDX.Direct3D9.Direct3D.LoadTexture(ArchiveManager.ExtractFile("sprites/chain1flare.tga"), true);
        WIonCannon.flaretex = SharpDX.Direct3D9.Direct3D.LoadTexture(ArchiveManager.ExtractFile("sprites/ioncannonflare.tga"), true);
        Projectiles.PlasmaBall.plasmaball = SharpDX.Direct3D9.Direct3D.LoadTexture(ArchiveManager.ExtractFile("sprites/plasmaball.tga"), true);
        Projectiles.Rocket.texbody = SharpDX.Direct3D9.Direct3D.LoadTexture(ArchiveManager.ExtractFile("sprites/rocketbody.tga"), true);
        Projectiles.Rocket.texexhaust = SharpDX.Direct3D9.Direct3D.LoadTexture(ArchiveManager.ExtractFile("sprites/rocketexhaust.tga"), true);
        Projectiles.Grenade.texbody = SharpDX.Direct3D9.Direct3D.LoadTexture(ArchiveManager.ExtractFile("sprites/grenadebody.tga"), true);
        Projectiles.IonBall.plasmaball = SharpDX.Direct3D9.Direct3D.LoadTexture(ArchiveManager.ExtractFile("sprites/ionball.tga"), true);
        Shock.texture = SharpDX.Direct3D9.Direct3D.LoadTexture(ArchiveManager.ExtractFile("sprites/shock.tga"), true);
        VisualSector.ceillightmap = SharpDX.Direct3D9.Direct3D.LoadTexture(ArchiveManager.ExtractFile("general.zip/white.bmp"), true);
        VisualSector.sectorshadowstexture = SharpDX.Direct3D9.Direct3D.LoadTexture(ArchiveManager.ExtractFile("general.zip/sectorshadow.tga"), true);
        ShieldEffect.shieldimage = SharpDX.Direct3D9.Direct3D.LoadTexture(ArchiveManager.ExtractFile("sprites/effect_shield.tga"), true);
        NukeSign.texture = SharpDX.Direct3D9.Direct3D.LoadTexture(ArchiveManager.ExtractFile("sprites/effect_nuke.tga"), true);
        FleshDebris.LoadGibLimps();
        WallDecal.LoadTextures();
        FloorDecal.LoadTextures();

        // Load actor animations
        Actor.LoadAnimations(teamgame);

        // Load animations
        Animation.Load("sprites/teleport.cfg");
        Animation.Load("sprites/rocketexplode.cfg");
        Animation.Load("sprites/ionexplode.cfg");
        Animation.Load("sprites/nukeexplode.cfg");
        Animation.Load("sprites/playerfire.cfg");
        Animation.Load("sprites/phoenixflare.cfg");
        Animation.Load("sprites/phoenixfire.cfg");
        Animation.Load("sprites/rage.cfg");

        // Light templates for static lights
        for(int i = 0; i < StaticLight.NUM_LIGHT_TEMPLATES; i++)
        {
            // Load light template
            StaticLight.lightimages[i] = SharpDX.Direct3D9.Direct3D.LoadSurfaceResource(ArchiveManager.ExtractFile("general.zip/lightimage" + i.ToString(CultureInfo.InvariantCulture) + ".bmp"), Pool.Default);
        }

        // Only when using dynamic lights
        if(DynamicLight.dynamiclights)
        {
            // Light templates for dynamic lights
            for(int i = 0; i < StaticLight.NUM_LIGHT_TEMPLATES; i++)
            {
                // Load light template
                DynamicLight.lightimages[i] = SharpDX.Direct3D9.Direct3D.LoadTexture(ArchiveManager.ExtractFile("general.zip/lightimage" + i.ToString(CultureInfo.InvariantCulture) + ".bmp"), true, false);
            }
        }

        // Create geometry
        SharpDX.Direct3D9.Sprite.CreateGeometry();
        WallDecal.CreateGeometry();
        Shadow.CreateGeometry();
        Weapons.Bullet.CreateGeometry();

        // Success
        return true;
    }

    // This unloads generic resources
    private static void UnloadGenericResources()
    {
        // Destroy generic stuff
        SharpDX.Direct3D9.Sprite.DestroyGeometry();
        WallDecal.DestroyGeometry();
        Shadow.DestroyGeometry();
        Weapons.Bullet.DestroyGeometry();

        // Clear animations
        Animation.UnloadAll();

        // Terminate the sound system
        SoundSystem.Terminate();
    }

    // This starts the map
    private static string LoadMap()
    {
        NetMessage msg, rep;
        int waittimeout;
        bool gotsnapshot = false;

        // Set long connection timeout
        conn.SetTimeout(LOADING_TIMEOUT);

        // Show loading screen (hud message)
        hud.ShowSmallMessage("Loading map...", 0);
        hud.ShowBigMessage(maptitle, 0);
        DoOneFrame(false, true, false);

        // Make clients array and add myself as client
        clients = new Client[maxclients];
        General.localclient = new Client(localclientid, true, TEAM.NONE, playername, true);
        clients[localclientid] = General.localclient;

        // Load the map
        try { map = new ClientMap(mapname, false, Paths.Instance.TempDir); }
        catch(FileNotFoundException) { return "You do not have the map \"" + mapname + "\"."; }

        // Load the arena
        arena = new Arena();

        // Render a single frame to let all lightmaps initialize
        DoOneFrame(false, true, false);

        // Show starting screen (hud message)
        hud.ShowSmallMessage("Waiting for snapshot...", 0);
        hud.ShowBigMessage("", 0);
        DoOneFrame(false, true, false);

        // Catch up lag
        CatchLag();

        // Done loading
        if(!conn.Disposed)
        {
            msg = conn.CreateMessage(MsgCmd.GameStarted, true);
            msg.Send();
        }

        // Wait for a snapshot
        waittimeout = SharedGeneral.GetCurrentTime() + 5000;
        while((conn != null) && !conn.Disposed && (waittimeout > SharedGeneral.GetCurrentTime()))
        {
            // Wait for a message
            rep = WaitForMessage(SharedGeneral.GetCurrentTime() + 1000);
            if(rep != null)
            {
                // Check for GameSnapshot message
                if(rep.Command == MsgCmd.GameSnapshot)
                {
                    // Handle message and get out of here
                    HandleNetworkMessage(rep);
                    gotsnapshot = true;
                    break;
                }
                // Check for MapChange message
                else if(rep.Command == MsgCmd.MapChange)
                {
                    // Handle message and get out of here
                    // The next LoadMap will take over now
                    HandleNetworkMessage(rep);
                    return "";
                }
                else
                {
                    // Handle message normally
                    HandleNetworkMessage(rep);
                }
            }
        }

        // Snapshot received?
        if(gotsnapshot)
        {
            // Hide the windows cursor
            gamewindow.Cursor = new Cursor(ArchiveManager.ExtractFile("general.zip/cursor_none.cur"));

            // Catch up lag
            CatchLag();

            // Set normal connection timeout
            conn.SetTimeout(Connection.DEFAULT_TIMEOUT);

            // Show welcome message
            if(serverwebsite.Trim() != "")
                console.AddMessage("Welcome to " + servertitle + "^7.  Visit us at " + serverwebsite);
            else
                console.AddMessage("Welcome to " + servertitle + "^7.");

            // Hide loading message
            hud.HideSmallMessage();
            hud.HideBigMessage();

            // Show mode message
            hud.ShowModeMessage();

            // No problems
            return "";
        }
        else
        {
            // Connection lost during load
            return "Connection to the game server was lost.";
        }
    }

    // This unloads the map
    private static void UnloadMap()
    {
        // Dispose clients
        if(clients != null)
        {
            foreach(Client c in clients) if(c != null) c.Dispose();
            clients = null;
            General.localclient = null;
        }

        // Unload arena and map
        if(arena != null) arena.Dispose(); arena = null;
        if(map != null) map.Dispose(); map = null;

        // Hide the scoreboard
        if(scoreboard != null) scoreboard.Visible = false;
    }

    // This processes network messages in the background
    private static void NetworkProcessor()
    {
        // Continue until interrupted
        while(true)
        {
            // Just process networking, but do not handle messages
            // This allows the networking system to respond to pings
            // and to buffer incoming packets.
            if(serverrunning) server.gateway.Process();
            if(clientrunning) gateway.Process();

            // Wait or leave when interrupted
            try { Thread.Sleep(100); }
            catch(Exception) { return; }
        }
    }

    // This starts the network processor
    private static void StartNetworkProcessor()
    {
        // Stop if any is running
        StopNetworkProcessor();

        // Start the network processor to answer pings
        networkproc = new Thread(new ThreadStart(NetworkProcessor));
        networkproc.Name = "NetworkProcessor";
        networkproc.Priority = ThreadPriority.AboveNormal;
        networkproc.Start();
    }

    // This stops the network processor
    private static void StopNetworkProcessor()
    {
        if(networkproc != null)
        {
            // Stop network processor
            networkproc.Interrupt();
            networkproc.Join();
            networkproc = null;
        }
    }

    #endregion

    #region ================== Startup

    // Main program entry
    // This is where the fun begins
    [STAThread] private static void Main(string[] args)
    {
        // Handle exceptions more nicely in production, but let the debugger to break by default if it's attached.
        if(Debugger.IsAttached)
        {
            // Run without exception handling
            _Main(args);
        }
        else
        {
            try
            {
                // Run within exception handlers
                _Main(args);
            }

            // Catch any errors
            catch(Exception e)
            {
                // Out of video memory?
                if(e is SharpDXException { ResultCode: var rc } && rc == ResultCode.OutOfVideoMemory)
                {
                    // Make a more descriptive error
                    var ex = new Exception("Out of video memory while loading the game. Please choose a lower screen resolution or lower graphics options.", e);

                    // Log the error
                    WriteErrorLine(ex);
                    OutputError(ex);
                }
                // Otherwise...
                else
                {
                    // Log the error
                    WriteErrorLine(e);
                    OutputError(e);
                }
            }
        }

        // Terminate game
        Terminate();

        // End of program
        Exit();
    }

    // This load the application
    private static void _Main(string[] args)
    {
        Host.Instance = new ClientHost();

        string deviceerror = null;

        // No arguments give nat all?
        if(args.Length == 0)
        {
            // No proper commands given
            // Run the standard launcher
            Process.Start(Paths.Instance.LauncherExecutablePath);
            return;
        }

        // Initialize
        if(Initialize())
        {
            try
            {
                // Load configuration
                if (LoadConfiguration())
                {
                    // Parse command line arguments
                    if (ApplyCmdConfiguration(args))
                    {
                        // Select adapter
                        SharpDX.Direct3D9.Direct3D.SelectAdapter(General.config.ReadSetting("displaydriver", 0));

                        // Validate adapter and, if not valid, select a valid adapter
                        deviceerror = SharpDX.Direct3D9.Direct3D.SelectValidAdapter();
                        if (deviceerror == null)
                        {
                            // Get the command-line instructions
                            string joinaddr = config.ReadSetting("join", "");
                            string hostfile = config.ReadSetting("host", "");
                            string dedfile = config.ReadSetting("dedicated", "");

                            // Join a game?
                            if (joinaddr != "")
                            {
                                // Joining
                                clientrunning = true;
                                serverrunning = false;

                                // Set address and port
                                string[] addrparts = joinaddr.Split(':');
                                serveraddress = addrparts[0];
                                serverport = int.Parse(addrparts[1]);

                                // Set password
                                serverpassword = config.ReadSetting("password", "");
                            }
                            // Hosting a game?
                            else if (hostfile != "")
                            {
                                // Hosting
                                clientrunning = true;
                                serverrunning = true;

                                // Correct path if needed
                                if (!File.Exists(hostfile)) hostfile = Path.Combine(Paths.Instance.ConfigDirPath, hostfile);

                                // Load server configuration
                                Configuration scfg = new Configuration(true);
                                scfg.LoadConfiguration(hostfile, true);
                                serverconfig = scfg.OutputConfiguration("", false);

                                // Set address and port
                                serveraddress = "127.0.0.1";
                                serverport = scfg.ReadSetting("port", 6969);

                                // Set password
                                serverpassword = scfg.ReadSetting("password", "");
                            }
                            // Hosting dedicated?
                            else if (dedfile != "")
                            {
                                // Hosting dedicated
                                clientrunning = false;
                                serverrunning = true;

                                // Correct path if needed
                                if (!File.Exists(dedfile)) hostfile = Path.Combine(Paths.Instance.ConfigDirPath, dedfile);

                                // Load server configuration
                                Configuration scfg = new Configuration(true);
                                scfg.LoadConfiguration(dedfile, true);
                                serverconfig = scfg.OutputConfiguration("", false);
                            }

                            // Start the game!
                            StartGame();
                        }
                        else
                        {
                            // No valid adapter exists
                            MessageBox.Show(null,
                                "You do not have a valid video device that meets the requirements for this game.\nProblem description: " +
                                deviceerror +
                                "\n\nPlease ensure you have the latest video drivers for your video card (see manufacturer website) and that Microsoft DirectX 9 is properly installed.",
                                "Bloodmasters", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            finally
            {
                SharpDX.Direct3D9.Direct3D.DeinitDirectX();
            }
        }
    }

    // This launches the game
    private static void StartGame()
    {
        bool started = false;
        disconnectreason = "";
        clients = null;
        string reason;
        connecting = true;

        // Initialize for client?
        if(clientrunning)
        {
            // Load game window
            gamewindow = new FormGame(SharpDX.Direct3D9.Direct3D.DisplayWindowed);

            // Initialize Direct3D
            if(!SharpDX.Direct3D9.Direct3D.Initialize(gamewindow)) return;

            // Initialize the sound system
            if (!SoundSystem.Initialize(gamewindow)) return;

            // Setup defaults and load standard components
            if (!LoadStandardComponents()) return;
        }

        // Discard events
        if(clientrunning) gamewindow.Update();
        Application.DoEvents();

        // Start server?
        if(serverrunning)
        {
            // Show dedicated server window?
            if(!clientrunning)
            {
                // Load server window and show it
                serverwindow = new FormServer();
                serverwindow.Show();
                serverwindow.Update();
            }
            else
            {
                // Show loading screen (hud message)
                hud.ShowSmallMessage("Starting game server...", 0);
                hud.ShowBigMessage("", 0);
                DoOneFrame(false, true, false);
            }

            // Create the server
            server = new GameServer();
            server.Initialize(serverconfig);
        }

        // Initialize for client?
        if(clientrunning)
        {
            // Show loading screen (hud message)
            hud.ShowSmallMessage("Connecting to game server...", 0);
            hud.ShowBigMessage(serveraddress + ":" + serverport, 0);
            DoOneFrame(false, true, false);

            // Connect to server
            int connectid = General.Connect(out reason);
            if(connectid == 0)
            {
                // Unable to connect!
                General.OutputCustomError("Unable to connect to the game server: " + reason);
                return;
            }
            else
            {
                // Login to server
                started = Login(connectid, out reason);
                if(started == false)
                {
                    // Unable to login!
                    General.OutputCustomError("Unable to connect to the game server: " + reason);
                    return;
                }
            }

            // Done conneccting
            connecting = false;

            // Start network processor
            // This will answer pings while game is being loaded
            StartNetworkProcessor();

            // Load generic resources
            if(!LoadGenericResources()) return;

            // Connected, now load the map
            reason = LoadMap();

            // Stop network processor
            StopNetworkProcessor();

            // Check game loading result
            if(reason != "")
            {
                General.OutputCustomError(reason);
                return;
            }
        }

        // Run the general loop
        GeneralLoop();

        // Check game result
        if(disconnectreason != "")
        {
            General.OutputCustomError(disconnectreason);
            return;
        }
    }

    #endregion

    #region ================== Misc Functions

    // This gets a description for a game type
    public static string GameTypeDescription(GAMETYPE g)
    {
        // Determine game type description
        switch(g)
        {
            case GAMETYPE.DM: return "Deathmatch";
            case GAMETYPE.CTF: return "Capture The Flag";
            case GAMETYPE.SC: return "Scavenger";
            case GAMETYPE.TDM: return "Team Deathmatch";
            case GAMETYPE.TSC: return "Team Scavenger";
            default: return "Unknown";
        }
    }

    // This returns a color for the given team number
    public static int TeamColor(TEAM t, float a)
    {
        switch(t)
        {
            case TEAM.NONE: return General.ARGB(a, 1f, 1f, 1f);
            case TEAM.RED: return General.ARGB(a, 1f, 0.1f, 0.1f);
            case TEAM.BLUE: return General.ARGB(a, 0.1f, 0.2f, 1f);
            default: return 0;
        }
    }

    // This returns the next power of 2
    public static int NextPowerOf2(int v)
    {
        int p = 0;

        // Continue increasing until higher than v
        while(Math.Pow(2, p) < v) p++;

        // Return power
        return (int)Math.Pow(2, p);
    }

    // This creates a string of random ASCII chars
    public static string RandomString(int len)
    {
        string result = "";

        // ASCII chars to use
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";

        // Make string
        for(int i = 0; i < len; i++)
        {
            result += chars[random.Next(chars.Length)];
        }

        // Returrn result
        return result;
    }

    // Make a random color
    public static int RandomColor()
    {
        return System.Drawing.Color.FromArgb((int)(random.NextDouble() * 255f), (int)(random.NextDouble() * 255f), (int)(random.NextDouble() * 255f)).ToArgb();
    }

    // Make a color from RGB
    public static int RGB(int r, int g, int b) { return System.Drawing.Color.FromArgb(r, g, b).ToArgb(); }

    // Make a color from RGB
    public static int RGB(float r, float g, float b)
    {
        return System.Drawing.Color.FromArgb((int)(r * 255f), (int)(g * 255f), (int)(b * 255f)).ToArgb();
    }

    // Make a color from ARGB
    public static int ARGB(float a, float r, float g, float b)
    {
        return System.Drawing.Color.FromArgb((int)(a * 255f), (int)(r * 255f), (int)(g * 255f), (int)(b * 255f)).ToArgb();
    }

    #endregion
}
