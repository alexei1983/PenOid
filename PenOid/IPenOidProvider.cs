
namespace org.goodspace.Utils.Misc
{
    /// <summary>
    /// Interface that defines a data provider to handle Private Enterprise Number (PEN) object identifiers (OIDs).
    /// </summary>
    public interface IPenOidProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOid"></param>
        void Update(IPenOid penOid);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOid"></param>
        void Create(IPenOid penOid);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOid"></param>
        void Delete(IPenOid penOid);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOid"></param>
        void Assign(IPenOid penOid);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOid"></param>
        void Unassign(IPenOid penOid);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oid"></param>
        IPenOid? Get(string oid);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOids"></param>
        void BulkAssign(params IPenOid[] penOids);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOids"></param>
        void BulkCreateOrUpdate(params IPenOid[] penOids);

        /// <summary>
        /// 
        /// </summary>
        IEnumerable<IPenOid> GetAll();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        IEnumerable<IPenOid> Get(IPenOidQuery? query);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        IPenOid? GetFirstOrDefault(IPenOidQuery? query);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOid"></param>
        /// <returns></returns>
        bool IsAssigned(IPenOid penOid);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOid"></param>
        /// <returns></returns>
        bool Exists(IPenOid penOid);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        IPenOidQuery? CreateQuery(object? criteria);
    }
}
