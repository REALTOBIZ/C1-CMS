﻿using System;

namespace Composite.ConsoleEventSystem
{
    /// <summary>    
    /// </summary>
    /// <exclude />
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)] 
    [Serializable]
    public sealed class BroadcastMessageQueueItem : IConsoleMessageQueueItem
	{
        public string Name { get; set; }
        public string Value { get; set; }
	}
}
