﻿// Licensed to the Swastika I/O Foundation under one or more agreements.
// The Swastika I/O Foundation licenses this file to you under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using Swastika.Domain.Core.Models;
using System;
using System.Collections.Generic;

namespace Swastika.Extension.Core.Interfaces
{
    /// <summary>
    /// Service Interface
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TView">The type of the view.</typeparam>
    public interface IService<T, TView>
        where T : class
        where TView : class
    {
        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns></returns>
        IEnumerable<TView> GetAll();

        /// <summary>
        /// Gets all history.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        IList<HistoryData<T>> GetAllHistory(Guid id);

        /// <summary>
        /// Gets all history.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        IList<HistoryData<T>> GetAllHistory(int id);

        /// <summary>
        /// Gets all history.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        IList<HistoryData<T>> GetAllHistory(string id);

        /// <summary>
        /// Gets the by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        TView GetById(Guid id);

        /// <summary>
        /// Registers the specified customer view model.
        /// </summary>
        /// <param name="TView">The customer view model.</param>
        void Register(TView TView);

        /// <summary>
        /// Removes the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        void Remove(Guid id);

        /// <summary>
        /// Updates the specified customer view model.
        /// </summary>
        /// <param name="TView">The customer view model.</param>
        void Update(TView TView);
    }
}
