using Composite.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;


namespace Composite.Workflow.Plugins.WorkflowRuntimeProvider
{
    [ConfigurationElementType(typeof(NonConfigurableWorkflowRuntimeProvider))]
    internal class WorkflowRuntimeProviderData : NameTypeManagerTypeConfigurationElement
    {
    }
}
