using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.PrimaryEntities.Interfaces;

namespace Platform.PrimaryEntities.Tests.Mocks
{
    public class SomeMetadata : Metadata
    {

        public virtual string Name { get; set; }
        public virtual string Color { get; set; }
        public virtual string Skill { get; set; }
        public virtual int Id { get; set; }

        //public SomeMetadata (IFactory factory):base(factory)
        //{
            
        //}


    }
}
