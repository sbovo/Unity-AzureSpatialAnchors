// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

#if UNITY_ANDROID
using Microsoft.Azure.SpatialAnchors.Unity.Android.ARCore;
using NativeAnchor = Microsoft.Azure.SpatialAnchors.Unity.Android.ARCore.UnityARCoreWorldAnchorComponent;
#elif UNITY_IOS
using Microsoft.Azure.SpatialAnchors.Unity.IOS.ARKit;
using UnityEngine.XR.iOS;
using NativeAnchor = UnityEngine.XR.iOS.UnityARUserAnchorComponent;
#elif WINDOWS_UWP || UNITY_WSA
using UnityEngine.XR.WSA;
using NativeAnchor = UnityEngine.XR.WSA.WorldAnchor;
#else
using NativeAnchor = UnityEngine.MonoBehaviour;
#endif

namespace Microsoft.Azure.SpatialAnchors.Unity
{
    /// <summary>
    /// Extension methods to help manage spatial anchors.
    /// </summary>
    public static class SpatialAnchorExtensions
    {
        /// <summary>
        /// Applies the specified cloud anchor to the GameObject by
        /// creating or updating the native anchor.
        /// to match.
        /// </summary>
        /// <param name="gameObject">
        /// The <see cref="GameObject"/> where the cloud anchor should be
        /// applied.
        /// </param>
        /// <param name="cloudAnchor">
        /// The cloud anchor to apply.
        /// </param>
        /// <returns>
        /// The <see cref="NativeAnchor"/> created or updated during the
        /// operation.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="gameObject"/> or <paramref name="cloudAnchor"/>
        /// are <see langword = "null" />.
        /// </exception>
        /// <exception cref="PlatformNotSupportedException">
        /// Thrown if the current platform is not supported by the SDK.
        /// </exception>
        static public NativeAnchor ApplyCloudAnchor(this GameObject gameObject, CloudSpatialAnchor cloudAnchor)
        {
            // Validate
            if (gameObject == null) { throw new ArgumentNullException(nameof(gameObject)); }
            if (cloudAnchor == null) { throw new ArgumentNullException(nameof(cloudAnchor)); }

            // Placeholder
            NativeAnchor nativeAnchor = null;

            #if WINDOWS_UWP || UNITY_WSA
            // On UWP we can just update the pointer on any existing WorldAnchor.
            // Doing so will also automatically update the objects pose.

            // Find or create the world anchor
            nativeAnchor = gameObject.FindOrCreateNativeAnchor();

            // Update the World Anchor to use the cloud-based native anchor
            nativeAnchor.SetNativeSpatialAnchorPtr(cloudAnchor.LocalAnchor);

            #elif UNITY_IOS || UNITY_ANDROID

            // On iOS and Android we need to remove any existing native anchor,
            // move the object to the new pose, and then re-apply the native anchor.

            // Delete any existing native anchor
            gameObject.DeleteNativeAnchor();

            // Get the pose from the cloud anchor
            Pose pose = cloudAnchor.GetPose();

            // Move the GameObject to match the new pose
            gameObject.transform.position = pose.position;
            gameObject.transform.rotation = pose.rotation;

            // Add the native anchor back on
            nativeAnchor = gameObject.CreateNativeAnchor();

            #else

            throw new PlatformNotSupportedException("Unable to apply the cloud anchor. The platform is not supported.");

            #endif

            // Return the created or updated anchor
            return nativeAnchor;
        }

        /// <summary>
        /// Creates and adds the appropriate platform-specific anchor.
        /// </summary>
        /// <param name="gameObject">
        /// The <see cref="GameObject"/> where the anchor should be added.
        /// </param>
        /// <returns>
        /// The newly added native anchor.
        /// </returns>
        /// <remarks>
        /// If a native anchor is already applied, calling this method will
        /// remove it and add a new one. If this behavior is not desired,
        /// please see other options listed in the See Also section.
        /// </remarks>
        /// <seealso cref="FindNativeAnchor(GameObject)"/>
        /// <seealso cref="FindOrCreateNativeAnchor(GameObject)"/>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="gameObject"/> is <see langword = "null" />.
        /// </exception>
        static public NativeAnchor CreateNativeAnchor(this GameObject gameObject)
        {
            // Validate
            if (gameObject == null)
            {
                throw new ArgumentNullException(nameof(gameObject));
            }

            // Remove any existing native anchor
            DeleteNativeAnchor(gameObject);

            // Add the platform-specific anchor
            return gameObject.AddComponent<NativeAnchor>();
        }

