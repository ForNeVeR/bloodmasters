//----------------------------------------------------------------------------
//
// File:        blockmap.cpp
// Date:        14-Jul-1995
// Programmer:  Marc Rousseau
//
// Description: This module contains the logic for the BLOCKMAP builder.
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

#include <math.h>
#include <memory.h>
#include <stdio.h>
#include <stdlib.h>
#include "common.hpp"
#include "level.hpp"
#include "ZenNode.hpp"
#include "console.hpp"

void AddLineDef ( sBlockList *block, int line )
{
    if (( block->count % 16 ) == 0 ) {
        int size = ( block->count + 16 ) * sizeof ( int );
        block->line = ( int * ) realloc ( block->line, size );
    }
    block->line [ block->count++ ] = line;
}

sBlockMap *GenerateBLOCKMAP ( DoomLevel *level )
{
    Status ( "Creating BLOCKMAP ... " );

    const wVertex *vertex   = level->GetVertices ();
    const wLineDef *lineDef = level->GetLineDefs ();

    int xLeft, xRight, yTop, yBottom;
    xRight = xLeft = vertex [0].x;
    yTop = yBottom = vertex [0].y;
    
    for ( int i = 1; i < level->VertexCount (); i++ ) {
        if ( vertex [i].x < xLeft ) xLeft = vertex [i].x;
        if ( vertex [i].x > xRight ) xRight = vertex [i].x;
        if ( vertex [i].y < yBottom ) yBottom = vertex [i].y;
        if ( vertex [i].y > yTop ) yTop = vertex [i].y;
    }

    xLeft -= 8;    xRight += 8;
    yBottom -= 8;  yTop += 8;

    int noCols = ( xRight - xLeft ) / 128 + 1;
    int noRows = ( yTop - yBottom ) / 128 + 1;
    int totalSize = noCols * noRows;

    sBlockList *blockList = new sBlockList [ totalSize ];
    for ( int i = 0; i < totalSize; i++ ) {
        blockList [i].firstIndex = i;
        blockList [i].offset     = 0;
        blockList [i].count      = 0;
        blockList [i].line       = NULL;
    }

    for ( int i = 0; i < level->LineDefCount (); i++ ) {

        const wVertex *vertS = &vertex [ lineDef [i].start ];
        const wVertex *vertE = &vertex [ lineDef [i].end ];

        long x0 = vertS->x - xLeft;
        long y0 = vertS->y - yBottom;
        long x1 = vertE->x - xLeft;
        long y1 = vertE->y - yBottom;

        int startX = x0 / 128, startY = y0 / 128;
        int endX = x1 / 128, endY = y1 / 128;

        int index = startX + startY * noCols;

        if ( startX == endX ) {
            AddLineDef ( &blockList [ index ], i );
            if ( startY != endY ) {	// vertical line
                int dy = (( endY - startY ) > 0 ) ? 1 : -1;
                do {
                    startY += dy;
                    index  += dy * noCols;
                    AddLineDef ( &blockList [ index ], i );
                } while ( startY != endY );
            }
        } else {
            if ( startY == endY ) {	// horizontal line
                AddLineDef ( &blockList [ index ], i );
                int dx = (( endX - startX ) > 0 ) ? 1 : -1;
                do {
                    startX += dx;
                    index  += dx;
                    AddLineDef ( &blockList [ index ], i );
                } while ( startX != endX );
            } else {			// diagonal line

                int dx = ( x1 - x0 );
                int dy = ( y1 - y0 );

                int sx = ( dx < 0 ) ? -1 : 1;
                int sy = ( dy < 0 ) ? -1 : 1;

                x1 *= dy;
                int nextX = x0 * dy;
                int deltaX = ( startY * 128 + 64 * ( 1 + sy ) - y0 ) * dx;

                bool done = false;

                do {
                    int thisX = nextX;
                    nextX += deltaX;
                    if (( sx * sy * nextX ) >= ( sx * sy * x1 )) nextX = x1, done = true;

                    int lastIndex = index + nextX / dy / 128 - thisX / dy / 128;

                    AddLineDef ( &blockList [ index ], i );
                    while ( index != lastIndex ) {
                        index += sx;
                        AddLineDef ( &blockList [ index ], i );
                    }

                    index += sy * noCols;
                    deltaX = ( 128 * dx ) * sy;

                } while ( ! done );

                int lastIndex = endX + endY * noCols;
                if ( index != lastIndex + sy * noCols ) {
                    AddLineDef ( &blockList [ lastIndex ], i );
                }
            }
        }
    }

    sBlockMap *blockMap = new sBlockMap;
    blockMap->xOrigin   = xLeft;
    blockMap->yOrigin   = yBottom;
    blockMap->noColumns = noCols;
    blockMap->noRows    = noRows;
    blockMap->data      = blockList;

    return blockMap;
}

