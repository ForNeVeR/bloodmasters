/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

// The HUD shows onscreen messages and player status as well as
// the score results at the end of the game.

using System;
using System.Drawing;
using System.Globalization;
using SharpDX.Direct3D9;

namespace CodeImp.Bloodmasters.Client;

public class HUD
{
    #region ================== Constants

    // Message timeouts
    public const int MSG_DEATH_TIMEOUT = 3000;
    public const int MSG_ITEM_TIMEOUT = 1000;

    // Fade speeds
    private const float SMALL_FADE_SPEED = 0.02f;
    private const float BIG_FADE_SPEED = 0.02f;
    private const float ITEM_FADE_SPEED = 0.01f;

    // Screen flashes
    private const float FLASH_RED = 0.8f;
    private const float FLASH_FADE = 0.01f;
    private const float FLASH_MAX = 0.9f;
    private const float FLASH_ALPHA = 0.7f;

    // Status bar flashes
    private const int FLASH_TIME = 200;

    // Window coordinates
    private const float BAR_TOP = 0.911f;
    private const float BAR_HEIGHT = 0.092f;
    private const float BORDER_SIZE = 0.02f;

    #endregion

    #region ================== Variables

    // Show HUD items?
    public static bool showhud = true;

    // Health/Armor/Weapon/Ammo
    private WindowBorder healthwnd;
    private WindowBorder armorwnd;
    private WindowBorder weaponwnd;
    private WindowBorder powerupwnd;
    private WindowBorder scorewnd;
    private TextureResource flashtexture;
    private TextureResource healthicon;
    private TextureResource armoricon;
    private TextureResource[] weaponicons = new TextureResource[(int)WEAPON.TOTAL_WEAPONS];
    private TextureResource[] powerupicons = new TextureResource[(int)POWERUP.TOTAL_POWERUPS];
    private TLVertex[] healthverts;
    private TLVertex[] armorverts;
    private TLVertex[] barverts;
    private TLVertex[] weaponverts;
    private TLVertex[] powerupverts;
    private TextResource healthtext;
    private TextResource armortext;
    private TextResource ammotext;
    private TextResource poweruptext;
    private TextResource scoretext;
    private TextResource callvotetext;
    private int prevhealth;
    private int healthflashtime;

    // Centered messages
    private TextResource itemmessage;
    private TextResource smallmessage;
    private TextResource bigmessage;
    private float smallfade = 0f;
    private int smallfadeout = 0;
    private float bigfade = 0f;
    private int bigfadeout = 0;
    private float itemfade = 0f;
    private int itemfadeout = 0;

    // FPS counter
    private bool showfps;				// Count and show the FPS?
    private int fps_lasttime;			// Time FPS was last measured
    private int fps_measuretime;		// Time to measure FPS
    private int fps_count;				// FPS measured
    private TextResource fps_text;		// The displayed FPS text
    private TextResource mspf_text;		// The displayed MSPF text

    // Screen flashes
    private Border flashborder;
    private float flashalpha;
    private bool showscreenflashes;

    // Callvotes
    private string callvotedesc;

    // Countdown
    private int lastcountdown;

    #endregion

    #region ================== Properties

