/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text;
using CodeImp.Bloodmasters.Server.Net;

namespace CodeImp.Bloodmasters.Server;

public sealed class GameServer
{
    #region ================== Constants

    public const int MIN_PLAYERS = 2;
    public const int DEFAULT_PORT = 6969;
    public const int CONNECT_TIMEOUT = 1000;
    public const int LOADING_TIMEOUT = 10000; //60000;
    public const int MASTER_UPDATE_INTERVAL = 60000;
    public const int MAX_CLIENTS = 10;
    public const int MAX_TIMELIMIT = 1000;
    public const string MASTER_URL = "http://www.bloodmasters.com/bloodmasterslist.php?action=update&port=%port%&time=%time%";

    // Game state times
    public const int COUNTDOWN_TIME = 10000;
    public const int ROUNDFINISH_TIME = 4000;
    public const int GAMEFINISH_TIME = 10000;

    // Callvote
    public const int CALLVOTE_TIME = 20000;
    public const float CALLVOTE_PERCENT = 0.66f;

    #endregion

    #region ================== Variables

    // Build description
    private string builddesc;

    // Standard settings
    private string title;
    private string password;
    private string website;
    private GAMETYPE gametype;
    private int maxclients;
    private int maxplayers;
    private int scorelimit;
    private int timelimit;
    private bool joinsmallest;
    private int port;
    private bool makepublic;
    private List<string> mapnames;
    private string rconpassword;
    private bool isteamgame;
    private bool jointeamspectating;

    // Current status
    private GAMESTATE gamestate = GAMESTATE.WAITING;
    private int currentmap = 0;
    private int gamestateend;

    // The basic map data
    public Bloodmasters.Map map;

    // Items
    public Dictionary<string, Item> items;

    // Projectiles
    public Dictionary<string, Projectile> projectiles;
    public List<Projectile> disposeprojectiles;
    public int nextprojectileid;

    // Dynamic sectors for processing
    public List<DynamicSector> dynamics;

    // Networking
    public Gateway gateway;
    private Thread masterupdater;

    // All clients
    public Client[] clients;
    public Dictionary<string, Client> clientsaddrs;
    private int numclients;

    // Team scores
    public int[] teamscore = new int[3];

    // Bans
    private string localbansfile;
    private List<string> localbans;
    private List<string> globalbans;

    // Callvote
    public int callvotetimeout;
    public string callvotecmd;
    public string callvoteargs;
    public string callvotedesc;
    public int callvotes;

    #endregion

    #region ================== Properties

    // Server properties
    public GAMETYPE GameType { get { return (GAMETYPE)gametype; } }
    public int MaxClients { get { return maxclients; } }
    public int MaxPlayers { get { return maxplayers; } }
    public int Scorelimit { get { return scorelimit; } }
    public int Timelimit { get { return timelimit; } }
    public int Port { get { return port; } }
    public GAMESTATE GameState { get { return gamestate; } }
    public int GameStateEndTime { get { return gamestateend; } }
    public bool JoinSmallest { get { return joinsmallest; } }
    public string RConPassword { get { return rconpassword; } }
    public string Title { get { return title; } }
    public string Password { get { return password; } }
    public string Website { get { return website; } }
    public Client[] Clients { get { return clients; } }
    public bool IsTeamGame { get { return isteamgame; } }
    public bool JoinTeamSpectating { get { return jointeamspectating; } }

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public GameServer()
    {
        // Get version info
        Version v = Assembly.GetExecutingAssembly().GetName().Version;

        // Build description
        builddesc = $"${Environment.OSVersion.Platform} {Host.Instance.HostKindName} version {v.ToString(4)}";
    }

    // This initializes the server
    // Server settings must be applied before this
    public void Initialize(string config)
    {
        // Read the configuration
        Configuration cfg = new Configuration(true);
        cfg.InputConfiguration(config, true);

        // Apply server settings
        gametype = (GAMETYPE)cfg.ReadSetting("gametype", (int)GAMETYPE.DM);
        title = cfg.ReadSetting("title", "The Unnamed Server");
        password = cfg.ReadSetting("password", "");
        website = cfg.ReadSetting("website", "");
        port = cfg.ReadSetting("port", GameServer.DEFAULT_PORT);
        scorelimit = cfg.ReadSetting("scorelimit", 0);
        joinsmallest = cfg.ReadSetting("joinsmallest", true);
        maxclients = cfg.ReadSetting("maxclients", 20);
        maxplayers = cfg.ReadSetting("maxplayers", 16);
        timelimit = cfg.ReadSetting("timelimit", 20);
        rconpassword = cfg.ReadSetting("rconpassword", "");
        makepublic = cfg.ReadSetting("public", true);
        localbansfile = cfg.ReadSetting("bansfile", "localbans.txt");

        // Correct max players if need
        if(maxplayers > maxclients) maxplayers = maxclients;

        // Make the maps list
        IDictionary maps = cfg.ReadSetting("maps", new ListDictionary());
        mapnames = new List<string>(maps.Count);
        foreach(DictionaryEntry de in maps) mapnames.Add(de.Key.ToString());

        // Fix possible input errors
        title = Markup.TrimColorCodes(title);
        website = Markup.TrimColorCodes(website);

        // Check for input errors
        if(maxclients > MAX_CLIENTS) throw(new Exception("Setting 'maxclients' cannot be larger than " + MAX_CLIENTS + "."));
        if(maxclients < 1) throw(new Exception("Setting 'maxclients' cannot be smaller than 1."));
        if(maxplayers > MAX_CLIENTS) throw(new Exception("Setting 'maxplayers' cannot be larger than " + MAX_CLIENTS + "."));
        if(maxplayers < 1) throw(new Exception("Setting 'maxplayers' cannot be smaller than 1."));
        if(scorelimit > 10000) throw(new Exception("Setting 'scorelimit' cannot be larger than 10000."));
        if(scorelimit < 0) throw(new Exception("Setting 'scorelimit' cannot be smaller than 0."));
        if(timelimit > MAX_TIMELIMIT) throw(new Exception("Setting 'timelimit' cannot be larger than 1000."));
        if(timelimit < 0) throw(new Exception("Setting 'timelimit' cannot be smaller than 0."));

        // Output server information
        WriteLine("Starting server '" + title + "' at UDP port " + port, true);

        // Initialize clients array
        clients = new Client[maxclients];
        clientsaddrs = new Dictionary<string, Client>(maxclients);

        // Start the gateway
        gateway = new ServerGateway(port, 0, 0);

        // Make server public?
        if(makepublic)
        {
            // Start the masterserver updater
            masterupdater = new Thread(new ThreadStart(MasterUpdater));
            masterupdater.Name = "MasterUpdater";
            masterupdater.Start();
        }

        // Determine several characteristic depending on game type
        switch(gametype)
        {
            case GAMETYPE.DM:
                isteamgame = false;
                jointeamspectating = false;
                DM_ToWaiting();
                break;

            case GAMETYPE.TDM:
                isteamgame = true;
                jointeamspectating = false;
                DM_ToWaiting();
                break;

            case GAMETYPE.CTF:
                isteamgame = true;
                jointeamspectating = false;
                CTF_ToWaiting();
                break;

            case GAMETYPE.SC:
                isteamgame = false;
                jointeamspectating = false;
                SC_ToWaiting();
                break;

            case GAMETYPE.TSC:
                isteamgame = true;
                jointeamspectating = false;
                SC_ToWaiting();
                break;
        }

        // Start with the first map
        StartCurrentMap(mapnames[currentmap]);
    }

    // This terminates the server
    public void Dispose()
    {
        // Master updater running?
        if(masterupdater != null)
        {
            // Interrupt and join the thread
            masterupdater.Interrupt();
            masterupdater.Join();
            masterupdater = null;
        }

        // Remove clients
        if(clients != null) foreach(Client c in clients) if(c != null) c.Dispose();

        // Dispose dynamics
        if(dynamics != null) foreach(DynamicSector ds in dynamics) ds.Dispose();

        // Dispose all items
        if(items != null)
        {
            foreach(Item i in items.Values) i.Dispose();
        }

        // Dispose projectiles
        if(projectiles != null)
        {
            foreach(Projectile p in projectiles.Values) p.Dispose();
        }

        // Clean up
        if(gateway != null) gateway.Dispose();
        if(map != null) map.Dispose();
        dynamics = null;
        map = null;
        gateway = null;
        mapnames = null;
        clients = null;
        items = null;
        projectiles = null;
    }

    #endregion

    #region ================== Bans

