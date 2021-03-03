using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerSyncPosition : NetworkBehaviour
{
    [SyncVar]
    private Vector2 syncPos;

    [SyncVar]
    private Quaternion syncRot;

    [SerializeField] Transform myTransform;

    void FixedUpdate()
    {
        TransmitPosition();
        LerpPosition();
    }

    void LerpPosition()
    {
        if (!isLocalPlayer)
        {
            myTransform.position = syncPos;
            myTransform.rotation = syncRot;
        }
    }

    [Command]
    void CmdProvidePositionToServer(Vector2 pos, Quaternion rot)
    {
        syncPos = pos;
        syncRot = rot;
    }

    [ClientCallback]
    void TransmitPosition()
    {
        if (isLocalPlayer)
        {
            CmdProvidePositionToServer(myTransform.position, myTransform.rotation);
        }
    }
}