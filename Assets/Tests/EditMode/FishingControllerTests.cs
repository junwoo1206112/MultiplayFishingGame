using System.Reflection;
using MultiplayFishing.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace MultiplayFishing.Tests
{
    public class FishingControllerTests
    {
        [Test]
        public void EnsureInitializedWithoutParentRepairKeepsDetachedHookInPlace()
        {
            GameObject controllerObject = new GameObject("FishingControllerTest");
            GameObject tipParentObject = new GameObject("TipParent");
            GameObject tipObject = new GameObject("TipPoint");
            GameObject hookObject = new GameObject("HookPoint");

            try
            {
                FishingController controller = controllerObject.AddComponent<FishingController>();
                tipObject.transform.SetParent(tipParentObject.transform, false);
                tipObject.transform.localPosition = new Vector3(1f, 2f, 3f);
                hookObject.transform.position = new Vector3(10f, 11f, 12f);

                SetPrivateField(controller, "tipPoint", tipObject.transform);
                SetPrivateField(controller, "hookPoint", hookObject.transform);

                MethodInfo ensureInitialized = typeof(FishingController).GetMethod(
                    "EnsureInitialized",
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(bool) },
                    null);

                Assert.That(ensureInitialized, Is.Not.Null);

                Vector3 originalPosition = hookObject.transform.position;
                ensureInitialized.Invoke(controller, new object[] { false });

                Assert.That(hookObject.transform.parent, Is.Null);
                Assert.That(hookObject.transform.position, Is.EqualTo(originalPosition));
            }
            finally
            {
                Object.DestroyImmediate(controllerObject);
                Object.DestroyImmediate(tipParentObject);
                Object.DestroyImmediate(hookObject);
            }
        }

        private static void SetPrivateField<T>(FishingController controller, string fieldName, T value)
        {
            FieldInfo field = typeof(FishingController).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(field, Is.Not.Null);
            field.SetValue(controller, value);
        }
    }
}
