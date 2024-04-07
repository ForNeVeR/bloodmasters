/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

// The Arena handles everything in the game world. This includes
// the map, players, items, decals, etc. This does NOT include
// the console, HUD or music.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using Bloodmasters.Client.Graphics;
using Bloodmasters.Client.Items;
using Bloodmasters.Client.LevelMap;
using Bloodmasters.Client.Lights;
using Bloodmasters.Client.Projectiles;
using Bloodmasters.Client.Weapons;
using Bloodmasters.LevelMap;
using SharpDX;
using SharpDX.Direct3D9;
using SharpDX.Mathematics.Interop;
using Color = System.Drawing.Color;
using Direct3D = Bloodmasters.Client.Graphics.Direct3D;
using Sprite = Bloodmasters.Client.Graphics.Sprite;

namespace Bloodmasters.Client;

public class Arena
{
    #region ================== Constants

    // Camera
    private const float CAMERA_HEIGHT = 60f;
    private const float CAMERA_SCROLL_SPEED = 1.5f;
    private const float CAMERA_SLIDE_MUL = 0.1f;
    private const float CAMERA_AIM_LENGTH = 60f;

    // View area
    private const float SCREEN_AREA_HEIGHT = 120f;
    private const float SCREEN_AREA_WIDTH = 120f;
    private const float SCREEN_AREA_X = -10f;
    private const float SCREEN_AREA_Y = 10f;

    // VisualSector merging
    private const bool MERGE_SECTORS = true;
    private const float MAX_MERGE_HEIGHT_DIFF = 4f;

    // Dynamic lightmap
    private const int DYNAMIC_LIGHTMAP_SIZE = 512;
    private const float DYNAMIC_LIGHTMAP_SCALE_X = DYNAMIC_LIGHTMAP_SIZE / SCREEN_AREA_WIDTH;
    private const float DYNAMIC_LIGHTMAP_SCALE_Y = DYNAMIC_LIGHTMAP_SIZE / SCREEN_AREA_HEIGHT;
    private const float DYNAMIC_LIGHTMAP_ADJUST_X = 9.88f; //9.99f;
    private const float DYNAMIC_LIGHTMAP_ADJUST_Y = -10.05f; //-9.99f;

    // Initial memory to allocate for objects
    public const int INITIAL_OBJECTS_MEMORY = 3000;

    // Liquid textures
    public const string LIQUID_TEX_WATER = "liquid01";
    public const string LIQUID_TEX_LAVA = "liquid02";
    public const string LIQUID_TEXFILE_WATER = "liquid01.bmp";
    public const string LIQUID_TEXFILE_LAVA = "liquid02.bmp";

    #endregion

    #region ================== Variables

    // Map sectors for rendering
    private List<VisualSector> visualSectors;

    // All game objects for rendering
    // This array is sorted back-to-front every frame
    private readonly List<VisualObject> objects;

    // Lights on the map
    private List<StaticLight> staticlights;
    private List<DynamicLight> dynamiclights;

    // Items on the map
    private Dictionary<string, Item> items;

    // Decals on the map
    private List<Decal> decals;

    // Actors on the map
    private List<Actor> actors;

    // Projectiles
    private readonly Dictionary<string, Projectile> projectiles;

    // Particles
    public ParticleCollection p_dust;
    public ParticleCollection p_magic;
    public ParticleCollection p_smoke;
    public ParticleCollection p_trail;
    public ParticleCollection p_blood;

    // Liquids
    public LiquidGraphics liquidwater;
    public LiquidGraphics liquidlava;

    // Dynamic lightmap
    public Texture dynamiclightmap;
    private float lightmapx;
    private float lightmapy;
    private Matrix lightmaptransform;
    private bool firstframe = true;

    // Camera
    private Vector2 c_target;
    private Vector2 c_pos;
    private Vector3 c_offset = new Vector3(-10f, 10f, -CAMERA_HEIGHT);
    private Vector3 c_rotate = new Vector3(0f, 0f, 1f);
    private Vector3 c_vec;
    private Vector3D hitonmap = new Vector3D();
    private Matrix c_matrix;
    private RectangleF screenarea;
    private int spectateplayer;
    private Matrix c_norm_projection = Matrix.OrthoRH(80f, 60f, -100f, 1000f);
    //private Matrix c_light_projection = Matrix.OrthoOffCenterRH(0, SCREEN_AREA_WIDTH, 0, SCREEN_AREA_HEIGHT, 0f, 1000f);
    private Matrix c_light_projection = Matrix.OrthoRH(SCREEN_AREA_WIDTH, -SCREEN_AREA_HEIGHT, -100f, 1000f);

    // Mouse aim
    // mouseactor are the XY on the local actor's XY plane
    // mousemap are the XY on the targeted sector's XY plane
    //private Vector3 mouseactor;
    private Vector3 mousemap;
    private Sector mousemapsector;

    #endregion

    #region ================== Properties

    public List<VisualSector> VisualSectors { get { return visualSectors; } }
    public List<StaticLight> StaticLights { get { return staticlights; } }
    public List<DynamicLight> DynamicLights { get { return dynamiclights; } }
    public List<Actor> Actors { get { return actors; } }
    public List<VisualObject> Objects { get { return objects; } }
    public Dictionary<string, Item> Items { get { return items; } }
    public Vector3 CameraVector { get { return c_vec; } }
    public RectangleF ScreenArea { get { return screenarea; } }
    public Vector3 MouseAtMap { get { return mousemap; } }
    public Sector MouseInSector { get { return mousemapsector; } }
    public int SpectatePlayer { get { return spectateplayer; } }
    public float LightmapScaleX { get { return DYNAMIC_LIGHTMAP_SCALE_X; } }
    public float LightmapScaleY { get { return DYNAMIC_LIGHTMAP_SCALE_Y; } }
    public float LightmapX { get { return lightmapx; } }
    public float LightmapY { get { return lightmapy; } }
    public Matrix LightmapMatrix { get { return lightmaptransform; } }
    public Vector3D HitOnMap { get { return hitonmap; } }

