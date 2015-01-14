﻿using Composite.Core.IO;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Composite.Core.WebClient
{
    internal static class BuildManagerHelper
    {
        private static volatile bool _preloadingInitialed;

        /// <summary>
        /// Preloading (compiling) all the controls. Speeds up first time editing in console.
        /// </summary>
        public static void InitializeControlPreLoading()
        {
            if (!_preloadingInitialed)
            {
                Task.Factory.StartNew(LoadAllControls);
            }
        }

        private static void LoadAllControls()
        {
            if (_preloadingInitialed)
            {
                return;
            }
            _preloadingInitialed = true;

            try
            {
                var config = XDocument.Load(PathUtil.Resolve("~/App_Data/Composite/Composite.config"));

                var controlPathes = (from element in config.Descendants()
                    let userControlVirtualPath = (string) element.Attribute("userControlVirtualPath")
                    where userControlVirtualPath != null
                    select userControlVirtualPath).ToList();

                var stopWatch = new Stopwatch();
                stopWatch.Start();

                foreach (var controlPath in controlPathes)
                {
                    try
                    {
                        BuildManagerHelper.GetCompiledType(controlPath);
                    }
                    catch (ThreadAbortException)
                    {
                        // this exception is automatically rethrown after this catch
                    }
                    catch (Exception ex)
                    {
                        Log.LogWarning("BuildManagerHelper", ex);
                    }
                }

                stopWatch.Stop();
                
                Log.LogVerbose("BuildManagerHelper", "Preloading all the contorls: " + stopWatch.ElapsedMilliseconds + "ms");

                Func<string,bool> isAspNetPath = f => f == ".asmx" || f == ".aspx" || f == ".ashx";
                var aspnetPaths = DirectoryUtils.GetFilesRecursively(PathUtil.Resolve("~/Composite")).Where(f => isAspNetPath(Path.GetExtension(f)));

                stopWatch.Reset();
                stopWatch.Start();

                foreach (var aspnetPath in aspnetPaths)
                {
                    try
                    {
                        BuildManagerHelper.GetCompiledType(PathUtil.GetWebsitePath(aspnetPath));
                    }
                    catch (ThreadAbortException)
                    {
                        // this exception is automatically rethrown after this catch
                    }
                    catch (Exception ex)
                    {
                        Log.LogWarning("BuildManagerHelper", ex);
                    }
                }

                stopWatch.Stop();

                Log.LogVerbose("BuildManagerHelper", "Preloading all asp.net files: " + stopWatch.ElapsedMilliseconds + "ms");
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                Log.LogWarning("BuildManagerHelper", ex);
            }
        }

        /// <summary>
        /// Gets a user control. Prevents an exception that appears in Visual Studio while debugging
        /// </summary>
        /// <param name="virtualPath"></param>
        /// <returns></returns>
        public static Type GetCompiledType(string virtualPath)
        {
            using(BuildManagerHelper.DisableUrlMetadataCachingScope())
            {
                return System.Web.Compilation.BuildManager.GetCompiledType(virtualPath);
            }
        }

        /// <summary>
        /// Disabling the "url metadata caching" prevents <see cref="System.Web.HttpException" /> in debugger 
        /// </summary>
        /// <param name="disableCaching"></param>
        public static void DisableUrlMetadataCaching(bool disableCaching)
        {
            if (!RuntimeInformation.IsDebugBuild)
            {
                return;
            }

            var systemWeb = typeof (System.Web.TraceMode).Assembly;
            Type cachedPathData = systemWeb.GetType("System.Web.CachedPathData", false);
            if (cachedPathData == null) return;

            FieldInfo field = cachedPathData.GetField("s_doNotCacheUrlMetadata", BindingFlags.Static | BindingFlags.NonPublic);
            if (field == null) return;

            field.SetValue(null, disableCaching);
        }

        /// <summary>
        /// Disabling the "url metadata caching" prevents <see cref="System.Web.HttpException" /> in debugger 
        /// </summary>
        public static IDisposable DisableUrlMetadataCachingScope()
        {
            return new DisableUrlMedataScope();
        }

        public class DisableUrlMedataScope : IDisposable
        {
            public DisableUrlMedataScope()
            {
                DisableUrlMetadataCaching(true);
            }

            public void Dispose()
            {
                DisableUrlMetadataCaching(false);
            }
        }
    }
}
