using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using MCPForUnity.Editor.Helpers;
using MCPForUnity.Editor.Tools;
using static MCPForUnityTests.Editor.TestUtilities;

namespace MCPForUnityTests.Editor.Tools
{
    /// <summary>
    /// manage_material's write actions must persist, not just dirty. EditorUtility.SetDirty only
    /// flags in-memory state; without an AssetDatabase save the .mat on disk never changes, and a
    /// -batchmode editor (the harness, CI) has no user and no quit prompt to flush it — so the edit
    /// is silently discarded after a success response.
    ///
    /// The asset tests assert on EditorUtility.IsDirty, which is exactly "was it written": a saved
    /// asset is no longer dirty. That covers materials edited by path and a renderer living on a
    /// prefab asset.
    ///
    /// The two scene tests are behaviour guards rather than regression tests — SetDirty on a scene
    /// component already marks its owning scene, so they pass both before and after the fix. They
    /// pin that down so a future change to the persistence helper cannot quietly break the path
    /// that does work.
    /// </summary>
    public class ManageMaterialPersistenceTests
    {
        private const string TempRoot = "Assets/Temp/ManageMaterialPersistenceTests";
        private string _matPath;
        private GameObject _go;

        // Built-in's Standard shader uses "_Color"; URP/HDRP lit shaders use "_BaseColor".
        private static string MainColorProperty()
        {
            return RenderPipelineUtility.GetActivePipeline() == RenderPipelineUtility.PipelineKind.BuiltIn
                ? "_Color"
                : "_BaseColor";
        }

        [SetUp]
        public void SetUp()
        {
            EnsureFolder(TempRoot);
            _matPath = $"{TempRoot}/PersistTest.mat";

            var create = ToJObject(ManageMaterial.HandleCommand(new JObject
            {
                ["action"] = "create",
                ["materialPath"] = _matPath,
                ["shader"] = "Standard"
            }));
            Assert.IsTrue(create.Value<bool>("success"), create.ToString());
        }

        [TearDown]
        public void TearDown()
        {
            // Reset the scene first: it closes the test scene and discards its dirty state before
            // the asset delete removes the .unity file underneath it. A dirty scene left behind
            // makes UTF's SaveModifiedSceneTask pop a blocking native Save dialog and wedge an
            // unattended editor (see RunTestsDirtyUntitledSceneTests).
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            _go = null;

            if (AssetDatabase.IsValidFolder(TempRoot))
            {
                AssetDatabase.DeleteAsset(TempRoot);
            }
            CleanupEmptyParentFolders(TempRoot);
        }

        private GameObject NewRendererObject()
        {
            _go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _go.name = "ManageMaterialPersistenceTarget";
            // Creating the primitive dirties the scene; clear it so the assertion measures the tool.
            EditorSceneManager.SaveScene(_go.scene, $"{TempRoot}/PersistScene.unity");
            return _go;
        }

        [Test]
        public void SetMaterialShaderProperty_WritesMaterialToDisk()
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(_matPath);
            Assert.IsNotNull(mat);

            var result = ToJObject(ManageMaterial.HandleCommand(new JObject
            {
                ["action"] = "set_material_shader_property",
                ["materialPath"] = _matPath,
                ["property"] = "_Metallic",
                ["value"] = 0.75f
            }));

            Assert.IsTrue(result.Value<bool>("success"), result.ToString());
            Assert.AreEqual(0.75f, mat.GetFloat("_Metallic"), 0.0001f);
            Assert.IsFalse(EditorUtility.IsDirty(mat),
                "Material is still dirty after a success response — the edit was never written to disk.");
        }

        [Test]
        public void SetMaterialColor_WritesMaterialToDisk()
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(_matPath);
            Assert.IsNotNull(mat);

            var result = ToJObject(ManageMaterial.HandleCommand(new JObject
            {
                ["action"] = "set_material_color",
                ["materialPath"] = _matPath,
                ["property"] = MainColorProperty(),
                ["color"] = new JArray { 0f, 1f, 0f, 1f }
            }));

            Assert.IsTrue(result.Value<bool>("success"), result.ToString());
            Assert.AreEqual(Color.green, mat.GetColor(MainColorProperty()));
            Assert.IsFalse(EditorUtility.IsDirty(mat),
                "Material is still dirty after a success response — the edit was never written to disk.");
        }

        [Test]
        public void AssignMaterialToRenderer_PrefabAsset_WritesPrefabToDisk()
        {
            var source = GameObject.CreatePrimitive(PrimitiveType.Cube);
            string prefabPath = $"{TempRoot}/Target.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(source, prefabPath);
            Object.DestroyImmediate(source);
            Assert.IsNotNull(prefab);

            var renderer = prefab.GetComponent<MeshRenderer>();
            Assert.IsFalse(EditorUtility.IsDirty(renderer), "precondition: freshly saved prefab is clean");

            var result = ToJObject(ManageMaterial.HandleCommand(new JObject
            {
                ["action"] = "assign_material_to_renderer",
                ["target"] = prefabPath,
                ["materialPath"] = _matPath,
                ["slot"] = 0
            }));

            Assert.IsTrue(result.Value<bool>("success"), result.ToString());
            Assert.IsFalse(EditorUtility.IsDirty(renderer),
                "Prefab renderer is still dirty after a success response — the assignment was never written to disk.");
        }

        [Test]
        public void AssignMaterialToRenderer_SceneObject_MarksSceneDirty()
        {
            var go = NewRendererObject();
            Assert.IsFalse(go.scene.isDirty, "precondition: scene starts clean");

            var result = ToJObject(ManageMaterial.HandleCommand(new JObject
            {
                ["action"] = "assign_material_to_renderer",
                ["target"] = go.name,
                ["materialPath"] = _matPath,
                ["slot"] = 0
            }));

            Assert.IsTrue(result.Value<bool>("success"), result.ToString());
            Assert.IsTrue(go.scene.isDirty,
                "Scene was not marked dirty — the assignment cannot be saved and is lost on scene reload.");
        }

        [Test]
        public void SetRendererColor_PropertyBlock_MarksSceneDirty()
        {
            var go = NewRendererObject();
            Assert.IsFalse(go.scene.isDirty, "precondition: scene starts clean");

            var result = ToJObject(ManageMaterial.HandleCommand(new JObject
            {
                ["action"] = "set_renderer_color",
                ["target"] = go.name,
                ["color"] = new JArray { 1f, 0f, 0f, 1f },
                ["mode"] = "property_block"
            }));

            Assert.IsTrue(result.Value<bool>("success"), result.ToString());
            Assert.IsTrue(go.scene.isDirty,
                "Scene was not marked dirty — the property block is lost on scene reload.");
        }
    }
}