    #endregion

    #region ================== Constructor / Destructor

    // Constructor
    public Arena()
    {
        int thinggametype = (int)Math.Pow(2, (int)General.gametype);
        VisualSector vs;
        Item item;

        // Assign reference already
        General.arena = this;

        // Create dynamic lightmap
        CreateLightmap();

        // Initialize projectiles
        Projectile.Initialize();

        // Load liquids
        liquidwater = new LiquidGraphics(LIQUID_TEXFILE_WATER, 12);
        liquidlava = new LiquidGraphics(LIQUID_TEXFILE_LAVA, 8);

        // Create dust particle collections
        p_dust = new ParticleCollection("general.zip/particle0.tga", DRAWMODE.PNORMAL);
        p_dust.Lightmapped = true;
        p_dust.Gravity = 0.006f;
        p_dust.MinimumSize = 0.2f;
        p_dust.RandomBright = 1f;
        p_dust.RandomSize = 0.4f;
        p_dust.Timeout = 1000;
        p_dust.RandomTimeout = 1000;
        p_dust.MinimumResize = -0.001f;
        p_dust.RandomResize = 0f;
        p_dust.FadeIn = false;

        // Create magical particle collections
        p_magic = new ParticleCollection("general.zip/particle1.tga", DRAWMODE.PADDITIVE);
        p_magic.Lightmapped = false;
        p_magic.Gravity = 0.005f;
        p_magic.MinimumSize = 2f;
        p_magic.RandomBright = 0.2f;
        p_magic.RandomSize = 0f;
        p_magic.Timeout = 200;
        p_magic.RandomTimeout = 600;
        p_magic.MinimumResize = 0f;
        p_magic.RandomResize = 0f;
        p_magic.FadeIn = false;

        // Create smoke particle collections
        p_smoke = new ParticleCollection("general.zip/particle2.tga", DRAWMODE.PNORMAL);
        p_smoke.Lightmapped = true;
        p_smoke.Gravity = -0.0001f;
        p_smoke.MinimumSize = 3f; //1f;
        p_smoke.RandomBright = 0.6f;
        p_smoke.RandomSize = 4f; //3f;
        p_smoke.Timeout = 1000;
        p_smoke.RandomTimeout = 1000;
        p_smoke.MinimumResize = 0.005f;
        p_smoke.RandomResize = 0.01f;
        p_smoke.FadeIn = true;

        // Create smoke particle collections
        p_trail = new ParticleCollection("general.zip/particle4.tga", DRAWMODE.PNORMAL);
        p_trail.Lightmapped = true;
        p_trail.Gravity = -0.001f;
        p_trail.MinimumSize = 2f; //1.5f;
        p_trail.RandomBright = 0.4f;
        p_trail.RandomSize = 2f;
        p_trail.Timeout = 100; //200;
        p_trail.RandomTimeout = 200;
        p_trail.MinimumResize = 0.004f;
        p_trail.RandomResize = 0.006f;
        p_trail.FadeIn = false;

        // Create blood particle collections
        p_blood = new ParticleCollection("general.zip/particle3.tga", DRAWMODE.PNORMAL);
        p_blood.Lightmapped = true;
        p_blood.Gravity = 0.006f;
        p_blood.MinimumSize = 0.5f; //0.2f;
        p_blood.RandomBright = 1f;
        p_blood.RandomSize = 0.5f; //0.3f;
        p_blood.Timeout = 200;
        p_blood.RandomTimeout = 400;
        p_blood.MinimumResize = 0.01f;
        p_blood.RandomResize = 0.01f;
        p_blood.FadeIn = false;

        // Camera position
        c_pos = new Vector2(50f, -80f);

        // Determine camera vector
        c_vec = Vector3.Subtract(new Vector3(0f, 0f, 0f), c_offset);
        c_vec.Normalize();
        spectateplayer = -1;

        // Make arrays
        visualSectors = new List<VisualSector>();
        staticlights = new List<StaticLight>();
        dynamiclights = new List<DynamicLight>();
        items = new Dictionary<string, Item>();
        objects = new List<VisualObject>(INITIAL_OBJECTS_MEMORY);
        decals = new List<Decal>();
        actors = new List<Actor>();
        projectiles = new Dictionary<string, Projectile>();

        // Ensure unique item ids start at 0
        Item.uniquekeyindex = 0;

        // Make all visual sectors
        for(int s = 0; s < General.map.Sectors.Length; s++)
        {
            // Setup visual sector
            vs = new VisualSector((ClientSector)General.map.Sectors[s]);
            visualSectors.Add(vs);
        }

        // Merge visual sectors
        if(MERGE_SECTORS) MergeVisualSectors();

        // Go for all things
        foreach(Thing t in General.map.Things)
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
                        if(Attribute.IsDefined(tp, typeof(ClientItem), false))
                        {
                            // Get item attribute
                            ClientItem attr = (ClientItem)Attribute.GetCustomAttribute(tp, typeof(ClientItem), false);

                            // Same ID number?
                            if(t.Type == attr.ThingID)
                            {
                                try
                                {
                                    // Create object for this item
                                    object[] args = new object[1];
                                    args[0] = t;
                                    item = (Item)asm.CreateInstance(tp.FullName, false, BindingFlags.Default,
                                        null, args, CultureInfo.CurrentCulture, new object[0]);
                                }
                                // Catch errors
                                catch(TargetInvocationException e)
                                {
                                    // Throw the actual exception
                                    throw(e.InnerException);
                                }

                                // If the item is not temporary
                                // then add it to the items list
                                if(!item.Temporary) items.Add(item.Key, item); else item.Dispose();
                            }
                        }
                    }
                }
            }
        }

        // Finishes the visual sectors
        FinishVisualSectors();

        // DEBUG: Output statistics
        //WriteMapDebugInfo();

        // Show map information
        General.console.AddMessage("Loaded map \"" + General.map.Title + "\" (" + General.map.Name + ") created by " + General.map.Author);
    }

    // Destructor
    public void Dispose()
    {
        // Dispose all visual sectors
        if(visualSectors != null) foreach(VisualSector s in visualSectors) s.Dispose();

        // Dispose all lights
        if(staticlights != null) while(staticlights.Count > 0) staticlights[0].Dispose();
        if(dynamiclights != null) while(dynamiclights.Count > 0) dynamiclights[0].Dispose();

        // Dispose all decals
        if(decals != null) while(decals.Count > 0) decals[0].Dispose();

        // Dispose all actors
        if(actors != null) while(actors.Count > 0) actors[0].Dispose();

        // Dispose all items
        if(items != null)
        {
            foreach(Item i in items.Values) i.Dispose();
        }

        // Dispose all projectiles
        if(projectiles != null)
        {
            foreach(Projectile p in projectiles.Values) p.Dispose();
        }

        // Dispose particles
        p_dust.Dispose();
        p_magic.Dispose();
        p_smoke.Dispose();
        p_trail.Dispose();
        p_blood.Dispose();

        // Destroy dynamic lightmap
        DestroyLightmap();

        // Clean up
        staticlights = null;
        dynamiclights = null;
        visualSectors = null;
        decals = null;
        actors = null;
        items = null;
        p_dust = null;
        p_magic = null;
        p_smoke = null;
        p_trail = null;
        p_blood = null;
    }

    #endregion

    #region ================== Map

    // This merges visual sectors
    private void MergeVisualSectors()
    {
        bool merged;

        do
        {
            // No merges yet
            merged = false;

            // Go for all visual sectors to test for merging
            foreach(VisualSector va in visualSectors)
            {
                // Go for all visual sectors to test for merging
                foreach(VisualSector vb in visualSectors)
                {
                    // Not the same visual sector?
                    // and not marked NO MERGING?
                    if((va != vb) &&
                       ((va.Sectors[0]).Effect != SECTOREFFECT.NOMERGE) &&
                       ((vb.Sectors[0]).Effect != SECTOREFFECT.NOMERGE))
                    {
                        // Determine floor height differences
                        float nh = Math.Max(va.HighestFloor, vb.HighestFloor);
                        float nl = Math.Min(va.LowestFloor, vb.LowestFloor);

                        // Check if merging is possible
                        if((va.FixedLight == vb.FixedLight) &&
                           (va.AmbientLight == vb.AmbientLight) &&
                           (va.DynamicLightmap == vb.DynamicLightmap) &&
                           (Math.Abs(nh - nl) < MAX_MERGE_HEIGHT_DIFF) &&
                           ((va.Sectors[0]).Dynamic == false) &&
                           ((vb.Sectors[0]).Dynamic == false))
                        {
                            // Make union bounary
                            RectangleF u = RectangleF.Union(va.SectorBounds,
                                vb.SectorBounds);

                            // Check if va is the bigger sector
                            if(u == va.SectorBounds)
                            {
                                // Merge vb into va
                                va.Merge(vb);
                                visualSectors.Remove(vb);
                                merged = true;
                                break;
                            }
                            // Check if vb is the bigger sector
                            else if(u == vb.SectorBounds)
                            {
                                // Merge va into vb
                                vb.Merge(va);
                                visualSectors.Remove(va);
                                merged = true;
                                break;
                            }
                        }
                    }
                }

                // Break when merged
                if(merged) break;
            }
        }
        // Continue until no more merging
        while(merged);
    }

    // This indexes and builds geometry for visual sectors
    private void FinishVisualSectors()
    {
        // Go for all visual sectors
        for(int i = 0; i < visualSectors.Count; i++)
        {
            // Get the sector object
            VisualSector s = visualSectors[i];

            // Set the index
            s.SetIndex(i);

            // Make lightmap
            s.CreateLightmap();

            // Build geometry
            s.BuildGeometry();
        }
    }

    #endregion

    #region ================== Debug

    // This outputs map information
    private void WriteMapDebugInfo()
    {
        int dynvissecs = 0;

        // Open file for writing
        FileStream outfile = File.Open(Path.Combine(Paths.Instance.LogDirPath, "mapinfo.txt"),
            FileMode.Create, FileAccess.Write, FileShare.Read);
        StreamWriter writer = new StreamWriter(outfile);

        // Count number of dynamic visualsectors
        foreach(VisualSector vs in visualSectors)
        {
            // Count
            if(vs.DynamicLightmap) dynvissecs++;
        }

        // Wring general information
        writer.WriteLine("Map information for: " + General.map.Title + " (" + General.map.Name + ")");
        writer.WriteLine("Things: " + General.map.Things.Length);
        writer.WriteLine("Sectors: " + General.map.Sectors.Length);
        writer.WriteLine("Linedefs: " + General.map.Linedefs.Length);
        writer.WriteLine("Sidedefs: " + General.map.Sidedefs.Length);
        writer.WriteLine("Vertices: " + General.map.Vertices.Length);
        writer.WriteLine("Subsectors: " + General.map.SubSectors.Length);
        writer.WriteLine("Segments: " + General.map.Segments.Length);
        writer.WriteLine("Nodes: " + General.map.Nodes.Length);
        writer.WriteLine("Lights: " + staticlights.Count);
        writer.WriteLine("VisualSectors: " + visualSectors.Count + " (" + dynvissecs + " dynamic)");

        // Go for all lights
        writer.WriteLine("");
        writer.WriteLine("Lights information");
        writer.WriteLine("============================================");
        for(int i = 0; i < staticlights.Count; i++)
        {
            StaticLight lg = staticlights[i];
            writer.WriteLine("");
            writer.WriteLine("Light " + i + " (thing " + lg.ThingIndex + ", lightmapsize: " + lg.LightmapSize + ")");
            writer.WriteLine("--------------------------------------------");
            lg.WriteLightDebugInfo(writer);
        }

        // Go for all visual sectors
        writer.WriteLine("");
        writer.WriteLine("VisualSectors structure information");
        writer.WriteLine("============================================");
        for(int i = 0; i < visualSectors.Count; i++)
        {
            VisualSector vs = visualSectors[i];
            writer.WriteLine("");
            writer.WriteLine("VisualSector " + i + " (sectors: " + vs.Sectors.Count + " sides: " + vs.VisualSidedefs.Count + ")");
            writer.WriteLine("--------------------------------------------");
            vs.WriteSectorDebugInfo(writer);
        }

        // Close file
        writer.Flush();
        outfile.Flush();
        writer.Close();
        outfile.Close();
    }

    #endregion

    #region ================== Resource Management

    // Destroys all resource for a device reset
    public void UnloadResources()
    {
        // Destroy all sector resources
        foreach(VisualSector s in visualSectors) s.UnloadResources();

        // Destroy all light resources
        foreach(StaticLight l in staticlights) l.UnloadResources();

        // Destroy floor decals
        foreach(Decal d in decals)
            if(d is FloorDecal floorDecal) floorDecal.DestroyGeometry();

        // Destroy dynamic lightmap
        DestroyLightmap();

        // Destroy liquids
        liquidlava.UnloadResources();
        liquidwater.UnloadResources();

        // Destroy generic stuff
        Sprite.DestroyGeometry();
        WallDecal.DestroyGeometry();
        Shadow.DestroyGeometry();
        Bullet.DestroyGeometry();
    }

    // Rebuilds the required resources
    public void ReloadResources()
    {
        // Reload sectors
        foreach(VisualSector s in visualSectors) s.ReloadResources();

        // Reload all lights
        foreach(StaticLight l in staticlights) l.ReloadResources();

        // Reload floor decals
        foreach(Decal d in decals)
            if(d is FloorDecal floorDecal) floorDecal.CreateGeometry();

        // Create dynamic lightmap
        CreateLightmap();

        // Reload liquids
        liquidlava.ReloadResources();
        liquidwater.ReloadResources();

        // Reload generic stuff
        Sprite.CreateGeometry();
        WallDecal.CreateGeometry();
        Shadow.CreateGeometry();
        Bullet.CreateGeometry();
    }

    #endregion

    #region ================== Lightmap

    // This makes the dynamic lightmap
    private void CreateLightmap()
    {
        // Only when using dynamic lights
        if(DynamicLight.dynamiclights)
        {
            // Make a rendertarget for lightmap
            dynamiclightmap = new Texture(Direct3D.d3dd, DYNAMIC_LIGHTMAP_SIZE, DYNAMIC_LIGHTMAP_SIZE, 1,
                Usage.RenderTarget, Direct3D.LightmapFormat, Pool.Default);
        }
    }

    // This destroys the dynamic lightmap
    private void DestroyLightmap()
    {
        // Clean up
        if(dynamiclightmap != null) dynamiclightmap.Dispose();
        dynamiclightmap = null;
    }

    #endregion

    #region ================== Items

    // This respawns ALL items
    public void RespawnAllItems()
    {
        // Go for all items
        foreach(Item i in items.Values)
        {
            // Respawn item now
            i.Respawn(false);
        }
    }

    // This returns an item or null if the item does not exist
    public Item GetItemByKey(string key)
    {
        // Check if the item exists
        if(items.TryGetValue(key, out Item item))
        {
            // Return the item
            return item;
        }
        else
        {
            // Return null
            return null;
        }
    }

    #endregion

    #region ================== Actors

    // This adds an actor
    public void AddActor(Actor a)
    {
        // Add actor to the list
        actors.Add(a);
    }

    // This removes an actor
    public void RemoveActor(Actor a)
    {
        // Remove the actor
        actors.Remove(a);
    }

    #endregion

    #region ================== Projectiles

    // This creates a projectile by ID number
    public Projectile CreateProjectile(PROJECTILE type, string id, Vector3D start, Vector3D vel)
    {
        Assembly asm = Assembly.GetExecutingAssembly();

        // Get the projectile type
        Type tp = Projectile.GetProjectileType(type);

        // Valid projectile type?
        if(tp != null)
        {
            try
            {
                // Create object from this projectile
                object[] args = new object[3];
                args[0] = id;
                args[1] = start;
                args[2] = vel;
                Projectile p = (Projectile)asm.CreateInstance(tp.FullName, false, BindingFlags.Default,
                    null, args, CultureInfo.CurrentCulture, Array.Empty<object>());

                // Add to array
                if(!p.Disposed) projectiles.Add(id, p);

                // Return projectile
                return p;
            }
            // Catch errors
            catch(TargetInvocationException e)
            {
                // Throw the actual exception
                throw(e.InnerException);
            }
        }

        // Nothing found!
        return null;
    }

    // Remove a projectile
    public void RemoveProjectile(Projectile p)
    {
        // Remove if exists
        projectiles.Remove(p.ID);
    }

    // Get a projectile
    public Projectile GetProjectile(string id)
    {
        // Check if exists
        if(projectiles.TryGetValue(id, out Projectile projectile))
        {
            // Return projectile
            return projectile;
        }
        else
        {
            // Return nothing
            return null;
        }
    }

    #endregion

    #region ================== Decals

    // This adds a decal
    public void AddDecal(Decal d)
    {
        // Add decal to the list
        decals.Add(d);
    }

    // This removes a decal
    public void RemoveDecal(Decal d)
    {
        // Remove the decal
        decals.Remove(d);
    }

    #endregion

    #region ================== Game Objects

    // This is called by VisualObject to add to sorted list
    public void AddVisualObject(VisualObject vo)
    {
        // Add object to list
        objects.Add(vo);
    }

    // This is called by VisualObject to remove from sorted list
    public void RemoveVisualObject(VisualObject vo)
    {
        // Remove object from list
        objects.Remove(vo);
    }

    #endregion

    #region ================== Camera

    // This changes spectator mode
    public void SwitchSpectatorMode()
    {
        // Spectating?
        if((General.localclient == null) || !General.localclient.IsSpectator) return;

        // Spectating someone?
        if(spectateplayer > -1)
        {
            // Stop specific spectating
            spectateplayer = -1;
            General.hud.ShowModeMessage();
        }
        else
        {
            // Spectate the first player
            spectateplayer = 0;
            if(!SpectateNextPlayer()) spectateplayer = -1;
        }
    }

    // This changes spectating to next player
    // Returns false when no next player could be found
    public bool SpectateNextPlayer()
    {
        int clientstried = 0;
        int newclient = spectateplayer;

        // Spectating?
        if((General.localclient == null) || !General.localclient.IsSpectator) return false;
        if(spectateplayer == -1) return false;

        do
        {
            // Next
            newclient++;
            if(newclient >= General.clients.Length) newclient = 0;

            // Cancel when too many clients tried
            clientstried++;
            if(clientstried > General.clients.Length + 2) return false;
        }
        while((General.clients[newclient] == null) ||
              General.clients[newclient].IsSpectator ||
              General.clients[newclient].IsLoading ||
              General.clients[newclient].IsLocal);

        // New spectator client
        spectateplayer = newclient;
        General.hud.ShowModeMessage();
        return true;
    }

    // This changes spectating to previous player
    // Returns false when no previous player could be found
    public bool SpectatePrevPlayer()
    {
        int clientstried = 0;
        int newclient = spectateplayer;

        // Spectating?
        if((General.localclient == null) || !General.localclient.IsSpectator) return false;
        if(spectateplayer == -1) return false;

        do
        {
            // Next
            newclient--;
            if(newclient < 0) newclient = General.clients.Length - 1;

            // Cancel when too many clients tried
            clientstried++;
            if(clientstried > General.clients.Length + 2) return false;
        }
        while((General.clients[newclient] == null) ||
              General.clients[newclient].IsSpectator ||
              General.clients[newclient].IsLoading ||
              General.clients[newclient].IsLocal);

        // New spectator client
        spectateplayer = newclient;
        General.hud.ShowModeMessage();
        return true;
    }

    // This controls the camera by input or actor
    private void PositionCamera()
    {
        float cx = 0f, cy = 0f;
        Vector3 vactor;

        // Check if spectating?
        if(General.localclient.IsSpectator)
        {
            // Spectating someone?
            if(spectateplayer > -1)
            {
                // Client available and playing?
                if((General.clients[spectateplayer] != null) &&
                   !General.clients[spectateplayer].IsSpectator &&
                   !General.clients[spectateplayer].IsLoading &&
                   !General.clients[spectateplayer].IsLocal)
                {
                    // Actor available?
                    if(General.clients[spectateplayer].Actor != null)
                    {
                        // Get actor camera coordinates
                        vactor = GetActorCameraPosition(General.clients[spectateplayer].Actor);

                        // Check if we can position the camera
                        if(!float.IsNaN(vactor.X) && !float.IsNaN(vactor.Y))
                        {
                            // Move camera to actor
                            c_target.X = vactor.X;
                            c_target.Y = vactor.Y;

                            // Slide camera to target
                            SlideCameraToTarget();
                        }
                    }
                }
                else
                {
                    // Switch to next player or to free mode
                    if(!SpectateNextPlayer())
                    {
                        // No players, lets go free spectator
                        spectateplayer = -1;
                        General.hud.ShowModeMessage();
                    }
                }
            }
            else
            {
                // Scroll down?
                if(General.gamewindow.ControlPressed("walkdown"))
                {
                    cx += (float)Math.Sin(Math.PI * 0.75);
                    cy += (float)Math.Cos(Math.PI * 0.75);
                }

                // Scroll up?
                if(General.gamewindow.ControlPressed("walkup"))
                {
                    cx += (float)Math.Sin(Math.PI * 1.75);
                    cy += (float)Math.Cos(Math.PI * 1.75);
                }

                // Scroll left?
                if(General.gamewindow.ControlPressed("walkleft"))
                {
                    cx += (float)Math.Sin(Math.PI * 1.25);
                    cy += (float)Math.Cos(Math.PI * 1.25);
                }

                // Scroll right?
                if(General.gamewindow.ControlPressed("walkright"))
                {
                    cx += (float)Math.Sin(Math.PI * 0.25);
                    cy += (float)Math.Cos(Math.PI * 0.25);
                }

                // Scroll anywhwere?
                if((cx != 0f) || (cy != 0f))
                {
                    // Make camera movement vector
                    float clen = 1f / (float)Math.Sqrt(cx * cx + cy * cy);
                    cx = (cx * clen) * CAMERA_SCROLL_SPEED;
                    cy = (cy * clen) * CAMERA_SCROLL_SPEED;

                    // Move camera
                    c_pos.X += cx;
                    c_pos.Y += cy;
                }
            }
        }
        else
        {
            // Move camera with actor
            if(General.localclient.Actor != null)
            {
                // Get actor camera coordinates
                vactor = GetActorCameraPosition(General.localclient.Actor);

                // Check if we can position the camera
                if(!float.IsNaN(vactor.X) && !float.IsNaN(vactor.Y))
                {
                    // Determine angle to move camera in
                    float dx = (float)General.gamewindow.Mouse.X - (float)Direct3D.DisplayWidth * 0.5f;
                    float dy = (float)General.gamewindow.Mouse.Y - (float)Direct3D.DisplayHeight * 0.5f;
                    float resolutionlen = (float)Math.Sqrt(Direct3D.DisplayWidth * Direct3D.DisplayWidth + Direct3D.DisplayHeight * Direct3D.DisplayHeight);
                    float length = ((float)Math.Sqrt(dx * dx + dy * dy) / resolutionlen) * CAMERA_AIM_LENGTH;
                    float angle = (float)Math.Atan2(dy, dx) + (float)Math.PI * 0.25f;

                    // Set target coordinates
                    c_target.X = vactor.X + (float)Math.Sin(angle) * length;
                    c_target.Y = vactor.Y + (float)Math.Cos(angle) * length;

                    // Slide camera to target
                    SlideCameraToTarget();
                }
            }
        }

        // Setup view matrix for camera
        Vector3 pos = new Vector3(c_pos.X, c_pos.Y, 60f);
        Vector3 tgt = Vector3.Add(pos, c_offset);
        c_matrix = Matrix.LookAtRH(pos, tgt, c_rotate);
    }

    // This calculates the visible screen rectangle in map coordinates
    private void DetermineScreenArea()
    {
        // Screen area
        float l = c_pos.X - SCREEN_AREA_WIDTH * 0.5f + SCREEN_AREA_X;
        float t = c_pos.Y - SCREEN_AREA_HEIGHT * 0.5f + SCREEN_AREA_Y;
        float w = SCREEN_AREA_WIDTH;
        float h = SCREEN_AREA_HEIGHT;
        screenarea = new RectangleF(l, t, w, h);

        // Lightmap offset to match screen area
        lightmapx = screenarea.X;
        lightmapy = screenarea.Y;

        // Create lightmap transformation matrix
        lightmaptransform = Matrix.Identity;
        lightmaptransform *= Direct3D.MatrixTranslateTx(-lightmapx, -lightmapy);
        lightmaptransform *= Matrix.Scaling(1f / SCREEN_AREA_WIDTH, 1f / SCREEN_AREA_HEIGHT, 1f);
    }

    // This moves the camera pos towards the target
    public void SlideCameraToTarget()
    {
        // Delta coordinates to move along
        float dx = c_target.X - c_pos.X;
        float dy = c_target.Y - c_pos.Y;

        // Calculate new position
        c_pos.X += dx * CAMERA_SLIDE_MUL;
        c_pos.Y += dy * CAMERA_SLIDE_MUL;
    }

    // This positions the camera immediately
    public void SetCamera(Vector2D pos)
    {
        // Position immediately
        c_pos = new Vector2(pos.x, pos.y);
        c_target = c_pos;
    }

    // This projects coordinates from world space to screen space
    public Vector3 Projected(Vector3 pos)
    {
        // Project coordinates
        return pos.Project(Direct3D.d3dd.Viewport, c_norm_projection, c_matrix, Matrix.Identity);
    }

    // This projects coordinates from screen space to world space
    public Vector3 Unprojected(Vector3 pos)
    {
        // Project coordinates
        return pos.Unproject(Direct3D.d3dd.Viewport,
            c_norm_projection, c_matrix, Matrix.Identity);
    }

    // This calculates the screen location of the actor
    private Vector3 GetActorCameraPosition(Actor a)
    {
        Vector3 vactor, acscreen;

        // To correctly take the Z coordinates of
        // the actor into account, we first cast the
        // coordinates to screen space, then back to
        // the camera space without Z.
        vactor = new Vector3(a.Position.x, a.Position.y, a.Position.z);
        acscreen = vactor.Project(Direct3D.d3dd.Viewport,
            c_norm_projection, c_matrix, Matrix.Identity);
        acscreen.Z = 0.1f;
        vactor = acscreen.Unproject(Direct3D.d3dd.Viewport,
            c_norm_projection, c_matrix, Matrix.Identity);

        // Return coordinates
        return vactor;
    }

    #endregion

    #region ================== Processing

    // This processes the entire arena
    public void Process()
    {
        int i;

        // Process all sectors
        foreach(Sector s in General.map.Sectors) s.Process();

        // Process all clients
        foreach(Client c in General.clients) if(c != null) c.Process();

        // Process visual objects
        for(i = objects.Count - 1; i >= 0; i--)
        {
            VisualObject o = objects[i];
            o.Process();
        }

        // Process particles
        p_dust.Process();
        p_magic.Process();
        p_smoke.Process();
        p_blood.Process();
        p_trail.Process();

        // Process liquids
        liquidwater.Process();
        liquidlava.Process();

        // Make the camera position
        PositionCamera();

        // Set the coordinates for sound listener
        Vector2 listenpos = Vector2.Add(c_pos, new Vector2(c_offset.X, c_offset.Y));
        SoundSystem.SetListenCoordinates(new Vector2D(listenpos.X, listenpos.Y));

        // Determine visible map portion
        DetermineScreenArea();

        // Process all dynamic lights
        for(i = dynamiclights.Count - 1; i >= 0; i--)
        {
            DynamicLight d  = dynamiclights[i];
            d.Process();
        }

        // Process all sectors
        foreach(VisualSector vs in visualSectors) vs.Process();

        // Process all items
        foreach(Item item in items.Values) item.Process();

        // Determine where the mouse points
        DetermineMouseMapLocation();

        // Process all decals
        for(i = decals.Count - 1; i >= 0; i--)
            decals[i].Process();

        // Sort all objects back-to-front
        objects.Sort();
    }

    // This determines where the mouse is in map coordinates
    private void DetermineMouseMapLocation()
    {
        Vector3 r1, r2;
        Vector3D p1, p2;
        Vector3D mouseonmap = new Vector3D();
        Vector3D mouseonmaphigh;
        Vector3D tempvec;
        object obj = null;
        float u = 2f;
        float uline = 0f;
        bool hit;

        // Mouse coordinates
        float mx = General.gamewindow.Mouse.X;
        float my = General.gamewindow.Mouse.Y;

        // Unproject mouse coordinates for ray start
        r1 = new Vector3(mx, my, 0f).Unproject(Direct3D.d3dd.Viewport,
            c_norm_projection, c_matrix, Matrix.Identity);

        // Unproject mouse coordinates for ray end
        r2 = new Vector3(mx, my, 1f).Unproject(Direct3D.d3dd.Viewport,
            c_norm_projection, c_matrix, Matrix.Identity);

        // Do a ray-map intersection test
        p1 = new Vector3D(r1.X, r1.Y, r1.Z);
        p2 = new Vector3D(r2.X, r2.Y, r2.Z);
        General.map.FindRayMapCollision(p1, p2, ref mouseonmap, ref obj, ref u, ref uline);

        // Hit found?
        if(obj != null)
        {
            // Copy hit coordinates
            mousemap = new Vector3(mouseonmap.x, mouseonmap.y, mouseonmap.z);

            // Copy sector
            if(obj is Sidedef sidedef)
                mousemapsector = sidedef.Sector;
            else if(obj is Sector sector)
                mousemapsector = sector;

            // Actor in game?
            if(General.localclient.Actor != null)
            {
                // Try to cast ray from actor to this point
                p1 = General.localclient.Actor.Position + new Vector3D(0f, 0f, 7f);
                mouseonmaphigh = mouseonmap + new Vector3D(0f, 0f, 7f);
                tempvec = mouseonmaphigh - p1;
                tempvec.MakeLength(100f);
                p2 = mouseonmaphigh + tempvec;
                u = 200f;
                hit = General.map.FindRayMapCollision(p1, p2, ref hitonmap, ref obj, ref u, ref uline);
                if(!hit) hitonmap = p2;

                /*
                {
                    // Cannot shoot here
                    MouseCursor.CursorColor = General.ARGB(1f, 1f, 0.1f, 0.1f);
                }
                else
                {
                    // Can shoot here
                    MouseCursor.CursorColor = -1;
                }
                */
            }
            else
            {
                // No actor
                //MouseCursor.CursorColor = -1;
            }
        }
        else
        {
            // No hit
            mousemapsector = null;
            //MouseCursor.CursorColor = -1;
        }
    }

    #endregion

    #region ================== Rendering

    // This will render a cross at given coordinates
    public void RenderPoint(Vector3D pos, Color c)
    {
        const float xlen = 0.4f;
        const float zlen = 2f;
        LVertex[] verts = new LVertex[6];

        verts[0].color = c.ToArgb();
        verts[0].x = pos.x - xlen;
        verts[0].y = pos.y;
        verts[0].z = pos.z;
        verts[1].color = c.ToArgb();
        verts[1].x = pos.x + xlen;
        verts[1].y = pos.y;
        verts[1].z = pos.z;
        verts[2].color = c.ToArgb();
        verts[2].x = pos.x;
        verts[2].y = pos.y - xlen;
        verts[2].z = pos.z;
        verts[3].color = c.ToArgb();
        verts[3].x = pos.x;
        verts[3].y = pos.y + xlen;
        verts[3].z = pos.z;
        verts[4].color = c.ToArgb();
        verts[4].x = pos.x;
        verts[4].y = pos.y;
        verts[4].z = pos.z - zlen;
        verts[5].color = c.ToArgb();
        verts[5].x = pos.x;
        verts[5].y = pos.y;
        verts[5].z = pos.z + zlen;

        // No matrices
        Direct3D.d3dd.SetTransform(TransformState.World, Matrix.Identity);

        // Draw line
        Direct3D.SetDrawMode(DRAWMODE.NLINES);
        Direct3D.d3dd.DrawUserPrimitives(PrimitiveType.LineList, 3, verts);
    }

    // This will render a line at given coordinates
    public void RenderLine(Vector3D start, Vector3D end, Color c)
    {
        LVertex[] verts = new LVertex[2];

        verts[0].color = c.ToArgb();
        verts[0].x = start.x;
        verts[0].y = start.y;
        verts[0].z = start.z;
        verts[1].color = c.ToArgb();
        verts[1].x = end.x;
        verts[1].y = end.y;
        verts[1].z = end.z;

        // No matrices
        Direct3D.d3dd.SetTransform(TransformState.World, Matrix.Identity);

        // Draw line
        Direct3D.SetDrawMode(DRAWMODE.NLINES);
        Direct3D.d3dd.DrawUserPrimitives(PrimitiveType.LineList, 1, verts);
    }

    // This will prepare for rendering
    public void PrepareRendering()
    {
        Surface lightmapsurface;

        // Render liquids
        liquidwater.Render();
        liquidlava.Render();

        // Set normal viewport
        //Direct3D.d3dd.Viewport = Direct3D.DisplayViewport;

        // Initializing lights?
        if(firstframe)
        {
            // Go for all nearby lights
            foreach(StaticLight l in staticlights)
            {
                // Prepare your lightmap
                l.PrepareLightmap();
            }
        }

        // Go for all sectors
        foreach(VisualSector s in visualSectors)
        {
            // Prepare your lightmap
            if(firstframe || s.InScreen) s.PrepareLightmap();
        }

        // Only when using dynamic lights
        if(DynamicLight.dynamiclights)
        {
            // Begin dynamic lightmap rendering
            lightmapsurface = dynamiclightmap.GetSurfaceLevel(0);
            Direct3D.d3dd.DepthStencilSurface = null;
            Direct3D.d3dd.SetRenderTarget(0, lightmapsurface);
            Direct3D.d3dd.Clear(ClearFlags.Target, new RawColorBGRA(), 0f, 0);
            Direct3D.d3dd.BeginScene();

            // Setup matrices
            Direct3D.d3dd.SetTransform(TransformState.Projection, c_light_projection);
            Direct3D.d3dd.SetTransform(TransformState.World, Matrix.Identity);
            Direct3D.d3dd.SetTransform(TransformState.View, Matrix.Translation(-c_pos.X + DYNAMIC_LIGHTMAP_ADJUST_X,
                -c_pos.Y + DYNAMIC_LIGHTMAP_ADJUST_Y, 0f));

            // Set drawing mode
            Direct3D.SetDrawMode(DRAWMODE.NLIGHTBLEND);

            // Set the light texture
            Direct3D.d3dd.SetTexture(0, DynamicLight.lightimages[2].texture);

            // Go for all dynamic lights
            foreach(DynamicLight d in dynamiclights)
            {
                // Render the light
                d.Render();
            }

            // Done rendering lightmap
            Direct3D.d3dd.EndScene();

            // Clean up
            lightmapsurface.Dispose();
        }
    }

    // This will render the entire arena
    public void Render()
    {
        // Setup matrices
        Direct3D.d3dd.SetTransform(TransformState.Projection, c_norm_projection);
        Direct3D.d3dd.SetTransform(TransformState.World, Matrix.Identity);
        Direct3D.d3dd.SetTransform(TransformState.View, c_matrix);
        Direct3D.d3dd.SetTransform(TransformState.Texture0, Matrix.Identity);
        Direct3D.d3dd.SetTransform(TransformState.Texture1, Matrix.Identity);
        if(DynamicLight.dynamiclights) Direct3D.d3dd.SetTransform(TransformState.Texture2, lightmaptransform);

        // Setup dynamic lightmap
        if(DynamicLight.dynamiclights) Direct3D.d3dd.SetTexture(2, dynamiclightmap);

        // Go for all sectors
        foreach(VisualSector s in visualSectors)
        {
            // Render this sector
            s.RenderGeometry();
        }

        if(Decal.showdecals)
        {
            // Render mode for decals
            Direct3D.SetDrawMode(DRAWMODE.NLIGHTMAPALPHA);
            Direct3D.d3dd.SetRenderState(RenderState.ZWriteEnable, false);
            Direct3D.d3dd.SetTransform(TransformState.Texture0, Matrix.Identity);

            // Go for all decals
            foreach(Decal d in decals)
            {
                // Render the decal
                d.Render();
            }
        }

        // Go for 2 render passes
        for(int p = 0; p < 2; p++) RenderObjectsPass(p);

        // Set drawing mode
        Direct3D.SetDrawMode(DRAWMODE.TLMODALPHA);
        Direct3D.d3dd.SetRenderState(RenderState.TextureFactor, -1);

        // Go for all actors
        foreach(Actor a in actors)
        {
            // Render the actor name
            if(a != null) a.RenderName();
        }

        // LASERS FOR REMOTE PLAYERS
        /*
        // Go for all actors
        foreach(Actor a in actors)
        {
            if(a != null)
            {
                // Not the local player? and alive?
                if((a != General.localclient.Actor) && !a.IsDead)
                {
                    // Do a trace from actor
                    object obj = null;
                    float u = 2f, uline = 2f;
                    Vector3D endpos = Laser.GetSourcePosition(a) + Vector3D.FromActorAngle(a.AimAngle, a.AimAngleZ, 200f);
                    Vector3D endpoint = endpos;
                    General.map.FindRayMapCollision(a.Position, endpos, ref endpoint, ref obj, ref u, ref uline);

                    // Render laser
                    Laser.Render(Laser.GetSourcePosition(a), endpoint);
                }
            }
        }
        */

        // Client available?
        if(General.localclient != null)
        {
            // Client in game?
            if(General.localclient.Actor != null)
            {
                // Render laser
                Laser.Render(Laser.GetSourcePosition(General.localclient.Actor), hitonmap);
            }
        }

        // Render all particles
        p_dust.Render();
        p_blood.Render();
        p_trail.Render();
        p_smoke.Render();
        p_magic.Render();

        // Do the 3rd render pass
        RenderObjectsPass(2);

        // DEBUG:
        //RenderPoint(new Vector3D(mousemap.X, mousemap.Y, mousemap.Z), Color.Aqua);

        /*
        // DEBUG:
        TLVertex[] verts = Direct3D.TLRect(0f, 0f, 256f, 256f);
        Direct3D.d3dd.SetTexture(0, dynamiclightmap);
        Direct3D.d3dd.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, verts);
        */

        // Unset textures and streams
        Direct3D.d3dd.SetTexture(0, null);
        Direct3D.d3dd.SetTexture(1, null);
        Direct3D.d3dd.SetTexture(2, null);
        Direct3D.d3dd.SetStreamSource(0, null, 0, 0);

        // No longer first frame
        firstframe = false;
    }

    // This renders one objects pass
    private void RenderObjectsPass(int pass)
    {
        // Render mode for shadows
        Direct3D.SetDrawMode(DRAWMODE.NALPHA);
        Direct3D.d3dd.SetRenderState(RenderState.ZWriteEnable, false);

        // Shadow texture and vertices
        Direct3D.d3dd.SetTexture(0, Shadow.texture.texture);
        Direct3D.d3dd.SetTexture(1, null);
        Direct3D.d3dd.SetStreamSource(0, Shadow.vertices, 0, MVertex.Stride);

        // Go for all visual objects
        foreach(VisualObject vo in objects)
        {
            // Render the object shadow
            if(vo.RenderPass == pass) vo.RenderShadow();
        }

        // Render mode for objects
        Direct3D.d3dd.SetTransform(TransformState.Texture0, Matrix.Identity);

        // Go for all visual objects
        foreach(VisualObject vo in objects)
        {
            // Render the object
            if(vo.RenderPass == pass) vo.Render();
        }
    }

    #endregion
}
