﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    internal interface IItemRunner
    {
        Guid Id { get; }
        object MasterItem { get; }
        string MasterName { get; }
        ISideRunner MasterRunner { get; }
        IEnumerable<IItemSideRunner> SideRunners { get; }
    }
}
