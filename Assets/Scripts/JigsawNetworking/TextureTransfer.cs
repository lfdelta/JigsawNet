using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TextureTransfer : MonoBehaviour
{
    const int ChunkSize = 4000;

    private class TextureMetadata
    {
        public int Width = 0;
        public int Height = 0;
        public int ChunkSz = 0;
        public TextureFormat Format;
        public int MipCount = 0;
        public byte[] RawBytes;
        public int NumChunksReceived = 0;
        public int NumChunksExpected = 0;
    };

    private Dictionary<short, TextureMetadata> ClientTextureBuffers;

    private JigsawHUD HUD;


    public class TextureMetaMsg : MessageBase
    {
        public short TextureId = 0;
        public int Width = 0;
        public int Height = 0;
        public int ChunkSz = 0;
        public TextureFormat Format;
        public int MipCount = 0;
    };


    public class TextureChunkMsg : MessageBase
    {
        public short TextureId = 0;
        public int StartByte = 0;  // First texture-byte index of this message
        public int NumBytes = 0;   // Number of bytes in this message
        public int TotalBytes = 0; // Total texture size across all messages
        public byte[] Bytes;
    };


    private void Start()
    {
        StaticJigsawData.ObjectManager.RequestObject("JigsawHUD", ReceiveJigsawHUD);
    }


    public void SetupClient(NetworkClient Client)
    {
        ClientTextureBuffers = new Dictionary<short, TextureMetadata>();
        
        Client.RegisterHandler(JigsawNetworkMsg.TextureMeta, OnClientReceiveTexMeta);
        Client.RegisterHandler(JigsawNetworkMsg.TextureChunk, OnClientReceiveTexChunk);
    }


    public void SendTextureToClient(int ConnectionId, short TextureId, Texture2D Texture)
    {
        // Spread messages out to keep from overflowing the send/receive buffer
        // TODO: if necessary, use a basic ACK system to detect and restart dropped data streams
        StartCoroutine(SendTextureToClient_Internal(ConnectionId, TextureId, Texture));
    }


    private IEnumerator SendTextureToClient_Internal(int ConnectionId, short TextureId, Texture2D Texture)
    {
        Debug.Log("Sending texture to client " + ConnectionId.ToString());

        // Send metadata
        TextureMetaMsg metaMsg = new TextureMetaMsg();
        metaMsg.TextureId = TextureId;
        metaMsg.Width = Texture.width;
        metaMsg.Height = Texture.height;
        metaMsg.ChunkSz = ChunkSize;
        metaMsg.Format = Texture.format;
        metaMsg.MipCount = Texture.mipmapCount;
        NetworkServer.SendToClient(ConnectionId, JigsawNetworkMsg.TextureMeta, metaMsg);

        // Send bytes, one chunk at a time
        byte[] rawData = Texture.GetRawTextureData();
        int lastChunk = (rawData.Length / ChunkSize);
        for (int i = 0; i < lastChunk; ++i)
        {
            TextureChunkMsg msg = new TextureChunkMsg();
            msg.TextureId = TextureId;
            msg.StartByte = i * ChunkSize;
            msg.NumBytes = ChunkSize;
            msg.TotalBytes = rawData.Length;
            msg.Bytes = new byte[ChunkSize];
            for (int j = 0; j < ChunkSize; ++j)
            {
                msg.Bytes[j] = rawData[msg.StartByte + j];
            }
            NetworkServer.SendToClient(ConnectionId, JigsawNetworkMsg.TextureChunk, msg);
            Debug.Log("Server sent chunk " + i.ToString() + "/" + lastChunk.ToString());
            yield return null;
        }
        if (lastChunk * ChunkSize < rawData.Length)
        {
            TextureChunkMsg finalMsg = new TextureChunkMsg();
            finalMsg.TextureId = TextureId;
            finalMsg.StartByte = lastChunk * ChunkSize;
            finalMsg.NumBytes = rawData.Length - finalMsg.StartByte;
            finalMsg.TotalBytes = rawData.Length;
            finalMsg.Bytes = new byte[finalMsg.NumBytes];
            for (int j = 0; j < finalMsg.NumBytes; ++j)
            {
                finalMsg.Bytes[j] = rawData[finalMsg.StartByte + j];
            }
            NetworkServer.SendToClient(ConnectionId, JigsawNetworkMsg.TextureChunk, finalMsg);
            Debug.Log("Server sent chunk " + lastChunk.ToString() + "/" + lastChunk.ToString());
        }
    }


    public void OnClientReceiveTexMeta(NetworkMessage Msg)
    {
        TextureMetaMsg metaMsg = Msg.ReadMessage<TextureMetaMsg>();
        TextureMetadata texData;

        if (ClientTextureBuffers.ContainsKey(metaMsg.TextureId))
        {
            texData = ClientTextureBuffers[metaMsg.TextureId];
        }
        else
        {
            texData = new TextureMetadata();
        }
        texData.Width = metaMsg.Width;
        texData.Height = metaMsg.Height;
        texData.ChunkSz = metaMsg.ChunkSz;
        texData.Format = metaMsg.Format;
        texData.MipCount = metaMsg.MipCount;

        Debug.Log("Client received metadata for texture " + metaMsg.TextureId.ToString());
        ClientTextureBuffers[metaMsg.TextureId] = texData;
    }


    private void ReceiveJigsawHUD(GameObject HUDObject)
    {
        HUD = HUDObject.GetComponent<JigsawHUD>();
    }


    public void OnClientReceiveTexChunk(NetworkMessage Msg)
    {
        TextureChunkMsg chunkMsg = Msg.ReadMessage<TextureChunkMsg>();
        TextureMetadata texData;

        // Allocate a dictionary element if necessary
        if (!ClientTextureBuffers.ContainsKey(chunkMsg.TextureId))
        {
            ClientTextureBuffers.Add(chunkMsg.TextureId, new TextureMetadata());
        }
        texData = ClientTextureBuffers[chunkMsg.TextureId];

        // Allocate fields if necessary
        if (texData.RawBytes == null)
        {
            texData.RawBytes = new byte[chunkMsg.TotalBytes];
            texData.NumChunksReceived = 0;
            texData.NumChunksExpected = (chunkMsg.TotalBytes + texData.ChunkSz - 1) / texData.ChunkSz;
        }

        // Fill in the received bytes
        for (int i = 0; i < chunkMsg.NumBytes; ++i)
        {
            texData.RawBytes[chunkMsg.StartByte + i] = chunkMsg.Bytes[i];
        }
        ++texData.NumChunksReceived;
        Debug.LogFormat("Client received chunk {0}/{1} for texture {2}", texData.NumChunksReceived, texData.NumChunksExpected, chunkMsg.TextureId.ToString());

        if (HUD)
        {
            HUD.LoadingProgressed((float)texData.NumChunksReceived / (float)texData.NumChunksExpected);
        }
        ClientCheckTextureIsComplete(chunkMsg.TextureId);
    }


    public void ClientCheckTextureIsComplete(short TextureId)
    {
        TextureMetadata texData = ClientTextureBuffers[TextureId];

        if (texData.Width <= 0 || texData.NumChunksExpected <= 0)
        {
            return;
        }
        if (texData.NumChunksReceived < texData.NumChunksExpected)
        {
            return;
        }

        // If we do have all chunks, deserialize the texture and clear the cached dict pair
        if (TextureId == 0)
        {
            Debug.Log("Client received all chunks for main puzzle texture; storing values into static PuzzleTexture");
            StaticJigsawData.PuzzleTexture = new Texture2D(texData.Width, texData.Height, texData.Format, texData.MipCount > 1);
            StaticJigsawData.PuzzleTexture.LoadRawTextureData(texData.RawBytes);
            StaticJigsawData.PuzzleTexture.Apply();
            ClientTextureBuffers.Remove(TextureId);

            HandlePuzzleTextureLoaded();
        }
        else
        {
            Debug.LogError("Client received all chunks for texture with unrecognized ID " + TextureId.ToString());
        }
    }


    private void HandlePuzzleTextureLoaded()
    {
        PuzzlePiece[] pieces = (PuzzlePiece[])FindObjectsOfType(typeof(PuzzlePiece));
        foreach (PuzzlePiece p in pieces)
        {
            p.ClientSetPuzzleTexture(StaticJigsawData.PuzzleTexture);
        }
        if (HUD)
        {
            HUD.LoadingFinished();
        }
    }
}
