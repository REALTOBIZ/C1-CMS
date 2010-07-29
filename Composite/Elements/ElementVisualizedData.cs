using Composite.ResourceSystem;


namespace Composite.Elements
{
    /// <summary>    
    /// </summary>
    /// <exclude />
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)] 
    public sealed class ElementVisualizedData
    {   
        public string Label { get; set; }
        public bool HasChildren { get; set; }
        public bool IsDisabled { get; set; }
        public ResourceHandle Icon { get; set; }
        public ResourceHandle OpenedIcon { get; set; }
        public string ToolTip { get; set; }
    }
}
