//----------------------------------------------------------------------------
//
// File:        level.hpp
// Date:        26-Oct-1994
// Programmer:  Marc Rousseau
//
// Description: Object classes for manipulating Doom Maps
//
// Copyright (c) 1994-2004 Marc Rousseau, All Rights Reserved.
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307, USA.
//
// Revision History:
//
//----------------------------------------------------------------------------

#ifndef LEVEL_HPP_
#define LEVEL_HPP_

#if ! defined ( COMMON_HPP_ )
    #include "common.hpp"
#endif

#if ! defined ( WAD_HPP_ )
    #include "wad.hpp"
#endif

#include "thing.hpp"
#include "lineDef.hpp"

struct wThing1 {
    INT16       xPos;                   // x position
    INT16       yPos;                   // y position
    UINT16      angle;                  // direction
    UINT16      type;                   // thing type
    UINT16      attr;                   // attributes of thing
};

struct wThing2 {        // HEXEN
    UINT16      tid;                    // Thing ID - for scripts & specials
    INT16       xPos;                   // x position
    INT16       yPos;                   // y position
    UINT16      altitude;               // starting altitude
    UINT16      angle;                  // direction
    UINT16      type;                   // thing type
    UINT16      attr;                   // attributes of thing
    UINT8       special;                // special type
    UINT8       arg [5];                // special arguments
};      

struct wThing {
    INT16       xPos;                   // x position
    INT16       yPos;                   // y position
    UINT16      angle;                  // in degrees not BAM
    UINT16      type;                   // thing type
    UINT16      attr;                   // attributes of thing
    UINT16      tid;                    // Thing ID - for scripts & specials
    UINT16      altitude;               // starting altitude
    UINT8       special;                // special type
    UINT8       arg [5];                // special arguments
};

struct wLineDef1 {
    UINT16      start;                  // from this vertex ...
    UINT16      end;                    // ... to this vertex
    UINT16      flags;
    UINT16      type;
    UINT16      tag;                    // crossing this linedef activates the sector with the same tag
    UINT16      sideDef[2];             // sidedef
};

struct wLineDef2 {      // HEXEN
    UINT16      start;                  // from this vertex ...
    UINT16      end;                    // ... to this vertex
    UINT16      flags;
    UINT8       special;                // special type
    UINT8       arg [5];                // special arguments
    UINT16      sideDef[2];             // sidedef
};

struct wLineDef {
    UINT16      start;                  // from this vertex ...
    UINT16      end;                    // ... to this vertex
    UINT16      flags;
    UINT16      type;
    UINT16      tag;                    // crossing this linedef activates the sector with the same tag
    UINT16      sideDef[2];             // sidedef
    UINT8       special;                // special type
    UINT8       arg [5];                // special arguments
};

#define NO_SIDEDEF      (( UINT16 ) -1 )
#define RIGHT_SIDEDEF   (( UINT16 )  0 )
#define LEFT_SIDEDEF    (( UINT16 )  1 )

#define EMPTY_TEXTURE   0x002D          // UINT16 version of ASCII "-"

struct wSideDef {
    INT16       xOff;                   // X offset for texture
    INT16       yOff;                   // Y offset for texture
    char        text1[MAX_LUMP_NAME];   // texture name for the part above
    char        text2[MAX_LUMP_NAME];   // texture name for the part below
    char        text3[MAX_LUMP_NAME];   // texture name for the regular part
    UINT16      sector;                 // adjacent sector
};

struct wVertex {
    INT16       x;                      // X coordinate
    INT16       y;                      // Y coordinate
};

struct wSector {
    INT16       floorh;                 // floor height
    INT16       ceilh;                  // ceiling height
    char        floorTexture[MAX_LUMP_NAME];    // floor texture
    char        ceilTexture[MAX_LUMP_NAME];     // ceiling texture
    UINT16      light;                  // light level (0-255)
    UINT16      special;                // special behaviour (0 = normal, 9 = secret, ...)
    UINT16      trigger;                // sector activated by a linedef with the same tag
};

