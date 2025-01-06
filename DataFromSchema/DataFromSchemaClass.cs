using System.Dynamic;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DataFromSchema
{
    public static class JsonExtensions
    {

        public static ExpandoObject CreateDynamicObjectByStringSchema(this object entity, string stringJsonSchema)
        {
            var selectedColumns = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(stringJsonSchema);
            ExpandoObject data = CreateDynamicObject(entity, selectedColumns);
            return data;
        }
        public static List<ExpandoObject> CreateDynamicListObjectByStringSchema(this List<object> entityList, string stringJsonSchema)
        {
            var selectedColumns = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(stringJsonSchema);
            List<ExpandoObject> dataList = entityList.Select(m => CreateDynamicObject(m, selectedColumns)).ToList();

            return dataList;
        }

        public static ExpandoObject CreateDynamicObject(object entity, Dictionary<string, List<string>> columns)
        {
            var expando = new ExpandoObject() as IDictionary<string, object>;
            var entityType = entity.GetType();
            var entityName = entityType.BaseType.Name == "Object" ? entityType.Name : entityType.BaseType.Name;
         



            if (!columns.ContainsKey(entityName))
                return (ExpandoObject)expando;

            foreach (var itemColumn in columns[entityName])
            {
                // Eğer alt nesneyle ilgili bir ifade varsa (ör. "Reviews:[Review.Id,Review.Comment]")
                var column = itemColumn.Replace(" ", "");
                if (column.Contains(":"))
                {
                    var match = Regex.Match(column, @"(?<navProperty>\w+):\[(?<subColumns>[^\]]+)\]");
                    if (match.Success)
                    {
                        var navProperty = match.Groups["navProperty"].Value;
                        var subColumns = match.Groups["subColumns"].Value.Split(',');

                        var navigationProperty = entityType.GetProperty(navProperty);
                        if (navigationProperty != null)
                        {
                            var navigationValue = navigationProperty.GetValue(entity);

                            if (navigationValue is IEnumerable<object> collection) // Koleksiyon kontrolü
                            {
                                var subObjects = new List<ExpandoObject>();
                                foreach (var item in collection)
                                {
                                    var subObjectColumns = new Dictionary<string, List<string>>
                                    {
                                        { navigationProperty.PropertyType.GenericTypeArguments[0].Name, subColumns.ToList() }
                                    };
                                    subObjects.Add(CreateDynamicObject(item, subObjectColumns));
                                }
                                expando[navProperty] = subObjects;
                            }

                        }
                    }
                }
                else if (column.Contains("."))
                {
                    // Tekil alt nesne (ör. "Category.Name")
                    var parts = column.Split('.');
                    var navProperty = parts[0];
                    var subColumn = parts[1];

                    var navigationProperty = entityType.GetProperty(navProperty);
                    if (navigationProperty != null)
                    {
                        var navigationValue = navigationProperty.GetValue(entity);

                        if (navigationValue != null)
                        {
                            var subObjectColumns = new Dictionary<string, List<string>>
                        {
                            { navigationProperty.PropertyType.Name, new List<string> { subColumn } }
                        };
                            expando[navProperty] = CreateDynamicObject(navigationValue, subObjectColumns);
                        }
                    }
                }
                else
                {
                    // Normal kolon
                    var propertyInfo = entityType.GetProperty(column);
                    if (propertyInfo != null)
                    {
                        expando[column] = propertyInfo.GetValue(entity);
                    }
                }
            }

            return (ExpandoObject)expando;
        }
        //public static  string Reverse(this string input)
        //{
        //    char[] chars = input.ToCharArray();
        //    Array.Reverse(chars);
        //    return new string(chars);
        //}
    }


}