        /// <summary>
        /// Removes and destroys any native anchor applied to the object.
        /// </summary>
        /// <param name="gameObject">
        /// The <see cref="GameObject"/> where the anchor should be removed.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="gameObject"/> is <see langword = "null" />.
        /// </exception>
        static public void DeleteNativeAnchor(this GameObject gameObject)
        {
            // Validate
            if (gameObject == null)
            {
                throw new ArgumentNullException(nameof(gameObject));
            }

            // Try to find the native anchor
            NativeAnchor nativeAnchor = FindNativeAnchor(gameObject);

            // If found destroy (which will also remove it)
            if (nativeAnchor != null)
            {
                GameObject.DestroyImmediate(nativeAnchor);
            }
        }

        /// <summary>
        /// Returns the native anchor, if one is applied.
        /// </summary>
        /// <param name="gameObject">
        /// The <see cref="GameObject"/> to search for the anchor.
        /// </param>
        /// <returns>
        /// The native anchor if one is found; otherwise <see langword = "null" />.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="gameObject"/> is <see langword = "null" />.
        /// </exception>
        static public NativeAnchor FindNativeAnchor(this GameObject gameObject)
        {
            // Validate
            if (gameObject == null) { throw new ArgumentNullException(nameof(gameObject)); }

            // Return anchor (if found)
            return gameObject.GetComponent<NativeAnchor>();
        }

        /// <summary>
        /// Returns any existing anchor if found; otherwise, creates and returns
        /// a new anchor.
        /// </summary>
        /// <param name="gameObject">
        /// The <see cref="GameObject"/> to search for or add the anchor.
        /// </param>
        /// <returns>
        /// The existing or newly created native anchor.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="gameObject"/> is <see langword = "null" />.
        /// </exception>
        static public NativeAnchor FindOrCreateNativeAnchor(this GameObject gameObject)
        {
            // Validate
            if (gameObject == null) { throw new ArgumentNullException(nameof(gameObject)); }

            // Try to find an existing anchor
            NativeAnchor anchor = FindNativeAnchor(gameObject);

            // If not found, create
            if (anchor == null)
            {
                anchor = CreateNativeAnchor(gameObject);
            }

            // Return the anchor
            return anchor;
        }

        /// <summary>
        /// Gets the underlying pointer for the native anchor.
        /// </summary>
        /// <param name="anchor">
        /// The native anchor to obtain the pointer for.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A Task that yields the pointer.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="anchor"/> is <see langword = "null" />.
        /// </exception>
        /// <exception cref="PlatformNotSupportedException">
        /// Thrown if the current platform is not supported by the SDK.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if a pointer could not be obtained from the native anchor.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// The operation was canceled.
        /// </exception>
        /// <remarks>
        /// This method is async because ARKit cannot provide an anchor
        /// pointer until at least one frame has been processed. If a pointer
        /// is not returned on the first request, this operation will wait for
        /// one to be created.
        /// </remarks>
#pragma warning disable CS1998 // Conditional compile statements are removing await
        static public async Task<IntPtr> GetPointerAsync(this NativeAnchor anchor, CancellationToken cancellationToken)
#pragma warning restore CS1998
        {
            // Validate
            if (anchor == null) { throw new ArgumentNullException(nameof(anchor)); }

            // Placeholder
            IntPtr ptr = IntPtr.Zero;

            #if UNITY_IOS

            ptr = anchor.GetAnchorPointer();

            // Wait for ARKit to assign an anchor ID.
            // ARKit will not assign a pointer until at least
            // one frame has been processed.
            while (ptr == IntPtr.Zero)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(10, cancellationToken);
                ptr = anchor.GetAnchorPointer();
            }

            #elif UNITY_ANDROID

            ptr = anchor.WorldAnchor.m_NativeHandle;

            #elif WINDOWS_UWP || UNITY_WSA

            ptr = anchor.GetNativeSpatialAnchorPtr();

            #else

            throw new PlatformNotSupportedException("Unable to retrieve the native anchor pointer. The platform is not supported.");

            #endif

            // Warn if the anchor didn't give us a valid value
            if (ptr == IntPtr.Zero)
            {
                throw new InvalidOperationException("Couldn't obtain a native anchor pointer");
            }

            // Return the pointer
            return ptr;
        }

        /// <summary>
        /// Gets the underlying pointer for the native anchor.
        /// </summary>
        /// <param name="anchor">
        /// The native anchor to obtain the pointer for.
        /// </param>
        /// <returns>
        /// A Task that yields the pointer.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="anchor"/> is <see langword = "null" />.
        /// </exception>
        /// <exception cref="PlatformNotSupportedException">
        /// Thrown if the current platform is not supported by the SDK.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if a pointer could not be obtained from the native anchor.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// The operation was canceled.
        /// </exception>
        /// <remarks>
        /// This method is async because ARKit cannot provide an anchor
        /// pointer until at least one frame has been processed. If a pointer
        /// is not returned on the first request, this operation will wait up
        /// to two seconds for one to be created.
        /// </remarks>
#pragma warning disable CS1998 // Conditional compile statements are removing await
        static public Task<IntPtr> GetPointerAsync(this NativeAnchor anchor)
#pragma warning restore CS1998
        {
            // Call other method with a 2 second cancellation token
            return GetPointerAsync(anchor, new CancellationTokenSource(2000).Token);
        }

