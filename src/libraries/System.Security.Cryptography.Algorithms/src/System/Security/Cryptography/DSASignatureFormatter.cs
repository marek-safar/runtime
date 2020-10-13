// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.Versioning;
using Internal.Cryptography;

namespace System.Security.Cryptography
{
    [UnsupportedOSPlatform("browser")]
    public class DSASignatureFormatter : AsymmetricSignatureFormatter
    {
        private DSA? _dsaKey;

        public DSASignatureFormatter() { }

        public DSASignatureFormatter(AsymmetricAlgorithm key) : this()
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));

            _dsaKey = (DSA)key;
        }

        public override void SetKey(AsymmetricAlgorithm key)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));

            _dsaKey = (DSA)key;
        }

        public override void SetHashAlgorithm(string strName)
        {
            if (strName.ToUpperInvariant() != HashAlgorithmNames.SHA1)
            {
                // To match desktop, throw here
                throw new CryptographicUnexpectedOperationException(SR.Cryptography_InvalidOperation);
            }
        }

        public override byte[] CreateSignature(byte[] rgbHash)
        {
            if (rgbHash is null)
                throw new ArgumentNullException(nameof(rgbHash));
            if (_dsaKey is null)
                throw new CryptographicUnexpectedOperationException(SR.Cryptography_MissingKey);

            return _dsaKey.CreateSignature(rgbHash);
        }
    }
}
