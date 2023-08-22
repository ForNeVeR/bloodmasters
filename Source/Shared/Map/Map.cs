/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace CodeImp.Bloodmasters;

public abstract class Map
{
    #region ================== Constants

    public const float MAP_SCALE_XY = 0.1f;
    public const float MAP_SCALE_Z = 0.2f;
    public const float INV_MAP_SCALE_XY = 1f / MAP_SCALE_XY;
    public const float INV_MAP_SCALE_Z = 1f / MAP_SCALE_Z;
    public const int VERTEX_PRECISION = 3;

    #endregion

    #region ================== Variables

    // Map information
    private string mapname;
    private string title;
    private string author;
    private int rec_players;
    private int ceilinglight;
    private Configuration config;

    // Supported game types
    private bool supportsdm;
    private bool supportstdm;
    private bool supportsctf;
    private bool supportssc;
    private bool supportstsc;
    private bool supportsst;
    private bool supportstst;

    // Boundaries
    private float boundaryleft;
    private float boundarytop;
    private float boundaryright;
    private float boundarybottom;

    // Map elements
    private Thing[] things;
    private Vector2D[] vertices;
    private Linedef[] linedefs;
    private Sidedef[] sidedefs;
    private Sector[] sectors;
    private Segment[] segments;
    private SubSector[] subsectors;
    private Node[] nodes;
    private RejectMap rejectmap;
    private BlockMap blockmap;

    #endregion

    #region ================== Properties

    // Map information
    public string Name { get { return mapname; } }
    public string Title { get { return title; } }
    public string Author { get { return author; } }
    public int RecommendedPlayers { get { return rec_players; } }
    public int CeilingLight { get { return ceilinglight; } }

    // Supported game types
    public bool SupportsDM { get { return supportsdm; } }
    public bool SupportsTDM { get { return supportstdm; } }
    public bool SupportsCTF { get { return supportsctf; } }
    public bool SupportsSC { get { return supportssc; } }
    public bool SupportsTSC { get { return supportstsc; } }
    public bool SupportsST { get { return supportsst; } }
    public bool SupportsTST { get { return supportstst; } }

    // Map boundaries
    public float BoundaryLeft { get { return boundaryleft; } }
    public float BoundaryRight { get { return boundaryright; } }
    public float BoundaryTop { get { return boundarytop; } }
    public float BoundaryBottom { get { return boundarybottom; } }

    // Map elements
    public Thing[] Things { get { return things; } }
    public Vector2D[] Vertices { get { return vertices; } }
    public Linedef[] Linedefs { get { return linedefs; } }
    public Sidedef[] Sidedefs { get { return sidedefs; } }
    public Sector[] Sectors { get { return sectors; } }
    public Segment[] Segments { get { return segments; } }
    public SubSector[] SubSectors { get { return subsectors; } }
    public Node[] Nodes { get { return nodes; } }
    public RejectMap RejectMap { get { return rejectmap; } }
    public BlockMap BlockMap { get { return blockmap; } }

    #endregion

    #region ================== Constructor / Destructor

    // Constructor: Loads the map data from an archive
    public Map(string mapname, bool infoonly, string temppath)
    {
        int numorigverts;
        string tempwadfile = Path.Combine(temppath, "___bmmap___.wad");

        // Keep the map name
        this.mapname = mapname;

        // Find the wad file
        string wadfile = ArchiveManager.FindFileArchive(mapname + ".wad");
        if(wadfile == "") throw new FileNotFoundException("No such map \"" + mapname + "\"", mapname + ".wad");
        wadfile  += "/" + mapname + ".wad";

        // Extract wad file
        wadfile = ArchiveManager.ExtractFile(wadfile, true);

        // Make a copy of the map so we dont modify the original
        if(File.Exists(tempwadfile)) File.Delete(tempwadfile);
        File.Copy(wadfile, tempwadfile);

        // Load the map configuration
        LoadConfiguration(tempwadfile);

        // Read the whole map as well?
        if(infoonly == false)
        {
            // Load the map data from GL structures
            numorigverts = LoadVertices(tempwadfile);
            LoadSectors(tempwadfile);
            LoadSidedefs(tempwadfile);
            LoadLinedefs(tempwadfile);
            LoadSegments(tempwadfile, numorigverts);
            LoadSubSectors(tempwadfile);
            LoadNodes(tempwadfile);
            LoadThings(tempwadfile);

            // Load the rejectmap and blockmap
            LoadReject(tempwadfile);
            LoadBlockmap(tempwadfile);

            // Find the map boundaries
            FindMapBoundaries();

            // Find adjacent sectors
            FindAdjacentSectors();

            // Make fake ceilings
            BuildFakeCeilingHeights();

            // Find adjacent floor levels
            FindLowestAdjHeights();
            FindHighestAdjHeights();
        }
    }

