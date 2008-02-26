/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.IO;
using System.Threading;
using Microsoft.DirectX;
using Microsoft.DirectX.DirectSound;
using CodeImp.Bloodmasters;
using CodeImp;

namespace CodeImp.Bloodmasters.Client
{
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
		
	}
}
