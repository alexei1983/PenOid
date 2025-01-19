using System.Data;

namespace org.goodspace.Utils.Misc
{
    /// <summary>
    /// 
    /// </summary>
    public class SqlPenOidProvider : IPenOidProvider
    {
        readonly IDbConnection conn;
        readonly SqlPenOidSettings settings;
        readonly string parameterNameOid;
        readonly string columnNameOid;
        readonly string parameterNameName;
        readonly string columnNameName;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbConnection"></param>
        /// <param name="settings"></param>
        public SqlPenOidProvider(IDbConnection dbConnection, SqlPenOidSettings settings)
        {
            conn = dbConnection;
            this.settings = settings;

            parameterNameName = this.settings.GetParameter(Name);
            parameterNameOid = this.settings.GetParameter(Oid);
            columnNameName = this.settings.Escape(Name);
            columnNameOid = this.settings.Escape(Oid);

            if (conn.State != ConnectionState.Open)
                conn.Open();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOid"></param>
        public void Update(IPenOid penOid)
        {
            var sqlCmd = $"UPDATE {settings.TableName} SET {columnNameName} = {parameterNameName} WHERE {columnNameOid} = {parameterNameOid}";
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sqlCmd;
            cmd.CommandType = CommandType.Text;

            var oidParam = cmd.CreateParameter();
            oidParam.ParameterName = parameterNameOid;
            oidParam.Value = penOid.ToString();
            cmd.Parameters.Add(oidParam);

            var nameParam = cmd.CreateParameter();
            nameParam.ParameterName = parameterNameName;
            nameParam.Value = penOid.Name == null ? DBNull.Value : penOid.Name;
            cmd.Parameters.Add(nameParam);

            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOid"></param>
        public void Create(IPenOid penOid)
        {
            var sqlCmd = $"INSERT INTO {settings.TableName} ({columnNameOid}, {columnNameName}) VALUES ({parameterNameOid}, {parameterNameName})";
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sqlCmd;
            cmd.CommandType = CommandType.Text;

            var oidParam = cmd.CreateParameter();
            oidParam.ParameterName = parameterNameOid;
            oidParam.Value = penOid.ToString();
            cmd.Parameters.Add(oidParam);

            var nameParam = cmd.CreateParameter();
            nameParam.ParameterName = parameterNameName;
            nameParam.Value = penOid.Name == null ? DBNull.Value : penOid.Name;
            cmd.Parameters.Add(nameParam);

            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOid"></param>
        public void Delete(IPenOid penOid)
        {
            var sqlCmd = $"DELETE FROM {settings.TableName} WHERE {columnNameOid} = {parameterNameOid}";
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sqlCmd;
            cmd.CommandType = CommandType.Text;

            var oidParam = cmd.CreateParameter();
            oidParam.ParameterName = parameterNameOid;
            oidParam.Value = penOid.ToString();
            cmd.Parameters.Add(oidParam);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOid"></param>
        public void Assign(IPenOid penOid)
        {
            Create(penOid);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOid"></param>
        public void Unassign(IPenOid penOid)
        {
            Delete(penOid);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOids"></param>
        public void BulkCreateOrUpdate(params IPenOid[] penOids)
        {
            if (penOids == null || penOids.Length == 0)
                return;

            var parents = penOids.Select(p => p.GetParent()).Distinct();

            List<IPenOid> updateOids = [];

            if (parents.Any())
            {
                var parameter = conn.CreateCommand().CreateParameter();
                parameter.ParameterName = parameterNameOid;
                parameter.DbType = DbType.String;

                foreach (var parent in parents)
                {
                    if (parent == null)
                        continue;

                    parameter.Value = $"{parent}.%";

                    var children = Get(CreateQuery(new
                    {
                        Sql = $"SELECT * FROM {settings.TableName} WHERE {columnNameOid} LIKE {parameterNameOid}",
                        Parameters = new List<IDbDataParameter>() { parameter },
                        CommandType = CommandType.Text
                    }));

                    children = children.Where(c => !updateOids.Contains(c));

                    if (children.Any())
                        updateOids.AddRange(children);
                }
            }

            List<IPenOid> createOids = [.. penOids.Except(updateOids)];

            if (createOids.Count == 0 && updateOids.Count == 0)
                return;

            var transaction = conn.BeginTransaction();

            var insertQuery = $"INSERT INTO {settings.TableName} ({columnNameOid}, {columnNameName}) VALUES ({parameterNameOid}, {parameterNameName})";
            var insertCmd = conn.CreateCommand();
            insertCmd.CommandText = insertQuery;
            insertCmd.CommandType = CommandType.Text;
            insertCmd.Transaction = transaction;

            var updateQuery = $"UPDATE {settings.TableName} SET {columnNameName} = {parameterNameName} WHERE {columnNameOid} = {parameterNameOid}";
            var updateCmd = conn.CreateCommand();
            updateCmd.CommandText = updateQuery;
            updateCmd.CommandType = CommandType.Text;
            updateCmd.Transaction = transaction;

            try
            {
                var oidParam = insertCmd.CreateParameter();
                oidParam.ParameterName = parameterNameOid;

                var nameParam = insertCmd.CreateParameter();
                nameParam.ParameterName = parameterNameName;

                foreach (var penOid in createOids)
                {
                    oidParam.Value = penOid.ToString();
                    insertCmd.Parameters.Add(oidParam);

                    nameParam.Value = penOid.Name == null ? DBNull.Value : penOid.Name;
                    insertCmd.Parameters.Add(nameParam);

                    insertCmd.ExecuteNonQuery();
                    insertCmd.Parameters.Clear();
                }

                oidParam = updateCmd.CreateParameter();
                oidParam.ParameterName = parameterNameOid;

                nameParam = updateCmd.CreateParameter();
                nameParam.ParameterName = parameterNameName;

                foreach (var penOid in updateOids)
                {
                    oidParam.Value = penOid.ToString();
                    updateCmd.Parameters.Add(oidParam);

                    nameParam.Value = penOid.Name == null ? DBNull.Value : penOid.Name;
                    updateCmd.Parameters.Add(nameParam);

                    updateCmd.ExecuteNonQuery();
                    updateCmd.Parameters.Clear();
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                insertCmd.Dispose();
                updateCmd.Dispose();
                transaction.Dispose();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOids"></param>
        public void BulkAssign(params IPenOid[] penOids)
        {
            if (penOids == null || penOids.Length == 0)
                return;

            var parents = penOids.Select(p => p.GetParent()).Distinct();

            List<IPenOid> existingPens = [];

            if (parents.Any())
            {
                foreach (var parent in parents)
                {
                    if (parent == null)
                        continue;

                    var query = CreateQuery(new
                    {
                        Sql = $"SELECT * FROM {settings.TableName} WHERE {columnNameOid} LIKE {parameterNameOid}",
                        Parameters = new Dictionary<string, object?>() { { parameterNameOid, $"{parent}.%" } },
                        CommandType = CommandType.Text
                    });

                    IPenOid[] children = [..Get(query)];

                    if (children.Length > 0)
                        existingPens.AddRange(children);
                }
            }

            penOids = [..penOids.Except(existingPens)];

            if (penOids.Length == 0)
                return;

            var transaction = conn.BeginTransaction();

            var sqlCmd = $"INSERT INTO {settings.TableName} ({columnNameOid}, {columnNameName}) VALUES ({parameterNameOid}, {parameterNameName})";
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sqlCmd;
            cmd.CommandType = CommandType.Text;
            cmd.Transaction = transaction;

            try
            {
                foreach (var penOid in penOids)
                {
                    var oidParam = cmd.CreateParameter();
                    oidParam.ParameterName = parameterNameOid;
                    oidParam.Value = penOid.ToString();
                    cmd.Parameters.Add(oidParam);

                    var nameParam = cmd.CreateParameter();
                    nameParam.ParameterName = parameterNameName;
                    nameParam.Value = penOid.Name == null ? DBNull.Value : penOid.Name;
                    cmd.Parameters.Add(nameParam);

                    cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                cmd.Dispose();
                transaction.Dispose();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oid"></param>
        public IPenOid? Get(string oid)
        {
            var sqlCmd = $"SELECT * FROM {settings.TableName} WHERE {columnNameOid} = {parameterNameOid}";
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sqlCmd;
            cmd.CommandType = CommandType.Text;

            var oidParam = cmd.CreateParameter();
            oidParam.ParameterName = parameterNameOid;
            oidParam.Value = oid;
            cmd.Parameters.Add(oidParam);
            return GetFromReader(cmd.ExecuteReader())?.FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<IPenOid> GetAll()
        {
            var sqlCmd = $"SELECT * FROM {settings.TableName}";
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sqlCmd;
            cmd.CommandType = CommandType.Text;
            foreach (var penOid in GetFromReader(cmd.ExecuteReader()))
                yield return penOid;
        }

        static IEnumerable<IPenOid> GetFromReader(IDataReader reader)
        {
            if (reader == null)
                yield break;

            using (reader)
            while (!reader.IsClosed && reader.Read())
            {
                string? oid = null;
                string? oidName = null;
                for (var i =  0; i < reader.FieldCount; i++)
                {
                    if (reader.IsDBNull(i))
                        continue;

                    switch (reader.GetName(i).ToLower())
                    {
                        case OidLower:
                            oid = reader.GetString(i);
                            break;

                        case NameLower:
                            oidName = reader.GetString(i);
                            break;

                        default:
                            continue;
                    }
                }

                if (!string.IsNullOrEmpty(oid))
                    yield return new PenOid(oid, oidName);               
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        public IEnumerable<IPenOid> Get(IPenOidQuery? query)
        {
            if (query is SqlPenOidQuery sqlQuery)
            {
                if (!string.IsNullOrEmpty(sqlQuery.Sql))
                {
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = sqlQuery.Sql;
                    cmd.CommandType = sqlQuery.CommandType;
                    foreach (var parameter in sqlQuery.Parameters)
                        cmd.Parameters.Add(parameter);

                    foreach (var penOid in GetFromReader(cmd.ExecuteReader()))
                        yield return penOid;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        public IPenOid? GetFirstOrDefault(IPenOidQuery? query)
        {
            if (query is SqlPenOidQuery sqlQuery)
            {
                if (!string.IsNullOrEmpty(sqlQuery.Sql))
                {
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = sqlQuery.Sql;
                    cmd.CommandType = sqlQuery.CommandType;
                    foreach (var parameter in sqlQuery.Parameters)
                        cmd.Parameters.Add(parameter);

                    return GetFromReader(cmd.ExecuteReader())?.FirstOrDefault();
                }
            }
            return default;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOid"></param>
        /// <returns></returns>
        public bool IsAssigned(IPenOid penOid)
        {
            return Exists(penOid);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOid"></param>
        /// <returns></returns>
        public bool Exists(IPenOid penOid)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT COUNT({columnNameOid}) FROM {settings.TableName} WHERE {columnNameOid} = {parameterNameOid}";
            cmd.CommandType = CommandType.Text;
            var oidParam = cmd.CreateParameter();
            oidParam.ParameterName = parameterNameOid;
            oidParam.Value = penOid.ToString();
            cmd.Parameters.Add(oidParam);
            var result = cmd.ExecuteScalar();

            if (result is null || result is DBNull)
                return false;
            else if (result is int intVal)
                return intVal > 0;
            else if (result is long longVal)
                return longVal > 0;
            else if (result is double dblVal)
                return dblVal > 0;
            else if (result.GetType().IsAssignableFrom(typeof(long)))
                return (long)result > 0;

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public IPenOidQuery? CreateQuery(object? criteria)
        {
            if (criteria == null)
                return default;

            if (criteria.IsAnonymousType())
            {
                var properties = criteria.GetAnonymousProperties();

                string? sqlText = null;
                CommandType commandType = CommandType.Text;
                List<IDbDataParameter> parameters = [];

                List<string> skipProps = [];

                foreach (var sqlProperty in properties.Where(p => SqlQueryPropertyNames.Contains(p.Name, StringComparer.OrdinalIgnoreCase)))
                {
                    var _sqlText = sqlProperty.GetValue(criteria);
                    if (_sqlText is string strSqlText && !string.IsNullOrEmpty(strSqlText))
                    {
                        sqlText = strSqlText;
                        skipProps.Add(sqlProperty.Name);
                        break;
                    }
                }

                // get the command type
                foreach (var cmdTypeProperty in properties.Where(p => CommandTypePropertyNames.Contains(p.Name, StringComparer.OrdinalIgnoreCase)))
                {
                    var _sqlType = cmdTypeProperty.GetValue(criteria);
                    if (_sqlType is string strSqlType && !string.IsNullOrEmpty(strSqlType))
                    {
                        if (Enum.TryParse(strSqlType, true, out commandType))
                        {
                            skipProps.Add(cmdTypeProperty.Name);
                            break;
                        }
                    }
                    else if (_sqlType is int sqlTypeInt)
                    {
                        if (Enum.IsDefined(typeof(CommandType), sqlTypeInt))
                        {
                            commandType = (CommandType)sqlTypeInt;
                            skipProps.Add(cmdTypeProperty.Name);
                            break;
                        }
                    }
                    else if (_sqlType is CommandType cmdType)
                    {
                        commandType = cmdType;
                        skipProps.Add(cmdTypeProperty.Name);
                        break;
                    }
                }

                foreach (var property in properties.Where(p => !skipProps.Contains(p.Name)))
                {
                    var propVal = property.GetValue(criteria);

                    if (propVal is null)
                        continue;

                    if (propVal is IDbDataParameter parameter)
                        parameters.Add(parameter);
                    else if (typeof(IEnumerable<IDbDataParameter>).IsAssignableFrom(property.PropertyType))
                        parameters.AddRange((IEnumerable<IDbDataParameter>)propVal);
                    else if (typeof(IEnumerable<KeyValuePair<string, object?>>).IsAssignableFrom(property.PropertyType))
                    {
                        var tmpCmd = conn.CreateCommand();

                        foreach (var kp in (IEnumerable<KeyValuePair<string, object?>>)propVal)
                        {
                            var tmpParameter = tmpCmd.CreateParameter();
                            tmpParameter.ParameterName = kp.Key;
                            tmpParameter.Value = kp.Value ?? DBNull.Value;
                            parameters.Add(tmpParameter);
                        }

                        tmpCmd.Dispose();
                    }
                }

                return new SqlPenOidQuery()
                {
                    CommandType = commandType,
                    Sql = sqlText,
                    Parameters = [.. parameters],
                };
            }
            return default;
        }

        const string Oid = "Oid";
        const string Name = "Name";
        const string OidLower = "oid";
        const string NameLower = "name";
        static readonly string[] SqlQueryPropertyNames = [ "sql", "statement", "query" ];
        static readonly string[] CommandTypePropertyNames = ["commandType", "cmdType", "type" ];
    }
}
