using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace _4tecture.UI.Common.Extensions
{
   
    public class LocalizableEnumExtension : MarkupExtension
    {
        private readonly Type _enumType;

        public LocalizableEnumExtension(Type enumType)
        {
            _enumType = enumType;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return (from object enumValue in Enum.GetValues(_enumType)
                    select new EnumMember { Value = enumValue, Description = ((Enum)enumValue).GetLocalizedDescription() }).ToArray();
        }

        public class EnumMember
        {
            public string Description { get; set; }
            public object Value { get; set; }
        }
    }

}
