﻿// <copyright file="Reactable.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace Velaptor.Reactables.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Defines a provider for push-based notifications.
    /// </summary>
    /// <typeparam name="TData">The data to send with the notification.</typeparam>
    public abstract class Reactable<TData> : IReactable<TData>
    {
        private readonly List<IReactor<TData>> reactors = new ();
        private bool isDisposed;

        /// <summary>
        /// Gets the list of reactors that are subscribed to this <see cref="Reactable{TData}"/>.
        /// </summary>
        public ReadOnlyCollection<IReactor<TData>> Reactors => new (this.reactors);

        /// <inheritdoc/>
        public virtual IDisposable Subscribe(IReactor<TData> reactor)
        {
            if (!this.reactors.Contains(reactor))
            {
                this.reactors.Add(reactor);
            }

            return new ReactorUnsubscriber<TData>(this.reactors, reactor);
        }

        /// <inheritdoc/>
        public abstract void PushNotification(TData data, bool unsubscribeAfterProcessing = false);

        /// <inheritdoc/>
        public void UnsubscribeAll() => this.reactors.Clear();

        /// <inheritdoc cref="IDisposable.Dispose"/>
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// <inheritdoc cref="IDisposable.Dispose"/>
        /// </summary>
        /// <param name="disposing">Disposes managed resources when <see langword="true"/>.</param>
        private void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                this.reactors.Clear();
            }

            this.isDisposed = true;
        }
    }
}
