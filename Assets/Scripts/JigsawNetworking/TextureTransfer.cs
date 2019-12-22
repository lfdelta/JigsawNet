using UnityEngine;
using UnityEngine.Networking;

// TODO: separate out message send/receive logic from client connect logic

// https://docs.unity3d.com/Manual/UNetMessages.html

public class TextureTransfer : MonoBehaviour
{
    NetworkClient tempClient;

    byte[] ClientTextureBuffer;


    public class TextureChunkMsg : MessageBase
    {
        public short TextureId = 0;
        public short StartByte = 0;  // First texture-byte index of this message
        public short NumBytes = 0;   // Number of bytes in this message
        public short TotalBytes = 0; // Total texture size across all messages
        public byte[] Bytes;
    };


    // Create a client and connect to the server port
    public void SetupClient()
    {
        tempClient = new NetworkClient();
        tempClient.RegisterHandler(MsgType.Connect, OnConnected);
        tempClient.RegisterHandler((short)JigsawNetworkMsg.TextureChunk, OnClientReceiveChunk);
        tempClient.Connect("127.0.0.1", 7777);
    }


    public void OnConnected(NetworkMessage Msg)
    {
        Debug.Log("Connected to server");
    }


    public void SendTextureToClient(int ConnectionId, short TextureId, ref byte[] RawData, short ChunkSize)
    {
        // TODO: redo this stuff to match the new struct layout

        short lastChunk = (short)(RawData.Length / ChunkSize);
        for (short i = 0; i < lastChunk; ++i)
        {
            TextureChunkMsg msg = new TextureChunkMsg();
            msg.TextureId = TextureId;
            msg.Chunk = i;
            msg.FinalChunk = lastChunk;
            msg.NumBytes = ChunkSize;
            msg.Bytes = new byte[ChunkSize];
            short rowOffset = (short)(i * ChunkSize);
            for (short j = 0; j < ChunkSize; ++j)
            {
                msg.Bytes[j] = RawData[rowOffset + j];
            }
            NetworkServer.SendToClient(ConnectionId, (short)JigsawNetworkMsg.TextureChunk, msg);
        }
        TextureChunkMsg finalMsg = new TextureChunkMsg();
        finalMsg.TextureId = TextureId;
        finalMsg.Chunk = lastChunk;
        finalMsg.FinalChunk = lastChunk;
        short finalRowOffset = (short)((lastChunk - 1) * ChunkSize);
        finalMsg.NumBytes = (short)(RawData.Length - finalRowOffset);
        finalMsg.Bytes = new byte[finalMsg.NumBytes];
        for (short j = 0; j < finalMsg.NumBytes; ++j)
        {
            finalMsg.Bytes[j] = RawData[finalRowOffset + j];
        }
        NetworkServer.SendToClient(ConnectionId, (short)JigsawNetworkMsg.TextureChunk, finalMsg);
    }


    public void OnClientReceiveChunk(NetworkMessage Msg)
    {
        TextureChunkMsg chunkMsg = Msg.ReadMessage<TextureChunkMsg>();

        if (ClientTextureBuffer.Length == 0)
        {
            ClientTextureBuffer = new byte[chunkMsg.TotalBytes];
        }

        for (short i = 0; i < chunkMsg.NumBytes; ++i)
        {
            ClientTextureBuffer[chunkMsg.StartByte + i] = chunkMsg.Bytes[i];
        }

        // TODO: verify that we have all of the chunks
        // if we do, save the byte array to StaticJigsawData.PuzzleTexture and then use it to reinit the PuzzlePieces shader
    }
}
