using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VolPro.Core.Configuration;

namespace VolPro.Core.DbManager.DbCopy
{
    public class DbCopySqlserver
    {
        public static string CopyDatabase(string dbEmpty, string dbName)
        {
            string DBPath = AppSetting.GetSettingString("DBPath");
            string DBBackPath = AppSetting.GetSettingString("DBBackPath");


            string sql = @$"USE [master] 
                CREATE DATABASE [{dbName}]
                 CONTAINMENT = NONE
                 ON  PRIMARY 
                ( NAME = N'{dbName}', FILENAME = N'{DBPath}\{dbName}.mdf' , SIZE = 5120KB , FILEGROWTH = 1024KB )
                 LOG ON 
                ( NAME = N'{dbName}_log', FILENAME = N'{DBPath}\{dbName}_log.ldf' , SIZE = 2048KB , FILEGROWTH = 10%)
                
                USE [master]
               BACKUP DATABASE [{dbEmpty}] TO  DISK = N'{DBBackPath}\{dbEmpty}.bak' WITH  COPY_ONLY, NOFORMAT, INIT,
			   NAME = N'{dbEmpty}', SKIP, NOREWIND, NOUNLOAD,  STATS = 10
              --还原数据库
                DECLARE @tomdf NVARCHAR(50)=N'{DBPath}\{dbName}.mdf'
                DECLARE @tolog NVARCHAR(50)=N'{DBPath}\{dbName}.ldf'
                 RESTORE DATABASE [{dbName}] FROM  DISK = N'{DBBackPath}\{dbEmpty}.bak' WITH  FILE = 1,
                 MOVE  N'{dbEmpty}' TO @tomdf,  MOVE N'{dbEmpty}_log' TO @tolog,  NOUNLOAD,  REPLACE,  STATS = 5;

                alter database {dbName} modify file(name={dbEmpty}, newname={dbName});
                alter database {dbName} modify file(name={dbEmpty}_log, newname={dbName}_log) ";

            return sql;

        }
    }
}