    // This checks if an IP is banned
    public bool CheckBanned(string ip)
    {
        string[] inputip;
        string[] bannedip;

        // Split the ip address
        inputip = ip.Split('.');

        // Global bans initialized?
        if(globalbans != null)
        {
            // Go for all global bans
            foreach(String b in globalbans)
            {
                // Split the ip address
                bannedip = b.Split('.');

                // Match IPs
                if(MatchIP(inputip, bannedip)) return true;
            }
        }

        // Local bans initialized?
        if(localbans != null)
        {
            // Go for all local bans
            foreach(String b in localbans)
            {
                // Split the ip address
                bannedip = b.Split('.');

                // Match IPs
                if(MatchIP(inputip, bannedip)) return true;
            }
        }

        // No matches
        return false;
    }

    // This checks if two IP addresses match
    private bool MatchIP(string[] ip1, string[] ip2)
    {
        int matches = 0;

        try
        {
            // Go for all segments in ip address
            for(int i = 0; i < 4; i++)
            {
                // When it does not match
                if((ip1[i] != ip2[i]) && (ip1[i] != "*") && (ip2[i] != "*"))
                {
                    // return false
                    return false;
                }
            }
        }
        catch(Exception) { return false; }

        // Matches
        return true;
    }

    // This fills the global bans list
    private void SetGlobalBans(StreamReader readbody)
    {
        string line;

        // Read all lines
        globalbans = new List<string>();
        while((line = readbody.ReadLine()) != null)
        {
            // Anything on this line?
            if(line.Trim() != "")
            {
                // Add to the list of global bans
                globalbans.Add(line.Trim());
            }
        }
    }

    // This loads the local bans list
    private void SetLocalBans(StreamReader readbody)
    {
        string line;

        // Read all lines
        localbans = new List<string>();
        while((line = readbody.ReadLine()) != null)
        {
            // Comment // on the line?
            if(line.IndexOf("//") > -1)
            {
                // Remove everything behind and including //
                line = line.Substring(0, line.IndexOf("//"));
            }

            // Anything on this line?
            if(line.Trim() != "")
            {
                // Add to the list of global bans
                localbans.Add(line.Trim());
            }
        }
    }

    // This reloads the bans list
    public void LoadLocalBans()
    {
        FileStream fstream = null;
        StreamReader readbody = null;

        // File exists?
        if(File.Exists(localbansfile))
        {
            try
            {
                // Load the file contents
                fstream = File.OpenRead(localbansfile);
                readbody = new StreamReader(fstream, Encoding.UTF8);
                SetLocalBans(readbody);
            }
            catch(UnauthorizedAccessException)
            {
                // Unable to read file
                this.WriteLine("Unable to reload the local ban list, permission denied.", false);
            }
            finally
            {
                // Clean up
                if(readbody != null) readbody.Close();
                if(fstream != null) fstream.Close();
            }
        }
    }

    // This adds a ban to the local bans list
    public string AddLocalBan(string ip, string comment)
    {
        FileStream fstream = null;
        StreamReader readbody = null;
        StreamWriter writebody = null;
        string result = "";

        try
        {
            // Load the file contents
            fstream = File.Open(localbansfile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            readbody = new StreamReader(fstream, Encoding.UTF8);
            writebody = new StreamWriter(fstream, Encoding.UTF8);

            // Reload all bans
            SetLocalBans(readbody);

            // Check if ban is not already in the list
            if(!localbans.Contains(ip.Trim()))
            {
                // Add to banlist
                localbans.Add(ip.Trim());
                fstream.Seek(0, SeekOrigin.End);
                writebody.WriteLine(ip + "      // " + comment);

                // Done
                result = ip + " has been added to the ban list.";
            }
            else
            {
                // Already on the ban list
                result = ip + " is already on the ban list.";
            }
        }
        catch(UnauthorizedAccessException)
        {
            // Unable to read file
            this.WriteLine("Unable to add to the local ban list, permission denied.", false);
            result = "Unable to add to the local bans list, permission denied.";
        }
        finally
        {
            // Clean up
            if(writebody != null) writebody.Close();
            if(readbody != null) readbody.Close();
            if(fstream != null) fstream.Close();
        }

        // Return result
        return result;
    }

    #endregion

    #region ================== Masterserver

    // This thread notifies the masterserver about this server
    private void MasterUpdater()
    {
        int timeoutscount = 0;

        // Continue until interrupted
        while(true)
        {
            HttpWebResponse resp = null;
            HttpWebRequest req = null;

            // Make complete master url
            string masterurl = MASTER_URL;
            masterurl = masterurl.Replace("%port%", port.ToString(CultureInfo.InvariantCulture));
            masterurl = masterurl.Replace("%time%", DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture));

            // Setup HTTP request
            req = (HttpWebRequest)WebRequest.Create(masterurl);
            req.Timeout = 5000;

            // Make the request
            try { resp = (HttpWebResponse)req.GetResponse(); }
            catch(Exception e)
            {
                // Check if interrupted
                if(e is ThreadInterruptedException)
                {
                    // Clean up and leave
                    if(resp != null) resp.Close();
                    return;
                }
                // Web exception
                else if(e is WebException)
                {
                    // Check if timed out
                    WebException we = (WebException)e;
                    if(we.Status == WebExceptionStatus.Timeout)
                    {
                        // Timout
                        timeoutscount++;
                        WriteLine("Masterserver update timed out (" + timeoutscount + ")", true);
                    }
                    else
                    {
                        // Other error
                        WriteLine(e.GetType().ToString() + " in MasterUpdater: " + we.Message + " (" + we.Status.ToString() + ")", false);
                    }
                }
                else
                {
                    // Other error
                    WriteLine(e.GetType().ToString() + " in MasterUpdater: " + e.Message, false);
                }
            }

            // The master server will send a list of
            // global banned ip addresses back
            if(resp != null)
            {
                // Success
                timeoutscount = 0;

                // Get the result
                Stream body = resp.GetResponseStream();
                StreamReader readbody = new StreamReader(body, Encoding.UTF8);

                // Fill the global bans list
                SetGlobalBans(readbody);

                // Done
                readbody.Close();
                resp.Close();
            }

            // Wait or leave when interrupted
            try { Thread.Sleep(MASTER_UPDATE_INTERVAL); }
            catch(Exception) { return; }
        }
    }

    #endregion

    #region ================== Control

    // This starts the next map
    public void NextMap()
    {
        // Move to next map in list or restart list
        if(currentmap >= mapnames.Count - 1) currentmap = 0; else currentmap++;
        StartCurrentMap(mapnames[currentmap]);
    }

    // This loads the map with the current settings
    public void StartCurrentMap(string nextmapname)
    {
        int thinggametype = (int)Math.Pow(2, (int)Host.Instance.Server.GameType);

        // Name of the next map
        //string nextmapname = mapnames[currentmap];
        Write("Server is loading map \"" + nextmapname + "\"...", true);

        // Load the map title
        Bloodmasters.Map mapcfg = new ServerMap(nextmapname, true, Paths.TempDir);
        string maptitle = mapcfg.Title;
        mapcfg.Dispose();

        // Go for all clients
        foreach(Client c in clients)
        {
            // Send map change info
            if(c != null) c.SendMapChange(nextmapname, maptitle);
        }

        // Force sending
        ProcessNetworking();

        // Dispose old dynamics
        if(dynamics != null) foreach(DynamicSector ds in dynamics) ds.Dispose();

        // Dispose old map
        if(map != null) map.Dispose();

        // Reload bans list
        LoadLocalBans();

        // Load the map
        map = new ServerMap(nextmapname, false, Paths.TempDir);

        // New items
        items = new Dictionary<string, Item>();

        // New projectiles
        projectiles = new Dictionary<string, Projectile>();
        disposeprojectiles = new List<Projectile>();
        nextprojectileid = 0;

        // Ensure unique item ids start at 0
        Item.uniquekeyindex = 0;

        // Go for all things
        foreach(Thing t in map.Things)
        {
            // This thing supposed to be in this game type?
            if(((int)t.Flags & thinggametype) == thinggametype)
            {
                // Determine in which sector thing is
                t.DetermineSector();

                // Go for all types in this assembly
                Assembly asm = Assembly.GetExecutingAssembly();
                Type[] asmtypes = asm.GetTypes();
                foreach(Type tp in asmtypes)
                {
                    // Check if this type is a class
                    if(tp.IsClass && !tp.IsAbstract && !tp.IsArray)
                    {
                        // Check if class has a ClientItem attribute
                        if(Attribute.IsDefined(tp, typeof(ServerItem), false))
                        {
                            // Get item attribute
                            ServerItem attr = (ServerItem)Attribute.GetCustomAttribute(tp, typeof(ServerItem), false);

                            // Same ID number?
                            if(t.Type == attr.ThingID)
                            {
                                // Create object for this item
                                object[] args = new object[1];
                                args[0] = t;
                                Item item = (Item)asm.CreateInstance(tp.FullName, false, BindingFlags.Default,
                                    null, args, CultureInfo.CurrentCulture, Array.Empty<object>());

                                // If the item is not temporary
                                // then add it to the items list
                                if(!item.Temporary) items.Add(item.Key, item); else item.Dispose();
                            }
                        }
                    }
                }
            }
        }

        // Make dynamic sectors
        dynamics = new List<DynamicSector>();
        foreach(Sector s in map.Sectors)
        {
            // Presume no dynamic item
            DynamicSector ds = null;

            // Check sector effect type
            switch(s.Effect)
            {
                case SECTOREFFECT.DOOR: ds = new ProxyDoor(s); break;
                case SECTOREFFECT.PLATFORMHIGH: ds = new Platform(s, false); break;
                case SECTOREFFECT.PLATFORMLOW: ds = new Platform(s, true); break;
            }

            // Add dynamic object for this sector
            if(ds != null) dynamics.Add(ds);
        }

        // Clear everything
        RemoveAllActors();
        ClearAllScores();

        // Change game states
        teamscore[1] = 0;
        teamscore[2] = 0;
        gamestate = GAMESTATE.WAITING;

        // Start with zero game time
        Host.Instance.CatchLag();

        // Set the timelimit timeout
        if(timelimit > 0)
            gamestateend = SharedGeneral.currenttime + timelimit * 60000;
        else
            gamestateend = SharedGeneral.currenttime + MAX_TIMELIMIT;

        // Done
        WriteLine(" Done", false);
    }

