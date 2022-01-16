﻿#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System.Threading;
using Appalachia.Utility.Async.Triggers;
using UnityEngine;

namespace Appalachia.Utility.Async
{
    public static class AppaTaskCancellationExtensions
    {
        /// <summary>This CancellationToken is canceled when the MonoBehaviour will be destroyed.</summary>
        public static CancellationToken GetCancellationTokenOnDestroy(this GameObject gameObject)
        {
            return gameObject.GetAsyncDestroyTrigger().CancellationToken;
        }

        /// <summary>This CancellationToken is canceled when the MonoBehaviour will be destroyed.</summary>
        public static CancellationToken GetCancellationTokenOnDestroy(this Component component)
        {
            return component.GetAsyncDestroyTrigger().CancellationToken;
        }
    }
}

namespace Appalachia.Utility.Async.Triggers
{
    public static partial class AsyncTriggerExtensions
    {
        // Util.

        private static T GetOrAddComponent<T>(GameObject gameObject)
            where T : Component
        {
#if UNITY_2019_2_OR_NEWER
            if (!gameObject.TryGetComponent<T>(out var component))
            {
                component = gameObject.AddComponent<T>();
            }
#else
            var component = gameObject.GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
            }
#endif

            return component;
        }

        // Special for single operation.

        /// <summary>This function is called when the MonoBehaviour will be destroyed.</summary>
        public static AppaTask OnDestroyAsync(this GameObject gameObject)
        {
            return gameObject.GetAsyncDestroyTrigger().OnDestroyAsync();
        }

        /// <summary>This function is called when the MonoBehaviour will be destroyed.</summary>
        public static AppaTask OnDestroyAsync(this Component component)
        {
            return component.GetAsyncDestroyTrigger().OnDestroyAsync();
        }

        public static AppaTask StartAsync(this GameObject gameObject)
        {
            return gameObject.GetAsyncStartTrigger().StartAsync();
        }

        public static AppaTask StartAsync(this Component component)
        {
            return component.GetAsyncStartTrigger().StartAsync();
        }

        public static AppaTask AwakeAsync(this GameObject gameObject)
        {
            return gameObject.GetAsyncAwakeTrigger().AwakeAsync();
        }

        public static AppaTask AwakeAsync(this Component component)
        {
            return component.GetAsyncAwakeTrigger().AwakeAsync();
        }
    }
}