    // Destructor
    public void Dispose()
    {
        // Kill em all! Kill em all!
        if(things != null) foreach(Thing t in things) t.Dispose();
        if(linedefs != null) foreach(Linedef l in linedefs) l.Dispose();
        if(sidedefs != null) foreach(Sidedef sd in sidedefs) sd.Dispose();
        if(sectors != null) foreach(Sector s in sectors) s.Dispose();
        if(subsectors != null) foreach(SubSector ss in subsectors) ss.Dispose();
        if(segments != null) foreach(Segment sg in segments) sg.Dispose();
        if(nodes != null) foreach(Node n in nodes) n.Dispose();
        if(rejectmap != null) rejectmap.Dispose();
        if(blockmap != null) blockmap.Dispose();

        things = null;
        linedefs = null;
        sidedefs = null;
        sectors = null;
        subsectors = null;
        segments = null;
        nodes = null;
        rejectmap = null;
        blockmap = null;
        config = null;
    }

    #endregion

    #region ================== Loading

    // This determines the map boundaries
    private void FindMapBoundaries()
    {
        // Reset boundaries
        boundaryleft = float.MaxValue;
        boundaryright = float.MinValue;
        boundarytop = float.MaxValue;
        boundarybottom = float.MinValue;

        // Go for all vertices
        foreach(Vector2D v in vertices)
        {
            // Adjust boundaries if needed
            if(v.x < boundaryleft) boundaryleft = v.x;
            if(v.x > boundaryright) boundaryright = v.x;
            if(v.y < boundarytop) boundarytop = v.y;
            if(v.y > boundarybottom) boundarybottom = v.y;
        }
    }

    // This finds the adjacents sectors
    private void FindAdjacentSectors()
    {
        // Go for all sectors
        foreach(Sector s in sectors) s.FindAdjacentSectors();
    }

    // This finds the lowest adjacent floor heights
    private void FindLowestAdjHeights()
    {
        bool result;

        do
        {
            // Presume no changes made
            result = false;

            // Go for all sectors
            foreach(Sector s in sectors)
            {
                // Check if this is a dynamic sector
                if(s.Dynamic)
                {
                    // Find lowest adjacent floor and keep result
                    result |= s.FindLowestAdjFloor();
                }
            }
        }
        // Continue as long as changes are still being made
        while(result);
    }

    // This finds the highest adjacent floor heights
    private void FindHighestAdjHeights()
    {
        bool result;

        do
        {
            // Presume no changes made
            result = false;

            // Go for all sectors
            foreach(Sector s in sectors)
            {
                // Check if this is a dynamic sector
                if(s.Dynamic)
                {
                    // Find highest adjacent floor and keep result
                    result |= s.FindHighestAdjFloor();
                }
            }
        }
        // Continue as long as changes are still being made
        while(result);
    }

    // This builds fake ceiling heights
    private void BuildFakeCeilingHeights()
    {
        bool result;

        do
        {
            // Presume no changes made
            result = false;

            // Go for all sectors
            foreach(Sector s in sectors)
            {
                // Make fake ceiling and keep result
                result |= s.CreateFakeCeiling();
            }
        }
        // Continue as long as changes are still being made
        while(result);
    }

    // This loads the map configuration from the wad file
    private void LoadConfiguration(string wadfile)
    {
        // Read CONFIG lump
        BinaryReader data = Wad.ReadLump(wadfile, "CONFIG");
        string mapinfostr = Encoding.ASCII.GetString(data.ReadBytes((int)data.BaseStream.Length));
        config = new Configuration();
        config.InputConfiguration(mapinfostr);

        // Read information from lump
        title = config.ReadSetting("title", "Untitled");
        author = config.ReadSetting("author", "Unknown");
        rec_players = config.ReadSetting("players", 0);
        float clight = config.ReadSetting("ceilinglight", 0f);
        supportsdm = config.ReadSetting("dm", false);
        supportstdm = config.ReadSetting("tdm", false);
        supportsctf = config.ReadSetting("ctf", false);
        supportssc = config.ReadSetting("sc", false);
        supportstsc = config.ReadSetting("tsc", false);
        supportsst = config.ReadSetting("st", false);
        supportstst = config.ReadSetting("tst", false);

        // Ceiling light color
        ceilinglight = System.Drawing.Color.FromArgb(255, (int)(255f * clight), (int)(255f * clight), (int)(255f * clight)).ToArgb();

        // Clean up
        data.Close();
    }