    // This tests the game state and switches when needed
    private void ControlGameState()
    {
        int playingclients = 0;
        int topscore = int.MinValue;

        // Go for all clients to gather information
        foreach(Client c in clients)
        {
            // Playing? Then count it
            if((c != null) && !c.Spectator && !c.Loading)
            {
                playingclients++;
                if((!isteamgame) && (c.Score > topscore)) topscore = c.Score;
            }
        }

        // In team games the top score is top of team scores
        if(isteamgame)
        {
            // Determine top score
            if(teamscore[1] > teamscore[2]) topscore = teamscore[1];
            else topscore = teamscore[2];
        }

        // Game type determines the state changes
        // See state transition diagram for more info
        switch(gametype)
        {
            // DEATHMATCH and TEAM DEATHMATCH
            case GAMETYPE.DM:
            case GAMETYPE.TDM:

                // Current state
                switch(gamestate)
                {
                    case GAMESTATE.WAITING:
                        if(playingclients >= MIN_PLAYERS) DM_ToSpawning();
                        else if(gamestateend <= SharedGeneral.currenttime) DM_ToFinish("Timelimit hit");
                        break;

                    case GAMESTATE.SPAWNING:
                        if(RespawnDeadClients()) DM_ToCountdown();
                        break;

                    case GAMESTATE.COUNTDOWN:
                        if(playingclients < MIN_PLAYERS) DM_ToWaiting();
                        if(gamestateend <= SharedGeneral.currenttime) DM_ToPlaying();
                        break;

                    case GAMESTATE.PLAYING:
                        if(playingclients < MIN_PLAYERS) DM_ToWaiting();
                        else if(gamestateend <= SharedGeneral.currenttime) DM_ToFinish("Timelimit hit");
                        else if((topscore >= scorelimit) && (scorelimit > 0)) DM_ToFinish("Scorelimit hit");
                        break;

                    case GAMESTATE.ROUNDFINISH:
                    case GAMESTATE.GAMEFINISH:
                        if(gamestateend <= SharedGeneral.currenttime) DM_ResetToWaiting();
                        break;
                }
                break;

            // CAPTURE THE FLAG
            case GAMETYPE.CTF:

                // Current state
                switch(gamestate)
                {
                    case GAMESTATE.WAITING:
                        if(playingclients >= MIN_PLAYERS) CTF_ToSpawning();
                        else if(gamestateend <= SharedGeneral.currenttime) CTF_ToFinish("Timelimit hit");
                        break;

                    case GAMESTATE.SPAWNING:
                        if(RespawnDeadClients()) CTF_ToCountdown();
                        break;

                    case GAMESTATE.COUNTDOWN:
                        if(playingclients < MIN_PLAYERS) CTF_ToWaiting();
                        if(gamestateend <= SharedGeneral.currenttime) CTF_ToPlaying();
                        break;

                    case GAMESTATE.PLAYING:
                        if(playingclients < MIN_PLAYERS) CTF_ToWaiting();
                        else if(gamestateend <= SharedGeneral.currenttime) CTF_ToFinish("Timelimit hit");
                        else if((topscore >= scorelimit) && (scorelimit > 0)) CTF_ToFinish("Scorelimit hit");
                        break;

                    case GAMESTATE.ROUNDFINISH:
                    case GAMESTATE.GAMEFINISH:
                        if(gamestateend <= SharedGeneral.currenttime) CTF_ResetToWaiting();
                        break;
                }
                break;

            // SCAVENGER and TEAM SCAVENGER
            case GAMETYPE.SC:
            case GAMETYPE.TSC:

                // Current state
                switch(gamestate)
                {
                    case GAMESTATE.WAITING:
                        if(playingclients >= MIN_PLAYERS) SC_ToSpawning();
                        else if(gamestateend <= SharedGeneral.currenttime) SC_ToFinish("Timelimit hit");
                        break;

                    case GAMESTATE.SPAWNING:
                        if(RespawnDeadClients()) SC_ToCountdown();
                        break;

                    case GAMESTATE.COUNTDOWN:
                        if(playingclients < MIN_PLAYERS) SC_ToWaiting();
                        if(gamestateend <= SharedGeneral.currenttime) DM_ToPlaying();
                        break;

                    case GAMESTATE.PLAYING:
                        if(playingclients < MIN_PLAYERS) SC_ToWaiting();
                        else if(gamestateend <= SharedGeneral.currenttime) SC_ToFinish("Timelimit hit");
                        else if((topscore >= scorelimit) && (scorelimit > 0)) SC_ToFinish("Scorelimit hit");
                        break;

                    case GAMESTATE.ROUNDFINISH:
                    case GAMESTATE.GAMEFINISH:
                        if(gamestateend <= SharedGeneral.currenttime) SC_ResetToWaiting();
                        break;
                }
                break;
        }
    }

    #endregion

    #region ================== Gamestate Changes DM / TDM

    // This resets the game (from game finish to waiting)
    private void DM_ResetToWaiting()
    {
        // Clear everything
        RemoveAllActors();
        ClearAllScores();

        // Move to next map in list or restart list
        if(currentmap >= mapnames.Count - 1) currentmap = 0; else currentmap++;
        StartCurrentMap(mapnames[currentmap]);

        // New gamestate
        DM_ToWaiting();
    }

    // This switches to waiting gamestate
    private void DM_ToWaiting()
    {
        // Set the timelimit timeout
        if(timelimit > 0)
            gamestateend = SharedGeneral.currenttime + timelimit * 60000;
        else
            gamestateend = SharedGeneral.currenttime + MAX_TIMELIMIT;

        // Broadcast new gamestate
        gamestate = GAMESTATE.WAITING;
        BroadcastGameStateChange();
    }

    // This switches to spawning gamestate
    private void DM_ToSpawning()
    {
        // Broadcast new gamestate
        gamestate = GAMESTATE.SPAWNING;
        BroadcastGameStateChange();
    }

    // This switches to countdown gamestate
    private void DM_ToCountdown()
    {
        // Set the countdown timeout
        gamestateend = SharedGeneral.currenttime + COUNTDOWN_TIME;

        // Broadcast new gamestate
        gamestate = GAMESTATE.COUNTDOWN;
        BroadcastGameStateChange();
    }

    // This switches to playing gamestate
    private void DM_ToPlaying()
    {
        // Set the timelimit timeout
        if(timelimit > 0)
            gamestateend = SharedGeneral.currenttime + timelimit * 60000;
        else
            gamestateend = SharedGeneral.currenttime + MAX_TIMELIMIT;

        // Broadcast new gamestate
        gamestate = GAMESTATE.PLAYING;
        BroadcastGameStateChange();

        // Respawn and reset everything
        ClearAllScores();
        teamscore[1] = 0;
        teamscore[2] = 0;
        RespawnAllItems();
        RespawnAllClients();
    }

