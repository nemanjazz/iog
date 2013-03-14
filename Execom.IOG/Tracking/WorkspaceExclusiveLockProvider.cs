// -----------------------------------------------------------------------
// <copyright file="WorkspaceExclusiveLockProvider.cs" company="Execom">
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

namespace Execom.IOG.Tracking
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// Defines provider of exclusive workspace lock
    /// </summary>
    /// <author>Nenad Sabo</author>
    internal class WorkspaceExclusiveLockProvider : IDisposable
    {
        private AutoResetEvent notification = new AutoResetEvent(false);
        private bool writerLockHeld = false;
        private object sync = new object();

        /// <summary>
        /// Creates new instance of WorkspaceExclusiveLockProvider type
        /// </summary>
        public WorkspaceExclusiveLockProvider()
        {
        }       

        /// <summary>
        /// Enters the lock exclusivelly
        /// </summary>
        public void EnterLockExclusive()
        {
            bool isHeld = false;

            lock (sync)
            {
                isHeld = writerLockHeld;
                if (!isHeld)
                {
                    writerLockHeld = true;
                }
            }

            while (isHeld)
            {
                notification.WaitOne();

                lock (sync)
                {
                    isHeld = writerLockHeld;

                    if (!isHeld)
                    {
                        writerLockHeld = true;
                    }
                }
            }            
        }

        /// <summary>
        /// Releases the lock
        /// </summary>
        public void ExitLockExclusive()
        {
            lock (sync)
            {
                writerLockHeld = false;
                notification.Set();
            }
        }        

        /// <summary>
        /// Disposes the workspace. 
        /// All uncommited changes in the workspace are cancelled.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {

            if (disposing)
            {
                notification.Close();
                notification = null;
            }
        }   
    }
}
