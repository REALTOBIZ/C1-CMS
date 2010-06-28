﻿using System;
using System.Collections.Generic;
using Composite.ResourceSystem;

namespace Composite.Forms.Flows
{
    internal interface IUiContainer
    {
        IUiControl Render(
            IUiControl innerForm,
            IUiControl customToolbarItems,
            IFormChannelIdentifier channel, 
            IDictionary<string, object> eventHandlerBindings, 
            string containerLabel,
            ResourceHandle containerIcon);
    }
}