    // This switches to game/round finish gamestate
    private void DM_ToFinish(string reason)
    {
        // Broadcast reason
        BroadcastShowMessage(reason, true, true);

        // Remove players
        RemoveAllActors();

        // Set the timeout
        gamestateend = SharedGeneral.currenttime + GAMEFINISH_TIME;

        // Broadcast new gamestate
        gamestate = GAMESTATE.GAMEFINISH;
        BroadcastGameStateChange();
    }

    #endregion

    #region ================== Gamestate Changes CTF

    // This resets the game (from game finish to waiting)
    private void CTF_ResetToWaiting()
    {
        // Clear everything
        RemoveAllActors();
        ClearAllScores();

        // Move to next map in list or restart list
        if(currentmap >= mapnames.Count - 1) currentmap = 0; else currentmap++;
        StartCurrentMap(mapnames[currentmap]);

        // New gamestate
        CTF_ToWaiting();
    }

    // This switches to waiting gamestate
    private void CTF_ToWaiting()
    {
        // Set the timelimit timeout
        if(timelimit > 0)
            gamestateend = SharedGeneral.currenttime + timelimit * 60000;
        else
            gamestateend = SharedGeneral.currenttime + MAX_TIMELIMIT;

        // Broadcast new gamestate
        gamestate = GAMESTATE.WAITING;
        BroadcastGameStateChange();
    }

    // This switches to spawning gamestate
    private void CTF_ToSpawning()
    {
        // Broadcast new gamestate
        gamestate = GAMESTATE.SPAWNING;
        BroadcastGameStateChange();
    }

    // This switches to countdown gamestate
    private void CTF_ToCountdown()
    {
        // Set the countdown timeout
        gamestateend = SharedGeneral.currenttime + COUNTDOWN_TIME;

        // Broadcast new gamestate
        gamestate = GAMESTATE.COUNTDOWN;
        BroadcastGameStateChange();
    }

    // This switches to playing gamestate
    private void CTF_ToPlaying()
    {
        // Set the timelimit timeout
        if(timelimit > 0)
            gamestateend = SharedGeneral.currenttime + timelimit * 60000;
        else
            gamestateend = SharedGeneral.currenttime + MAX_TIMELIMIT;

        // Broadcast new gamestate
        gamestate = GAMESTATE.PLAYING;
        BroadcastGameStateChange();

        // Respawn and reset everything
        ClearAllScores();
        teamscore[1] = 0;
        teamscore[2] = 0;
        RespawnAllItems();
        RespawnAllClients();
    }

    // This switches to game/round finish gamestate
    private void CTF_ToFinish(string reason)
    {
        // Broadcast reason
        BroadcastShowMessage(reason, true, true);

        // Remove players
        RemoveAllActors();

        // Set the timeout
        gamestateend = SharedGeneral.currenttime + GAMEFINISH_TIME;

        // Broadcast new gamestate
        gamestate = GAMESTATE.GAMEFINISH;
        BroadcastGameStateChange();
    }

    #endregion

    #region ================== Gamestate Changes SC / TSC

    // This resets the game (from game finish to waiting)
    private void SC_ResetToWaiting()
    {
        // Clear everything
        RemoveAllActors();
        ClearAllScores();

        // Move to next map in list or restart list
        if(currentmap >= mapnames.Count - 1) currentmap = 0; else currentmap++;
        StartCurrentMap(mapnames[currentmap]);

        // New gamestate
        SC_ToWaiting();
    }

    // This switches to waiting gamestate
    private void SC_ToWaiting()
    {
        // Set the timelimit timeout
        if(timelimit > 0)
            gamestateend = SharedGeneral.currenttime + timelimit * 60000;
        else
            gamestateend = SharedGeneral.currenttime + MAX_TIMELIMIT;

        // Broadcast new gamestate
        gamestate = GAMESTATE.WAITING;
        BroadcastGameStateChange();
    }

    // This switches to spawning gamestate
    private void SC_ToSpawning()
    {
        // Broadcast new gamestate
        gamestate = GAMESTATE.SPAWNING;
        BroadcastGameStateChange();
    }

    // This switches to countdown gamestate
    private void SC_ToCountdown()
    {
        // Set the countdown timeout
        gamestateend = SharedGeneral.currenttime + COUNTDOWN_TIME;

        // Broadcast new gamestate
        gamestate = GAMESTATE.COUNTDOWN;
        BroadcastGameStateChange();
    }

    // This switches to playing gamestate
    private void SC_ToPlaying()
    {
        // Set the timelimit timeout
        if(timelimit > 0)
            gamestateend = SharedGeneral.currenttime + timelimit * 60000;
        else
            gamestateend = SharedGeneral.currenttime + MAX_TIMELIMIT;

        // Broadcast new gamestate
        gamestate = GAMESTATE.PLAYING;
        BroadcastGameStateChange();

        // Respawn and reset everything
        ClearAllScores();
        teamscore[1] = 0;
        teamscore[2] = 0;
        RespawnAllItems();
        RespawnAllClients();
    }

    // This switches to game/round finish gamestate
    private void SC_ToFinish(string reason)
    {
        // Broadcast reason
        BroadcastShowMessage(reason, true, true);

        // Remove players
        RemoveAllActors();

        // Set the timeout
        gamestateend = SharedGeneral.currenttime + GAMEFINISH_TIME;

        // Broadcast new gamestate
        gamestate = GAMESTATE.GAMEFINISH;
        BroadcastGameStateChange();
    }

    #endregion

    #region ================== Static Methods

    // This validates a player name and returns
    // the problem as a description
    public static string ValidatePlayerName(string name)
    {
        string strippedname = Markup.StripColorCodes(name);

        // Check length
        if(strippedname.Length < 1)
        {
            // Name too long
            return "Please enter a valid player name.";
        }

        // Check actual length
        if(name.Length > Consts.MAX_PLAYER_NAME_STR_LEN)
        {
            // Name too long
            return "Player name, including color codes, may not be longer than " + Consts.MAX_PLAYER_NAME_STR_LEN + " characters.";
        }

        // Check length
        if(strippedname.Length > Consts.MAX_PLAYER_NAME_LEN)
        {
            // Name too long
            return "Player name may not be longer than " + Consts.MAX_PLAYER_NAME_LEN + " characters.";
        }

        // Check first character
        if(strippedname.StartsWith("#"))
        {
            // May not start with #
            return "Player name may not start with #.";
        }

        // Check if name contains any of the required characters
        if(strippedname.IndexOfAny(Consts.REQ_PLAYER_CHARS.ToCharArray()) == -1)
        {
            // Must contain an alphanumeric character
            return "Player name must contain at least one alphanumeric character.";
        }

        // Check for invalid characters
        if(name.IndexOf(',') != -1) return "Player name may not contain a comma.";
        if(name.IndexOf('\"') != -1) return "Player name may not contain quotes.";
        if(name.IndexOf('\'') != -1) return "Player name may not contain quotes.";

        // No problems
        return null;
    }

    // This takes a new projectile and adds it to the list
    public string NewProjectile(Projectile prj)
    {
        int nid = nextprojectileid;
        string pid = "P" + nid.ToString(CultureInfo.InvariantCulture);
        nextprojectileid++;

        // Add projectile
        projectiles.Add(pid, prj);

        // Return ID
        return pid;
    }

    #endregion

    #region ================== Clients

    // This counts the total number of clients
    public int CountTotalClients()
    {
        int num = 0;

        // Go for all clients
        foreach(Client c in clients)
        {
            // Client here? Then count it
            if(c != null) num++;
        }

        // Return count
        return num;
    }

    // This counts the actual number of playing clients
    public int CountPlayingClients()
    {
        int num = 0;

        // Go for all clients
        foreach(Client c in clients)
        {
            // Client here?
            if(c != null)
            {
                // Playing? Then count it
                if(!c.Spectator && !c.Loading) num++;
            }
        }

        // Return count
        return num;
    }

    // This counts the actual number of playing clients for a specific team
    public int CountPlayingClients(TEAM t)
    {
        int num = 0;

        // Go for all clients
        foreach(Client c in clients)
        {
            // Client here?
            if(c != null)
            {
                // Playing? Then count it
                if(!c.Spectator && !c.Loading && (c.Team == t)) num++;
            }
        }

        // Return count
        return num;
    }

