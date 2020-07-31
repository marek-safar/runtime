// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.IO
{
    internal static partial class PersistedFiles
    {
        internal static string GetHomeDirectory()
        {
            string? home = Environment.GetEnvironmentVariable("HOME");
            return string.IsNullOrEmpty(home) ? "/" : home;
        }
    }
}