struct wSegs {
    UINT16      start;                  // from this vertex ...
    UINT16      end;                    // ... to this vertex
    UINT16      angle;                  // angle (0 = east, 16384 = north, ...)
    UINT16      lineDef;                // linedef that this seg goes along*/
    UINT16      flip;                   // true if not the same direction as linedef
    UINT16      offset;                 // distance from starting point
};

struct wSSector {
    UINT16      num;                    // number of Segs in this Sub-Sector
    UINT16      first;                  // first Seg
};

struct wBound {
    INT16       maxy, miny;
    INT16       minx, maxx;             // bounding rectangle
};

struct wNode {
    INT16       x, y;                   // starting point
    INT16       dx, dy;                 // offset to ending point
    wBound      side[2];
    UINT16      child[2];               // Node or SSector (if high bit is set)
};

struct wReject {
    UINT16      dummy;
};

struct wBlockMap {
    INT16       xOrigin;
    INT16       yOrigin;
    UINT16      noColumns;
    UINT16      noRows;
//    UINT16    data [];
};

class DoomLevel {

    struct sLevelLump {
        bool    changed;
        int     byteOrder;
        int     elementSize;
        int     elementCount;
        UINT32  dataSize;
        void   *rawData;

        sLevelLump ();
    };

    WAD        *m_Wad;
    wLumpName   m_Name;

    bool        m_IsHexen;
    const char *m_Title;
    const char *m_Music;
    int         m_Cluster;

    wThing     *m_ThingData;
    wLineDef   *m_LineDefData;

    sLevelLump  m_Map;
    sLevelLump  m_Thing;
    sLevelLump  m_LineDef;
    sLevelLump  m_SideDef;
    sLevelLump  m_Vertex;
    sLevelLump  m_Sector;
    sLevelLump  m_Segs;
    sLevelLump  m_SubSector;
    sLevelLump  m_Node;
    sLevelLump  m_Reject;
    sLevelLump  m_BlockMap;
    sLevelLump  m_Behavior;

    static void ConvertRaw1ToThing ( int, wThing1 *, wThing * );
    static void ConvertRaw2ToThing ( int, wThing2 *, wThing * );
    static void ConvertThingToRaw1 ( int, wThing *, wThing1 * );
    static void ConvertThingToRaw2 ( int, wThing *, wThing2 * );

    static void ConvertRaw1ToLineDef ( int, wLineDef1 *, wLineDef * );
    static void ConvertRaw2ToLineDef ( int, wLineDef2 *, wLineDef * );
    static void ConvertLineDefToRaw1 ( int, wLineDef *, wLineDef1 * );
    static void ConvertLineDefToRaw2 ( int, wLineDef *, wLineDef2 * );

    void ReplaceVertices ( int *, wVertex *, int );

    void DetermineType ();

    bool Load ();
    bool LoadHexenInfo ();

    bool LoadThings ( bool );
    bool LoadLineDefs ( bool );

    void StoreThings ();
    void StoreLineDefs ();

    void NewEntry ( sLevelLump *, int, void * );

    bool ReadEntry ( sLevelLump *, const char *, const wadDirEntry *, const wadDirEntry *, bool );
    bool UpdateEntry ( sLevelLump *, const char *, const char *, bool );

    void AdjustByteOrderMap ( int );
    void AdjustByteOrderThing ( int );
    void AdjustByteOrderLineDef ( int );
    void AdjustByteOrderSideDef ( int );
    void AdjustByteOrderVertex ( int );
    void AdjustByteOrderSector ( int );
    void AdjustByteOrderSegs ( int );
    void AdjustByteOrderSubSector ( int );
    void AdjustByteOrderNode ( int );
    void AdjustByteOrderReject ( int );
    void AdjustByteOrderBlockMap ( int );
    void AdjustByteOrderBehavior ( int );

    void AdjustByteOrder ( int );

    void CleanUpEntry ( sLevelLump * );
    void CleanUp ();

public:

    DoomLevel ( const char *, WAD *, bool = true );
    ~DoomLevel ();

    const WAD *GetWAD () const                  { return m_Wad; }

