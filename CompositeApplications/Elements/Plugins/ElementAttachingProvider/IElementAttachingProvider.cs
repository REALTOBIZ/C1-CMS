﻿using System.Collections.Generic;
using Composite.Security;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ObjectBuilder;
using Composite.Elements.Plugins.ElementAttachingProvider.Runtime;


namespace Composite.Elements.Plugins.ElementAttachingProvider
{
    internal enum ElementAttachingProviderPosition
    {
        /// <summary>
        /// At the top
        /// </summary>
        Top,

        /// <summary>
        /// At the bottom
        /// </summary>
        Bottom
    }



    internal sealed class ElementAttachingProviderResult
    {
        public ElementAttachingProviderResult()
        {
            this.Position = ElementAttachingProviderPosition.Bottom;
        }

        /// <summary>
        /// IF this is null, then the hole result is ignored
        /// </summary>
        public IEnumerable<Element> Elements { get; set; }


        public ElementAttachingProviderPosition Position { get; set; }


        /// <summary>
        /// This is used if more than one element attaching provider is adding elements.
        /// Bigger is higher.
        /// </summary>
        public int PositionPriority { get; set; }
    }



    [CustomFactory(typeof(ElementAttachingProviderCustomFactory))]
    [ConfigurationNameMapper(typeof(ElementAttachingProviderDefaultNameRetriever))]
	internal interface IElementAttachingProvider
	{
        /// <summary>
        /// The system will supply an ElementProviderContext to the provider
        /// to use for creating ElementHandles
        /// </summary>
        ElementProviderContext Context { set; }



        /// <summary>
        /// This is only called when rendering root nodes. Used to switch HasChildren from false to true.
        /// </summary>
        /// <param name="parentElement"></param>
        /// <returns></returns>
        bool HaveCustomChildElements(EntityToken parentEntityToken, Dictionary<string, string> piggybag);


        /// <summary>
        /// If null is returned, the result is ignored
        /// </summary>
        /// <param name="parentEntityToken"></param>
        /// <returns></returns>
        ElementAttachingProviderResult GetAlternateElementList(EntityToken parentEntityToken, Dictionary<string, string> piggybag);



        IEnumerable<Element> GetChildren(EntityToken parentEntityToken, Dictionary<string, string> piggybag);
	}
}
