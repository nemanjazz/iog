﻿// -----------------------------------------------------------------------
// <copyright file="IEvictingStorage.cs" company="Execom">
// Copyright 2011 EXECOM d.o.o
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>
// -----------------------------------------------------------------------

namespace Execom.IOG.Storage
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class KeysEvictedEventArgs<TIdentifier>: EventArgs
    {
        public IEnumerable<TIdentifier> Keys;

        public KeysEvictedEventArgs(IEnumerable<TIdentifier> keys)
        {
            this.Keys = keys;
        }
    }

    /// <summary>
    /// Defines storage which shall evict some items due to space limitations.
    /// </summary>
    internal interface IEvictingStorage<TIdentifier>
    {
        /// <summary>
        /// Event thrown before keys are removed from storage.
        /// </summary>
        event EventHandler<KeysEvictedEventArgs<TIdentifier>> OnBeforeKeysEvicted;
    }
}
