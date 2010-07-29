﻿using System;
using System.Linq;
using Composite.ConsoleEventSystem;
using Composite.Logging;
using Composite.Security;


namespace Composite.Actions
{
    /// <summary>    
    /// </summary>
    /// <exclude />
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)] 
    public sealed class ParentTreeRefresher
	{
        private bool _postRefreshMessegesCalled = false;
        private FlowControllerServicesContainer _flowControllerServicesContainer;



        public ParentTreeRefresher(FlowControllerServicesContainer flowControllerServicesContainer)
        {
            if (flowControllerServicesContainer == null) throw new ArgumentNullException("flowControllerServicesContainer");

            _flowControllerServicesContainer = flowControllerServicesContainer;
        }


        public void PostRefreshMesseges(EntityToken childEntityToken)
        {
            PostRefreshMesseges(childEntityToken, 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="childEntityToken"></param>
        /// <param name="parentLevel">1 means the first parent, 2 means the second, etc.</param>
        public void PostRefreshMesseges(EntityToken childEntityToken, int parentLevel)
        {
            if (childEntityToken == null) throw new ArgumentNullException("childEntityToken");


            if (_postRefreshMessegesCalled == true)
            {
                throw new InvalidOperationException("Only one PostRefreshMesseges call is allowed");
            }
            else
            {
                _postRefreshMessegesCalled = true;

                RelationshipGraph relationshipGraph = new RelationshipGraph(childEntityToken, RelationshipGraphSearchOption.Both);

                if (relationshipGraph.Levels.Count() > parentLevel)
                {
                    RelationshipGraphLevel relationshipGraphLevel = relationshipGraph.Levels.ElementAt(parentLevel);

                    IManagementConsoleMessageService messageService = _flowControllerServicesContainer.GetService<IManagementConsoleMessageService>();

                    foreach (EntityToken entityToken in relationshipGraphLevel.AllEntities)
                    {
                        messageService.RefreshTreeSection(entityToken);
                        LoggingService.LogVerbose("FirstParentTreeRefresher", string.Format("Refreshing EntityToken: Type = {0}, Source = {1}, Id = {2}, EntityTokenType = {3}", entityToken.Type, entityToken.Source, entityToken.Id, entityToken.GetType()));
                    }
                }                
            }
        }
	}
}
