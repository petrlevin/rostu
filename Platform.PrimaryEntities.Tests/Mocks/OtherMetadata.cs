using Platform.PrimaryEntities.Interfaces;

namespace Platform.PrimaryEntities.Tests.Mocks
{
    public class OtherMetadata : Metadata
    {

        public virtual bool IsHere { get; set; }
        public virtual string Voice { get; set; }
        public virtual string Name { get; set; }
        public virtual int Id { get; set; }

        //public OtherMetadata (IFactory factory):base(factory)
        //{
            
        //}

    }
}