    // This loads the reject map from the wad file
    private void LoadReject(string wadfile)
    {
        // Get the REJECT lump data
        BinaryReader data = Wad.ReadLump(wadfile, "REJECT");

        // Check if missing structure
        if((data == null) || (data.BaseStream.Length == 0))
            throw(new Exception("WAD file is missing REJECT structure."));

        // Make reject table
        rejectmap = new RejectMap(data, sectors.Length);

        // Clean up
        data.Close();
    }

    // This loads the block map from the wad file
    private void LoadBlockmap(string wadfile)
    {
        // Get the BLOCKMAP lump data
        BinaryReader data = Wad.ReadLump(wadfile, "BLOCKMAP");

        // Check if missing structure
        if((data == null) || (data.BaseStream.Length == 0))
            throw(new Exception("WAD file is missing BLOCKMAP structure."));

        // Make blockmap
        blockmap = new BlockMap(data, linedefs, this);

        // Clean up
        data.Close();
    }

    // This loads all nodes from the wad file
    private void LoadNodes(string wadfile)
    {
        // Get the GL_NODES lump data
        BinaryReader data = Wad.ReadLump(wadfile, "GL_NODES");

        // Check if missing structure
        if((data == null) || (data.BaseStream.Length == 0))
            throw(new Exception("WAD file is missing GL_NODES structure."));

        // Calculate the number of nodes
        int numnodes = (int)(data.BaseStream.Length / 32);

        // Make nodes array
        nodes = new Node[numnodes];

        // Read all nodes
        for(int i = 0; i < numnodes; i++) nodes[i] = new Node(data, subsectors, this);

        // Clean up
        data.Close();
    }

    // This loads all subsectors from the wad file
    private void LoadSubSectors(string wadfile)
    {
        // Get the GL_SSECT lump data
        BinaryReader data = Wad.ReadLump(wadfile, "GL_SSECT");

        // Check if missing structure
        if((data == null) || (data.BaseStream.Length == 0))
            throw(new Exception("WAD file is missing GL_SSECT structure."));

        // Calculate the number of subsectors
        int numssecs = (int)(data.BaseStream.Length / 8);

        // Make subsectors array
        subsectors = new SubSector[numssecs];

        // Read all subsectors
        for(int i = 0; i < numssecs; i++) subsectors[i] = new SubSector(data, segments, vertices);

        // Clean up
        data.Close();
    }

    // This loads all segments from the wad file
    private void LoadSegments(string wadfile, int numorigverts)
    {
        // Get the GL_SEGS lump data
        BinaryReader data = Wad.ReadLump(wadfile, "GL_SEGS");

        // Check if missing structure
        if((data == null) || (data.BaseStream.Length == 0))
            throw(new Exception("WAD file is missing GL_SEGS structure."));

        // Calculate the number of segments
        int numsegs = (int)(data.BaseStream.Length / 16);

        // Make segments array
        segments = new Segment[numsegs];

        // Read all segments
        for(int i = 0; i < numsegs; i++) segments[i] = new Segment(data, linedefs, numorigverts);

        // Clean up
        data.Close();
    }

    // This loads all sectors from the wad file
    private void LoadSectors(string wadfile)
    {
        // Get the SECTORS lump data
        BinaryReader data = Wad.ReadLump(wadfile, "SECTORS");

        // Check if missing structure
        if((data == null) || (data.BaseStream.Length == 0))
            throw(new Exception("WAD file is missing SECTORS structure."));

        // Calculate the number of sectors
        int numsectors = (int)(data.BaseStream.Length / 26);

        // Make sectors array
        sectors = new Sector[numsectors];

        // Read all sectors
        for(int i = 0; i < numsectors; i++) sectors[i] = CreateSector(data, i);

        // Clean up
        data.Close();
    }

