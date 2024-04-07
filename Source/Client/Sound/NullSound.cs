namespace Bloodmasters.Client;

/// <summary>
/// Summary description for NullSound.
/// </summary>
public class NullSound : ISound
{
    // Constructor
    public NullSound()
    {
    }

    public void ResetSettings()
    {
    }

    public void Update()
    {
    }

    public void SetRandomOffset()
    {
    }

    public void Play()
    {
    }

    void ISound.Play(bool repeat)
    {
    }

    void ISound.Play(float volume, bool repeat)
    {
    }

    public void Stop()
    {
    }

    public bool Repeat
    {
        get
        {
            return false;
        }
    }

    public bool AutoDispose
    {
        get
        {
            return false;
        }
        set
        {
        }
    }

    public string Filename
    {
        get
        {
            return null;
        }
    }

    public float Volume
    {
        get
        {
            return 0;
        }
        set
        {
        }
    }

    public bool Playing
    {
        get
        {
            return false;
        }
    }

    public bool Positional
    {
        get
        {
            return false;
        }
    }

    public Vector2D Position
    {
        get
        {
            return new Vector2D ();
        }
        set
        {
        }
    }

    public int Length => 0;
    public int CurrentPosition => 0;

    public bool Disposed
    {
        get { return false; }
    }

    public void Dispose()
    {
    }
}
