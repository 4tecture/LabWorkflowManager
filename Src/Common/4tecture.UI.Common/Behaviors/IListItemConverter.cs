using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4tecture.UI.Common.Behaviors
{
    public interface IListItemConverter
    {
        /// <summary>
        /// Converts the specified master list item.
        /// </summary>
        /// <param name="masterListItem">The master list item.</param>
        /// <returns>The result of the conversion.</returns>
        object Convert(object masterListItem);

        /// <summary>
        /// Converts the specified target list item.
        /// </summary>
        /// <param name="targetListItem">The target list item.</param>
        /// <returns>The result of the conversion.</returns>
        object ConvertBack(object targetListItem);
    }
}
