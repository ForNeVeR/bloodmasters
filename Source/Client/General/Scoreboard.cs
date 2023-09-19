/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

// Table columns:
// Name Score Frags Deaths Ping / Loss

using System;
using System.Collections.Generic;
using System.Drawing;
using CodeImp.Bloodmasters.Client.Graphics;
using CodeImp.Bloodmasters.Client.Items;
using CodeImp.Bloodmasters.Client.Resources;
using SharpDX.Direct3D9;
using Direct3D = CodeImp.Bloodmasters.Client.Graphics.Direct3D;

namespace CodeImp.Bloodmasters.Client;

public class Scoreboard
{
    #region ================== Constants

    private const float BORDER_SIZE = 0.05f;
    private const float ALPHA_NORMAL = 1.0f;
    private const float ALPHA_DEAD = 0.6f;
    private const float TABLE_X = 0.2f;
    private const float TABLE_Y = 0.1f;
    private const float TABLE_WIDTH = 0.6f;
    private const float TABLE_HEIGHT = 0.8f;
    private const float TABLE_BORDERSPACING = 0.05f;
    private const int TABLE_COLS = 6;
    private const int TABLE_ROWS = 50;
    private const float CELL_HEIGHT = 0.02f;
    private const float CELL_NAME_WIDTH = 0.2f;
    private const float CELL_SCORE_WIDTH = 0.07f;
    private const float CELL_FRAGS_WIDTH = 0.07f;
    private const float CELL_DEATHS_WIDTH = 0.07f;
    private const float CELL_PING_WIDTH = 0.11f;
    private const float CELL_LOSS_WIDTH = 0.075f;
    private const float TEAMSCORE_WIDTH = 0.135f;
    private const float TEAMSCORE2_OFFSET = 0.3f;
    private const float TEAMSCORE_TOP = CELL_HEIGHT * 37;
    private const float GAMESETUP_TOP = CELL_HEIGHT * 38;
    private const float LINE_SIZE = 0.0017f;
    private const float FLAG_YOFFSET = 0f;
    private const float FLAG_SPACING = 0f;
    private const float FLAG_WIDTH = 0.02f;
    private const float FLAG_HEIGHT = 0.02f;

    #endregion

    #region ================== Variables

    // Properties
    private bool visible;
    private bool updateneeded = true;

    // Clients array
    private List<Client> clients;

    // Window border and lines
    private WindowBorder window;
    private Border[] headerline;

    // Table cells
    private TextResource[,] cell;

    // Flag icons
    private TextureResource redflag;
    private TextureResource blueflag;
    private TLVertex[] redflagclient;
    private TLVertex[] blueflagclient;

    // Timing
    private int lasttimeleftsec = -1;

    #endregion

    #region ================== Properties

    public bool Visible { get { return visible; } set { visible = value; } }

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public Scoreboard()
    {
        float rowtop;
        float colleft;
        string tempfile;

        // Clients array
        clients = new List<Client>(32);

        // Create window
        window = new WindowBorder(TABLE_X - TABLE_BORDERSPACING,
            TABLE_Y - TABLE_BORDERSPACING,
            TABLE_WIDTH + TABLE_BORDERSPACING * 2f,
            TABLE_HEIGHT + TABLE_BORDERSPACING * 2f,
            BORDER_SIZE);

        // Load the flag icons
        tempfile = ArchiveManager.ExtractFile("Sprites/redflag_hud.tga");
        redflag = Direct3D.LoadTexture(tempfile, true, false);
        tempfile = ArchiveManager.ExtractFile("Sprites/blueflag_hud.tga");
        blueflag = Direct3D.LoadTexture(tempfile, true, false);

        // Create header lines
        headerline = new Border[4];
        headerline[0] = new Border(General.ARGB(1f, 1f, 1f, 1f));
        headerline[1] = new Border(General.ARGB(1f, 1f, 0.3f, 0.2f));
        headerline[2] = new Border(General.ARGB(1f, 0.2f, 0.3f, 1f));
        headerline[3] = new Border(General.ARGB(1f, 0.6f, 0.6f, 0.6f));

        // Create cells array
        cell = new TextResource[TABLE_COLS, TABLE_ROWS];

        // Create all required cells
        for(int y = 0; y < TABLE_ROWS; y++)
        {
            // Create cells for this row
            for(int x = 0; x < TABLE_COLS; x++)
                cell[x, y] = CreateTextResource("");

            // Calculate start coordinates
            rowtop = TABLE_Y + (y * CELL_HEIGHT);
            colleft = TABLE_X;

            // Set up each cell
            cell[0, y].Viewport = new RectangleF(colleft, rowtop, CELL_NAME_WIDTH, CELL_HEIGHT); colleft += CELL_NAME_WIDTH;
            cell[1, y].Viewport = new RectangleF(colleft, rowtop, CELL_SCORE_WIDTH, CELL_HEIGHT); colleft += CELL_SCORE_WIDTH;
            cell[2, y].Viewport = new RectangleF(colleft, rowtop, CELL_FRAGS_WIDTH, CELL_HEIGHT); colleft += CELL_FRAGS_WIDTH;
            cell[3, y].Viewport = new RectangleF(colleft, rowtop, CELL_DEATHS_WIDTH, CELL_HEIGHT); colleft += CELL_DEATHS_WIDTH;
            cell[4, y].Viewport = new RectangleF(colleft, rowtop, CELL_PING_WIDTH, CELL_HEIGHT); colleft += CELL_PING_WIDTH;
            cell[5, y].Viewport = new RectangleF(colleft, rowtop, CELL_LOSS_WIDTH, CELL_HEIGHT); colleft += CELL_LOSS_WIDTH;

            // Align cells
            cell[0, y].HorizontalAlign = TextAlignX.Left;
            cell[1, y].HorizontalAlign = TextAlignX.Right;
            cell[2, y].HorizontalAlign = TextAlignX.Right;
            cell[3, y].HorizontalAlign = TextAlignX.Right;
            cell[4, y].HorizontalAlign = TextAlignX.Right;
            cell[5, y].HorizontalAlign = TextAlignX.Right;
        }
    }

