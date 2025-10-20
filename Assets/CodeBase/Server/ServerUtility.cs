using UnityEngine;

namespace CodeBase.Server
{
    /// <summary>
    /// Утилиты для работы с сервером
    /// </summary>
    public static class ServerUtility
    {
        public static bool IsServerInstance()
        {
            //TODO
#if UNITY_EDITOR
            Debug.Log("IsServerInstance called. IMPORT NECESSARY LIBRARIES");
            // var playerRole = MultiplayerRolesManager.ActiveMultiplayerRoleMask; 
            // return playerRole == MultiplayerRoleFlags.Server;
            return false;
#elif UNITY_SERVER
            return true;
#else
            return false;
#endif
        }
    }
}

