/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using CodeImp;

namespace CodeImp.Bloodmasters
{
	// Message commands
	public enum MsgCmd : int
	{
		PingOrConfirm = 0,
		ConnectRequest = 1,
		ConnectConfirm = 2,
		ConnectRefused = 3,
		Disconnect = 4,
		PlayerLogin = 5,
		StartGameInfo = 6,
		GameStarted = 7,
		GameSnapshot = 8,
		Command = 9,
		SayMessage = 10,
		ShowMessage = 11,
		ClientUpdate = 12,
		ServerInfo = 13,
		ChangeTeam = 14,
		ClientDisposed = 15,
		SpawnActor = 16,
		Snapshot = 17,
		SectorMovement = 18,
		ClientMove = 19,
		ClientCorrection = 20,
		ItemPickup = 21,
		StatusUpdate = 22,
		TakeDamage = 23,
		ClientDead = 24,
		RespawnRequest = 25,
		GameStateChange = 26,
		TeleportClient = 27,
		Suicide = 28,
		SwitchWeapon = 29,
		SpawnProjectile = 30,
		UpdateProjectile = 31,
		TeleportProjectile = 32,
		DestroyProjectile = 33,
		ShieldHit = 34,
		DamageGiven = 35,
		PowerupCountUpdate = 36,
		FireIntensity = 37,
		ScoreFlag = 38,
		ReturnFlag = 39,
		MapChange = 41,
		FirePowerup = 42,
		SayTeamMessage = 43,
		PlayerNameChange = 44,
		NeedActor = 45,
		CallvoteStatus = 46,
		CallvoteEnd = 47,
		CallvoteSubmit = 48,
		CallvoteRequest = 49
	}
}
