using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class JigsawPlayerController : NetworkBehaviour
{
    public override void OnStartLocalPlayer()
    {
        Debug.Log("Local player started!");

        // TODO: attach to camera
    }


    void Update()
    {
        // TODO: local camera controls (pan, rotate, reset to fixed camera orientation)
        // TODO: RPC puzzle controls (select, deselect, drag)
    }
}