    public bool ShowFPS { get { return showfps; } set { showfps = value; } }
    public int LastCountdownNumber { get { return lastcountdown; } set { lastcountdown = value; } }
    public string CallVoteDescription { get { return callvotedesc; } set { callvotedesc = value; } }
    public bool ShowScreenFlashes { get { return showscreenflashes; } set { showscreenflashes = value; } }

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public HUD()
    {
        string tempfile;
        int i;

        // Read settings
        showhud = General.config.ReadSetting("showhud", true);

        // Vertices for bottom bar
        barverts = Direct3D.TLRect(0f, 0.91f * (float)Direct3D.DisplayHeight,
            (float)Direct3D.DisplayWidth, (float)Direct3D.DisplayHeight);
        barverts[0].color = General.ARGB(0f, 0f, 0f, 0f);
        barverts[1].color = General.ARGB(0f, 0f, 0f, 0f);
        barverts[2].color = General.ARGB(1f, 0f, 0f, 0f);
        barverts[3].color = General.ARGB(1f, 0f, 0f, 0f);

        // Make windows
        healthwnd = new WindowBorder(-0.003f, BAR_TOP, 0.19f, BAR_HEIGHT, BORDER_SIZE);
        armorwnd = new WindowBorder(0.19f, BAR_TOP, 0.19f, BAR_HEIGHT, BORDER_SIZE);
        weaponwnd = new WindowBorder(0.384f, BAR_TOP, 0.23f, BAR_HEIGHT, BORDER_SIZE);
        powerupwnd = new WindowBorder(0.618f, BAR_TOP, 0.155f, BAR_HEIGHT, BORDER_SIZE);
        scorewnd = new WindowBorder(0.776f, BAR_TOP, 0.226f, BAR_HEIGHT, BORDER_SIZE);

        // Load textures
        tempfile = ArchiveManager.ExtractFile("General.rar/healthicon.tga");
        healthicon = Direct3D.LoadTexture(tempfile, true);
        tempfile = ArchiveManager.ExtractFile("General.rar/armoricon.tga");
        armoricon = Direct3D.LoadTexture(tempfile, true);
        tempfile = ArchiveManager.ExtractFile("General.rar/red.bmp");
        flashtexture = Direct3D.LoadTexture(tempfile, true);

        // Weapon textures
        for(i = 0; i < (int)WEAPON.TOTAL_WEAPONS; i++)
        {
            int weaponnum = i + 1;
            tempfile = ArchiveManager.ExtractFile("General.rar/weapon" + weaponnum.ToString(CultureInfo.InvariantCulture) + "icon.tga");
            weaponicons[i] = Direct3D.LoadTexture(tempfile, true);
        }

        // Powerup textures
        for(i = 0; i < (int)POWERUP.TOTAL_POWERUPS; i++)
        {
            int powerupnum = i + 1;
            tempfile = ArchiveManager.ExtractFile("General.rar/powerup" + powerupnum.ToString(CultureInfo.InvariantCulture) + "icon.tga");
            powerupicons[i] = Direct3D.LoadTexture(tempfile, true);
        }

        // Create screen flash
        flashborder = new Border(General.ARGB(0f, FLASH_RED, 0f, 0f));
        flashborder.Position(0f, 0f, 1f, 1f);
        flashborder.Color = General.ARGB(1f, 1f, 1f, 1f);
        flashborder.Texture = flashtexture;

        // Make vertices for health icon
        healthverts = Direct3D.TLRect(0.016f * (float)Direct3D.DisplayWidth,
            0.935f * (float)Direct3D.DisplayHeight,
            0.052f * (float)Direct3D.DisplayWidth,
            0.981f * (float)Direct3D.DisplayHeight);

        // Setup health text
        healthtext = Direct3D.CreateTextResource(General.charset_shaded);
        healthtext.Texture = General.font_shaded.texture;
        healthtext.HorizontalAlign = TextAlignX.Left;
        healthtext.VerticalAlign = TextAlignY.Middle;
        healthtext.Viewport = new RectangleF(0.07f, 0.94f, 0.04f, 0.04f);
        healthtext.Colors = TextResource.color_brighttext;
        healthtext.Scale = 1.0f;

        // Make vertices for armor icon
        armorverts = Direct3D.TLRect(0.208f * (float)Direct3D.DisplayWidth,
            0.926f * (float)Direct3D.DisplayHeight,
            0.253f * (float)Direct3D.DisplayWidth,
            0.984f * (float)Direct3D.DisplayHeight);

        // Setup armor text
        armortext = Direct3D.CreateTextResource(General.charset_shaded);
        armortext.Texture = General.font_shaded.texture;
        armortext.HorizontalAlign = TextAlignX.Left;
        armortext.VerticalAlign = TextAlignY.Middle;
        armortext.Viewport = new RectangleF(0.27f, 0.94f, 0.04f, 0.04f);
        armortext.Colors = TextResource.color_brighttext;
        armortext.Scale = 1.0f;

        // Make vertices for weapon icon
        weaponverts = Direct3D.TLRect(0.400f * (float)Direct3D.DisplayWidth,
            0.930f * (float)Direct3D.DisplayHeight,
            0.495f * (float)Direct3D.DisplayWidth,
            0.995f * (float)Direct3D.DisplayHeight);

        // Setup ammo text
        ammotext = Direct3D.CreateTextResource(General.charset_shaded);
        ammotext.Texture = General.font_shaded.texture;
        ammotext.HorizontalAlign = TextAlignX.Left;
        ammotext.VerticalAlign = TextAlignY.Middle;
        ammotext.Viewport = new RectangleF(0.5f, 0.94f, 0.04f, 0.04f);
        ammotext.Colors = TextResource.color_brighttext;
        ammotext.Scale = 1.0f;

        // Make vertices for powerup icon
        powerupverts = Direct3D.TLRect(0.634f * (float)Direct3D.DisplayWidth,
            0.933f * (float)Direct3D.DisplayHeight,
            0.678f * (float)Direct3D.DisplayWidth,
            0.982f * (float)Direct3D.DisplayHeight);

        // Setup powerup text
        poweruptext = Direct3D.CreateTextResource(General.charset_shaded);
        poweruptext.Texture = General.font_shaded.texture;
        poweruptext.HorizontalAlign = TextAlignX.Left;
        poweruptext.VerticalAlign = TextAlignY.Middle;
        poweruptext.Viewport = new RectangleF(0.685f, 0.94f, 0.04f, 0.04f);
        poweruptext.Colors = TextResource.color_brighttext;
        poweruptext.Scale = 1.0f;

        // Setup score text
        scoretext = Direct3D.CreateTextResource(General.charset_shaded);
        scoretext.Texture = General.font_shaded.texture;
        scoretext.HorizontalAlign = TextAlignX.Left;
        scoretext.VerticalAlign = TextAlignY.Middle;
        scoretext.Viewport = new RectangleF(0.8f, 0.94f, 0.04f, 0.04f);
        scoretext.Colors = TextResource.color_brighttext;
        scoretext.Scale = 1.0f;

        // Setup small message
        smallmessage = Direct3D.CreateTextResource(General.charset_shaded);
        smallmessage.Texture = General.font_shaded.texture;
        smallmessage.HorizontalAlign = TextAlignX.Center;
        smallmessage.VerticalAlign = TextAlignY.Middle;
        smallmessage.Viewport = new RectangleF(0f, 0.12f, 1f, 0f);
        smallmessage.Colors = TextResource.color_brighttext;
        smallmessage.Scale = 0.6f;

        // Setup big message
        bigmessage = Direct3D.CreateTextResource(General.charset_shaded);
        bigmessage.Texture = General.font_shaded.texture;
        bigmessage.HorizontalAlign = TextAlignX.Center;
        bigmessage.VerticalAlign = TextAlignY.Middle;
        bigmessage.Viewport = new RectangleF(0f, 0.18f, 1f, 0f);
        bigmessage.Colors = TextResource.color_brighttext;
        bigmessage.Scale = 1.0f;

        // Setup item message
        itemmessage = Direct3D.CreateTextResource(General.charset_shaded);
        itemmessage.Texture = General.font_shaded.texture;
        itemmessage.HorizontalAlign = TextAlignX.Center;
        itemmessage.VerticalAlign = TextAlignY.Middle;
        itemmessage.Viewport = new RectangleF(0f, 0.89f, 1f, 0f);
        itemmessage.Colors = TextResource.color_brighttext;
        itemmessage.Scale = 0.6f;

        // Setup FPS text
        fps_text = Direct3D.CreateTextResource(General.charset_shaded);
        fps_text.Texture = General.font_shaded.texture;
        fps_text.HorizontalAlign = TextAlignX.Right;
        fps_text.VerticalAlign = TextAlignY.Top;
        fps_text.Viewport = new RectangleF(0f, 0.025f, 0.995f, 1f);
        fps_text.Colors = TextResource.color_brighttext;
        fps_text.Scale = 0.4f;

        // Setup MSPF text
        mspf_text = Direct3D.CreateTextResource(General.charset_shaded);
        mspf_text.Texture = General.font_shaded.texture;
        mspf_text.HorizontalAlign = TextAlignX.Right;
        mspf_text.VerticalAlign = TextAlignY.Top;
        mspf_text.Viewport = new RectangleF(0f, 0.005f, 0.995f, 1f);
        mspf_text.Colors = TextResource.color_brighttext;
        mspf_text.Scale = 0.4f;

        // Setup callvote text
        callvotetext = Direct3D.CreateTextResource(General.charset_shaded);
        callvotetext.Texture = General.font_shaded.texture;
        callvotetext.HorizontalAlign = TextAlignX.Left;
        callvotetext.VerticalAlign = TextAlignY.Top;
        callvotetext.Viewport = new RectangleF(0.005f, 0.86f, 0.9f, 0.1f);
        callvotetext.Colors = TextResource.color_brighttext;
        callvotetext.Scale = 0.4f;
    }

