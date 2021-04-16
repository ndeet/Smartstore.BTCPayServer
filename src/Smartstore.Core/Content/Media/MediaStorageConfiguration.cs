﻿using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Smartstore.Data;
using Smartstore.Engine;
using Smartstore.Utilities;

namespace Smartstore.Core.Content.Media
{
    public class MediaStorageConfiguration : IMediaStorageConfiguration
    {
        private readonly IApplicationContext _appContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MediaStorageConfiguration(IApplicationContext appContext, IHttpContextAccessor httpContextAccessor)
        {
            _appContext = appContext;
            _httpContextAccessor = httpContextAccessor;

            IsCloudStorage = false;
            PublicPath = GetPublicPath(appContext);
            StoragePath = GetStoragePath(appContext, out var pathIsAbsolute);
            StoragePathIsAbsolute = pathIsAbsolute;
            RootPath = pathIsAbsolute 
                ? StoragePath.Replace('/', '\\')
                : Path.Combine(appContext.ContentRoot.Root, StoragePath.Replace('/', '\\'));
        }

        public string PublicPath { get; }
        public string StoragePath { get; }
        public string RootPath { get; }
        public bool StoragePathIsAbsolute { get; }
        public bool IsCloudStorage { get; }

        private static string GetPublicPath(IApplicationContext appContext)
        {
            var path = appContext.AppConfiguration.MediaPublicPath?.Trim().NullEmpty() ?? "media";

            if (path.IsWebUrl())
            {
                throw new NotSupportedException($"Fully qualified URLs are not supported for the {TypeHelper.NameOf<SmartConfiguration>(x => x.MediaPublicPath)} setting.");
            }

            return path.TrimStart('~', '/').Replace('\\', '/').ToLower().EnsureEndsWith('/');
        }

        private static string GetStoragePath(IApplicationContext appContext, out bool pathIsAbsolute)
        {
            var path = appContext.AppConfiguration.MediaStoragePath?.Trim().NullEmpty();
            if (path == null)
            {
                path = "App_Data/Tenants/" + DataSettings.Instance.TenantName + "/Media";
            }

            pathIsAbsolute = PathHelper.IsAbsolutePhysicalPath(path);

            return path;
        }

        //private static string GetRootPath(IApplicationContext appContext)
        //{
        //    var storagePath =
        //        appContext.AppConfiguration.MediaStoragePath.NullEmpty() ??
        //        "\\App_Data\\Tenants\\" + DataSettings.Instance.TenantName + "\\Media";

        //    if (storagePath == null)
        //    {
        //        return Path.Combine(appContext.TenantRoot.Root, "Media");
        //    }

        //    return storagePath;
        //}
    }
}