    // Disposer
    public void Dispose()
    {
        // Clean up all cells
        for(int y = 0; y < TABLE_ROWS; y++)
        for(int x = 0; x < TABLE_COLS; x++)
        {
            // Create cell
            if(cell[x, y] != null) cell[x, y].Destroy();
        }

        // Clean up header lines
        for(int i = 0; i < 4; i++) headerline[i].Dispose();

        // Clean up
        window.Dispose();
        cell = null;
        GC.SuppressFinalize(this);
    }

    #endregion

    #region ================== Resource Management

    // Destroys all resource for a device reset
    public void UnloadResources()
    {
        // Clean up
        window.DestroyGeometry();
    }

    // Rebuilds the required resources
    public void ReloadResources()
    {
        // Reload
        window.CreateGeometry();
    }

    #endregion

    #region ================== Clients

    // This returns the client with the highest score
    public Client GetWinningClient()
    {
        Client winner = null;

        // Go for all clients
        foreach(Client c in clients)
        {
            // Check if playing
            if(!c.IsSpectator)
            {
                // Check if better score
                if((winner == null) || (c.Score > winner.Score)) winner = c;
            }
        }

        // Return result
        return winner;
    }

    // This adds a client on the scoreboard
    public void AddClient(Client c)
    {
        // Add client if not listed yet
        if(!clients.Contains(c)) clients.Add(c);

        // Update needed
        updateneeded = true;
    }

    // This removes a client from the scoreboard
    public void RemoveClient(Client c)
    {
        // Remove client if listed
        if(clients.Contains(c)) clients.Remove(c);

        // Update needed
        updateneeded = true;
    }

    // This returns the section index for a client
    public static int GetSectionIndex(Client c)
    {
        // Spectators to the bottom
        if(c.IsSpectator) return 3; else return (int)c.Team;
    }

    // This returns the status string
    private string GetClientStatus(Client c)
    {
        // Loading?
        if(c.IsLoading) return "Loading";
        else if(c.IsSpectator) return "Spectating";
        else if(c.Actor == null) return "Dead";
        else return "Playing";
    }

    #endregion

    #region ================== Board Drawing

    // This sets up a standard scoreboard font textresource
    private TextResource CreateTextResource(string text)
    {
        // Make text resource
        TextResource t = Direct3D.CreateTextResource(General.charset_shaded);
        t.Texture = General.font_shaded.texture;
        t.HorizontalAlign = TextAlignX.Left;
        t.VerticalAlign = TextAlignY.Middle;
        t.Viewport = new RectangleF(0f, 0f, 0f, 0f);
        t.Colors = TextResource.color_brighttext;
        t.Scale = 0.4f;
        t.Text = text;
        return t;
    }

    // This returns the section color
    private string GetSectionColor(int section)
    {
        switch(section)
        {
            case 0: return "^7";
            case 1: return "^4";
            case 2: return "^1";
            case 3: return "^0";
            default: return "";
        }
    }

    // This sets the alpha intensity for an entire row
    private void SetRowAlpha(int row, float alpha)
    {
        // Set alpha on all cells
        for(int x = 0; x < TABLE_COLS; x++) cell[x, row].ModulateColor = General.ARGB(alpha, 1f, 1f, 1f);
    }

