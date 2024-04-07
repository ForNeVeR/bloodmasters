/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;

namespace Bloodmasters.Client.Sound;

public interface ISound : IDisposable
{
    void ResetSettings();
    void Update();
    void SetRandomOffset();
    void Play();
    void Play(bool repeat);
    void Play(float volume, bool repeat);
    void Stop();

    bool Repeat { get; }
    bool AutoDispose { get; set; }
    string Filename { get; }
    float Volume { get; set; }
    bool Playing { get; }
    bool Positional { get; }
    Vector2D Position { get; set; }
    bool Disposed { get; }
    int Length { get; }
    int CurrentPosition { get; }
}
