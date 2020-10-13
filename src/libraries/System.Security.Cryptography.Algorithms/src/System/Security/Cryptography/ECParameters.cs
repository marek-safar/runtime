// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Buffers;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Cryptography.Asn1;

namespace System.Security.Cryptography
{
    /// <summary>
    /// Represents the public and private key of the specified elliptic curve.
    /// </summary>
    [UnsupportedOSPlatform("browser")]
    public struct ECParameters
    {
        /// <summary>
        /// Public point.
        /// </summary>
        public ECPoint Q;

        /// <summary>
        /// Private Key. Not always present.
        /// </summary>
        public byte[]? D;

        /// <summary>
        /// The Curve.
        /// </summary>
        public ECCurve Curve;

        /// <summary>
        /// Validate the current object.
        /// </summary>
        /// <exception cref="CryptographicException">
        ///     if the key or curve parameters are not valid for the current CurveType.
        /// </exception>
        public void Validate()
        {
            bool hasErrors = true;

            if (D is not null && Q.Y is null && Q.X is null)
                hasErrors = false;
            if (Q.Y is not null && Q.X is not null && Q.Y.Length == Q.X.Length)
                hasErrors = false;

            if (!hasErrors)
            {
                if (Curve.IsExplicit)
                {
                    // Explicit curves require D length to match Curve.Order
                    hasErrors = (D is not null && (D.Length != Curve.Order!.Length));
                }
                else if (Curve.IsNamed && Q.X is not null)
                {
                    // Named curves require D length to match Q.X and Q.Y if Q
                    // is present.
                    hasErrors = (D is not null && (D.Length != Q.X.Length));
                }
            }

            if (hasErrors)
            {
                throw new CryptographicException(SR.Cryptography_InvalidCurveKeyParameters);
            }

            Curve.Validate();
        }
    }
}
