using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Data.Erp;
public static class ErpEntityExtensions
{
    public static object CopyEntity(this object entityObject)
    {
        var objectType = entityObject.GetType();
        var copiedObject = Activator.CreateInstance(objectType);

        var propertyInfoList = objectType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

        foreach (var propertyInfo in propertyInfoList)
        {
            if (propertyInfo.PropertyType.IsPrimitive || propertyInfo.PropertyType == typeof(string) ||
                propertyInfo.PropertyType == typeof(decimal) || propertyInfo.PropertyType == typeof(DateTime))
            {
                var propertyVal = propertyInfo.GetValue(entityObject);
                if (propertyVal != null)
                {
                    try
                    {
                        propertyInfo.SetValue(copiedObject, propertyVal);
                    }
                    catch (Exception e)
                    {
                        continue;
                    }
                }
            }
        }

        return copiedObject;
    }
}
