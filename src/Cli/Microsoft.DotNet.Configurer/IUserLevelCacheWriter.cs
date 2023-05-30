﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.DotNet.Configurer
{
    public interface IUserLevelCacheWriter
    {
        string RunWithCache(string cacheKey, Func<string> getValueToCache);
        string RunWithCacheInFilePath(string cacheFilepath, Func<string> getValueToCache);
    }
}
