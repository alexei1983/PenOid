
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
        public PenOid Create(string oid, string name)
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
        public PenOid CreateNext(PenOid sourcePenOid, string? name = null)
        {
            if (!sourcePenOid.IsInitialized)
                throw new ArgumentException("Invalid OID state.", nameof(sourcePenOid));

            return Create(sourcePenOid.Next(name));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOid"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public PenOid Create(PenOid penOid)
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
        /// <param name="oid"></param>
        /// <returns></returns>
        public bool Delete(string oid)
        {
            return Delete(new PenOid(oid));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOid"></param>
        /// <returns></returns>
        public bool Delete(PenOid penOid)
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
        public PenOid Update(string oid, string name)
        {
            return Update(new PenOid(oid, name));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOid"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public PenOid Update(PenOid penOid)
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
        public bool Assign(PenOid penOid)
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
        public bool AssignNext(PenOid sourcePenOid, string? name = null)
        {
            if (!sourcePenOid.IsInitialized)
                throw new ArgumentException("Invalid OID state.", nameof(sourcePenOid));

            return Assign(sourcePenOid.Next(name));
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
        public IEnumerable<PenOid> GetAll()
        {
            return provider.GetAll();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public IEnumerable<PenOid> Get(object? criteria)
        {
            var query = provider.CreateQuery(criteria);
            return provider.Get(query);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oid"></param>
        /// <returns></returns>
        public bool Unassign(string oid)
        {
            return Unassign(new PenOid(oid));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOid"></param>
        /// <returns></returns>
        public bool Unassign(PenOid penOid)
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
        /// <param name="oid"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsAssigned(string oid, string name)
        {
            return IsAssigned(new PenOid(oid, name));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOids"></param>
        public void BulkAssign(params PenOid[] penOids)
        {
            provider.BulkAssign(penOids);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oid"></param>
        /// <returns></returns>
        public bool IsAssigned(string oid)
        {
            return IsAssigned(new PenOid(oid));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOid"></param>
        /// <returns></returns>
        public bool IsAssigned(PenOid penOid)
        {
            return provider.IsAssigned(penOid);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOid"></param>
        /// <returns></returns>
        public bool Exists(PenOid penOid)
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
