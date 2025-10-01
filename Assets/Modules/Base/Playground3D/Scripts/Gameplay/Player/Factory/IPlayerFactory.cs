using CodeBase.Core.Patterns.ObjectCreation;
using UnityEngine;

namespace Modules.Base.Playground3D.Scripts.Gameplay.Player.Factory
{
    /// <summary>
    /// Factory interface for creating players with VContainer dependency injection
    /// </summary>
    public interface IPlayerFactory : IFactory<Vector3, Quaternion, GameObject>
    {
        /// <summary>
        /// Creates a new player instance at specified position and rotation with all dependencies injected
        /// </summary>
        /// <param name="position">Spawn position</param>
        /// <param name="rotation">Spawn rotation</param>
        /// <returns>Player GameObject with injected dependencies</returns>
        GameObject Create(Vector3 position, Quaternion rotation);
    }
}
