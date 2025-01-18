
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
        public void Update(PenOid penOid);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOid"></param>
        public void Create(PenOid penOid);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOid"></param>
        public void Delete(PenOid penOid);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOid"></param>
        public void Assign(PenOid penOid);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOid"></param>
        public void Unassign(PenOid penOid);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oid"></param>
        public PenOid? Get(string oid);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOids"></param>
        void BulkAssign(params PenOid[] penOids);

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<PenOid> GetAll();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        public IEnumerable<PenOid> Get(IPenOidQuery? query);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        public PenOid? GetFirstOrDefault(IPenOidQuery? query);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOid"></param>
        /// <returns></returns>
        public bool IsAssigned(PenOid penOid);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOid"></param>
        /// <returns></returns>
        public bool Exists(PenOid penOid);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public IPenOidQuery? CreateQuery(object? criteria);
    }
}