    // Dispose
    public void Dispose()
    {
        // Clean up
        if(flashborder != null) flashborder.Dispose();
        if(smallmessage != null) smallmessage.Destroy();
        if(bigmessage != null) bigmessage.Destroy();
        if(itemmessage != null) itemmessage.Destroy();
        if(fps_text != null) fps_text.Destroy();
        if(mspf_text != null) mspf_text.Destroy();
        if(scoretext != null) scoretext.Destroy();
        if(healthtext != null) healthtext.Destroy();
        if(armortext != null) armortext.Destroy();
        if(poweruptext != null) poweruptext.Destroy();
        if(healthwnd != null) healthwnd.Dispose();
        if(armorwnd != null) armorwnd.Dispose();
        if(weaponwnd != null) weaponwnd.Dispose();
        if(powerupwnd != null) powerupwnd.Dispose();
        if(scorewnd != null) scorewnd.Dispose();
        GC.SuppressFinalize(this);
    }

    #endregion

    #region ================== Resource Management

    // Destroys all resource for a device reset
    public void UnloadResources()
    {
        // Clean up
        if(healthwnd != null) healthwnd.DestroyGeometry();
        if(armorwnd != null) armorwnd.DestroyGeometry();
        if(weaponwnd != null) weaponwnd.DestroyGeometry();
        if(powerupwnd != null) powerupwnd.DestroyGeometry();
        if(scorewnd != null) scorewnd.DestroyGeometry();
    }

