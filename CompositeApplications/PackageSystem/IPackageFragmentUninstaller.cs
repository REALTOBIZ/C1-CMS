﻿using System.Collections.Generic;
using System.Xml.Linq;


namespace Composite.PackageSystem
{
    internal interface IPackageFragmentUninstaller
    {
        void Initialize(IEnumerable<XElement> configuration, PackageUninstallerContext packageUninstallerContex);
        IEnumerable<PackageFragmentValidationResult> Validate();
        void Uninstall();
    }
}