    protected abstract Sector CreateSector(BinaryReader data, int i);
    protected abstract Sidedef CreateSidedef(BinaryReader data, Sector[] sectors, int index);

    // This loads all sidedefs from the wad file
    private void LoadSidedefs(string wadfile)
    {
        // Get the SIDEDEFS lump data
        BinaryReader data = Wad.ReadLump(wadfile, "SIDEDEFS");

        // Check if missing structure
        if((data == null) || (data.BaseStream.Length == 0))
            throw(new Exception("WAD file is missing SIDEDEFS structure."));

        // Calculate the number of sides
        int numsides = (int)(data.BaseStream.Length / 30);

        // Make sidedefs array
        sidedefs = new Sidedef[numsides];

        // Read all sidedefs
        for(int i = 0; i < numsides; i++) sidedefs[i] = CreateSidedef(data, sectors, i);

        // Clean up
        data.Close();
    }

    // This loads all linedefs from the wad file
    private void LoadLinedefs(string wadfile)
    {
        // Get the LINEDEFS lump data
        BinaryReader data = Wad.ReadLump(wadfile, "LINEDEFS");

        // Check if missing structure
        if((data == null) || (data.BaseStream.Length == 0))
            throw(new Exception("WAD file is missing LINEDEFS structure."));

        // Calculate the number of lines
        int numlines = (int)(data.BaseStream.Length / 16);

        // Make linedefs array
        linedefs = new Linedef[numlines];

        // Read all linedefs
        for(int i = 0; i < numlines; i++) linedefs[i] = new Linedef(data, vertices, sidedefs, this, i);

        // Clean up
        data.Close();
    }

    // This loads all things from the wad file
    private void LoadThings(string wadfile)
    {
        // Get the THINGS lump data
        BinaryReader data = Wad.ReadLump(wadfile, "THINGS");

        // Check if missing structure
        if((data == null) || (data.BaseStream.Length == 0))
            throw(new Exception("WAD file is missing THINGS structure."));

        // Calculate the number of things
        int numthings = (int)(data.BaseStream.Length / 20);

        // Make things array
        things = new Thing[numthings];

        // Read all things
        for(int i = 0; i < numthings; i++) things[i] = new Thing(data, i, this);

        // Clean up
        data.Close();
    }

    // This loads all vertices from the wad file
    // Returns the number of vertices in the VERTEXES lump
    private int LoadVertices(string wadfile)
    {
        BinaryReader verts;
        BinaryReader glverts;
        int i;

        // Get the VERTEXES lump data
        verts = Wad.ReadLump(wadfile, "VERTEXES");
        glverts = Wad.ReadLump(wadfile, "GL_VERT");

        // Check if missing structure
        if((verts == null) || (verts.BaseStream.Length == 0))
            throw(new Exception("WAD file is missing VERTEXES structure."));

        // Check if missing structure
        if((glverts == null) || (glverts.BaseStream.Length == 0))
            throw(new Exception("WAD file is missing GL_VERT structure."));

        // Calculate the number of vertices
        int numverts = (int)(verts.BaseStream.Length / 4);
        int numglverts = (int)((glverts.BaseStream.Length - 4) / 8);

        // Make vertices array
        vertices = new Vector2D[numverts + numglverts];

        // Go for all original vertices
        for(i = 0; i < numverts; i++)
        {
            // Read vertex
            vertices[i].x = (float)Math.Round((float)verts.ReadInt16() * MAP_SCALE_XY, VERTEX_PRECISION);
            vertices[i].y = (float)Math.Round((float)verts.ReadInt16() * MAP_SCALE_XY, VERTEX_PRECISION);
        }

        // Not interested in the 4 four bytes of gl vertices
        glverts.BaseStream.Seek(4, SeekOrigin.Begin);

        // Go for all gl-vertices
        for(i = numverts; i < (numverts + numglverts); i++)
        {
            // Read vertex X
            float x1 = glverts.ReadInt16();
            float x2 = glverts.ReadInt16();
            vertices[i].x = (float)Math.Round((x2 + x1 / 65536f) * MAP_SCALE_XY, VERTEX_PRECISION);

            // Read vertex Y
            float y1 = glverts.ReadInt16();
            float y2 = glverts.ReadInt16();
            vertices[i].y = (float)Math.Round((y2 + y1 / 65536f) * MAP_SCALE_XY, VERTEX_PRECISION);
        }

        // Clean up
        verts.Close();
        glverts.Close();

        // Return number of original vertices
        // We need this later when reading the segs
        return numverts;
    }

