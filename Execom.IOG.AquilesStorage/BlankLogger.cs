// -----------------------------------------------------------------------
// <copyright file="BlankLogger.cs" company="Execom">
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Execom.IOG.AquilesStorage
{
    /// <summary>
    /// Implementation of Aquiles logger which does not perform any logging.
    /// It is recommended to to use this logger for production, because of performance.
    /// </summary>
    /// <author>Nenad Sabo</author>
    public class BlankLogger : Aquiles.Core.Diagnostics.ILogger
    {

        public void Debug(string className, object message, Exception exception)
        {
        }

        public void Debug(string className, object message)
        {
        }

        public void Error(string className, object message, Exception exception)
        {
        }

        public void Error(string className, object message)
        {
        }

        public void Fatal(string className, object message, Exception exception)
        {
        }

        public void Fatal(string className, object message)
        {
        }

        public void Info(string className, object message, Exception exception)
        {
        }

        public void Info(string className, object message)
        {
        }

        public void Trace(string className, object message, Exception exception)
        {
        }

        public void Trace(string className, object message)
        {
        }

        public void Warn(string className, object message, Exception exception)
        {
        }

        public void Warn(string className, object message)
        {
        }
    }
}
