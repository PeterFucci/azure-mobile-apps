﻿// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using Microsoft.Datasync.Client;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Datasync.Integration.Test.Client.RemoteTableOfT
{
    [ExcludeFromCodeCoverage]
    public abstract class BaseOperationTest : BaseTest
    {
        protected readonly DatasyncClient client;
        protected readonly IRemoteTable<ClientMovie> soft, table;
        protected readonly DateTimeOffset startTime = DateTimeOffset.UtcNow;

        protected BaseOperationTest()
        {
            client = GetMovieClient();
            table = client.GetRemoteTable<ClientMovie>("movies");
            soft = client.GetRemoteTable<ClientMovie>("soft");
        }
    }
}
