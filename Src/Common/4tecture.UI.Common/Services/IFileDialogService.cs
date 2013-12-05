using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4tecture.UI.Common.Services
{
    public interface IFileDialogService
    {
        bool OpenFile(out string file);
        bool SaveFile(out string file);
    }
}
