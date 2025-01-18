using System.Data;

namespace org.goodspace.Utils.Misc
{
    /// <summary>
    /// 
    /// </summary>
    public class SqlPenOidQuery : IPenOidQuery
    {
        /// <summary>
        /// 
        /// </summary>
        public string? Sql { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IDbDataParameter[] Parameters { get; set; } = [];

        /// <summary>
        /// 
        /// </summary>
        public CommandType CommandType { get; set; } = CommandType.Text;
    }
}