    // Rebuilds the required resources
    public void ReloadResources()
    {
        // Reload
        if(healthwnd != null) healthwnd.CreateGeometry();
        if(armorwnd != null) armorwnd.CreateGeometry();
        if(weaponwnd != null) weaponwnd.CreateGeometry();
        if(powerupwnd != null) powerupwnd.CreateGeometry();
        if(scorewnd != null) scorewnd.CreateGeometry();
    }

    #endregion

    #region ================== Messages

    // This shows the spectating message when spectating
    // Affects only the small message
    public void ShowModeMessage()
    {
        // Spectating?
        if(General.localclient.IsSpectator)
        {
            // Spectating someone?
            if((General.arena.SpectatePlayer > -1) &&
               (General.clients[General.arena.SpectatePlayer] != null))
            {
                // Show spectating player name
                ShowSmallMessage("Spectating:  " + General.clients[General.arena.SpectatePlayer].FormattedName, 0);
            }
            else
            {
                // Just show spectating
                ShowSmallMessage("Spectating", 0);
            }
        }
        else
        {
            // Waiting gamestate?
            if(General.gamestate == GAMESTATE.WAITING)
            {
                // Show waiting message
                ShowSmallMessage("Waiting for opponents", 0);
            }
            // Respawn gamestate?
            else if(General.gamestate == GAMESTATE.SPAWNING)
            {
                // Show respawning message
                ShowSmallMessage("Respawning players", 0);
            }
            // Countdown gamestate?
            else if(General.gamestate == GAMESTATE.COUNTDOWN)
            {
                // Show countdown message
                ShowSmallMessage("Prepare to fight!", 0);
            }
            // Round finish gamestate?
            else if(General.gamestate == GAMESTATE.ROUNDFINISH)
            {
                // Find the winning client
                Client winner = General.scoreboard.GetWinningClient();

                // Show winner in message
                ShowSmallMessage(winner.Name + " ^7has won this round!", 0);
            }
            else
            {
                // No message
                HideSmallMessage();
            }
        }
    }

