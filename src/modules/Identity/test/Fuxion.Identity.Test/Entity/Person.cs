﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    public class Person : Base
    {
        public IList<Skill> Skills { get; set; }
    }
}