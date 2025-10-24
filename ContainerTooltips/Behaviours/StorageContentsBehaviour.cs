using System;
using System.Linq;

namespace ContainerTooltips
{
    public sealed class StorageContentsBehaviour : KMonoBehaviour
    {
        private Guid statusHandle;
        private Storage[]? storages;
        private KSelectable? selectable;

        // private static readonly EventSystem.IntraObjectHandler<StorageContentsBehaviour> OnStorageChangeDelegate = new((component, _) => component.OnStorageChange(_));

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            storages = GetComponents<Storage>();
            selectable = GetComponent<KSelectable>();
            // Debug.Log($"[ContainerTooltips]: StorageContentsBehaviour.OnPrefabInit on {gameObject.name} storages={UserMod.GetNames(storages)} selectable={UserMod.GetName(selectable)}");
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            // Debug.Log($"[ContainerTooltips]: StorageContentsBehaviour.OnSpawn on {gameObject.name} storages={UserMod.GetNames(storages)} selectable={UserMod.GetName(selectable)}");
            RefreshStatus();
            // Subscribe((int)GameHashes.OnStorageChange, OnStorageChangeDelegate);
        }

        protected override void OnCleanUp()
        {
            base.OnCleanUp();
            // Debug.Log($"[ContainerTooltips]: StorageContentsBehaviour.OnCleanUp on {gameObject.name} storages={UserMod.GetNames(storages)} selectable={UserMod.GetName(selectable)}");
            // Unsubscribe((int)GameHashes.OnStorageChange, OnStorageChangeDelegate);
            ClearStatus();
        }

        // private void OnStorageChange(object _)
        // {
        //     Debug.Log($"[ContainerTooltips]: StorageContentsBehaviour.OnStorageChange event received for {gameObject.name} storages={UserMod.GetNames(storages)} selectable={UserMod.GetName(selectable)}");
        //     RefreshStatus();
        // }

        private void RefreshStatus()
        {
            if (statusHandle != Guid.Empty)
            {
                Debug.Log($"[ContainerTooltips]: StorageContentsBehaviour.RefreshStatus called on {gameObject.name} storages={UserMod.GetNames(storages)} selectable={UserMod.GetName(selectable)} handle={statusHandle}");
            }

            if (storages == null || storages.Length == 0 || selectable == null)
            {
                Debug.LogWarning($"[ContainerTooltips]: StorageContentsBehaviour.RefreshStatus missing storage or selectable on {gameObject.name} storages={UserMod.GetNames(storages)} selectable={UserMod.GetName(selectable)} handle={statusHandle}");
                return;
            }

            if (UserMod.ContentsStatusItem == null)
            {
                Debug.LogError("[ContainerTooltips]: StorageContentsBehaviour.RefreshStatus found null contentsStatusItem");
                ClearStatus();
                return;
            }

            if (statusHandle != Guid.Empty && storages?.Any(s => s?.showInUI == true) != true)
            {
                Debug.Log("[ContainerTooltips]: StorageContentsBehaviour.RefreshStatus cleaning our status item since no storage is set to show in UI");
                ClearStatus();
                return;
            }

            var newStatusHandle = selectable.ReplaceStatusItem(statusHandle, UserMod.ContentsStatusItem, storages);
            if (statusHandle != Guid.Empty)
            {
                Debug.Log($"[ContainerTooltips]: StorageContentsBehaviour.RefreshStatus replaced status item on {gameObject.name}, new handle={newStatusHandle}");
            }
            statusHandle = newStatusHandle;
        }

        private void ClearStatus()
        {
            // Debug.Log($"[ContainerTooltips]: StorageContentsBehaviour.ClearStatus called on {gameObject.name} storages={UserMod.GetNames(storages)} selectable={UserMod.GetName(selectable)} handle={statusHandle}");
            if (statusHandle != Guid.Empty && selectable != null)
            {
                selectable.RemoveStatusItem(statusHandle, immediate: false);
                Debug.Log($"[ContainerTooltips]: StorageContentsBehaviour.ClearStatus removed status item on {gameObject.name}, handle={statusHandle}");
                statusHandle = Guid.Empty;
            }
        }
    }
}