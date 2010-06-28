﻿using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Composite.Elements.Plugins.ElementAttachingProvider;
using Composite.Security;


namespace Composite.Trees
{
    internal interface ITreeFacade
    {
        void Initialize();
        Tree GetTree(string treeId);
        IEnumerable<Tree> AllTrees { get; }
        bool HasAttachmentPoints(EntityToken parentEntityToken);
        bool HasPossibleAttachmentPoints(EntityToken parentEntityToken);
        IEnumerable<Tree> GetTreesByEntityToken(EntityToken parentEntityToken);
        bool AddPersistedAttachmentPoint(string treeId, Type interfaceType, object keyValue, ElementAttachingProviderPosition position = ElementAttachingProviderPosition.Top);
        bool RemovePersistedAttachmentPoint(string treeId, Type interfaceType, object keyValue);
        bool AddCustomAttachmentPoint(string treeId, EntityToken entityToken, ElementAttachingProviderPosition position = ElementAttachingProviderPosition.Top);
        Tree LoadTreeFromDom(string treeId, XDocument document);

        void OnFlush();
    }
}