    // This sets up a row for a specific client
    private void SetupClientRow(int row, Client c)
    {
        float left, right, top, bottom;

        // Determine section color
        int section = GetSectionIndex(c);
        string sc = GetSectionColor(section);

        // Spectator section?
        if(section == 3)
        {
            // Setup cells
            cell[0, row].Text = "^7" + c.FormattedName;
            cell[1, row].Text = "";
            cell[2, row].Text = "";
            cell[3, row].Text = sc + GetClientStatus(c);
            cell[4, row].Text = sc + c.Ping + "ms";
            cell[5, row].Text = sc + c.Loss + "%";
        }
        else
        {
            // Setup cells
            cell[0, row].Text = "^7" + c.FormattedName;
            cell[1, row].Text = sc + c.Score;
            cell[2, row].Text = sc + c.Frags;
            cell[3, row].Text = sc + c.Deaths;
            cell[4, row].Text = sc + c.Ping + "ms";
            cell[5, row].Text = sc + c.Loss + "%";

            // Only show dead clients translucent when playing
            if(General.gamestate != GAMESTATE.GAMEFINISH)
            {
                // Show normal when alive, translucent when dead
                if(c.Actor != null) SetRowAlpha(row, ALPHA_NORMAL);
                else SetRowAlpha(row, ALPHA_DEAD);
            }
            else
            {
                // Show normal
                SetRowAlpha(row, ALPHA_NORMAL);
            }

            // Client carrying the flag?
            if((c.Carrying != null) && (c.Carrying is Flag))
            {
                // Determine flag icon cooridnates
                left = (cell[0, row].Viewport.Left - FLAG_WIDTH) - FLAG_SPACING;
                top = cell[0, row].Viewport.Top + FLAG_YOFFSET;
                right = left + FLAG_WIDTH;
                bottom = top + FLAG_HEIGHT;

                // Red or blue?
                if(c.Carrying is RedFlag)
                {
                    // Setup red flag icon for this player
                    redflagclient = Direct3D.TLRect(left * Direct3D.DisplayWidth,
                        top * Direct3D.DisplayHeight,
                        right * Direct3D.DisplayWidth,
                        bottom * Direct3D.DisplayHeight,
                        32f, 32f);
                }
                else
                {
                    // Setup blue flag icon for this player
                    blueflagclient = Direct3D.TLRect(left * Direct3D.DisplayWidth,
                        top * Direct3D.DisplayHeight,
                        right * Direct3D.DisplayWidth,
                        bottom * Direct3D.DisplayHeight,
                        32f, 32f);
                }
            }
        }
    }

    // This sets up a row with headers and an underline
    private void SetupHeaderRow(int row, int section)
    {
        string desc;
        string tscore = "";

        // Determine section color
        string sc = GetSectionColor(section);

        // Determine section description
        switch(section)
        {
            case 0: desc = "Players"; break;
            case 1: desc = "Red Team"; break;
            case 2: desc = "Blue Team"; break;
            case 3: desc = "Spectators"; break;
            default: desc = "Clients"; break;
        }

        // Spectator section?
        if(section == 3)
        {
            // Setup cells
            cell[0, row].Text = sc + desc;
            cell[1, row].Text = "";
            cell[2, row].Text = "";
            cell[3, row].Text = sc + "Status";
            cell[4, row].Text = sc + "Ping";
            cell[5, row].Text = sc + "Loss";
        }
        else
        {
            // Add team score?
            if((section == 1) || (section == 2)) tscore = " (" + General.teamscore[section] + ")";

            // Setup cells
            cell[0, row].Text = sc + desc;
            cell[1, row].Text = sc + "Score" + tscore;
            cell[2, row].Text = sc + "Frags";
            cell[3, row].Text = sc + "Deaths";
            cell[4, row].Text = sc + "Ping";
            cell[5, row].Text = sc + "Loss";
        }
    }

    // This makes a row with server info
    private void SetupServerInfoRow(int row)
    {
        // Make the complete string
        string info = "Server:  " + General.servertitle + "  ^7(" + General.serveraddress + ":" + General.serverport + ")";

        // Setup cells
        cell[0, row].Text = info;
        cell[1, row].Text = "";
        cell[2, row].Text = "";
        cell[3, row].Text = "";
        cell[4, row].Text = "";
        cell[5, row].Text = "";
    }

