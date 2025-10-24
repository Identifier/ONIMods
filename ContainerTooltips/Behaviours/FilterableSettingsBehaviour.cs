using System;
using ProcGen.Noise;
using UnityEngine;

namespace ContainerTooltips
{
    public sealed class FilterableSettingsBehaviour : KMonoBehaviour
    {
        private Guid statusHandle;
        private Filterable[]? filterables;
        private TreeFilterable[]? treeFilterables;
        private FlatTagFilterable[]? flatTagFilterables;
        private KSelectable? selectable;

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            filterables = GetComponents<Filterable>();
            treeFilterables = GetComponents<TreeFilterable>();
            flatTagFilterables = GetComponents<FlatTagFilterable>();
            selectable = GetComponent<KSelectable>();
            // Debug.Log($"[ContainerTooltips]: FilterableSettingsBehaviour.OnPrefabInit on {gameObject.name} filterables={UserMod.GetNames(filterables)} treeFilterables={UserMod.GetNames(treeFilterables)} flatTagFilterables={UserMod.GetNames(flatTagFilterables)} selectable={UserMod.GetName(selectable)}");
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            // Debug.Log($"[ContainerTooltips]: FilterableSettingsBehaviour.OnSpawn on {gameObject.name} filterables={UserMod.GetNames(filterables)} treeFilterables={UserMod.GetNames(treeFilterables)} flatTagFilterables={UserMod.GetNames(flatTagFilterables)} selectable={UserMod.GetName(selectable)}");
            RefreshStatus();
        }

        protected override void OnCleanUp()
        {
            base.OnCleanUp();
            // Debug.Log($"[ContainerTooltips]: FilterableSettingsBehaviour.OnCleanUp on {gameObject.name} filterables={UserMod.GetNames(filterables)} treeFilterables={UserMod.GetNames(treeFilterables)} flatTagFilterables={UserMod.GetNames(flatTagFilterables)} selectable={UserMod.GetName(selectable)}");
            ClearStatus();
        }

        private void RefreshStatus()
        {
            if (statusHandle != Guid.Empty)
            {
                Debug.Log($"[ContainerTooltips]: FilterableSettingsBehaviour.RefreshStatus called on {gameObject.name} filterables={UserMod.GetNames(filterables)} treeFilterables={UserMod.GetNames(treeFilterables)} flatTagFilterables={UserMod.GetNames(flatTagFilterables)} selectable={UserMod.GetName(selectable)} handle={statusHandle}");
            }

            if ((filterables == null && treeFilterables == null && flatTagFilterables == null) || selectable == null)
            {
                Debug.LogWarning($"[ContainerTooltips]: FilterableSettingsBehaviour.RefreshStatus missing filterables or selectable on {gameObject.name} filterables={UserMod.GetNames(filterables)} treeFilterables={UserMod.GetNames(treeFilterables)} flatTagFilterables={UserMod.GetNames(flatTagFilterables)} selectable={UserMod.GetName(selectable)} handle={statusHandle}");
                ClearStatus();
                return;
            }

            if (UserMod.FiltersStatusItem == null)
            {
                Debug.LogError("[ContainerTooltips]: FilterableSettingsBehaviour.RefreshStatus found null FiltersStatusItem");
                ClearStatus();
                return;
            }

            var newStatusHandle = selectable.ReplaceStatusItem(statusHandle, UserMod.FiltersStatusItem, (filterables, treeFilterables, flatTagFilterables));
            if (statusHandle != Guid.Empty)
            {
                Debug.Log($"[ContainerTooltips]: FilterableSettingsBehaviour.RefreshStatus replaced status item on {gameObject.name} new handle={newStatusHandle}");
            }
            statusHandle = newStatusHandle;
        }

        private void ClearStatus()
        {
            // Debug.Log($"[ContainerTooltips]: FilterableSettingsBehaviour.ClearStatus called on {gameObject.name} filterables={UserMod.GetNames(filterables)} treeFilterables={UserMod.GetNames(treeFilterables)} flatTagFilterables={UserMod.GetNames(flatTagFilterables)} selectable={UserMod.GetName(selectable)} handle={statusHandle}");
            if (statusHandle != Guid.Empty && selectable != null)
            {
                selectable.RemoveStatusItem(statusHandle, immediate: false);
                Debug.Log($"[ContainerTooltips]: FilterableSettingsBehaviour.ClearStatus removed status item on {gameObject.name}, handle={statusHandle}");
                statusHandle = Guid.Empty;
            }
        }

    }
}