int CreateBLOCKMAP ( DoomLevel *level, const sBlockMapOptions &options )
{
    // Generate the data
    sBlockMap *blockMap = GenerateBLOCKMAP ( level );

    Status ( "Packing BLOCKMAP ... " );

    sBlockList *blockList = blockMap->data;

    // Count unique blockList elements
    int totalSize = blockMap->noColumns * blockMap->noRows;
    int blockListSize = 0, savings = 0;
    int zeroIndex = -1;
    for ( int i = 0; i < totalSize; i++ ) {
        if ( options.Compress ) {
            if ( blockList [i].count == 0 ) {
                if ( zeroIndex != -1 ) {
                    blockList [i].firstIndex = zeroIndex;
                    savings += 0 + 2;
                    continue;
                }
                zeroIndex = i;
            } else {
                // Only go back to the beginning of the previous row
                int rowStart = ( i / blockMap->noColumns ) * blockMap->noColumns;
                int lastStart = rowStart ? rowStart - blockMap->noColumns : 0;
                int index = i - 1;
                while ( index >= lastStart ) {
                    int count = blockList[i].count;
                    if (( blockList[index].count == count ) &&
                        ( memcmp ( blockList[i].line, blockList[index].line, count * sizeof ( int )) == 0 )) {
                        blockList [i].firstIndex = index;
                        savings += count + 2;
                        break;
                    }
                    index--;
                }
                if ( index >= lastStart ) continue;
            }
        }
        blockList [i].firstIndex = i;
        blockListSize += 2 + blockList [i].count;
    }

    Status ( "Saving BLOCKMAP ... " );

    int blockSize = sizeof ( wBlockMap ) +
                    totalSize * sizeof ( INT16 ) +
                    blockListSize * sizeof ( INT16 );
    char *start = new char [ blockSize ];
    wBlockMap *map = ( wBlockMap * ) start;
    map->xOrigin   = ( INT16 ) blockMap->xOrigin;
    map->yOrigin   = ( INT16 ) blockMap->yOrigin;
    map->noColumns = ( UINT16 ) blockMap->noColumns;
    map->noRows    = ( UINT16 ) blockMap->noRows;

    // Fill in data & offsets
    UINT16 *offset = ( UINT16 * ) ( map + 1 );
    UINT16 *data   = offset + totalSize;
    for ( int i = 0; i < totalSize; i++ ) {
        sBlockList *block = &blockList [i];
        if ( block->firstIndex == i ) {
            block->offset = data - ( UINT16 * ) start;
            *data++ = 0;
            for ( int x = 0; x < block->count; x++ ) {
                *data++ = ( UINT16 ) block->line [x];
            }
            *data++ = ( UINT16 ) -1;
        } else {
            block->offset = blockList [ block->firstIndex ].offset;
        }
    }

    bool errors = false;

    for ( int i = 0; i < totalSize; i++ ) {
        if ( blockList [i].offset > 0xFFFF ) {
            errors = true;
        }
        offset [i] = ( UINT16 ) blockList [i].offset;
        if ( blockList [i].line ) free ( blockList [i].line );
    }

    delete [] blockList;
    delete blockMap;

    if ( errors == true ) {
        delete [] start;
        return -1;
    }

    level->NewBlockMap ( blockSize, map );

    return savings * sizeof ( INT16 );
}
