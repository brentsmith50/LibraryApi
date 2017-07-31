using System.Collections.Generic;

namespace LibraryApi.Services
{
    public class PropertyMapping<TSource, TDestination> :  IPropertyMapping
    {
        public Dictionary<string, PropertyMappingValue> mappingDictionary { get; private set; }

        public PropertyMapping(Dictionary<string, PropertyMappingValue> mappingDictionary)
        {
            this.mappingDictionary = mappingDictionary;
        }  
    }
}