    // This clears scores for all players
    public void ClearAllScores()
    {
        // Go for all clients
        foreach(Client c in clients)
        {
            // Reset scores
            if(c != null)
            {
                c.ResetScores();
                BroadcastClientUpdate(c);
            }
        }
    }

    // This removes all player actors
    public void RemoveAllActors()
    {
        // Go for all clients
        foreach(Client c in clients)
        {
            // Remove actor
            if(c != null) c.RemoveActor();
        }
    }

    // This respawns all dead clients
    // but not spectators and loading clients
    // Returns TRUE when everyone is spawned
    public bool RespawnDeadClients()
    {
        bool success = true;

        // Go for all clients
        foreach(Client c in clients)
        {
            // Respawn when playing
            if((c != null) && !c.Spectator && !c.Loading && !c.IsAlive) success &= c.Spawn(false);
        }

        // Return result
        return success;
    }

    // This respawns all playing and dead clients
    // but not spectators and loading clients
    public bool RespawnAllClients()
    {
        bool success = true;

        // Go for all clients
        foreach(Client c in clients)
        {
            // Respawn when playing
            if((c != null) && !c.Spectator && !c.Loading)
            {
                c.RemoveActor();
                c.Spawn(true);
            }
        }

        // Return result
        return success;
    }

    // Adds a client to a slot
    public void AddClient(int index, Client clnt)
    {
        // Set the client slot
        clients[index] = clnt;
        clientsaddrs.Add(clnt.Address, clnt);
        numclients++;
    }

    // Removes a client from a slot
    public void RemoveClient(int index)
    {
        // Set the client slot
        clientsaddrs.Remove(clients[index].Address);
        clients[index] = null;
        numclients--;
    }

    // This finds the first free client spot
    private int GetFreeClientSlot()
    {
        // Go for all clients and return the first empty slot
        for(int i = 0; i < maxclients; i++) if(clients[i] == null) return i;
        return -1;
    }

