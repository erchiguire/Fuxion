﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    [DataContract(IsReference = true)]
    public class SessionPreview
    {
        internal SessionPreview() { }
        internal SessionPreview(Guid id) { Id = id; }
        [DataMember]
        public Guid Id { get; set; }
        [DataMember]
        public IList<WorkPreview> Works { get; set; }
        public ResourceManager ResourceManager { get; set; }
        public int ChangesCount { get { return Works.Sum(w => w.ChangesCount); } }
        public void Print() => Printer.Foreach("Preview: ", Works, work => work.Print(), false);
    }
}
