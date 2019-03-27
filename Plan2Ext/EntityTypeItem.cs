using System;

namespace Plan2Ext
{
    internal class EntityTypeItem
    {
        private readonly Type _type;
        public EntityTypeItem(Type type)
        {
            if (type == null) throw new ArgumentNullException();
            _type = type;
        }

        public Type Type
        {
            get { return _type; }
        }

        public override string ToString()
        {
            return Type.GetGermanName();
        }
    }
}
