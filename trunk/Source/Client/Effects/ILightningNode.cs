/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Drawing;
using System.Collections;
using CodeImp.Bloodmasters;
using CodeImp;

namespace CodeImp.Bloodmasters.Client
{
	public interface ILightningNode
	{
		// Required properties
		Vector3D Position { get; }
		Vector3D Velocity { get; }
		
		// Required methods
		void RemoveLightning(Lightning l);
		void AddLightning(Lightning l);
	}
}
