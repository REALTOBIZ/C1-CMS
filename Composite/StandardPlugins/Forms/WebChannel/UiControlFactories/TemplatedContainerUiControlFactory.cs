using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.UI;
using Composite.Extensions;
using Composite.Forms;
using Composite.Forms.CoreUiControls;
using Composite.Forms.Plugins.UiControlFactory;
using Composite.Forms.WebChannel;
using Composite.Logging;
using Composite.StandardPlugins.Forms.WebChannel.Foundation;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ObjectBuilder;
using Microsoft.Practices.ObjectBuilder;


namespace Composite.StandardPlugins.Forms.WebChannel.UiControlFactories
{
    /// <summary>
    /// Use this as base type for User Controls that render a Forms UiControl Container.
    /// Access details about child elements through the FormControlDefinitions property.
    /// </summary>
    /// <exclude />
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)] 
    public class ContainerTemplateUserControlBase : UserControl
    {
        private List<FormControlDefinition> _formControlDefinitions = new List<FormControlDefinition>();
        private Dictionary<string, object> _settings = new Dictionary<string, object>();

        internal List<LazyLoadedContainerInfo> LazyLoadedChildControlIDs { get; set; }

        public string Label { get; set; }
        public string Help { get; set; }
        public int PreSelectedIndex { get; set; }
        public Dictionary<string, object> Settings { get { return _settings; } }

        public List<FormControlDefinition> FormControlDefinitions
        {
            get { return _formControlDefinitions; }
        }

        internal void AddFormControlDefinition(string label, Control formControl, string help, bool isFullWidthControl)
        {
            if (label == null) label = "";

            string toolTip = label;

            FormControlDefinition definition = new FormControlDefinition(label, toolTip, formControl, help, isFullWidthControl);

            _formControlDefinitions.Add(definition);
        }



        protected void RegisterLazyChildControl(int childIndex, string postBackName)
        {
            if (this.LazyLoadedChildControlIDs == null)
            {
                this.LazyLoadedChildControlIDs = new List<LazyLoadedContainerInfo>();
            }

            this.LazyLoadedChildControlIDs.Add(new LazyLoadedContainerInfo { ChildIndex = childIndex, PostBackName = postBackName });
        }




        #region support classes
        public class FormControlDefinition
        {
            private string _label;
            private string _toolTip;
            private Control _formControl;
            private string _help;
            private bool _isFullWidthControl;

            internal FormControlDefinition(string label, string toolTip, Control formControl, string help, bool isFullWidthControl)
            {
                _label = (label != null ? label : "");
                _toolTip = toolTip;
                _formControl = formControl;
                _help = help;
                _isFullWidthControl = isFullWidthControl;
            }

            public bool IsFullWidthControl
            {
                get { return _isFullWidthControl; }
            }

            public string Help
            {
                get { return _help; }
            }

            public string ToolTip
            {
                get { return _toolTip; }
            }

            public string Label
            {
                get { return _label; }
            }

            public Control FormControl
            {
                get { return _formControl; }
            }
        }


        internal class LazyLoadedContainerInfo
        {
            public int ChildIndex { get; set; }
            public string PostBackName { get; set; }
        }
#endregion

    }



    internal sealed class TemplatedTabbedContainerUiControl : TemplatedContainerUiControl, ITabbedContainerUiControl
    {
        internal TemplatedTabbedContainerUiControl(Type userControlType)
            : base( userControlType ) 
        {
            this.PreSelectedIndex = 0;
        }

        [FormsProperty()]
        public int PreSelectedIndex { get; set; }
    }



    internal class TemplatedContainerUiControl : ContainerUiControlBase, IWebUiControl
    {
        private Type _userControlType;
        private ContainerTemplateUserControlBase _userControl;

        internal TemplatedContainerUiControl(Type userControlType)
        {
            _userControlType = userControlType;
        }

        public override void BindStateToControlProperties()
        {
            int childIndex = 0;
            foreach (IUiControl childControl in this.UiControls)
            {
                if (ControlIsActive(childIndex))
                {
                    childControl.BindStateToControlProperties();
                }
                else
                {
                    IWebUiControl childWebUIControl = (childControl as IWebUiControl);
                    Verify.IsNotNull(childWebUIControl, "Child control has to inherit '{0}' interface", typeof(IWebUiControl).FullName);

                    childWebUIControl.InitializeViewState();
                }

                childIndex++;
            }
        }

        private bool ControlIsActive(int childIndex)
        {
            if (_userControl.LazyLoadedChildControlIDs != null)
            {
                var lazyInfo = _userControl.LazyLoadedChildControlIDs.Where(f => f.ChildIndex == childIndex).FirstOrDefault();

                if (lazyInfo != null)
                {
                    string lazyContainerActivated = _userControl.Request[lazyInfo.PostBackName];
                    if (lazyContainerActivated == "false")
                    {
                        return false;
                    }

                    if (lazyContainerActivated != "true")
                    {
                        LoggingService.LogWarning("UiControlContainer", string.Format("Posibility of data corruption detected. Unexpected lazy bound state information from client. Expected 'true' or 'false', got '{0}' for post back field '{1}'. Will try to bind on child controls.", lazyContainerActivated, lazyInfo.PostBackName));
                    }
                }
            }

            return true;
        }

        public void InitializeViewState()
        {
            foreach (IUiControl tab in this.UiControls)
            {
                IWebUiControl WebUiChild = tab as IWebUiControl;
                WebUiChild.InitializeViewState();
            }
        }

        public Control BuildWebControl()
        {
            _userControl = _userControlType.ActivateAsUserControl<ContainerTemplateUserControlBase>(this.UiControlID);
            _userControl.Label = this.Label;
            _userControl.Help = this.Help;

            if (this is ITabbedContainerUiControl)
            {
                _userControl.PreSelectedIndex = ((ITabbedContainerUiControl)this).PreSelectedIndex;
            }
            else
            {
                _userControl.PreSelectedIndex = -1;
            }

            foreach (IUiControl tab in this.UiControls)
            {
                IWebUiControl WebUiChild = tab as IWebUiControl;
                Control WebChild = WebUiChild.BuildWebControl();
                _userControl.AddFormControlDefinition(tab.Label, WebChild, tab.Help, WebUiChild.IsFullWidthControl);
            }

            return _userControl;
        }

        public bool IsFullWidthControl { get; internal set; }

        public string ClientName { get { return null; } }

        public override void InitializeLazyBindedControls()
        {
            int childIndex = 0;
            foreach (IUiControl childControl in this.UiControls)
            {
                if (ControlIsActive(childIndex))
                {
                    if (childControl is TemplatedContainerUiControl)
                    {
                        (childControl as TemplatedContainerUiControl).InitializeLazyBindedControls();
                    }
                }
                else
                {
                    IWebUiControl childWebUIControl = (childControl as IWebUiControl);
                    Verify.IsNotNull(childWebUIControl, "Child control has to inherit '{0}' interface", typeof(IWebUiControl).FullName);

                    childWebUIControl.InitializeViewState();
                }

                childIndex++;
            }
        }
    }


    [ConfigurationElementType(typeof(TemplatedContainerUiControlFactoryData))]
    internal sealed class TemplatedContainerUiControlFactory : Base.BaseTemplatedUiControlFactory
    {
        private TemplatedContainerUiControlFactoryData _data;

        public TemplatedContainerUiControlFactory(TemplatedContainerUiControlFactoryData data)
            : base(data)
        {
            _data = data;
        }

        public override IUiControl CreateControl()
        {
            TemplatedContainerUiControl control;

            if (_data.IsTabbedContainer == true)
            {
                control = new TemplatedTabbedContainerUiControl(this.UserControlType);
            }
            else
            {
                control = new TemplatedContainerUiControl(this.UserControlType);
            }

            control.IsFullWidthControl = _data.IsFullWidthControl;

            return control;
        }
    }



    [Assembler(typeof(TemplatedContainerUiControlFactoryAssembler))]
    internal sealed class TemplatedContainerUiControlFactoryData : ContainerUiControlFactoryData, Base.ITemplatedUiControlFactoryData
    {
        private const string _userControlVirtualPathPropertyName = "userControlVirtualPath";
        private const string _cacheCompiledUserControlTypePropertyName = "cacheCompiledUserControlType";
        private const string _isTabbedContainerPropertyName = "IsTabbedContainer";
        private const string _isFullWidthControlPropertyName = "IsFullWidthControl";
        
        [ConfigurationProperty(_userControlVirtualPathPropertyName, IsRequired = true)]
        public string UserControlVirtualPath
        {
            get { return (string)base[_userControlVirtualPathPropertyName]; }
            set { base[_userControlVirtualPathPropertyName] = value; }
        }

        [ConfigurationProperty(_cacheCompiledUserControlTypePropertyName, IsRequired = false, DefaultValue = true)]
        public bool CacheCompiledUserControlType
        {
            get { return (bool)base[_cacheCompiledUserControlTypePropertyName]; }
            set { base[_cacheCompiledUserControlTypePropertyName] = value; }
        }

        [ConfigurationProperty(_isTabbedContainerPropertyName, IsRequired = false, DefaultValue = false)]
        public bool IsTabbedContainer
        {
            get { return (bool)base[_isTabbedContainerPropertyName]; }
            set { base[_isTabbedContainerPropertyName] = value; }
        }


        [ConfigurationProperty(_isFullWidthControlPropertyName, IsRequired = false, DefaultValue = false)]
        public bool IsFullWidthControl
        {
            get { return (bool)base[_isFullWidthControlPropertyName]; }
            set { base[_isFullWidthControlPropertyName] = value; }
        }
    }


    internal sealed class TemplatedContainerUiControlFactoryAssembler : IAssembler<IUiControlFactory, UiControlFactoryData>
    {
        public IUiControlFactory Assemble(IBuilderContext context, UiControlFactoryData objectConfiguration, IConfigurationSource configurationSource, ConfigurationReflectionCache reflectionCache)
        {
            return new TemplatedContainerUiControlFactory(objectConfiguration as TemplatedContainerUiControlFactoryData);
        }
    }


}
