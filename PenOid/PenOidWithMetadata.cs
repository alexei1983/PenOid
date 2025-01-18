
namespace org.goodspace.Utils.Misc
{
    /// <summary>
    /// Represents a Private Enterprise Number (PEN) assignment from IANA.
    /// </summary>
    /// <remarks>
    /// Creates a new instance of the <see cref="PenOidWithMetadata"/> class.
    /// </remarks>
    /// <param name="pen">Private Enterprise Number (PEN)</param>
    /// <param name="name">Registrant name, usually an organization or company</param>
    /// <param name="assignee">Assignee name, usually an individual</param>
    /// <param name="contactInfo">Contact information, almost always an email address or phone number</param>
    public class PenOidWithMetadata(uint pen, string? name, string? assignee, string? contactInfo) : PenOid(pen, name), IPenOid
    {
        /// <summary>
        /// Private Enterprise Number (PEN) assignee, usually an individual.
        /// </summary>
        public string? Assignee { get; set; } = assignee;

        /// <summary>
        /// Contact information for the Private Enterprise Number (PEN) assignment.
        /// </summary>
        public string? Contact { get; set; } = contactInfo;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public PenOid GetPenOid()
        {
            return new PenOid(this);
        }
    }
}