    const char *Name () const                   { return m_Name; }
    const char *Title () const                  { return m_Title ? m_Title : m_Name; }
    const char *Music () const                  { return m_Music ? m_Music : NULL; }
    int MapCluster () const                     { return m_Cluster; }

    int ThingCount () const                     { return m_Thing.elementCount; }
    int LineDefCount () const                   { return m_LineDef.elementCount; }
    int SideDefCount () const                   { return m_SideDef.elementCount; }
    int VertexCount () const                    { return m_Vertex.elementCount; }
    int SectorCount () const                    { return m_Sector.elementCount; }
    int SegCount () const                       { return m_Segs.elementCount; }
    int SubSectorCount () const                 { return m_SubSector.elementCount; }
    int NodeCount () const                      { return m_Node.elementCount; }
    int RejectSize () const                     { return m_Reject.dataSize; }
    int BlockMapSize () const                   { return m_BlockMap.dataSize; }
    int BehaviorSize () const                   { return m_Behavior.dataSize; }

    const wThing    *GetThings () const         { return m_ThingData; }
    const wLineDef  *GetLineDefs () const       { return m_LineDefData; }
    const wSideDef  *GetSideDefs () const       { return ( wSideDef * ) m_SideDef.rawData; }
    const wVertex   *GetVertices () const       { return ( wVertex * ) m_Vertex.rawData; }
    const wSector   *GetSectors () const        { return ( wSector * ) m_Sector.rawData; }
    const wSegs     *GetSegs () const           { return ( wSegs * ) m_Segs.rawData; }
    const wSSector  *GetSubSectors () const     { return ( wSSector * ) m_SubSector.rawData; }
    const wNode     *GetNodes () const          { return ( wNode * ) m_Node.rawData; }
    const wReject   *GetReject () const         { return ( wReject * ) m_Reject.rawData; }
    const wBlockMap *GetBlockMap () const       { return ( wBlockMap * ) m_BlockMap.rawData; }
    const UINT8     *GetBehavior () const       { return ( UINT8 * ) m_Behavior.rawData; }

    void NewThings ( int, wThing * );
    void NewLineDefs ( int, wLineDef * );
    void NewSideDefs ( int, wSideDef * );
    void NewVertices ( int, wVertex * );
    void NewSectors ( int, wSector * );
    void NewSegs ( int, wSegs * );
    void NewSubSectors ( int, wSSector * );
    void NewNodes ( int, wNode * );
    void NewReject ( int, UINT8 * );
    void NewBlockMap ( int, wBlockMap * );
    void NewBehavior ( int, char * );

    bool IsValid ( bool, bool = true ) const;
    bool IsDirty () const;

    void TrimVertices ();
    void PackVertices ();

    void PackSideDefs ();
    void UnPackSideDefs ();

    bool UpdateWAD ();
    void AddToWAD ( WAD *wad );

    sThingDesc *FindThing ( int type );
    static sLineDefDesc *FindLineDef ( int type );

    sThingDesc *GetThing ( int index );
    static sLineDefDesc *GetLineDef ( int index );
};

#if ( BYTE_ORDER == LITTLE_ENDIAN )

    inline void DoomLevel::AdjustByteOrderMap ( int )       {}
    inline void DoomLevel::AdjustByteOrderThing ( int )     {}
    inline void DoomLevel::AdjustByteOrderLineDef ( int )   {}
    inline void DoomLevel::AdjustByteOrderSideDef ( int )   {}
    inline void DoomLevel::AdjustByteOrderVertex ( int )    {}
    inline void DoomLevel::AdjustByteOrderSector ( int )    {}
    inline void DoomLevel::AdjustByteOrderSegs ( int )      {}
    inline void DoomLevel::AdjustByteOrderSubSector ( int ) {}
    inline void DoomLevel::AdjustByteOrderNode ( int )      {}
    inline void DoomLevel::AdjustByteOrderReject ( int )    {}
    inline void DoomLevel::AdjustByteOrderBlockMap ( int )  {}
    inline void DoomLevel::AdjustByteOrderBehavior ( int )  {}

    inline void DoomLevel::AdjustByteOrder ( int )          {}

#endif

#endif
