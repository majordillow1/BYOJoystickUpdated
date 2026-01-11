using System.Collections;
using System.Reflection;
using UnityEngine;

namespace BYOJoystick.Controls.Sync
{
    public class InteractableSyncWrapper
    {
        public bool IsInteracting { get; private set; }

        private readonly VRInteractable     _interactable;
        private readonly VRInteractableSync _iSync;
        private          float              _interactTimer;
        private          float              _interactTime;

        private static readonly MethodInfo SendInteractRpcMethod = typeof(VRInteractableSync).GetMethod("SendInteractRPC", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                                                   ?? typeof(VRInteractableSync).GetMethod("SendInteractRpc", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        private static readonly MethodInfo SetExclusiveUserMethod = typeof(VRInteractableSync).GetMethod("SetExclusiveUser", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        private static readonly MethodInfo GetCurrentExclusiveUserMethod = typeof(VRInteractableSync).GetMethod("GetCurrentExclusiveUser", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        private InteractableSyncWrapper(VRInteractableSync iSync, VRInteractable interactable)
        {
            _iSync        = iSync;
            _interactable = interactable;
        }

        public static InteractableSyncWrapper Create(VRInteractable interactable)
        {
            var iSync = interactable.GetComponent<VRInteractableSync>();
            if (iSync != null)
                Plugin.Log($"Creating InteractableSyncWrapper for {interactable.name}");
            return iSync == null ? null : new InteractableSyncWrapper(iSync, interactable);
        }

        private ulong GetCurrentExclusiveUser()
        {
            if (GetCurrentExclusiveUserMethod != null)
                return (ulong)(GetCurrentExclusiveUserMethod.Invoke(_iSync, null) ?? 0UL);

            // Try reflectively by name at runtime
            var m = _iSync.GetType().GetMethod("GetCurrentExclusiveUser", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (m != null)
                return (ulong)(m.Invoke(_iSync, null) ?? 0UL);

            return 0UL;
        }

        private void CallSendInteractRpc(bool isRightCon, bool state)
        {
            if (SendInteractRpcMethod != null)
            {
                SendInteractRpcMethod.Invoke(_iSync, new object[] { isRightCon, state });
                return;
            }

            var mi = _iSync.GetType().GetMethod("SendInteractRPC", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                     ?? _iSync.GetType().GetMethod("SendInteractRpc", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (mi != null)
            {
                mi.Invoke(_iSync, new object[] { isRightCon, state });
                return;
            }

            // Best-effort no-op if method not present
        }

        private void CallSetExclusiveUser(ulong id)
        {
            if (SetExclusiveUserMethod != null)
            {
                SetExclusiveUserMethod.Invoke(_iSync, new object[] { id });
                return;
            }

            var mi = _iSync.GetType().GetMethod("SetExclusiveUser", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (mi != null)
            {
                mi.Invoke(_iSync, new object[] { id });
                return;
            }

            // no-op fallback
        }

        public bool TryInteractTimed(bool isRightCon, float time)
        {
            ulong exclusiveUser = GetCurrentExclusiveUser();
            if (IsInteracting)
            {
                if (_iSync.exclusive && exclusiveUser != 0L && exclusiveUser != BDSteamClient.mySteamID)
                {
                    StopInteracting(isRightCon);
                    return false;
                }
                SetTimer(time);
                return true;
            }

            if (!TryInteracting(isRightCon))
            {
                _interactTime = 0;
                return false;
            }

            StartCoroutine(time);
            return true;
        }

        public bool TryInteracting(bool isRightCon)
        {
            ulong exclusiveUser = GetCurrentExclusiveUser();
            if (_iSync.exclusive && exclusiveUser != 0L && exclusiveUser != BDSteamClient.mySteamID)
            {
                IsInteracting = false;
                return false;
            }

            CallSendInteractRpc(isRightCon, true);

            if (!_iSync.exclusive || !_iSync.isMine)
            {
                IsInteracting = true;
                return true;
            }

            if (exclusiveUser == 0L)
            {
                _iSync.SendRPC("SetExclusiveUser", exclusiveUser = BDSteamClient.mySteamID);
                CallSetExclusiveUser(BDSteamClient.mySteamID);
                IsInteracting = true;
                return true;
            }

            if (exclusiveUser != BDSteamClient.mySteamID)
            {
                IsInteracting = false;
                return false;
            }

            IsInteracting = true;
            return true;
        }

        public void StopInteracting(bool isRightCon)
        {
            IsInteracting = false;
            ulong exclusiveUser = GetCurrentExclusiveUser();
            CallSendInteractRpc(isRightCon, false);
            if (!_iSync.exclusive || exclusiveUser != BDSteamClient.mySteamID)
                return;

            _iSync.SendRPC("SetExclusiveUser", 0);
            CallSetExclusiveUser(0);
        }

        private void SetTimer(float time)
        {
            _interactTime  = time;
            _interactTimer = 0;
        }

        private void StartCoroutine(float time)
        {
            SetTimer(time);
            _iSync.StartCoroutine(InteractingCoroutine());
        }

        private IEnumerator InteractingCoroutine()
        {
            while (IsInteracting)
            {
                _interactTimer += Time.deltaTime;
                if (_interactTimer > _interactTime || !IsInteracting)
                {
                    StopInteracting(false);
                    _interactTimer = 0;
                    yield break;
                }

                yield return null;
            }
        }
    }
}