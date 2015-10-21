using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class Lab02b_PlayerControlPrediction: NetworkBehaviour
{

    struct PlayerState
    {
        public int movementNumber;
        public float posX, posY, posZ;
        public float rotX, rotY, rotZ;
        public CharacterState animationState;
    }

    //[SyncVar]
    //PlayerState state;

    [SyncVar(hook = "OnServerStateChanged")]
    PlayerState serverState;//represents state of player to server

    PlayerState predictedState;//represents state of player as predicted. On client only!

    Queue <KeyCode> pendingMoves;//represents moves the player is attempting that haven't been
                                 //acknowledged by server yet! Remember: Queue is FIFO.
    CharacterState characterAnimationState;

    public Animator controller;

    //The [Server] attribute marks a function that is only allowed to be called on the server
    [Server]
    void InitState()
    {
        serverState = new PlayerState
        {
            movementNumber = 0,

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
        PlayerState stateToRender = isLocalPlayer ? predictedState : serverState;

        transform.position = new Vector3(stateToRender.posX, stateToRender.posY, stateToRender.posZ);
        transform.rotation = Quaternion.Euler(stateToRender.rotX, stateToRender.rotY, stateToRender.rotZ);
        controller.SetInteger("CharacterState", (int)stateToRender.animationState);
    }

    // Use this for initialization
    void Start()
    {
        InitState();
        predictedState = serverState;
        if(isLocalPlayer)
        {
            pendingMoves = new Queue<KeyCode>();
            UpdatePredictedState();
        }
        SyncState();
    }

    // Update is called once per frame
    void Update()
    {
        if (isLocalPlayer)
        {
            //Debug.Log("Pending moves: " + pendingMoves.Count);

            KeyCode[] possibleKeys = { KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.W, KeyCode.Q, KeyCode.E, KeyCode.Space };
            bool somethingPressed = false;

            foreach (KeyCode possibleKey in possibleKeys)
            {
                //if the currently observed key code is not pressed, then to nothing.
                if (!Input.GetKey(possibleKey))
                {
                    continue;
                }

                somethingPressed = true;
                pendingMoves.Enqueue(possibleKey);
                UpdatePredictedState();
                CmdMoveOnServer(possibleKey);
            }

            if(!somethingPressed)
            {
                pendingMoves.Enqueue(KeyCode.Alpha0);
                UpdatePredictedState();
                CmdMoveOnServer(KeyCode.Alpha0);
            }
        }

        SyncState();
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
            movementNumber = 1 + previous.movementNumber,

            posX = deltaX + previous.posX,
            posY = deltaY + previous.posY,
            posZ = deltaZ + previous.posZ,

            rotX = previous.rotX,
            rotY = deltaRotationY + previous.rotY,
            rotZ = previous.rotZ,

            animationState = CalcAnimation(deltaX, deltaY, deltaZ, deltaRotationY)
        };
    }

    [Command]
    void CmdMoveOnServer(KeyCode pressedKey)
    {
        serverState = Move(serverState, pressedKey);
    }

    void OnServerStateChanged(PlayerState newState)
    {
        serverState = newState;
        if (pendingMoves != null)
        {
            while(pendingMoves.Count > (predictedState.movementNumber - serverState.movementNumber))
            {
                pendingMoves.Dequeue();
            }
            UpdatePredictedState();
        }
    }

    void UpdatePredictedState()
    {
        predictedState = serverState;
        foreach(KeyCode moveKey in pendingMoves)
        {
            predictedState = Move(predictedState, moveKey);
        }
    }

    public enum CharacterState
    {
        Idle = 0,
        WalkingForward = 1,
        WalkingBackwards = 2,
        Jumping = 4
    }

    CharacterState CalcAnimation(float dX, float dY, float dZ, float dRY)
    {
        if (dX == 0 && dY == 0 && dZ == 0)
            return CharacterState.Idle;
        if(dX != 0 || dZ != 0)
        {
            if (dX > 0 || dZ > 0)
                return CharacterState.WalkingForward;
            else
                return CharacterState.WalkingBackwards;
        }
        return CharacterState.Idle;
    }
}

