﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Lab02a_PlayerControl : NetworkBehaviour {

	struct PlayerState
	{
		public float posX, posY, posZ;
		public float rotX, rotY, rotZ;
	}

	[SyncVar]
	PlayerState state;

	//The [Server] attribute marks a function that is only allowed to be called on the server
	[Server]
	void InitState()
	{
		state = new PlayerState
		{
			posX = -119f,
			posY = 165.08f,
			posZ = -924f,

			rotX = 0f,
			rotY = 0f,
			rotZ = 0f
		};
	}

	void SyncState()
	{
		transform.position = new Vector3 (state.posX, state.posY, state.posZ);
		transform.rotation = Quaternion.Euler (state.rotX, state.rotY, state.rotZ);
	}

	// Use this for initialization
	void Start () {
		InitState ();
		SyncState ();
	}
	
	// Update is called once per frame
	void Update () {
		if (isLocalPlayer) 
		{
			KeyCode[] possibleKeys = { KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.W, KeyCode.Q, KeyCode.E, KeyCode.Space };
			foreach(KeyCode possibleKey in possibleKeys)
			{
				//if the currently observed key code is not pressed, then to nothing.
				if(!Input.GetKey(possibleKey))
				{
					continue;
				}
			}
		}

		SyncState ();
	}

	PlayerState Move(PlayerState previous, KeyCode newKey)
	{
		float deltaX = 0, deltaY = 0, deltaZ = 0;
		float deltaRotationY = 0;

		switch (newKey) 
		{
			case KeyCode.Q:
				deltaX = -0.5f;
				break;
			case KeyCode.S:
				deltaZ = -0.5f;
				break;
			case KeyCode.E:
				deltaX = 0.5f;
				break;
			case KeyCode.W:
				deltaZ = 0.5f;
				break;
			case KeyCode.A:
				deltaRotationY = -1f;
				break;
			case KeyCode.D:
				deltaRotationY = 1f;
				break;
		}

		return new PlayerState
		{
			posX = deltaX + previous.posX,
			posY = deltaY + previous.posY,
			posZ = deltaZ + previous.posZ,

			rotX = previous.rotX,
			rotY = deltaRotationY + previous.rotY,
			rotZ = previous.rotZ
		};
	}

	[Command]
	void CmdMoveOnServer(KeyCode pressedKey)
	{
		state = Move (state, pressedKey);
	}

}
