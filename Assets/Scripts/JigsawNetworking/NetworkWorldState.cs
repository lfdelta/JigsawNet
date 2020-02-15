using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class NetworkWorldState : NetworkBehaviour
{
    public class SyncListPlayerState : SyncListStruct<JigsawPlayerState> { }

    [HideInInspector] public SyncListPlayerState ConnectedPlayers = new SyncListPlayerState();

    public delegate void NetworkWorldDelegate();
    public event NetworkWorldDelegate OnConnectedPlayersUpdated;

    public int MaxPlayerCount = 8;
    public Color[] AssignablePlayerColors;

    private uint NextAssignedId = 0;
    private int AssignedColors = 0; // Bitflag


    public override void OnStartClient()
    {
        ConnectedPlayers.Callback = OnClientPlayersUpdated;

        StaticJigsawData.ObjectManager.RegisterObject(gameObject, "NetworkWorldState");
    }

    private void OnClientPlayersUpdated(SyncListPlayerState.Operation op, int index)
    {
        if (OnConnectedPlayersUpdated != null)
        {
            OnConnectedPlayersUpdated();
        }
    }


    public JigsawPlayerState RegisterPlayer(string Username)
    {
        JigsawPlayerState state = new JigsawPlayerState();
        state.Id = NextAssignedId;
        ++NextAssignedId;
        state.Username = Username;
        state.UserColor = AssignColor();

        ConnectedPlayers.Add(state);
        Debug.LogFormat("Registered player {0} with Id {1}; total {2} players", Username, state.Id, ConnectedPlayers.Count);
        return state;
    }


    public void DeregisterPlayer(JigsawPlayerState State)
    {
        ConnectedPlayers.Remove(State);
        UnassignColor(State.UserColor);
        Debug.LogFormat("Deregistered player {0} with Id {1}; total {2} players", State.Username, State.Id, ConnectedPlayers.Count);
    }


    private Color AssignColor()
    {
        int numFreeColors = 0;
        for (int i = 0; i < AssignablePlayerColors.Length; ++i)
        {
            if ((AssignedColors & (1 << i)) == 0)
            {
                ++numFreeColors;
            }
        }

        int freeColorInd = Random.Range(0, numFreeColors);
        numFreeColors = 0;
        for (int i = 0; i < AssignablePlayerColors.Length; ++i)
        {
            if ((AssignedColors & (1 << i)) == 0)
            {
                if (numFreeColors == freeColorInd)
                {
                    AssignedColors |= (1 << i);
                    Debug.LogFormat("Assigned color {0}, bitflag {1}", i, AssignedColors);
                    return AssignablePlayerColors[i];
                }
                ++numFreeColors;
            }
        }
        Debug.LogError("NetworkWorldState::AssignColor could not find an available color");
        return Color.black;
    }


    private void UnassignColor(Color color)
    {
        for (int i = 0; i < AssignablePlayerColors.Length; ++i)
        {
            if (AssignablePlayerColors[i] == color)
            {
                AssignedColors &= ~(1 << i);
                Debug.LogFormat("Unassigned color {0}, bitflag {1}", i, AssignedColors);
                return;
            }
        }
    }
}