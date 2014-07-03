using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _4tecture.UI.Common.Helper;

namespace _4tecture.UI.Common.Extensions
{
    public static class LocalizedEnumDescriptionExtension
    {
        public static string GetLocalizedDescription(this Enum value)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());
            var attribute = fieldInfo.GetCustomAttributes(typeof(LocalizableDescriptionAttribute), false).FirstOrDefault() as LocalizableDescriptionAttribute;

            return attribute != null ? attribute.Description : value.ToString();
        }

    }
}
