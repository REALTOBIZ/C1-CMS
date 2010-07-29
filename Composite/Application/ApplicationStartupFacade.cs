﻿using System;
using Composite.Application.Foundation;
using Composite.Application.Foundation.PluginFacades;


namespace Composite.Application
{
    /// <summary>    
    /// </summary>
    /// <exclude />
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)] 
	public static class ApplicationStartupFacade
	{
        public static void FireBeforeSystemInitialize()
        {
            foreach (string hanlderName in ApplicationStartupHandlerRegistry.ApplicationStartupHandlerNames)
            {
                ApplicationStartupHandlerPluginFacade.OnBeforeInitialize(hanlderName);
            }
        }

        public static void FireSystemInitialized()
        {
            foreach (string hanlderName in ApplicationStartupHandlerRegistry.ApplicationStartupHandlerNames)
            {
                ApplicationStartupHandlerPluginFacade.OnInitialized(hanlderName);
            }
        }
	}
}
