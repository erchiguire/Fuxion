﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.DatabaseTest.Entity
{
    public class Invoice
    {
        public Department Domain { get; set; }
        [DiscriminatedBy(typeof(Department))]
        public string DomainId { get; set; }
    }
}
