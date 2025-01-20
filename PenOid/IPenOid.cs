
namespace org.goodspace.Utils.Misc
{
    /// <summary>
    /// 
    /// </summary>
    public interface IPenOid : ICloneable, IEquatable<IPenOid>, IComparable<IPenOid>
    {
        /// <summary>
        /// 
        /// </summary>
        uint Pen { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string? Name {  get; set; }

        /// <summary>
        /// 
        /// </summary>
        string? Value { get; set; }

        /// <summary>
        /// 
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// 
        /// </summary>
        uint[] Components { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IPenOid? GetParent();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOid"></param>
        /// <returns></returns>
        bool IsDescendentOf(IPenOid penOid);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOid"></param>
        /// <returns></returns>
        bool IsChildOf(IPenOid penOid);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IPenOid? Next(string? name = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IPenOid Previous(string? name = null);
    }
}
