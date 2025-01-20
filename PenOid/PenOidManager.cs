
namespace org.goodspace.Utils.Misc
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    /// <param name="provider"></param>
    public class PenOidManager(IPenOidProvider provider)
    {
        readonly IPenOidProvider provider = provider ?? throw new ArgumentNullException(nameof(provider));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oid"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public IPenOid Create(string oid, string name)
        {
            var penOid = new PenOid(oid, name);
            return Create(penOid);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourcePenOid"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public IPenOid? CreateNext(IPenOid sourcePenOid, string? name = null)
        {
            if (!sourcePenOid.IsInitialized)
                throw new ArgumentException("Invalid OID state.", nameof(sourcePenOid));

            var nextPen = sourcePenOid.Next(name);

            if (nextPen != null)
                return Create(nextPen);
            return default;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOid"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public IPenOid Create(IPenOid penOid)
        {
            if (!Exists(penOid))
            {
                provider.Create(penOid);
                return penOid;
            }
            else
                throw new Exception("OID already exists.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOid"></param>
        /// <returns></returns>
        public bool Delete(IPenOid penOid)
        {
            try
            {
                provider.Delete(penOid);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oid"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public IPenOid Update(string oid, string name)
        {
            return Update(new PenOid(oid, name));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOid"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public IPenOid Update(IPenOid penOid)
        {
            if (Exists(penOid))
            {
                provider.Update(penOid);
                return penOid;
            }
            else
                throw new Exception($"OID {penOid} does not exist.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOid"></param>
        /// <returns></returns>
        public bool Assign(IPenOid penOid)
        {
            if (!IsAssigned(penOid))
            {
                provider.Assign(penOid);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourcePenOid"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public bool AssignNext(IPenOid sourcePenOid, string? name = null)
        {
            if (!sourcePenOid.IsInitialized)
                throw new ArgumentException("Invalid OID state.", nameof(sourcePenOid));

            var nextPen = sourcePenOid.Next(name);

            if (nextPen != null)
                return Assign(nextPen);
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oid"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Assign(string oid, string name)
        {
            var penOid = new PenOid(oid, name);
            return Assign(penOid);   
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IPenOid> GetAll()
        {
            return provider.GetAll();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public IEnumerable<IPenOid> Get(object? criteria)
        {
            var query = provider.CreateQuery(criteria);
            return provider.Get(query);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOid"></param>
        /// <returns></returns>
        public bool Unassign(IPenOid penOid)
        {
            if (IsAssigned(penOid))
            {
                provider.Unassign(penOid);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOids"></param>
        public void BulkAssign(params IPenOid[] penOids)
        {
            provider.BulkAssign(penOids);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOid"></param>
        /// <returns></returns>
        public bool IsAssigned(IPenOid penOid)
        {
            return provider.IsAssigned(penOid);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOid"></param>
        /// <returns></returns>
        public bool Exists(IPenOid penOid)
        {
            return provider.Exists(penOid);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oid"></param>
        /// <returns></returns>
        public bool Exists(string oid)
        {
            return provider.Exists(new PenOid(oid));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oid"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Exists(string oid, string name)
        {
            return provider.Exists(new PenOid(oid, name));
        }
    }
}
