using CodeSmile.BetterNetcode;
using CodeSmile.Statemachine.Netcode;
using System;
using System.Linq;
using Unity.Multiplayer.Playmode;
using UnityEngine;

namespace CodeSmileEditor.BetterNetcode
{
    public class MppmLauncher : MonoBehaviour
    {
#if UNITY_EDITOR
        void Start()
        {
            var mppmRole = GetNetworkRoleFromMppmTags();
            Debug.Log("MPPM NetworkRole is: " + mppmRole);

            switch (mppmRole)
            {
                case NetcodeRole.Client:
                    Components.NetcodeState.RequestStartClient();
                    break;
                case NetcodeRole.Host:
                    Components.NetcodeState.RequestStartHost();
                    break;
                case NetcodeRole.Server:
                    Components.NetcodeState.RequestStartServer();
                    break;
                case NetcodeRole.None:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private NetcodeRole GetNetworkRoleFromMppmTags()
        {
            var tags = CurrentPlayer.ReadOnlyTags();

            var roleCount = Enum.GetValues(typeof(NetcodeRole)).Length;
            for (var roleIndex = 0; roleIndex < roleCount; roleIndex++)
                if (tags.Contains(((NetcodeRole)roleIndex).ToString()))
                    return (NetcodeRole)roleIndex;

            return NetcodeRole.None;
        }
#endif
    }
}
