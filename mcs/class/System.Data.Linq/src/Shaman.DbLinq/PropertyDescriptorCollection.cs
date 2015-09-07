#if CORECLR
namespace System.ComponentModel
{
    internal class PropertyDescriptorCollection
    {
        internal PropertyDescriptor Find(string propertyName, bool v)
        {
            throw new NotImplementedException();
        }
    }
}
#endif