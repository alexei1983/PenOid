
namespace org.goodspace.Utils.Misc
{
    /// <summary>
    /// 
    /// </summary>
    public interface IPenOid : ICloneable
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
        /// <returns></returns>
        IPenOid? Next(string? name = null);
    }
}
