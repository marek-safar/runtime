// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;

namespace System.Security.Cryptography.Pkcs
{
    public sealed class PublicKeyInfo
    {
        internal PublicKeyInfo(AlgorithmIdentifier algorithm, byte[] keyValue)
        {
            Debug.Assert(algorithm is not null);
            Debug.Assert(keyValue is not null);

            Algorithm = algorithm;
            KeyValue = keyValue;
        }

        public AlgorithmIdentifier Algorithm { get; }

        public byte[] KeyValue { get; }
    }
}
