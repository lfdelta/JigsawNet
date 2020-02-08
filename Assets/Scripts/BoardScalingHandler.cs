using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BoardScalingHandler : NetworkBehaviour
{
    [SyncVar]
    private Vector2 Dimensions;


    public void SetBoardDimensions(Vector2 Dims)
    {
        Dimensions = Dims;
        if (StaticJigsawData.IsHost)
        {
            ApplyDimensions();
        }
    }


    public override void OnStartClient()
    {
        base.OnStartClient();
        ApplyDimensions();
    }


    private void ApplyDimensions()
    {
        Vector3 scale = transform.localScale;
        scale.x = Dimensions.x;
        scale.z = Dimensions.y;
        transform.localScale = scale;
    }
}