        /// <summary>
        /// Gets the cloud anchors pose.
        /// </summary>
        /// <param name="cloudAnchor">
        /// The cloud anchor to return the pose for.
        /// </param>
        /// <returns><see cref="Pose"/>
        /// The anchors pose.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="gameObject"/> is <see langword = "null" />.
        /// </exception>
        /// <exception cref="PlatformNotSupportedException">
        /// Thrown if the current platform is not supported by the SDK.
        /// </exception>
        static public Pose GetPose(this CloudSpatialAnchor cloudAnchor)
        {
            // Validate
            if (cloudAnchor == null)
            {
                throw new ArgumentNullException(nameof(cloudAnchor));
            }

            // Placeholder
            Pose anchorPose = Pose.identity;

            #if UNITY_IOS
            Matrix4x4 matrix4X4 = ARKitNativeHelpers.GetAnchorTransform(cloudAnchor.LocalAnchor).ToMatrix4x4();
            anchorPose = new Pose(UnityARMatrixOps.GetPosition(matrix4X4), UnityARMatrixOps.GetRotation(matrix4X4));
            return anchorPose;
            #elif UNITY_ANDROID
            anchorPose = GoogleARCoreInternal.LifecycleManager.Instance.NativeSession.AnchorApi.GetPose(cloudAnchor.LocalAnchor);
            return anchorPose;
            #else
            throw new PlatformNotSupportedException($"Platform is not supported.");
            #endif
        }

        /// <summary>
        /// Sets the pose of the <see cref="GameObject"/>, modifying native
        /// anchors if necessary.
        /// </summary>
        /// <param name="gameObject">
        /// The <see cref="GameObject"/> to set the pose on.
        /// </param>
        /// <param name="position">
        /// The new position to set.
        /// </param>
        /// <param name="rotation">
        /// The new rotation to set.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="gameObject"/> is <see langword = "null" />.
        /// </exception>
        static public void SetPose(this GameObject gameObject, Vector3 position, Quaternion rotation)
        {
            // Validate
            if (gameObject == null) { throw new ArgumentNullException(nameof(gameObject)); }

            // Check to see if we had a native anchor
            bool hadNative = gameObject.FindNativeAnchor();

            // Delete the native anchor if it exists
            if (hadNative) { gameObject.DeleteNativeAnchor(); }

            // Update the pose
            gameObject.transform.position = position;
            gameObject.transform.rotation = rotation;

            // If there was a native anchor, add it back on
            if (hadNative) { gameObject.CreateNativeAnchor(); }
        }

        /// <summary>
        /// Sets the pose of the <see cref="GameObject"/>, modifying native
        /// anchors if necessary.
        /// </summary>
        /// <param name="gameObject">
        /// The <see cref="GameObject"/> to set the pose on.
        /// </param>
        /// <param name="pose">
        /// The pose to set.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="gameObject"/> is <see langword = "null" />.
        /// </exception>
        static public void SetPose(this GameObject gameObject, Pose pose)
        {
            SetPose(gameObject, pose.position, pose.rotation);
        }

        /// <summary>
        /// Creates or updates the <see cref="CloudSpatialAnchor"/> returned by
        /// <see cref="CloudAnchor"/> to reflect the same data as the native anchor.
        /// </summary>
        /// <param name="anchor">
        /// The native anchor to convert to a cloud anchor.
        /// </param>
        /// <returns>
        /// A Task that yields the <see cref="CloudSpatialAnchor"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="anchor"/> is <see langword = "null" />.
        /// </exception>
        /// <remarks>
        /// This method is async because ARKit needs to process at least one
        /// frame before any requested anchor is fully created. This method
        /// will return only after any underlying native processes are
        /// complete.
        /// </remarks>
        static public async Task<CloudSpatialAnchor> ToCloudAsync(this NativeAnchor anchor)
        {
            // Validate
            if (anchor == null) { throw new ArgumentNullException(nameof(anchor)); }

            // Get the native pointer
            IntPtr ptr = await anchor.GetPointerAsync();

            // Create the cloud version
            CloudSpatialAnchor cloudAnchor = new CloudSpatialAnchor();

            // Set the local pointer
            cloudAnchor.LocalAnchor = ptr;

            // Done!
            return cloudAnchor;
        }
    }
}
