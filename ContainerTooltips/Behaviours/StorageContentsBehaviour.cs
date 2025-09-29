using System;

namespace ContainerTooltips
{
    public sealed class StorageContentsBehaviour : KMonoBehaviour
    {
        private Guid statusHandle;
        private Storage? storage;
        private KSelectable? selectable;

        // private static readonly EventSystem.IntraObjectHandler<StorageContentsBehaviour> OnStorageChangeDelegate = new((component, _) => component.OnStorageChange(_));

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            storage = GetComponent<Storage>();
            selectable = GetComponent<KSelectable>();
            // Debug.Log($"[ContainerTooltips]: StorageContentsBehaviour.OnPrefabInit on {gameObject.name} storage={(storage != null ? storage.name : "<null>")} selectable={(selectable != null ? selectable.name : "<null>")}");
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            // Debug.Log($"[ContainerTooltips]: StorageContentsBehaviour.OnSpawn on {gameObject.name} storage={(storage != null ? storage.name : "<null>")} selectable={(selectable != null ? selectable.name : "<null>")}");
            RefreshStatus();
            // Subscribe((int)GameHashes.OnStorageChange, OnStorageChangeDelegate);
        }

        protected override void OnCleanUp()
        {
            base.OnCleanUp();
            Debug.Log($"[ContainerTooltips]: StorageContentsBehaviour.OnCleanUp on {gameObject.name} storage={(storage != null ? storage.name : "<null>")} selectable={(selectable != null ? selectable.name : "<null>")}");
            // Unsubscribe((int)GameHashes.OnStorageChange, OnStorageChangeDelegate);
            ClearStatus();
        }

        private void OnStorageChange(object _)
        {
            Debug.Log($"[ContainerTooltips]: StorageContentsBehaviour.OnStorageChange event received for {gameObject.name} storage={(storage != null ? storage.name : "<null>")} selectable={(selectable != null ? selectable.name : "<null>")}");
            RefreshStatus();
        }

        private void RefreshStatus()
        {
            if (statusHandle != Guid.Empty)
            {
                Debug.Log($"[ContainerTooltips]: StorageContentsBehaviour.RefreshStatus called on {gameObject.name} storage={(storage != null ? storage.name : "<null>")} selectable={(selectable != null ? selectable.name : "<null>")} handle={statusHandle}");
            }

            if (storage == null || selectable == null)
            {
                Debug.LogWarning($"[ContainerTooltips]: StorageContentsBehaviour.RefreshStatus missing storage or selectable on {gameObject.name} storage={(storage != null ? storage.name : "<null>")} selectable={(selectable != null ? selectable.name : "<null>")} handle={statusHandle}");
                return;
            }

            if (UserMod.ContentsStatusItem == null)
            {
                Debug.LogError("[ContainerTooltips]: StorageContentsBehaviour.RefreshStatus found null contentsStatusItem");
                ClearStatus();
                return;
            }

            if (statusHandle != Guid.Empty && !storage.showInUI)
            {
                Debug.Log("[ContainerTooltips]: StorageContentsBehaviour.RefreshStatus cleaning our status item since storage is now set to not show in UI");
                ClearStatus();
                return;
            }

            var newStatusHandle = selectable.ReplaceStatusItem(statusHandle, UserMod.ContentsStatusItem, storage);
            if (statusHandle != Guid.Empty)
            {
                Debug.Log($"[ContainerTooltips]: StorageContentsBehaviour.RefreshStatus applied status item on {gameObject.name}, new handle={newStatusHandle}");
            }
            statusHandle = newStatusHandle;
        }

        private void ClearStatus()
        {
            // Debug.Log($"[ContainerTooltips]: StorageContentsBehaviour.ClearStatus called on {gameObject.name} storage={(storage != null ? storage.name : "<null>")} selectable={(selectable != null ? selectable.name : "<null>")} handle={statusHandle}");
            if (statusHandle != Guid.Empty && selectable != null)
            {
                selectable.RemoveStatusItem(statusHandle, immediate: false);
                Debug.Log($"[ContainerTooltips]: StorageContentsBehaviour.ClearStatus removed status item on {gameObject.name}, handle={statusHandle}");
                statusHandle = Guid.Empty;
            }
        }
    }
}