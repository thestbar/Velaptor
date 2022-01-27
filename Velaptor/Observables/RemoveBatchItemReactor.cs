﻿// <copyright file="RemoveBatchItemReactor.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace Velaptor.Observables
{
    // ReSharper disable RedundantNameQualifier
    using System.Diagnostics.CodeAnalysis;
    using Velaptor.Observables.Core;
    using Velaptor.Observables.ObservableData;

    // ReSharper restore RedundantNameQualifier

    /// <summary>
    /// Creates an observable to send push notifications to remove items from a batching service.
    /// </summary>
    internal class RemoveBatchItemReactor : Reactor<RemoveBatchItemData>
    {
        /// <summary>
        /// Sends a push notification to remove a batch item from a batch service.
        /// </summary>
        /// <param name="data">The ID of the batch item to remove.</param>
        [SuppressMessage(
            "ReSharper",
            "ForCanBeConvertedToForeach",
            Justification = "Required for proper observable operation.")]
        public override void PushNotification(RemoveBatchItemData data)
        {
            /* Work from the end to the beginning of the list
               just in case the observable is disposed(removed)
               in the OnNext() method.
             */
            for (var i = Observers.Count - 1; i >= 0; i--)
            {
                Observers[i].OnNext(data);
            }
        }
    }
}