    #endregion

    #region ================== Methods

    // This returns the sound filename for a given index
    public string GetSoundFilename(int index)
    {
        // Read and return setting
        return config.ReadSetting("sound" + index.ToString(CultureInfo.InvariantCulture), "");
    }

    // This finds all "touching sectors"
    // these are the sectors an object is overlapping
    public List<Sector> FindTouchingSectors(float x, float y, float radius)
    {
        List<Sector> sectors = new  List<Sector>();

        // Get all the nearby lines to check for intersection
        List<Linedef> lines = blockmap.GetCollisionLines(x, y, radius);

        // Go for all lines
        foreach(Linedef ld in lines)
        {
            // Get the distance to line
            float dist = ld.DistanceToLine(x, y);

            // Check for intersection
            if(dist < radius)
            {
                // Touching both sectors
                if(ld.Front != null) sectors.Add(ld.Front.Sector);
                if(ld.Back != null) sectors.Add(ld.Back.Sector);
            }
        }

        // Not touching any lines?
        // Then do a simple subsector intersection test
        if(sectors.Count == 0) sectors.Add(GetSubSectorAt(x, y).Sector);

        // Return result
        return sectors;
    }

    // This returns true when given coordinate is within map boundaries
    public bool WithinBoundaries(float x, float y)
    {
        return (x >= boundaryleft) && (x <= boundaryright) &&
               (y >= boundarytop) && (y <= boundarybottom);
    }

    // This returns the subsector in which the given coordinates are
    public SubSector GetSubSectorAt(float x, float y)
    {
        float s;

        // Start at the last node (begin of the tree)
        Node n = nodes[nodes.Length - 1];

        // Browse the tree
        while(true)
        {
            // Get the side of split line
            s = n.SideOfLine(x, y);

            // On the right side?
            if(s > 0)
            {
                // Return the subsector on the right
                // or advance to the next node
                if(n.RightSubSector != null)
                    return n.RightSubSector;
                else
                    n = n.RightNode;
            }
            else
            {
                // Return the subsector on the left
                // or advance to the next node
                if(n.LeftSubSector != null)
                    return n.LeftSubSector;
                else
                    n = n.LeftNode;
            }
        }
    }

    // This returns the linedef nearest to the given coordinates
    public Linedef GetNearestLine(float x, float y)
    {
        // From all lines in map
        return GetNearestLine(x, y, linedefs);
    }

    // This returns the linedef nearest to the given coordinates
    public Linedef GetNearestLine(float x, float y, IEnumerable<Linedef> linedefs)
    {
        Linedef foundline = null;
        float founddist = float.MaxValue;
        float d;

        // Go for all linedefs
        foreach(Linedef l in linedefs)
        {
            // Get shortest distance to line
            d = l.DistanceToLine(x, y);

            // Check if closer
            if(d < founddist)
            {
                // Keep this line
                foundline = l;
                founddist = d;
            }
        }

        // Return result
        return foundline;
    }

    // This tests a ray for collision
    public bool FindRayMapCollision(Vector3D start, Vector3D end)
    {
        float u = 2f, ul = 2f;
        object obj = null;
        Vector3D p = new Vector3D();
        return FindRayMapCollision(start, end, ref p, ref obj, ref u, ref ul);
    }