    // This shows a small message
    // If timeout is 0 it will be displayed until removed manually
    public void ShowSmallMessage(string msg, int timeout)
    {
        // Set the message
        smallmessage.Text = msg;
        smallfade = 1f;

        // Determine disappear time
        if(timeout == 0)
        {
            // Forever
            smallfadeout = int.MaxValue;
        }
        else
        {
            // Timeout
            smallfadeout = SharedGeneral.currenttime + timeout;
        }
    }

    // This hides (fades out) the small message
    public void HideSmallMessage()
    {
        // Fade out now
        smallfadeout = SharedGeneral.currenttime;
    }

    // This shows a big message
    // If timeout is 0 it will be displayed until removed manually
    public void ShowBigMessage(string msg, int timeout)
    {
        // Set the message
        bigmessage.Text = msg;
        bigfade = 1f;

        // Determine disappear time
        if(timeout == 0)
        {
            // Forever
            bigfadeout = int.MaxValue;
        }
        else
        {
            // Timeout
            bigfadeout = SharedGeneral.currenttime + timeout;
        }
    }

    // This hides (fades out) the big message
    public void HideBigMessage()
    {
        // Fade out now
        bigfadeout = SharedGeneral.currenttime;
    }

    // This shows an item message
    public void ShowItemMessage(string msg)
    {
        // Set the message
        itemmessage.Text = msg;
        itemfade = 1f;
        itemfadeout = SharedGeneral.currenttime + MSG_ITEM_TIMEOUT;
    }

    // This hides (fades out) the item message
    public void HideItemMessage()
    {
        // Fade out now
        itemfadeout = SharedGeneral.currenttime;
    }

    #endregion

    #region ================== Screen Flashes

    // This cumulatively adds brightness to the screen flash
    public void FlashScreen(float amount)
    {
        // Flash
        flashalpha += amount;
    }

    #endregion

    #region ================== Methods

    // This updates the score values
    public void UpdateScore()
    {
        int totalitems, score, topscore;
        Client c;

        // Local client available?
        if(General.localclient == null) return;

        // Deathmatch?
        if((General.gametype == GAMETYPE.DM))
        {
            // Winning client
            c = General.scoreboard.GetWinningClient();
            if(c != null) topscore = c.Score; else topscore = 0;

            // Show scores
            scoretext.Text = General.localclient.Score + " / " + topscore;
        }
        // Team Deathmatch / CTF?
        else if((General.gametype == GAMETYPE.TDM) ||
                (General.gametype == GAMETYPE.CTF))
        {
            // Show scores
            scoretext.Text = "^4" + General.teamscore[1] + " ^7/ ^1" + General.teamscore[2];
        }
        // Scavenger?
        else if((General.gametype == GAMETYPE.SC) ||
                (General.gametype == GAMETYPE.TSC))
        {
            // Calculate values
            totalitems = ScavengerItem.CountTotalItems(General.localclient.Team);
            score = totalitems - ScavengerItem.CountRemainingItems(General.localclient.Team);

            // Score text dislays items taken and total items
            scoretext.Text = score + " / " + totalitems;
        }
    }

