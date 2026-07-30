using NUnit.Framework;
using MCPForUnity.Editor.Services;
using MCPForUnity.Editor.Constants;
using UnityEditor;

namespace MCPForUnityTests.Editor.Services
{
    /// <summary>
    /// Unit tests for EditorConfigurationCache.
    /// </summary>
    [TestFixture]
    public class EditorConfigurationCacheTests
    {
        private bool _originalDebugLogs;
        private string _originalUvxPath;

        [SetUp]
        public void SetUp()
        {
            // Save original values
            _originalDebugLogs = EditorPrefs.GetBool(EditorPrefKeys.DebugLogs, false);
            _originalUvxPath = EditorPrefs.GetString(EditorPrefKeys.UvxPathOverride, string.Empty);

            // Refresh cache to ensure clean state
            EditorConfigurationCache.Instance.Refresh();
        }

        [TearDown]
        public void TearDown()
        {
            // Restore original values
            EditorPrefs.SetBool(EditorPrefKeys.DebugLogs, _originalDebugLogs);
            EditorPrefs.SetString(EditorPrefKeys.UvxPathOverride, _originalUvxPath);

            // Refresh cache
            EditorConfigurationCache.Instance.Refresh();
        }

        #region Singleton Tests

        [Test]
        public void Instance_ReturnsSameInstance()
        {
            // Act
            var instance1 = EditorConfigurationCache.Instance;
            var instance2 = EditorConfigurationCache.Instance;

            // Assert
            Assert.AreSame(instance1, instance2, "Should return the same singleton instance");
        }

        [Test]
        public void Instance_IsNotNull()
        {
            // Assert
            Assert.IsNotNull(EditorConfigurationCache.Instance);
        }

        #endregion

        #region Read Tests


        [Test]
        public void DebugLogs_ReturnsEditorPrefsValue()
        {
            // Arrange
            EditorPrefs.SetBool(EditorPrefKeys.DebugLogs, true);
            EditorConfigurationCache.Instance.Refresh();

            // Assert
            Assert.IsTrue(EditorConfigurationCache.Instance.DebugLogs);
        }

        [Test]
        public void UvxPathOverride_ReturnsEditorPrefsValue()
        {
            // Arrange
            string testPath = "/custom/path/to/uvx";
            EditorPrefs.SetString(EditorPrefKeys.UvxPathOverride, testPath);
            EditorConfigurationCache.Instance.Refresh();

            // Assert
            Assert.AreEqual(testPath, EditorConfigurationCache.Instance.UvxPathOverride);
        }

        #endregion

        #region Write Tests


        [Test]
        public void SetDebugLogs_UpdatesCacheAndEditorPrefs()
        {
            // Act
            EditorConfigurationCache.Instance.SetDebugLogs(true);

            // Assert
            Assert.IsTrue(EditorConfigurationCache.Instance.DebugLogs);
            Assert.IsTrue(EditorPrefs.GetBool(EditorPrefKeys.DebugLogs, false));
        }

        [Test]
        public void SetUvxPathOverride_UpdatesCacheAndEditorPrefs()
        {
            // Arrange
            string testPath = "/test/uvx/path";

            // Act
            EditorConfigurationCache.Instance.SetUvxPathOverride(testPath);

            // Assert
            Assert.AreEqual(testPath, EditorConfigurationCache.Instance.UvxPathOverride);
            Assert.AreEqual(testPath, EditorPrefs.GetString(EditorPrefKeys.UvxPathOverride, string.Empty));
        }

        [Test]
        public void SetUvxPathOverride_NullBecomesEmptyString()
        {
            // Act
            EditorConfigurationCache.Instance.SetUvxPathOverride(null);

            // Assert
            Assert.AreEqual(string.Empty, EditorConfigurationCache.Instance.UvxPathOverride);
        }

        #endregion

        #region Change Notification Tests


        [Test]
        public void SetSameValue_DoesNotFireOnConfigurationChanged()
        {
            // Arrange
            int eventCount = 0;
            EditorConfigurationCache.Instance.OnConfigurationChanged += (key) => eventCount++;
            bool currentValue = EditorConfigurationCache.Instance.DebugLogs;

            // Act - set same value
            EditorConfigurationCache.Instance.SetDebugLogs(currentValue);

            // Assert - no event fired
            Assert.AreEqual(0, eventCount, "Should not fire event when value doesn't change");

            // Cleanup
            EditorConfigurationCache.Instance.OnConfigurationChanged -= (key) => eventCount++;
        }

        #endregion

        #region InvalidateKey Tests

        [Test]
        public void InvalidateKey_RefreshesSingleValue()
        {
            // Arrange
            EditorConfigurationCache.Instance.SetDebugLogs(false);
            Assert.IsFalse(EditorConfigurationCache.Instance.DebugLogs);

            // Directly modify EditorPrefs (simulating external change)
            EditorPrefs.SetBool(EditorPrefKeys.DebugLogs, true);

            // Act
            EditorConfigurationCache.Instance.InvalidateKey(nameof(EditorConfigurationCache.DebugLogs));

            // Assert
            Assert.IsTrue(EditorConfigurationCache.Instance.DebugLogs);
        }

        [Test]
        public void InvalidateKey_FiresOnConfigurationChanged()
        {
            // Arrange
            string changedKey = null;
            EditorConfigurationCache.Instance.OnConfigurationChanged += (key) => changedKey = key;

            // Act
            EditorConfigurationCache.Instance.InvalidateKey(nameof(EditorConfigurationCache.DebugLogs));

            // Assert
            Assert.AreEqual(nameof(EditorConfigurationCache.DebugLogs), changedKey);

            // Cleanup
            EditorConfigurationCache.Instance.OnConfigurationChanged -= (key) => changedKey = key;
        }

        #endregion

        #region Refresh Tests

        [Test]
        public void Refresh_UpdatesAllCachedValues()
        {
            // Arrange - directly set EditorPrefs
            EditorPrefs.SetBool(EditorPrefKeys.DebugLogs, true);
            EditorPrefs.SetString(EditorPrefKeys.UvxPathOverride, "/refreshed/path");

            // Act
            EditorConfigurationCache.Instance.Refresh();

            // Assert
            Assert.IsTrue(EditorConfigurationCache.Instance.DebugLogs);
            Assert.AreEqual("/refreshed/path", EditorConfigurationCache.Instance.UvxPathOverride);
        }

        #endregion
    }
}