    // This tests a ray for collision with a client
    public bool FindRayPlayerCollision(Vector3D start, Vector3D end, Client exclude, ref Vector3D point, ref object obj, ref float u)
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
        foreach(Client c in Host.Instance.Server.clients)
        {
            // This client in the game and not the excluded client
            if((c != null) && (c != exclude) && (c.State != null) && !c.Loading && !c.Disposed)
            {
                // Calculate intersection offset
                uray = ((c.State.pos.x - start.x) * (end.x - start.x) + (c.State.pos.y - start.y) * (end.y - start.y)) / delta2dlensq;
                if((uray > 0f) && (uray < 1f) && (uray < u))
                {
                    // Calculate intersection point
                    intp = start + (delta * uray);

                    // Check if within Z heights
                    if((intp.z > c.State.pos.z) && (intp.z < (c.State.pos.z + Consts.PLAYER_HEIGHT)))
                    {
                        // Calculate 2D distance from collision to player
                        Vector2D dist = (Vector2D)intp - (Vector2D)c.State.pos;

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

    #region ================== Network Receiving

    // This processes networking
    public void ProcessNetworking()
    {
        NetMessage msg;
        Client clnt;

        // Process gateway
        gateway.Process();

        // Get next message
        while((msg = gateway.GetNextMessage()) != null)
        {
            try
            {
                // Connectionless?
                if(msg.Connection == null)
                {
                    // Handle connection request command
                    switch(msg.Command)
                    {
                        case MsgCmd.ConnectRequest: hConnectRequest(msg); break;
                        case MsgCmd.ServerInfo: hServerInfo(msg); break;
                        default: WriteLine("Unknown command message " + msg.Command + " received from " + msg.Address, true); break;
                    }
                }
                else
                {
                    // Client with this connection?
                    if(clientsaddrs.TryGetValue(msg.Address.ToString(), out clnt))
                    {
                        // Handle message command
                        switch(msg.Command)
                        {
                            case MsgCmd.Disconnect: clnt.hDisconnect(msg); break;
                            case MsgCmd.GameStarted: clnt.hGameStarted(msg); break;
                            case MsgCmd.SayMessage: clnt.hSayMessage(msg); break;
                            case MsgCmd.SayTeamMessage: clnt.hSayTeamMessage(msg); break;
                            case MsgCmd.Command: clnt.hCommand(msg); break;
                            case MsgCmd.ChangeTeam: clnt.hChangeTeam(msg); break;
                            case MsgCmd.ClientMove: clnt.hClientMove(msg); break;
                            case MsgCmd.RespawnRequest: clnt.hRespawnRequest(msg); break;
                            case MsgCmd.Suicide: clnt.hSuicide(msg); break;
                            case MsgCmd.SwitchWeapon: clnt.hSwitchWeapon(msg); break;
                            case MsgCmd.FirePowerup: clnt.hFirePowerup(msg); break;
                            case MsgCmd.ServerInfo: hServerInfo(msg); break;
                            case MsgCmd.PlayerNameChange: clnt.hPlayerNameChange(msg); break;
                            case MsgCmd.NeedActor: clnt.hNeedActor(msg); break;
                            case MsgCmd.CallvoteRequest: clnt.hCallvoteRequest(msg); break;
                            case MsgCmd.CallvoteSubmit: clnt.hCallvoteSubmit(msg); break;
                            default: WriteLine("Unknown command message " + msg.Command + " received from " + msg.Address + " (" + Markup.StripColorCodes(clnt.Name) + ")", true); break;
                        }
                    }
                    else
                    {
                        // Handle connection request command
                        // or player login command
                        switch(msg.Command)
                        {
                            case MsgCmd.ConnectRequest: hConnectRequest(msg); break;
                            case MsgCmd.PlayerLogin: hPlayerLogin(msg); break;
                            case MsgCmd.ServerInfo: hServerInfo(msg); break;
                            default: WriteLine("Unknown command message " + msg.Command + " received from " + msg.Address, true); break;
                        }
                    }
                }
            }
            // Malicious message?
            catch(Exception e)
            {
                // Output error
                WriteLine(e.GetType().Name + " in " + e.TargetSite.DeclaringType.Name + "." + e.TargetSite.Name + " on " + msg.Command +  " message from " + msg.Address + ": " + e.Message, false);
                Host.Instance.OutputError(e);
                Host.Instance.WriteErrorLine(e);
            }
        }
    }

    // Handle ServerInfo
    private void hServerInfo(NetMessage msg)
    {
        NetMessage rep;

        // Read the time signature
        int timesig = msg.GetInt();

        // DEBUG:
        //WriteLine("ServerInfo request received from " + msg.Address, false);

        // Reply with server information
        // Send this back in the reply for ping measuring
        rep = msg.Reply(MsgCmd.ServerInfo);
        if(rep != null)
        {
            // Add generic server information
            // This may not be changed, it must stay compitable with other versions
            rep.AddData((int)timesig);
            rep.AddData((string)Markup.StripColorCodes(title));
            rep.AddData((bool)(password != ""));
            rep.AddData((string)website);
            rep.AddData((byte)maxclients);
            rep.AddData((byte)maxplayers);
            rep.AddData((byte)gametype);
            rep.AddData((string)map.Name);
            rep.AddData((byte)numclients);
            rep.AddData((byte)CountPlayingClients());
            rep.AddData((byte)Gateway.PROTOCOL_VERSION);

            // Now add extra information
            // This may change for other versions
            rep.AddData((short)scorelimit);
            rep.AddData((short)timelimit);
            rep.AddData((bool)joinsmallest);

            // Add all clients
            foreach(Client c in clients)
            {
                // Client on this slot?
                if(c != null)
                {
                    // Add client information
                    rep.AddData((string)Markup.StripColorCodes(c.Name));
                    rep.AddData((byte)c.Team);
                    rep.AddData((bool)c.Spectator);
                    rep.AddData((short)c.Connection.LastPing);
                }
            }

            // Add build description
            rep.AddData((string)builddesc);

            // Send reply
            rep.Send();
        }
    }

    // Handle ConnectRequest
    private void hConnectRequest(NetMessage msg)
    {
        NetMessage rep;
        Connection con;

        // Check if banned
        if(CheckBanned(msg.Address.Address.ToString()))
        {
            // Address banned, leave!
            rep = msg.Reply(MsgCmd.ConnectRefused);
            if(rep != null)
            {
                rep.AddData("\nYou are banned, now go away.");
                rep.Send();
            }
            return;
        }

        // Read the parameters
        int protocol = msg.GetInt();

        // Check protocol version
        if(protocol < Gateway.PROTOCOL_VERSION)
        {
            // Show info
            WriteLine("Connection from " + msg.Address + " denied (protocol " + protocol + " is outdated)", true);

            // Invalid protocol version
            rep = msg.Reply(MsgCmd.ConnectRefused);
            if(rep != null)
            {
                rep.AddData("\nYou are using an incompatible version of Bloodmasters.\n" +
                            "Browse to www.bloodmasters.com for the latest version!");
                rep.Send();
            }
        }
        // Check protocol version
        else if(protocol > Gateway.PROTOCOL_VERSION)
        {
            // Show info
            WriteLine("Connection from " + msg.Address + " denied (protocol " + protocol + " is newer)", true);

            // Invalid protocol version
            rep = msg.Reply(MsgCmd.ConnectRefused);
            if(rep != null)
            {
                rep.AddData("\nThe server is using an older version of Bloodmasters.");
                rep.Send();
            }
        }
        else
        {
            // Server not full?
            if(CountTotalClients() >= maxclients)
            {
                // Server is full, sorry
                rep = msg.Reply(MsgCmd.ConnectRefused);
                if(rep != null)
                {
                    rep.AddData("\nServer is full, please try again later.");
                    rep.Send();
                }
                return;
            }

            // Create or re-use connection
            if(msg.Connection == null)
            {
                // Create new connection
                con = gateway.CreateConnection(msg.Address);
            }
            else
            {
                // Use existing connection
                con = msg.Connection;
            }

            // Initiate a short period connection
            // This will become a long period connection as
            // soon as the client answers.
            con.SetTimeout(CONNECT_TIMEOUT);

            // Perform measurements on server side
            con.MeasurePings = true;

            // Reply with confirmed message
            rep = msg.Reply(MsgCmd.ConnectConfirm);
            if(rep != null)
            {
                rep.AddData((int)con.RandomID);
                rep.Send();
            }
        }
    }

    // Handle PlayerLogin
    private void hPlayerLogin(NetMessage msg)
    {
        NetMessage rep;
        string playernameerror;

        // Read the parameters
        int connectid = msg.GetInt();
        string givenpassword = msg.GetString();
        string playername = msg.GetString();
        int snaps = msg.GetInt();
        bool autoswitch = msg.GetBool();

        // Check the connection id
        if(connectid == msg.Connection.RandomID)
        {
            // Not already logged in?
            if(!clientsaddrs.ContainsKey(msg.Connection.Address.ToString()))
            {
                // Check the password
                if((givenpassword == password) || (password.Trim() == ""))
                {
                    // Check the player name
                    playernameerror = GameServer.ValidatePlayerName(playername);
                    if(playernameerror == null)
                    {
                        // Create the client
                        int id = GetFreeClientSlot();
                        Client c = new Client(msg.Connection, playername, id, snaps, autoswitch);

                        // Send StartGameInfo message
                        c.SendGameStartInfo();

                        // Make a long period timeout so that
                        // the client can load the game
                        c.Connection.SetTimeout(LOADING_TIMEOUT);
                    }
                    else
                    {
                        // Invalid player name
                        rep = msg.Reply(MsgCmd.ConnectRefused);
                        if(rep != null)
                        {
                            rep.AddData("\n" + playernameerror);
                            rep.Send();
                        }
                    }
                }
                else
                {
                    // Invalid password
                    rep = msg.Reply(MsgCmd.ConnectRefused);
                    if(rep != null)
                    {
                        rep.AddData("\nIncorrect server password.");
                        rep.Send();
                    }
                }
            }
        }
    }

    #endregion

    #region ================== Network Broadcasting

    // This broadcasts callvote status update
    public void BroadcastCallvoteStatus()
    {
        // Broadcast to all clients
        foreach(Client c in Host.Instance.Server.clients)
        {
            if((c != null) && (!c.Loading)) c.SendCallvoteStatus();
        }
    }

    // This broadcasts callvote emd
    public void BroadcastCallvoteEnd()
    {
        // Broadcast to all clients
        foreach(Client c in Host.Instance.Server.clients)
        {
            if((c != null) && (!c.Loading)) c.SendCallvoteEnd();
        }
    }

    // This broadcasts player name change event
    public void BroadcastPlayerNameChange(Client cc)
    {
        // Broadcast to all clients
        foreach(Client c in Host.Instance.Server.clients)
        {
            if((c != null) && (!c.Loading)) c.SendPlayerNameChange(cc);
        }
    }

    // This broadcasts a flag score
    public void BroadcastScoreFlag(Client scorer, Item opponentflag)
    {
        // Broadcast to all clients
        foreach(Client c in Host.Instance.Server.clients)
        {
            if((c != null) && (!c.Loading)) c.SendScoreFlag(scorer, opponentflag);
        }
    }

    // This broadcasts a flag return
    public void BroadcastReturnFlag(Client returner, Item flag)
    {
        // Broadcast to all clients
        foreach(Client c in Host.Instance.Server.clients)
        {
            if((c != null) && (!c.Loading)) c.SendReturnFlag(returner, flag);
        }
    }

    // This broadcasts a fire intensity update
    public void BroadcastFireIntensity(Client clnt)
    {
        // Broadcast to all clients
        foreach(Client c in Host.Instance.Server.clients)
        {
            if((c != null) && (!c.Loading)) c.SendFireIntensity(clnt);
        }
    }

    // This broadcasts shield hit event
    public void BroadcastShieldHit(Client clnt, float angle, float fadeout)
    {
        // Broadcast to all clients
        foreach(Client c in Host.Instance.Server.clients)
        {
            if((c != null) && (!c.Loading)) c.SendShieldHit(clnt, angle, fadeout);
        }
    }

    // This broadcasts projectile event
    public void BroadcastSpawnProjectile(Projectile p)
    {
        // Broadcast to all clients
        foreach(Client c in Host.Instance.Server.clients)
        {
            if((c != null) && (!c.Loading)) c.SendSpawnProjectile(p);
        }
    }

    // This broadcasts projectile event
    public void BroadcastUpdateProjectile(Projectile p)
    {
        // Broadcast to all clients
        foreach(Client c in Host.Instance.Server.clients)
        {
            if((c != null) && (!c.Loading)) c.SendUpdateProjectile(p);
        }
    }

    // This broadcasts projectile event
    public void BroadcastTeleportProjectile(Vector3D oldpos, Projectile p)
    {
        // Broadcast to all clients
        foreach(Client c in Host.Instance.Server.clients)
        {
            if((c != null) && (!c.Loading)) c.SendTeleportProjectile(oldpos, p);
        }
    }

    // This broadcasts projectile event
    public void BroadcastDestroyProjectile(Projectile p, bool silent, Client hitplayer)
    {
        // Broadcast to all clients
        foreach(Client c in Host.Instance.Server.clients)
        {
            if((c != null) && (!c.Loading)) c.SendDestroyProjectile(p, silent, hitplayer);
        }
    }

    // This broadcasts e teleport event
    public void BroadcastTeleportClient(Client clnt, Vector3D oldpos, Vector3D newpos)
    {
        // Broadcast the teleport event
        foreach(Client c in Host.Instance.Server.clients)
        {
            if((c != null) && (!c.Loading)) c.SendTeleportClient(clnt, oldpos, newpos);
        }
    }

    // This broadcasts sector movement events
    public void BroadcastSectorMovements()
    {
        // Check if there are sector movements to send
        if(DynamicSector.sendsectormovements)
        {
            // Broadcast the actor spawn
            foreach(Client c in Host.Instance.Server.clients)
            {
                if((c != null) && (!c.Loading)) c.SendSectorMovements();
            }

            // Clear all update indicators
            foreach(DynamicSector ds in dynamics)
            {
                // Clear indicator
                ds.SendSectorUpdate = false;
            }

            // Done
            DynamicSector.sendsectormovements = false;
        }
    }

    // This broadcasts the gamestate to all clients
    public void BroadcastGameStateChange()
    {
        // Broadcast the gamestate
        foreach(Client c in Host.Instance.Server.clients)
        {
            if((c != null) && (!c.Loading)) c.SendGameStateChange();
        }
    }

    // This broadcasts an item pickup to all clients
    public void BroadcastItemPickup(Client clnt, Item item, bool attach)
    {
        // Broadcast the pickup
        foreach(Client c in Host.Instance.Server.clients)
        {
            if((c != null) && (!c.Loading)) c.SendItemPickup(clnt, item, attach, false);
        }
    }

    // This broadcasts damage to all clients
    public void BroadcastTakeDamage(Client target, int damage, int health, DEATHMETHOD method)
    {
        // Broadcast the death
        foreach(Client c in Host.Instance.Server.clients)
        {
            if((c != null) && (!c.Loading)) c.SendTakeDamage(target, damage, health, method);
        }
    }

    // This broadcasts an death to all clients
    public void BroadcastClientDeath(Client source, Client target, string message, DEATHMETHOD method, PhysicsState targetstate, Vector2D targetpush)
    {
        // Broadcast the death
        foreach(Client c in Host.Instance.Server.clients)
        {
            if((c != null) && (!c.Loading)) c.SendClientDeath(source, target, message, method, targetstate, targetpush);
        }
    }

    // This broadcasts an actor spawn to all clients
    public void BroadcastSpawnActor(Client clnt, bool start)
    {
        // Broadcast the actor spawn
        foreach(Client c in Host.Instance.Server.clients)
        {
            if((c != null) && (!c.Loading)) c.SendSpawnActor(clnt, start);
        }
    }

    // This broadcasts a client disposed to all clients
    public void BroadcastClientDisposed(Client clnt)
    {
        // Broadcast the disposed
        // But dont send it to the client involved
        foreach(Client c in Host.Instance.Server.clients)
        {
            if((c != null) && (c != clnt)) c.SendClientDisposed(clnt);
        }
    }

    // This broadcasts a client update to all clients
    public void BroadcastClientUpdate(Client clnt)
    {
        // Broadcast the update
        foreach(Client c in Host.Instance.Server.clients)
        {
            if(c != null) c.SendClientUpdate(clnt);
        }
    }

    // This sends ShowMessage to all clients
    public void BroadcastShowMessage(string message, bool onscreen, bool onconsolescreen)
    {
        // Show in console
        if (onconsolescreen && Host.Instance.IsConsoleVisible) WriteLine(message, false);

        // Broadcast the message
        foreach(Client c in Host.Instance.Server.clients)
        {
            if(c != null) c.SendShowMessage(message, onscreen);
        }
    }

    // This sends SayMessage to all clients
    public void BroadcastSayMessage(Client speaker, string message)
    {
        // Show in console
        if (Host.Instance.IsConsoleVisible) WriteLine(message, false);

        // Broadcast the message
        foreach(Client c in Host.Instance.Server.clients)
        {
            if(c != null) c.SendSayMessage(speaker, message);
        }
    }

    // This sends SayMessage to all clients
    public void BroadcastSayMessageSpectators(Client speaker, string message)
    {
        if (Host.Instance.IsServer)
        {
            // Always show in console
            WriteLine(message, false);
        }

        // Broadcast the message
        foreach(Client c in Host.Instance.Server.clients)
        {
            if((c != null) && c.Spectator) c.SendSayMessage(speaker, message);
        }
    }

    // This sends SayMessage to all clients
    public void BroadcastSayMessageTeam(Client speaker, string message, TEAM team)
    {
        if (Host.Instance.IsServer)
        {
            // Always show in console
            WriteLine(message, false);
        }

        // Broadcast the message
        foreach(Client c in Host.Instance.Server.clients)
        {
            if((c != null) && (c.Team == team) && !c.Spectator) c.SendSayMessage(speaker, message);
        }
    }

    #endregion

    #region ================== Messages

    // This writes an output message
    public void WriteLine(string text, bool showwhenclient)
    {
        Write(text + Environment.NewLine, showwhenclient);
    }

    // This writes an output message
    public void Write(string text, bool showwhenclient)
    {
        Host.Instance.WriteMessage(text, showwhenclient);
    }

    #endregion

    #region ================== Callvote

    // This starts a callvote
    public void StartCallvote(Client c, string cmd, string args, string description)
    {
        // Set up the callvote
        callvotedesc = description;
        callvotecmd = cmd;
        callvoteargs = args;
        callvotetimeout = Host.Instance.RealTime + CALLVOTE_TIME;
        callvotes = 0;

        // Go for all clients to reset callvote status
        foreach(Client cc in clients) if(cc != null) cc.CallvoteState = 0;

        // Have this client vote immediately
        if(c != null) { c.CallvoteState = 1; callvotes = 1; }

        // Show message
        BroadcastShowMessage(c.Name + " ^7calls voting for " + description, true, true);

        // Broadcast to change
        BroadcastCallvoteStatus();

        // Check if one is enough
        CheckCallvote();
    }

    // This ends a callvote
    public void EndCallvote()
    {
        // Set up the callvote
        callvotetimeout = 0;
        callvotes = 0;

        // Broadcast the end
        BroadcastCallvoteEnd();
    }

    // This checks the callvote
    public void CheckCallvote()
    {
        float fplayers, fvotes, percent;

        // Callvote in progress?
        if(callvotetimeout > 0)
        {
            // Count the votes
            callvotes = 0;
            foreach(Client c in clients)
                if((c != null) && (c.CallvoteState > 0)) callvotes++;

            // Calculate percentage that voted
            fplayers = (float)CountTotalClients();
            fvotes = (float)callvotes;
            percent = fvotes / fplayers;

            // Enough to allow callvote?
            if(percent >= CALLVOTE_PERCENT)
            {
                // End the callvote
                EndCallvote();

                // Perform the command
                PerformCommand(null, callvotecmd, callvoteargs);
            }
        }
    }

    // This processes the callvote
    private void ProcessCallvote()
    {
        // Callvote in progress?
        if(callvotetimeout > 0)
        {
            // Check if time to end the callvote
            if(callvotetimeout < Host.Instance.RealTime)
            {
                // End the callvote
                EndCallvote();

                // Callvote failed.
                BroadcastShowMessage("Callvote failed", true, true);
            }
        }
    }

    #endregion

    #region ================== RCon Commands

    // This handles RCON commands
    // Login command is handled at network input
    public void PerformCommand(Client c, string cmd, string args)
    {
        // Handle command
        switch(cmd)
        {
            case "kick": cKick(c, args); break;
            case "ban": cBan(c, args); break;
            case "map": cMap(c, args); break;
            case "nextmap": cNextMap(c, args); break;
            case "restartmap": cRestartMap(c, args); break;
            case "ip": cIP(c, args); break;
            case "login": if(c != null) c.SendShowMessage("Already authorized for remote control", false); break;
            default: if(c != null) c.SendShowMessage("Unknown command \"" + cmd + "\"", false); break;
        }
    }

    // Handle client kick
    private void cIP(Client cc, string args)
    {
        Client c = null;
        int id;

        // Show info when no arguments given
        if(args.Trim().Length == 0)
        {
            // Show info
            if(cc != null) cc.SendShowMessage("Usage:  /rcon ip ^0player^7", false);
            if(cc != null) cc.SendShowMessage("Where ^0player^7 can be the player name or ID", false);
            return;
        }

        // Check if the player name is an index
        if(args.StartsWith("#"))
        {
            try
            {
                // Get the index
                id = int.Parse(args.Substring(1), CultureInfo.InvariantCulture);
            }
            catch(Exception)
            {
                // WTF?
                if(cc != null) cc.SendShowMessage("Invalid player name or ID \"" + args + "\"", false);
                return;
            }

            // Check if index within bounds
            if((id > -1) && (id < Host.Instance.Server.MaxClients))
            {
                // Get client by index
                c = Host.Instance.Server.clients[id];
                if(c == null)
                {
                    // No client on this index
                    if(cc != null) cc.SendShowMessage("No client connected on slot #" + id, false);
                }
            }
            else
            {
                // No client on this index
                if(cc != null) cc.SendShowMessage("No client connected on slot #" + id, false);
            }
        }
        else
        {
            // Find the player by name
            for(int i = 0; i < Host.Instance.Server.MaxClients; i++)
            {
                // Client here?
                if(Host.Instance.Server.clients[i] != null)
                {
                    // Name matches?
                    string n = Host.Instance.Server.clients[i].Name;
                    n = Markup.StripColorCodes(n);
                    if(string.Compare(n.Trim(), args.Trim(), true) == 0)
                    {
                        // Get the client
                        c = Host.Instance.Server.clients[i];
                    }
                }
            }

            // No client found?
            if(c == null)
            {
                // No client with that name
                if(cc != null) cc.SendShowMessage("No client connected with the name \"" + args.Trim() + "\"", false);
            }
        }

        // Client found?
        if(c != null)
        {
            // Show IP
            if(cc != null) cc.SendShowMessage("IP address of " + c.Name + " ^7 is " + c.Connection.Address.Address, false);
        }
    }

    // Handles map change command
    private void cMap(Client c, string args)
    {
        // Show info when no arguments given
        if(args.Trim().Length == 0)
        {
            // Show info
            if(c != null) c.SendShowMessage("Usage:  /rcon map ^0mapname^7", false);
            if(c != null) c.SendShowMessage("Where ^0mapname^7 is the short name of the map to load", false);
            return;
        }

        // Check if this map exists
        if(ArchiveManager.FindFileArchive(args + ".wad") != "")
        {
            // Start the map
            Host.Instance.Server.StartCurrentMap(args);
        }
        else
        {
            // No such map on the server
            if(c != null) c.SendShowMessage("This server does not have a map named \"" + args + "\".", true);
        }
    }

    // Handles nextmap command
    private void cNextMap(Client c, string args)
    {
        // Next map
        Host.Instance.Server.NextMap();
    }

    // Handles restartmap command
    private void cRestartMap(Client c, string args)
    {
        // Reload same map
        Host.Instance.Server.StartCurrentMap(Host.Instance.Server.map.Name);
    }

    // Handle client kick
    private void cKick(Client cc, string args)
    {
        Client c = null;
        string reason = "Kicked";
        int id;

        // Show info when no arguments given
        if(args.Trim().Length == 0)
        {
            // Show info
            if(cc != null) cc.SendShowMessage("Usage:  /rcon kick ^0player^7 [, ^0reason^7]", false);
            if(cc != null) cc.SendShowMessage("Where ^0player^7 can be the player name or ID", false);
            return;
        }

        // Split player name and reason
        string[] allargs = args.Split(new char[] {','}, 2);

        // Check if the player name is an index
        if(allargs[0].StartsWith("#"))
        {
            try
            {
                // Get the index
                id = int.Parse(allargs[0].Substring(1), CultureInfo.InvariantCulture);
            }
            catch(Exception)
            {
                // WTF?
                if(cc != null) cc.SendShowMessage("Invalid player name or ID \"" + allargs[0] + "\"", false);
                return;
            }

            // Check if index within bounds
            if((id > -1) && (id < Host.Instance.Server.MaxClients))
            {
                // Get client by index
                c = Host.Instance.Server.clients[id];
                if(c == null)
                {
                    // No client on this index
                    if(cc != null) cc.SendShowMessage("No client connected on slot #" + id, false);
                }
            }
            else
            {
                // No client on this index
                if(cc != null) cc.SendShowMessage("No client connected on slot #" + id, false);
            }
        }
        else
        {
            // Find the player by name
            for(int i = 0; i < Host.Instance.Server.MaxClients; i++)
            {
                // Client here?
                if(Host.Instance.Server.clients[i] != null)
                {
                    // Name matches?
                    string n = Host.Instance.Server.clients[i].Name;
                    n = Markup.StripColorCodes(n);
                    if(string.Compare(n.Trim(), allargs[0].Trim(), true) == 0)
                    {
                        // Get the client
                        c = Host.Instance.Server.clients[i];
                    }
                }
            }

            // No client found?
            if(c == null)
            {
                // No client with that name
                if(cc != null) cc.SendShowMessage("No client connected with the name \"" + allargs[0].Trim() + "\"", false);
            }
        }

        // Client found?
        if(c != null)
        {
            // Determine reason
            if(allargs.Length > 1) reason = allargs[1].Trim();
            reason = Markup.StripColorCodes(reason);

            // Disconnect client
            c.SendDisconnect(reason);
            c.Dispose();

            // Show message
            if(allargs.Length > 1)
                Host.Instance.Server.BroadcastShowMessage(c.Name + "^7 kicked (" + reason + ")", true, true);
            else
                Host.Instance.Server.BroadcastShowMessage(c.Name + "^7 kicked", true, true);
        }
    }

    // Handle client ban
    private void cBan(Client cc, string args)
    {
        Client c = null;
        string address = "";
        string reason = "Banned";
        string[] ipparts;
        int id;

        // Show info when no arguments given
        if(args.Trim().Length == 0)
        {
            // Show info
            if(cc != null) cc.SendShowMessage("Usage:  /rcon ban ^0player^7 [, ^0reason^7]", false);
            if(cc != null) cc.SendShowMessage("Where ^0player^7 can be the player name or ID, or IP address", false);
            return;
        }

        // Split player name and reason
        string[] allargs = args.Split(new char[] {','}, 2);

        // Check if the player name is an index
        if(allargs[0].StartsWith("#"))
        {
            try
            {
                // Get the index
                id = int.Parse(allargs[0].Substring(1), CultureInfo.InvariantCulture);
            }
            catch(Exception)
            {
                // WTF?
                if(cc != null) cc.SendShowMessage("Invalid player name, ID or IP address \"" + allargs[0] + "\"", false);
                return;
            }

            // Check if index within bounds
            if((id > -1) && (id < Host.Instance.Server.MaxClients))
            {
                // Get client by index
                c = Host.Instance.Server.clients[id];
                if(c == null)
                {
                    // No client on this index
                    if(cc != null) cc.SendShowMessage("No client connected on slot #" + id, false);
                }
                else
                {
                    // Get client IP address
                    address = c.Connection.Address.Address.ToString();
                }
            }
            else
            {
                // No client on this index
                if(cc != null) cc.SendShowMessage("No client connected on slot #" + id, false);
            }
        }
        else
        {
            // Find the player by name
            for(int i = 0; i < Host.Instance.Server.MaxClients; i++)
            {
                // Client here?
                if(Host.Instance.Server.clients[i] != null)
                {
                    // Name matches?
                    string n = Host.Instance.Server.clients[i].Name;
                    n = Markup.StripColorCodes(n);
                    if(string.Compare(n.Trim(), allargs[0].Trim(), true) == 0)
                    {
                        // Get the client
                        c = Host.Instance.Server.clients[i];
                    }
                }
            }

            // No client found?
            if(c == null)
            {
                // Get parts of IP address
                ipparts = allargs[0].Trim().Split('.');

                // Check if the name is an IP address
                if(ipparts.Length == 4)
                {
                    try
                    {
                        int.Parse(ipparts[0], CultureInfo.InvariantCulture);
                        int.Parse(ipparts[1], CultureInfo.InvariantCulture);
                        int.Parse(ipparts[2], CultureInfo.InvariantCulture);
                        int.Parse(ipparts[3], CultureInfo.InvariantCulture);
                    }
                    catch(Exception)
                    {
                        // WTF?
                        if(cc != null) cc.SendShowMessage("Invalid player name, ID or IP address \"" + allargs[0] + "\"", false);
                        return;
                    }

                    // Use argument as IP address
                    address = allargs[0].Trim();
                }
                else
                {
                    // No client with that name
                    if(cc != null) cc.SendShowMessage("No client connected with the name \"" + allargs[0].Trim() + "\"", false);
                }
            }
            else
            {
                // Get client IP address
                address = c.Connection.Address.Address.ToString();
            }
        }

        // IP address found?
        if(address != "")
        {
            // Determine reason
            if(allargs.Length > 1) reason = allargs[1].Trim();
            reason = Markup.StripColorCodes(reason);

            // Add IP address to ban list and show the result
            if(cc != null) cc.SendShowMessage(Host.Instance.Server.AddLocalBan(address, reason), true);
        }
    }

    #endregion

    #region ================== Items

    // This respawns all items
    public void RespawnAllItems()
    {
        // Respawn all items
        foreach(Item item in items.Values) item.Respawn();
    }

    #endregion

    #region ================== Processing

    // This processes one pass
    public void Process()
    {
        // Process networking
        ProcessNetworking();

        // Process all dynamic sectors
        foreach(DynamicSector ds in dynamics)
        {
            // Process sector
            ds.Process();
        }

        // Process all projectiles
        List<Projectile> prjs = new List<Projectile>(projectiles.Values);
        foreach(Projectile p in prjs)
        {
            // Process the projectile
            p.Process();
        }

        // Remove disposed projectiles
        foreach(Projectile p in disposeprojectiles)
        {
            // Remove it
            projectiles.Remove(p.ID);
        }
        disposeprojectiles.Clear();

        // Send sector movements, if any
        BroadcastSectorMovements();

        // Process all items
        foreach(Item item in items.Values) item.Process();

        // Process all clients
        for(int i = 0; i < maxclients; i++)
        {
            // Process client
            if(clients[i] != null) clients[i].Process();
        }

        // Process callvote
        ProcessCallvote();

        // Change gamestate if needed
        ControlGameState();
    }

    #endregion
}