    // This tests a ray for collision
    public bool FindRayMapCollision(Vector3D start, Vector3D end, ref Vector3D point, ref object obj, ref float u, ref float uline)
    {
        bool intersecting;
        float uray = 0f;
        float side = 0f;
        float ul = 0f;
        Vector3D intp = new Vector3D();
        bool found = false;
        Sidedef sd;
        bool[] sectortested = new bool[sectors.Length];

        // Find all lines near the trajectory
        List<Linedef> lines = blockmap.GetCollisionLines(start.x, start.y, end.x, end.y);

        // No lines?
        if(lines.Count == 0)
        {
            // Find sector at end coordinates
            Sector sc = GetSubSectorAt(end.x, end.y).Sector;

            // Test the sector floor
            if(RaySectorCollision(start, end, sc, u, sc.CurrentFloor, ref intp, ref uray))
            {
                // Collision with the floor
                u = uray;
                obj = sc;
                point = intp;
                found = true;
            }
        }
        else
        {
            // Go for all nearby lines
            foreach(Linedef ld in lines)
            {
                // Can this line in any way block something?
                if((ld.Front == null) || (ld.Back == null) ||
                   (ld.Front.Sector.CurrentFloor != ld.Back.Sector.CurrentFloor) ||
                   ld.Front.Sector.HasCeiling || ld.Back.Sector.HasCeiling)
                {
                    // Get collision point
                    intersecting = ld.IntersectLine(start.x, start.y, end.x, end.y, out uray, out ul);

                    // Ray intersecting the line?
                    if(intersecting && (uray < u))
                    {
                        // Calculate intersection point with the wall
                        intp = end * uray + start * (1f - uray);

                        // Check on which side of the line the trajectory starts
                        // and get the corresponding sidedef
                        side = ld.SideOfLine(start.x, start.y);
                        if(side < 0f) sd = ld.Front; else sd = ld.Back;

                        // Check if intersection is actually on a wall part
                        if((sd != null) && (sd.OtherSide != null) &&
                           ((intp.z < sd.OtherSide.Sector.CurrentFloor) ||
                            ((intp.z > sd.OtherSide.Sector.HeightCeil) &&
                             (intp.z < sd.OtherSide.Sector.FakeHeightCeil))))
                        {
                            // Collision with wall
                            uline = ul;
                            u = uray;
                            obj = sd;
                            point = intp;
                            found = true;
                        }
                    }
                }

                // FRONT SIDE AVAILABLE?
                if(ld.Front != null)
                {
                    // Sector not already tested?
                    if(!sectortested[ld.Front.Sector.Index])
                    {
                        // Test the sector floor
                        if(RaySectorCollision(start, end, ld.Front.Sector, u,
                               ld.Front.Sector.CurrentFloor, ref intp, ref uray))
                        {
                            // Collision with the floor
                            u = uray;
                            obj = ld.Front.Sector;
                            point = intp;
                            found = true;
                        }

                        // Sector has a ceiling?
                        if(ld.Front.Sector.HasCeiling)
                        {
                            // Check sector ceiling
                            if(RaySectorCollision(start, end, ld.Front.Sector, u,
                                   ld.Front.Sector.HeightCeil, ref intp, ref uray))
                            {
                                // Collision with the ceiling
                                u = uray;
                                obj = ld.Front.Sector;
                                point = intp;
                                found = true;
                            }
                        }

                        // Sector tested
                        sectortested[ld.Front.Sector.Index] = true;
                    }
                }

                // BACK SIDE AVAILABLE?
                if(ld.Back != null)
                {
                    // Sector not already tested?
                    if(!sectortested[ld.Back.Sector.Index])
                    {
                        // Test the sector floor
                        if(RaySectorCollision(start, end, ld.Back.Sector, u,
                               ld.Back.Sector.CurrentFloor, ref intp, ref uray))
                        {
                            // Collision with the floor
                            u = uray;
                            obj = ld.Back.Sector;
                            point = intp;
                            found = true;
                        }

                        // Sector has a ceiling?
                        if(ld.Back.Sector.HasCeiling)
                        {
                            // Check sector ceiling
                            if(RaySectorCollision(start, end, ld.Back.Sector, u,
                                   ld.Back.Sector.HeightCeil, ref intp, ref uray))
                            {
                                // Collision with the ceiling
                                u = uray;
                                obj = ld.Back.Sector;
                                point = intp;
                                found = true;
                            }
                        }

                        // Sector tested
                        sectortested[ld.Back.Sector.Index] = true;
                    }
                }
            }
        }

        // Return result
        return found;
    }

    // This checks collision of a ray with a sector
    private bool RaySectorCollision(Vector3D start, Vector3D end, Sector sc, float u,
        float sectorheight, ref Vector3D intp, ref float uray)
    {
        // Get intersection at sector level
        uray = (sectorheight - start.z) / (end.z - start.z);

        // Worth checking any further?
        if((uray > 0f) && (uray < u) && !float.IsNaN(uray) && !float.IsInfinity(uray))
        {
            // Calculate intersection point with the sector
            intp = (end * uray) + (start * (1f - uray));

            // Coordinates within the sector?
            return sc.IntersectXY(intp.x, intp.y);
        }
        else
        {
            // No collision
            return false;
        }
    }

    #endregion
}
