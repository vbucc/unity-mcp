using UnityEditor;
using UnityEngine;

namespace MCPForUnity.Editor.Helpers
{
    /// <summary>
    /// Writes tool edits to their backing store.
    ///
    /// EditorUtility.SetDirty only flags in-memory state. A scene object needs nothing more —
    /// SetDirty marks its owning scene, and a scene save picks the change up. An asset has no
    /// such owner: without an AssetDatabase write the edit is discarded on reimport, on domain
    /// reload, and on a -batchmode exit, which has no user and no save prompt. A tool that
    /// returns success after only dirtying an asset is therefore reporting a write that never
    /// happened.
    /// </summary>
    public static class EditorPersistence
    {
        /// <summary>
        /// Marks an edited object dirty and writes it to disk when it is a persisted asset.
        /// </summary>
        /// <remarks>
        /// SaveAssetIfDirty writes only this asset rather than flushing every dirty asset in the
        /// project. Unlike AssetDatabase.SaveAssets() it does not fire
        /// AssetModificationProcessor.OnWillSaveAssets, so checkout-on-save VCS hooks will not
        /// see these writes.
        /// </remarks>
        public static void PersistAsset(Object edited)
        {
            EditorUtility.SetDirty(edited);
            if (EditorUtility.IsPersistent(edited))
            {
                AssetDatabase.SaveAssetIfDirty(edited);
            }
        }

        /// <summary>
        /// Marks an edited component dirty and, when it belongs to a prefab asset, writes that
        /// prefab to disk. A component in a scene needs no explicit write.
        /// </summary>
        public static void PersistComponent(Component component)
        {
            EditorUtility.SetDirty(component);
            if (EditorUtility.IsPersistent(component))
            {
                AssetDatabase.SaveAssets();
            }
        }
    }
}