    #endregion

    #region ================== Processing

    // This processes the HUD
    public void Process()
    {
        // Counting down?
        if(General.gamestate == GAMESTATE.COUNTDOWN)
        {
            // Determine countdown number in seconds
            int thiscountdown = (int)Math.Ceiling(((float)General.gamestateend - (float)SharedGeneral.currenttime) / 1000f);
            if((thiscountdown != lastcountdown) && (thiscountdown > 0))
            {
                // Change countdown number
                lastcountdown = thiscountdown;
                ShowBigMessage(thiscountdown.ToString(), 400);
                if(thiscountdown < 4) DirectSound.PlaySound("voc_" + thiscountdown + ".wav");
            }
        }

        // Callvote in progress?
        if(General.callvotetimeout > 0)
        {
            // Determine countdown number in seconds
            int votecountdown = (int)Math.Ceiling(((float)General.callvotetimeout - (float)SharedGeneral.currenttime) / 1000f);

            // Remove callvote when countdown reaches 0
            if(votecountdown <= 0) General.callvotetimeout = 0;

            // Make the callvote text
            callvotetext.Text = "Votes for " + callvotedesc + ": " + General.callvotes + "  (callvote timeout: " + votecountdown + ")";
        }

        // Fade out small message?
        if((smallfadeout < SharedGeneral.currenttime) && (smallfade > 0f)) smallfade -= SMALL_FADE_SPEED;

        // Fade out big message?
        if((bigfadeout < SharedGeneral.currenttime) && (bigfade > 0f)) bigfade -= BIG_FADE_SPEED;

        // Fade out item message?
        if((itemfadeout < SharedGeneral.currenttime) && (itemfade > 0f)) itemfade -= ITEM_FADE_SPEED;

        // Decrease flash fade
        if(flashalpha > FLASH_MAX) flashalpha = FLASH_MAX;
        else if(flashalpha > 0f) flashalpha -= FLASH_FADE;
        else if(flashalpha < 0f) flashalpha = 0f;

        // Health lowering?
        if(General.localclient.Health < prevhealth)
        {
            // Flash health red
            healthtext.Colors = TextResource.color_code[4];
            healthflashtime = SharedGeneral.currenttime + FLASH_TIME;
        }

        // Keep prev health
        prevhealth = General.localclient.Health;

        // Reset health flash?
        if((healthflashtime > 0) && (healthflashtime < SharedGeneral.currenttime))
        {
            // Reset health flash color
            healthtext.Colors = TextResource.color_brighttext;
            healthflashtime = 0;
        }
    }

    #endregion

    #region ================== Rendering

    // This render screen flashes
    public void RenderScreenFlashes()
    {
        float a;

        // Flash?
        if(showscreenflashes && (flashalpha > 0f))
        {
            // Render flash
            if(flashalpha > FLASH_ALPHA) a = FLASH_ALPHA; else a = flashalpha;
            flashborder.ModulateColor = General.ARGB(a, 1f, 1f, 1f);
            flashborder.Render();
        }
    }

    // This renders the status
    public void RenderStatus()
    {
        // Set drawing mode
        Direct3D.SetDrawMode(DRAWMODE.TLMODALPHA);

        // Render bottom bar?
        if(!General.localclient.IsSpectator)
        {
            // Render the bar
            //Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, -1);
            //Direct3D.d3dd.SetTexture(0, null);
            //Direct3D.d3dd.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, barverts);

            // Render windows
            healthwnd.Render();
            armorwnd.Render();
            weaponwnd.Render();
            if(General.localclient.Powerup != POWERUP.NONE) powerupwnd.Render();
            scorewnd.Render();

            // Render the health
            Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, -1);
            Direct3D.d3dd.SetTexture(0, healthicon.texture);
            Direct3D.d3dd.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, healthverts);
            healthtext.Text = General.localclient.Health.ToString();
            healthtext.Render();

