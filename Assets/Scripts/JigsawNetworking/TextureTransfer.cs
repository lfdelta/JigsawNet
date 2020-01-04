using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

// TODO: separate out message send/receive logic from client connect logic
// https://docs.unity3d.com/Manual/UNetMessages.html

public class TextureTransfer : MonoBehaviour
{
    const int ChunkSize = 2048;

    private class TextureMetadata
    {
        public int Width = 0;
        public int Height = 0;
        public int ChunkSz = 0;
        public TextureFormat Format;
        public int MipCount = 0;
        public byte[] RawBytes;
        public bool[] ReceivedChunks;
    };

    Dictionary<short, TextureMetadata> ClientTextureBuffers;


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


    // Create a client and connect to the server port
    public void SetupClient(NetworkClient Client)
    {
        ClientTextureBuffers = new Dictionary<short, TextureMetadata>();
        
        Client.RegisterHandler(JigsawNetworkMsg.TextureMeta, OnClientReceiveTexMeta);
        Client.RegisterHandler(JigsawNetworkMsg.TextureChunk, OnClientReceiveTexChunk);
    }


    public void SendTextureToClient(int ConnectionId, short TextureId, Texture2D Texture)
    {
        // Spread messages out to keep from overflowing the send/receive buffer
        // TODO: use a basic ACK system to detect and restart dropped data streams
        // TODO: preprocess the texture as needed to optimize the data stream, e.g. remove mips
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
            texData.ReceivedChunks = new bool[(chunkMsg.TotalBytes + texData.ChunkSz - 1) / texData.ChunkSz];
            for (int i = 0; i < texData.ReceivedChunks.Length; ++i)
            {
                texData.ReceivedChunks[i] = false;
            }
        }

        // Fill in the received bytes
        for (int i = 0; i < chunkMsg.NumBytes; ++i)
        {
            texData.RawBytes[chunkMsg.StartByte + i] = chunkMsg.Bytes[i];
        }
        texData.ReceivedChunks[chunkMsg.StartByte / texData.ChunkSz] = true;
        Debug.Log("Client received chunk " + (chunkMsg.StartByte / texData.ChunkSz).ToString() + "/" + (texData.ReceivedChunks.Length).ToString() + " for texture " + chunkMsg.TextureId.ToString());
        ClientCheckTextureIsComplete(chunkMsg.TextureId);
    }


    public void ClientCheckTextureIsComplete(short TextureId)
    {
        TextureMetadata texData = ClientTextureBuffers[TextureId];
        
        // Verify whether we have metadata and all chunks for this texture
        if (texData.Width <= 0)
        {
            return;
        }
        for (int i = texData.ReceivedChunks.Length - 1; i >= 0; --i)
        {
            if (!texData.ReceivedChunks[i])
            {
                // If not, exit and try again when the next chunk is received
                return;
            }
        }

        // If we do have all chunks, deserialize the texture and clear the cached dict pair
        if (TextureId == 0)
        {
            Debug.Log("Client received all chunks for main puzzle texture; storing values into static PuzzleTexture");
            StaticJigsawData.PuzzleTexture = new Texture2D(texData.Width, texData.Height, texData.Format, texData.MipCount > 1);
            StaticJigsawData.PuzzleTexture.LoadRawTextureData(texData.RawBytes);
            StaticJigsawData.PuzzleTexture.Apply();
            ClientTextureBuffers.Remove(TextureId);

            // TODO: send a signal to apply PuzzleTexture to the pieces
        }
        else
        {
            Debug.LogError("Client received all chunks for texture with unrecognized ID " + TextureId.ToString());
        }
    }


    // TEMP / Debug
    void OnGUI()
    {
        if (StaticJigsawData.PuzzleTexture)
        {
            GUI.DrawTexture(Camera.current.pixelRect, StaticJigsawData.PuzzleTexture);
        }
    }
}
