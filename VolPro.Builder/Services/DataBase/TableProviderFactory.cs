
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using VolPro.Builder.IServices;
using VolPro.Core.Const;
using VolPro.Core.DBManager;
using VolPro.Core.EFDbContext;
using VolPro.Core.Extensions.AutofacManager;
using VolPro.Core.Utilities;

namespace VolPro.Builder.Services;

public class TableProviderFactory : ITableProviderFactory, IDependency
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public TableProviderFactory(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public ITableDatabaseProvider GetProvider(string dbService)
    {
        var type = DbRelativeCache.GetDbContextType(dbService);
        object dbObj = _httpContextAccessor.HttpContext.RequestServices.GetService(type);
        BaseDbContext _context = dbObj as BaseDbContext;
        string dbType = DbRelativeCache.GetDbType(dbService);
        switch (dbType)
        {
            case "MySql":
                return new MySqlTableProvider(_context);
            case "PgSql":
                return new PgSqlTableProvider(_context);
            case "MsSql":
                return new SqlServerTableProvider(_context);
            default:
                throw new Exception("数据库未支持");
        }
    }
}

