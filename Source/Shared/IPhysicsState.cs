/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Drawing;
using System.Collections;
using CodeImp;

namespace CodeImp.Bloodmasters
{
	public interface IPhysicsState
	{
		PhysicsState State { get; }
	}
}
