using System.IO;
using Bloodmasters.Client.Graphics;
using Bloodmasters.LevelMap;

namespace Bloodmasters.Client.LevelMap;

public class ClientSector : Sector
{
    // Visual Sector
    private VisualSector vissector = null;
    private int updatetime = 0;
    private int firstfloorvertex = -1;
    private int firstceilvertex = -1;
    private int numfaces = 0;
    private ISound sound = null;
    private bool playmovementsound = false;

    public VisualSector VisualSector { get { return vissector; } set { vissector = value; } }
    public int FirstFloorVertex { get { return firstfloorvertex; } set { firstfloorvertex = value; } }
    public int FirstCeilVertex { get { return firstceilvertex; } set { firstceilvertex = value; } }
    public int NumFaces { get { return numfaces; } set { numfaces = value; } }
    public bool PlayMovementSound { get { return playmovementsound; } set { playmovementsound = value; } }

    public ClientSector(BinaryReader data, int index, Map map) : base(data, index, map)
    {
    }

    protected override void UpdateClientSounds()
    {
        var playsound = false;

        // Set lightmap update timer
        updatetime = SharedGeneral.currenttime;

        // No sound playing?
        if(sound == null) playsound = true;
        else if((sound.Filename == SOUND_END) || !sound.Playing) playsound = true;

        // Play start sound?
        if(playsound && playmovementsound)
        {
            // Dispose old sound
            if(sound != null) sound.Dispose();

            // Play start sound
            sound = SoundSystem.GetSound(SOUND_START, true);
            sound.Position = new Vector2D(bounds.X + bounds.Width / 2f, bounds.Y + bounds.Height / 2f);
            sound.Play();
        }
    }

    protected override void UpdateLightmaps()
    {
        if(vissector != null)
        {
            // Update sector lightmaps
            vissector.UpdateLightmap = true;
            foreach(ClientSector s in adjsectors) if(s.VisualSector != null) s.VisualSector.UpdateLightmap = true;
        }
    }

    protected override void UpdateClientLighting()
    {
        // Time to update?
        if(updatetime <= SharedGeneral.currenttime)
        {
            // Update sector lightmaps
            UpdateLightmaps();

            // Set timer
            updatetime = SharedGeneral.currenttime + UPDATE_LIGHTMAP_INTERVAL;
        }

        // Sound finished playing?
        if((sound != null) && !sound.Playing && playmovementsound)
        {
            // Dispose old sound
            sound.Dispose();

            // Play moving sound
            sound = SoundSystem.GetSound(SOUND_RUN, true);
            sound.Position = new Vector2D(bounds.X + bounds.Width / 2f, bounds.Y + bounds.Height / 2f);
            sound.Play(true);
        }

        // Running in client mode?
        if(General.map == this.map)
        {
            // Go for all actors
            foreach(Actor a in General.arena.Actors)
            {
                // Actor in this sector and on the floor?
                if((a.HighestSector == this) && (a.IsOnFloor))
                {
                    // Drop on to highest sector
                    a.DropImmediately();
                }
            }
        }
    }

    protected override void DropPlayers()
    {
    }

    protected override void PlaySounds(bool playstopsound)
    {
        // Play stop sound?
        if(playstopsound && playmovementsound)
        {
            // Dispose old sound
            if(sound != null) sound.Dispose();

            // Play stop sound
            sound = SoundSystem.GetSound(SOUND_END, true);
            sound.Position = new Vector2D(bounds.X + bounds.Width / 2f, bounds.Y + bounds.Height / 2f);
            sound.Play();
        }
    }

    public override void Dispose()
    {
        if(sound != null) sound.Dispose();
        sound = null;

        base.Dispose();
    }
}