    // This makes a row with game info
    private void SetupGameInfoRow(int row)
    {
        // Make the complete string
        string info = "Game type:  " + General.GameTypeDescription(General.gametype) +
                      "         Score limit:  " + General.scorelimit;

        // Show time remaining?
        if(General.gamestate == GAMESTATE.PLAYING)
        {
            // Make a nice string from remaining milliseconds
            int msleft = General.gamestateend - SharedGeneral.currenttime;
            TimeSpan t = new TimeSpan((long)msleft * 10000L);
            string timeleft = (int)Math.Floor(t.TotalMinutes) + ":" + t.Seconds.ToString("00");

            // Append the string
            info += "         Time left:  " + timeleft;

            // Keep last number of seconds
            lasttimeleftsec = t.Seconds;
        }
        else
        {
            // No last time left
            lasttimeleftsec = -1;
        }

        // Setup cells
        cell[0, row].Text = info;
        cell[1, row].Text = "";
        cell[2, row].Text = "";
        cell[3, row].Text = "";
        cell[4, row].Text = "";
        cell[5, row].Text = "";
    }

    // This clears a row
    private void SetupEmptyRow(int row)
    {
        // Clear all cells
        for(int x = 0; x < TABLE_COLS; x++) cell[x, row].Text = "";
        //cell[0, row].Text = row.ToString();
    }

    // This makes a line on a row
    private void SetupLineRow(int row, int section)
    {
        // Clear all cells
        for(int x = 0; x < TABLE_COLS; x++) cell[x, row].Text = "";

        // Setup line
        headerline[section].Visible = true;
        headerline[section].Position(cell[0, row].Left,
            (cell[0, row].Top + (CELL_HEIGHT * 0.5f)) - LINE_SIZE,
            cell[5, row].Right,
            (cell[5, row].Top + (CELL_HEIGHT * 0.5f)) + LINE_SIZE);
    }

    // This updates the board immediately
    private void UpdateBoard()
    {
        int section = -1;
        int y = 0;

        // Sort the clients
        clients.Sort(new ClientComparer());

        // Reset the lines
        for(int i = 0; i < 4; i++) headerline[i].Visible = false;

        // Reset flags
        blueflagclient = null;
        redflagclient = null;

        // Setup scoreboard header
        SetupServerInfoRow(y); y++;
        SetupGameInfoRow(y); y++;

        // Start at the top
        foreach(Client c in clients)
        {
            // Different section begins here?
            if(GetSectionIndex(c) != section)
            {
                // Spacing
                SetupEmptyRow(y); y++;
                SetupEmptyRow(y); y++;

                // Change current section
                section = GetSectionIndex(c);

                // Setup section header
                SetupHeaderRow(y, section); y++;
                SetupLineRow(y, section); y++;
            }

            // Setup client row
            SetupClientRow(y, c); y++;
        }

        // Continue clearing the board until the end
        while(y < TABLE_ROWS)
        {
            // Make an empty row
            SetupEmptyRow(y); y++;
        }

        // Updated
        updateneeded = false;
    }

    #endregion

    #region ================== Processing

    // This indicates the board needs updating
    public void Update()
    {
        // Update needed
        updateneeded = true;
        General.hud.UpdateScore();
    }

    // Processing
    public void Process()
    {
        // Scores key pressed?
        if(General.gamewindow.ControlPressed("showscores"))
        {
            // Always visible when show scores is pressed
            visible = true;
        }
        // Otherwise...
        else
        {
            // Show only when game has ended
            visible = (General.gamestate == GAMESTATE.GAMEFINISH);
        }

        // Update the board?
        if(visible && updateneeded) UpdateBoard();

        // Calculate number of seconds
        int msleft = General.gamestateend - SharedGeneral.currenttime;
        TimeSpan t = new TimeSpan((long)msleft * 10000L);

        // Time to update game line?
        if(t.Seconds != lasttimeleftsec)
        {
            // Update game info line
            SetupGameInfoRow(1);
        }
    }

    #endregion

    #region ================== Rendering

    // Rendering
    public void Render()
    {
        // Visible?
        if(visible)
        {
            // Render the window
            window.Render();

            // Go for all cells to render
            for(int y = 0; y < TABLE_ROWS; y++)
            for(int x = 0; x < TABLE_COLS; x++)
            {
                // Render cell
                Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, cell[x, y].ModulateColor);
                cell[x, y].Render();
            }

            // Normal blending
            Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, -1);

            // Render flag icons
            Direct3D.d3dd.SetTexture(0, redflag.texture);
            if(redflagclient != null) Direct3D.d3dd.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, redflagclient);
            Direct3D.d3dd.SetTexture(0, blueflag.texture);
            if(blueflagclient != null) Direct3D.d3dd.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, blueflagclient);

            // Render the lines
            for(int i = 0; i < 4; i++)
            {
                // Render if used
                if(headerline[i].Visible) headerline[i].Render();
            }
        }
    }

    #endregion
}
