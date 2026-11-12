using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolPro.Builder.IServices
{
    public interface ITableProviderFactory
    {
        ITableDatabaseProvider GetProvider(string dbService);
    }
}
