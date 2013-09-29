using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Applications.ServiceConfigurator.Code
{
    internal interface IWpfListItem<T>
    {
        string Identifier { get; }

        void UpdateFrom(T original);
    }
}
