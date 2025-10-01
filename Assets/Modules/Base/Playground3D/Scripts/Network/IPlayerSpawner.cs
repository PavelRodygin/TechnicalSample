using Mirror;
using UnityEngine;

namespace Modules.Base.Playground3D.Scripts.Network
{
    /// <summary>
    /// Interface for network player spawning strategies
    /// </summary>
    public interface IPlayerSpawner
    {
        /// <summary>
        /// Spawns a player for the given network connection
        /// </summary>
        /// <param name="spawnPosition">Position to spawn the player</param>
        /// <param name="spawnRotation">Rotation to spawn the player</param>
        /// <returns>Spawned player GameObject</returns>
        GameObject SpawnPlayer(int connId, Vector3 spawnPosition, Quaternion spawnRotation);
    }
}