            // Render the armor
            Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, -1);
            Direct3D.d3dd.SetTexture(0, armoricon.texture);
            Direct3D.d3dd.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, armorverts);
            armortext.Text = General.localclient.Armor.ToString();
            armortext.Render();

            // Weapon?
            if(General.localclient.CurrentWeapon != null)
            {
                // Get current client weapon info
                Weapon w = General.localclient.CurrentWeapon;

                // Render the weapon/ammo
                Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, -1);
                Direct3D.d3dd.SetTexture(0, weaponicons[(int)w.WeaponID].texture);
                Direct3D.d3dd.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, weaponverts);
                ammotext.Text = General.localclient.Ammo[(int)w.AmmoType].ToString();
                ammotext.Render();
            }

            // Powerup?
            if(General.localclient.Powerup != POWERUP.NONE)
            {
                // Get the powerup number
                int pid = (int)General.localclient.Powerup;

                // Determine countdown in seconds
                int pcount = (int)General.localclient.PowerupCount;
                int pcountsec = (int)Math.Ceiling((float)pcount / 1000f);

                // Powerup fired?
                if(General.localclient.PowerupFired)
                {
                    // Show fired powerup countdown in red
                    poweruptext.Text = "^4" + pcountsec.ToString();
                }
                else
                {
                    // Show normal powerup countdown
                    poweruptext.Text = pcountsec.ToString();
                }

                // Render the powerup
                Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, -1);
                Direct3D.d3dd.SetTexture(0, powerupicons[pid - 1].texture);
                Direct3D.d3dd.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, powerupverts);
                poweruptext.Render();
            }

            // Render the score
            Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, -1);
            scoretext.Render();
        }

        // Callvote in progress?
        if(General.callvotetimeout > 0)
        {
            // Render the callvote
            Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, -1);
            callvotetext.Render();
        }
    }

    // This renders the 2 big messages
    public void RenderMessages()
    {
        // Set drawing mode
        Direct3D.SetDrawMode(DRAWMODE.TLMODALPHA);

        // Show the HUD parts?
        if(HUD.showhud)
        {
            // Render small message?
            if(smallfade > 0.01f)
            {
                // Render small message
                Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, General.ARGB(smallfade, 1f, 1f, 1f));
                smallmessage.Render();
            }

            // Render big message?
            if(bigfade > 0.01f)
            {
                // Render big message
                Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, General.ARGB(bigfade, 1f, 1f, 1f));
                bigmessage.Render();
            }

            // Render item message?
            if(itemfade > 0.01f)
            {
                // Render big message
                Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, General.ARGB(itemfade, 1f, 1f, 1f));
                itemmessage.Render();
            }
        }
    }

    // This renders the FPS
    public void RenderFPS()
    {
        // Set drawing mode
        Direct3D.SetDrawMode(DRAWMODE.TLMODALPHA);

        // Show FPS?
        if(showfps)
        {
            // Count this frame
            fps_count++;

            // Time to measure the FPS?
            if(SharedGeneral.currenttime >= fps_measuretime)
            {
                // Update the FPS text object
                fps_text.Text = fps_count + " FPS";

                // Update the MSPF text object
                float mspf = (float)(SharedGeneral.currenttime - fps_lasttime) / (float)fps_count;
                mspf_text.Text = mspf.ToString("0.00") + " MSPF";

                // Reset for next measure
                fps_count = 0;
                fps_lasttime = SharedGeneral.currenttime;

                // If the frame took too long, skip ahead to current time, otherwise,
                // only add a second to the previous time for accurate measuring.
                if(SharedGeneral.currenttime - fps_measuretime > 2000)
                    fps_measuretime = SharedGeneral.currenttime + 1000;
                else
                    fps_measuretime += 1000;
            }

            // Render FPS and MSPF
            Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, General.ARGB(1f, 1f, 1f, 1f));
            fps_text.Render();
            mspf_text.Render();
        }
    }

    #endregion
}
