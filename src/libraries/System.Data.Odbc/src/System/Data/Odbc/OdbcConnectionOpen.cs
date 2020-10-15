// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data.Common;
using System.Data.ProviderBase;

namespace System.Data.Odbc
{
    internal sealed class OdbcConnectionOpen : DbConnectionInternal
    {
        // Construct from a compiled connection string
        internal OdbcConnectionOpen(OdbcConnection outerConnection, OdbcConnectionString connectionOptions)
        {
            OdbcEnvironmentHandle environmentHandle = OdbcEnvironment.GetGlobalEnvironmentHandle();
            outerConnection.ConnectionHandle = new OdbcConnectionHandle(outerConnection, connectionOptions, environmentHandle);
        }

        internal OdbcConnection OuterConnection
        {
            get
            {
                OdbcConnection? outerConnection = (OdbcConnection?)Owner;

                if (outerConnection is null)
                    throw ODBC.OpenConnectionNoOwner();

                return outerConnection;
            }
        }

        public override string ServerVersion
        {
            get
            {
                // TODO-NULLABLE: This seems like it returns null if the connection is open, whereas the docs say it should throw
                // InvalidOperationException
                return OuterConnection.Open_GetServerVersion()!;
            }
        }

        protected override void Activate()
        {
        }

        public override DbTransaction BeginTransaction(IsolationLevel isolevel)
        {
            return BeginOdbcTransaction(isolevel);
        }

        internal OdbcTransaction BeginOdbcTransaction(IsolationLevel isolevel)
        {
            return OuterConnection.Open_BeginTransaction(isolevel);
        }

        public override void ChangeDatabase(string value)
        {
            OuterConnection.Open_ChangeDatabase(value);
        }

        protected override DbReferenceCollection CreateReferenceCollection()
        {
            return new OdbcReferenceCollection();
        }

        protected override void Deactivate()
        {
            NotifyWeakReference(OdbcReferenceCollection.Closing);
        }
    }
}
