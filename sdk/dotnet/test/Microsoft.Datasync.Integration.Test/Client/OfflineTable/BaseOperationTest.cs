﻿// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using FluentAssertions.Common;
using FluentAssertions.Specialized;
using Microsoft.Datasync.Client;
using Microsoft.Datasync.Client.Offline;
using Microsoft.Datasync.Client.SQLiteStore;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Datasync.Integration.Test.Client.OfflineTable
{
    [ExcludeFromCodeCoverage]
    public abstract class BaseOperationTest : BaseTest, IDisposable
    {
        protected readonly ITestOutputHelper Logger;
        protected readonly string filename;
        protected readonly string connectionString;
        protected readonly OfflineSQLiteStore store;
        protected readonly DatasyncClient client;
        protected IOfflineTable? soft, table;

        protected JObject MovieDefinition = new()
        {
            { "id", string.Empty },
            { "deleted", false },
            { "updatedAt", DateTimeOffset.UtcNow },
            { "version", string.Empty },
            { "bestPictureWinner", false },
            { "duration", 0 },
            { "rating", string.Empty },
            { "releaseDate", DateTimeOffset.UtcNow },
            { "title", string.Empty },
            { "year", 0 }
        };

        protected BaseOperationTest(ITestOutputHelper logger)
        {
            Logger = logger;
            filename = Path.GetTempFileName();
            connectionString = new UriBuilder(filename) { Query = "?mode=rwc" }.Uri.ToString();
            store = new OfflineSQLiteStore(connectionString);
            store.DefineTable("movies", MovieDefinition);
            store.DefineTable("soft", MovieDefinition);
            client = GetMovieClient(store: store);
        }

        protected async Task InitializeAsync(bool pullItems = true)
        {
            await client.InitializeOfflineStoreAsync();

            table = client.GetOfflineTable("movies");
            soft = client.GetOfflineTable("soft");

            if (pullItems)
            {
                await table.PullItemsAsync("", new PullOptions());
                await soft.PullItemsAsync("", new PullOptions());
            }
        }

        protected static void AssertSystemPropertiesMatch(EFMovie expected, JObject actual)
        {
            Assert.Equal(expected.Id, actual.Value<string>("id"));
            Assert.Equal(expected.Deleted, actual.Value<bool>("deleted"));
            Assert.Equal(Convert.ToBase64String(expected.Version), actual.Value<string>("version"));
            var actualUpdatedAt = actual.Value<DateTime>("updatedAt").ToDateTimeOffset();
            Assert.Equal(expected.UpdatedAt.ToUnixTimeMilliseconds(), actualUpdatedAt.ToUnixTimeMilliseconds());
        }

        protected async Task ModifyServerVersionAsync(string id)
        {
            var remoteTable = client.GetRemoteTable<ClientMovie>("movies");
            var item = await remoteTable!.GetItemAsync(id);
            var temp = item.Title;
            item.Title = "Foo";
            await remoteTable!.ReplaceItemAsync(item);
            item.Title = temp;
            await remoteTable!.ReplaceItemAsync(item);
        }

        protected static void AssertVersionMatches(byte[] expected, string actual)
        {
            string expstr = Convert.ToBase64String(expected);
            Assert.Equal(expstr, actual);
        }

        public void Dispose()
        {
            store.DbConnection.connection.Close();
            File.Delete(filename);
            GC.SuppressFinalize(this);
        }
    }
}
