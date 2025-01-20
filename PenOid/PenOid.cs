using System.Collections.Concurrent;
using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace org.goodspace.Utils.Misc
{
    /// <summary>
    /// Represents an IANA-assigned Private Enterprise Number (PEN) object identifier (OID).
    /// </summary>
    public partial class PenOid : IPenOid, ICloneable, IEquatable<PenOid>, IEquatable<IPenOid>, IEquatable<string>, 
                                  IEquatable<Oid>, IComparable, IComparable<PenOid>, IComparable<IPenOid>,
                                  IComparable<string>, IComparable<Oid>, IFormattable
    {
        /// <summary>
        /// The prefix for Private Enterprise Number (PEN) assignments.
        /// </summary>
        const string PEN_PREFIX = "1.3.6.1.4.1";

        /// <summary>
        /// The minimum number of components of a Private Enterprise Number (PEN) 
        /// OID managed by IANA, including the PEN itself.
        /// </summary>
        const int PEN_PREFIX_COMPONENT_COUNT = 7;

        /// <summary>
        /// The character separating each OID component.
        /// </summary>
        const char OID_SEPARATOR = '.';

        /// <summary>
        /// Private Enterprise Number (PEN) assigned by IANA.
        /// </summary>
        public uint Pen
        {
            get
            {
                return pen ?? 0;
            }

            set
            {
                if (pen == null)
                {
                    ResetComponents(false);
                    pen = value;
                    components = [.. components, pen.Value];
                }
            }
        }

        /// <summary>
        /// Has the current <see cref="PenOid"/> instance been properly initialized?
        /// </summary>
        public bool IsInitialized
        {
            get
            {
                return components.Length >= PEN_PREFIX_COMPONENT_COUNT && pen.HasValue;
            }
        }

        /// <summary>
        /// Friendly name of the OID.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Full OID value in dotted notation.
        /// </summary>
        public string? Value
        {
            get
            {
                var oid = ToString();
                if (!string.IsNullOrEmpty(oid))
                    return oid;
                return null;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                    ResetComponents(true);
                else
                    ParsePen(value, true);
            }
        }

        /// <summary>
        /// Array of <see cref="uint"/> values representing the component 
        /// parts of the current OID.
        /// </summary>
        public uint[] Components
        {
            get
            {
                if (Pen >= 0)
                    return components;
                return [];
            }
        }

        uint[] components = [];
        uint? pen;

        /// <summary>
        /// Creates a new instance of the <see cref="PenOid"/> class.
        /// </summary>
        public PenOid()
        {
            ResetComponents(false);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="PenOid"/> class.
        /// </summary>
        /// <param name="pen">Private Enterprise Number (PEN) from IANA.</param>
        public PenOid(int pen) : this(pen, null)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="PenOid"/> class.
        /// </summary>
        /// <param name="pen">Private Enterprise Number (PEN) from IANA.</param>
        /// <param name="name">Friendly name of the OID.</param>
        /// <exception cref="ArgumentException"></exception>
        public PenOid(int pen, string? name) : this()
        {
            Name = name;
            Pen = pen >= uint.MinValue ? (uint)pen : throw new ArgumentException($"Invalid Private Enterprise Number (PEN): {pen}", nameof(pen));
        }

        /// <summary>
        /// Creates a new instance of the <see cref="PenOid"/> class.
        /// </summary>
        /// <param name="pen">Private Enterprise Number (PEN) from IANA.</param>
        /// <param name="name">Friendly name of the OID.</param>
        public PenOid(uint pen, string? name) : this()
        {
            Name = name;
            Pen = pen;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="PenOid"/> class.
        /// </summary>
        /// <param name="pen">Private Enterprise Number (PEN) from IANA.</param>
        public PenOid(uint pen) : this(pen, null)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="PenOid"/> class.
        /// </summary>
        /// <param name="name">Friendly name of the OID.</param>
        /// <param name="components">Array of components for the OID.</param>
        /// <exception cref="ArgumentException"></exception>
        private PenOid(string? name = null, params uint[] components)
        {
            if (components.Length < PEN_PREFIX_COMPONENT_COUNT)
                throw new ArgumentException($"Invalid component array for OID: length must be at least {PEN_PREFIX_COMPONENT_COUNT}.",
                                            nameof(components));

            pen = components[6];
            this.components = components;
            Name = name;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="PenOid"/> class.
        /// </summary>
        /// <param name="oid">Source OID as a <see cref="string"/>.</param>
        /// <param name="name">Friendly name of the OID.</param>
        /// <exception cref="ArgumentException"></exception>
        public PenOid(string oid, string? name) : this()
        {
            if (!IsPenOid(oid))
                throw new ArgumentException($"Invalid OID: {oid}", nameof(oid));
            ParsePen(oid, true);
            Name = name;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="PenOid"/> class.
        /// </summary>
        /// <param name="oid">Source OID as a <see cref="string"/>.</param>
        public PenOid(string oid) : this(oid, (string?)null) { }

        /// <summary>
        /// Creates a new instance of the <see cref="PenOid"/> class.
        /// </summary>
        /// <param name="oid">Source <see cref="Oid"/> object.</param>
        /// <exception cref="ArgumentException"></exception>
        public PenOid(Oid oid) : this()
        {
            if (!IsPenOid(oid))
                throw new ArgumentException($"Value is not a Private Enterprise Number (PEN) OID: {oid.Value ?? "null"}", nameof(oid));
            ParsePen(oid.Value ?? string.Empty);
            Name = oid.FriendlyName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oidCollection"></param>
        /// <returns></returns>
        public static IEnumerable<IPenOid> FromOidCollection(OidCollection oidCollection)
        {
            foreach (var oid in oidCollection)
            {
                if (oid == null)
                    continue;
                yield return new PenOid(oid);
            }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="PenOid"/> class.
        /// </summary>
        /// <param name="source">Source <see cref="PenOid"/> object.</param>
        public PenOid(PenOid source) : this()
        {
            Pen = source.Pen;
            components = source.Components;
            Name = source.Name;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current instance.
        /// </summary>
        /// <param name="obj">Object to compare to the current instance.</param>
        /// <returns>True on equality, else false.</returns>
        public override bool Equals(object? obj)
        {
            //if (obj is PenOid penOid)
            //    return penOid.ToString().Equals(ToString());

            if (obj is IPenOid iPenOid)
                return iPenOid.IsInitialized && IsInitialized && CompareTo(iPenOid) == 0;

            if (obj is Oid oid)
                return !string.IsNullOrEmpty(oid.Value) && ToString().Equals(oid.Value);

            if (obj is string strOid)
                return !string.IsNullOrEmpty(strOid) && ToString().Equals(strOid);

            return false;
        }

        /// <summary>
        /// Determines whether the specified <see cref="PenOid"/> object is equal 
        /// to the current instance.
        /// </summary>
        /// <param name="penOid"><see cref="PenOid"/> object to compare to the current instance.</param>
        /// <returns>True on equality, else false.</returns>
        public bool Equals(PenOid? penOid)
        {
            return Equals((object?)penOid);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Oid"/> object is equal to 
        /// the current instance.
        /// </summary>
        /// <param name="oid"><see cref="Oid"/> object to compare to the current instance.</param>
        /// <returns>True on equality, else false.</returns>
        public bool Equals(Oid? oid)
        {
            return Equals((object?)oid);
        }

        /// <summary>
        /// Determines whether or not the specified OID <see cref="string"/> is equal to 
        /// the current instance.
        /// </summary>
        /// <param name="str"><see cref="string"/> OID to compare to the current instance.</param>
        /// <returns>True on equality, else false.</returns>
        public bool Equals(string? str)
        {
            return Equals((object?)str);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object? obj)
        {
            if (obj == null)
                return -1;

            uint[] _components = [];

            try
            {
                if (obj is string strOid)
                    _components = new PenOid(strOid).Components;
                else if (obj is IPenOid penOid)
                    _components = penOid.Components;
                else if (obj is Oid oidObj)
                    _components = new PenOid(oidObj).Components;
                else
                    return -1;
            }
            catch
            {
                return -1;
            }

            long[] theseComponents = Components.Select(Convert.ToInt64).ToArray();
            long[] __components = _components.Select(Convert.ToInt64).ToArray();

            if (__components.Length > theseComponents.Length)
            {
                for (int i = 0; i < __components.Length; i++)
                    if (i > theseComponents.Length - 1)
                        theseComponents = [.. theseComponents, -1];
            }


            if (__components.Length < theseComponents.Length)
            {
                for (int i = 0; i < theseComponents.Length; i++)
                    if (i > __components.Length - 1)
                        __components = [.. __components, -1];
            }

            for (int x = 0; x < theseComponents.Length; x++)
            {
                var comparison = theseComponents[x].CompareTo(__components[x]);

                if (comparison == 0)
                    continue;

                return comparison;
            }

            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        private void ClearPen()
        {
            pen = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clearPen"></param>
        private void ResetComponents(bool clearPen = false)
        {
            components = [1, 3, 6, 1, 4, 1];
            if (clearPen)
                ClearPen();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked
            {
                const int hashingBase = 2142136263;
                const int hashingMultiplier = 13791239;

                var hash = hashingBase;

                hash = (hash * hashingMultiplier) ^ (GetType().GetHashCode());

                if (!IsInitialized)
                {
                    hash = (hash * hashingMultiplier) ^ (Guid.NewGuid().GetHashCode());
                    return hash;
                }

                var tmpComponentHash = (uint)hashingBase;

                foreach (var component in Components)
                    tmpComponentHash ^= component;

                hash = (hash * hashingMultiplier) ^ (tmpComponentHash.GetHashCode());
                hash = (hash * hashingMultiplier) ^ (pen.HasValue ? pen.Value.GetHashCode() : 0);

                return hash;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(PenOid? a, PenOid? b) => Equals(a, b);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(PenOid? a, PenOid? b) => !Equals(a, b);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator >(PenOid? a, PenOid? b)
        {
            if (a == null || b == null)
                return false;

            return a.CompareTo(b) > 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator <(PenOid? a, PenOid? b)
        {
            if (a == null || b == null)
                return false;

            return a.CompareTo(b) < 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator <=(PenOid? a, PenOid? b)
        {
            if (a == null)
                return b == null;

            return a.CompareTo(b) <= 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOid"></param>
        /// <returns></returns>
        public static PenOid operator ++(PenOid penOid)
        {
            if (penOid.Next() is PenOid _penOid)
                return _penOid;
            return penOid;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOid"></param>
        /// <returns></returns>
        public static PenOid operator --(PenOid penOid)
        {
            if (penOid.Previous() is PenOid _penOid)
                return _penOid;
            return penOid;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator >=(PenOid? a, PenOid? b)
        {
            if (a == null)
                return b == null;

            return a.CompareTo(b) >= 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOid"></param>
        public static implicit operator string?(PenOid? penOid)
        {
            return penOid?.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oid"></param>
        public static explicit operator PenOid?(string? oid)
        {
            if (!string.IsNullOrEmpty(oid))
                return new PenOid(oid);
            return default;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="components"></param>
        public static explicit operator PenOid?(uint[]? components)
        {
            if (components != null && components.Length > 0)
                return new PenOid(string.Join(OID_SEPARATOR, components));
            return default;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oid"></param>
        public static explicit operator PenOid?(Oid? oid)
        {
            if (oid != null)
                return new PenOid(oid);
            return default;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOid"></param>
        public static implicit operator Oid?(PenOid? penOid)
        {
            if (penOid != null && penOid.IsInitialized)
                return new Oid(penOid.Value, penOid.Name);

            return default;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOid"></param>
        public static implicit operator uint[]?(PenOid? penOid)
        {
            if (penOid != null && penOid.IsInitialized)
                return penOid.Components;

            return default;
        }

        /// <summary>
        /// Returns a string representation of the current <see cref="PenOid"/>.
        /// </summary>
        /// <returns><see cref="string"/></returns>
        public override string ToString()
        {
            return ToString("O", CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Oid GetOid()
        {
            return new Oid(ToString(), Name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oid"></param>
        /// <returns></returns>
        private static bool ValidateOid(string oid)
        {
            return OidRegex().IsMatch(oid);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="component"></param>
        public void AppendComponent(uint component)
        {
            if (components.Length < PEN_PREFIX_COMPONENT_COUNT)
                throw new Exception("Cannot add component: OID is not properly initialized.");

            components = [.. components, component];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="component"></param>
        /// <exception cref="ArgumentException"></exception>
        public void AppendComponent(int component)
        {
            if (component < uint.MinValue)
                throw new ArgumentException($"Invalid OID component: {component}", nameof(component));

            AppendComponent((uint)component);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="component"></param>
        /// <param name="position"></param>
        /// <exception cref="ArgumentException"></exception>
        public void AddComponent(uint component, int position)
        {
            if (components.Length < PEN_PREFIX_COMPONENT_COUNT)
                throw new Exception("Cannot add component: OID is not properly initialized.");

            if (position < 0)
                throw new ArgumentException($"Invalid position: {position}", nameof(position));

            position += PEN_PREFIX_COMPONENT_COUNT;
            if (position >= components.Length)
                AppendComponent(component);
            else
            {
                var componentList = components.ToList();
                componentList.Insert(position, component);
                components = [.. componentList];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="component"></param>
        /// <param name="position"></param>
        /// <exception cref="ArgumentException"></exception>
        public void AddComponent(int component, int position)
        {
            if (component < uint.MinValue)
                throw new ArgumentException($"Invalid OID component: {component}", nameof(component));

            AddComponent((uint)component, position);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IPenOid? GetParent(string? name = null)
        {
            return GetParent(this, name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IPenOid? GetParent()
        {
            return GetParent(this, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOid"></param>
        /// <param name="parentName"></param>
        /// <returns></returns>
        private static PenOid? GetParent(PenOid penOid, string? parentName = null)
        {
            uint[] components = penOid.Components[..^1];
            if (components.Length >= 7)
                return new PenOid(parentName, components);
            return default;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IPenOid Next(string? name = null)
        {
            return Increment(this, name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IPenOid Previous(string? name = null)
        {
            return Decrement(this, name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static PenOid Increment(PenOid start, string? name = null)
        {
            if (start.Components.Clone() is uint[] _components)
            {
                for (int i = _components.Length - 1; i >= _components.Length - 7; i--)
                {
                    if (i < 7)
                        throw new Exception($"Cannot increment OID: {start}");

                    ulong currentComponent = _components[i];
                    currentComponent++;

                    if (currentComponent > uint.MaxValue)
                    {
                        _components[i] = 0;
                        continue;
                    }
                    else if (currentComponent >= uint.MinValue)
                    {
                        _components[i] = (uint)currentComponent;
                        break;
                    }
                }

                var newPen = new PenOid(name, _components);

                if (newPen.Equals(start))
                    throw new Exception($"Cannot increment OID: {start}");

                return newPen;
            }
            throw new Exception($"Cannot clone OID components from source: {start}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static PenOid Decrement(PenOid start, string? name = null)
        {
            if (start.Components.Clone() is uint[] _components)
            {
                for (int i = _components.Length - 1; i >= _components.Length - 7; i--)
                {
                    if (i < 7)
                        throw new Exception($"Cannot decrement OID: {start}");

                    long currentComponent = _components[i];
                    currentComponent--;

                    if (currentComponent < uint.MinValue)
                    {
                        _components[i] = uint.MaxValue;
                        continue;
                    }
                    else if (currentComponent >= uint.MinValue)
                    {
                        _components[i] = (uint)currentComponent;
                        break;
                    }
                }

                var newPen = new PenOid(name, _components);

                if (newPen.Equals(start))
                    throw new Exception($"Cannot decrement OID: {start}");

                return newPen;
            }
            throw new Exception($"Cannot clone OID components from source: {start}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <exception cref="ArgumentException"></exception>
        public void RemoveComponent(int position)
        {
            if (components.Length < PEN_PREFIX_COMPONENT_COUNT)
                throw new Exception("Cannot remove component: OID is not properly initialized.");

            if (position < 0)
                throw new ArgumentException($"Invalid position: {position}", nameof(position));

            position += PEN_PREFIX_COMPONENT_COUNT;

            if (position >= components.Length)
                throw new ArgumentException($"Invalid position: {position}", nameof(position));

            var componentList = components.ToList();
            componentList.RemoveAt(position);

            components = [.. componentList];
        }

        /// <summary>
        /// Returns a string representation of the current <see cref="PenOid"/>.
        /// </summary>
        /// <param name="format">String format</param>
        /// <returns><see cref="string"/></returns>
        public string ToString(string? format)
        {
            return ToString(format, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Returns a string representation of the current <see cref="PenOid"/>.
        /// </summary>
        /// <param name="format">String format</param>
        /// <param name="provider">Format provider</param>
        /// <returns><see cref="string"/></returns>
        /// <exception cref="FormatException"></exception>
        public string ToString(string? format, IFormatProvider? provider)
        {
            provider ??= CultureInfo.CurrentCulture;

            if (string.IsNullOrEmpty(format))
                format = "G";

            return format switch
            {
                // General format, use the display format (e.g., 1.3.6.1.4.1.32473 - Friendly Name)
                "G" => ToString("D", provider),

                // Full OID in dotted format (e.g., 1.3.6.1.4.1.32473)
                "O" or "o" => Components.Length >= PEN_PREFIX_COMPONENT_COUNT ? string.Join(OID_SEPARATOR, components) : string.Empty,

                // Display format (e.g., 1.3.6.1.4.1.32473 - Friendly Name)
                "D" or "d" => !string.IsNullOrEmpty(Name) ? $"{ToString("O", provider)} - {Name}" : ToString("O", provider),

                // OID friendly name
                "N" => Name ?? string.Empty,

                // Private Enterprise Number (PEN) (e.g., 32473)
                "P" => pen.HasValue ? Pen.ToString(provider) : string.Empty,

                // Invalid format string
                _ => throw new FormatException($"Invalid format string: {format}"),
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public IEnumerable<PenOid> Generate(uint start, uint end)
        {
            if (end < start)
                throw new ArgumentException("Ending number cannot be less than starting number.", nameof(end));

            yield return this;

            for (var i = start; i <= end; i++)
            {
                var newPen = new PenOid(this);
                newPen.AppendComponent(i);
                yield return newPen;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public IEnumerable<PenOid> Generate(int start, int end)
        {
            if (start < uint.MinValue)
                throw new ArgumentException($"Invalid starting number: {start}", nameof(start));

            if (end < uint.MinValue)
                throw new ArgumentException($"Invalid ending number: {end}", nameof(end));

            foreach (var pen in Generate((uint)start, (uint)end))
                yield return pen;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentStart"></param>
        /// <param name="parentEnd"></param>
        /// <param name="childStart"></param>
        /// <param name="childEnd"></param>
        /// <returns></returns>
        public async Task<PenOid[]> GenerateAsync(uint parentStart, uint parentEnd, uint childStart, uint childEnd)
        {
            return await GenerateAsync(parentStart, parentEnd, childStart, childEnd, CancellationToken.None);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentStart"></param>
        /// <param name="parentEnd"></param>
        /// <param name="childStart"></param>
        /// <param name="childEnd"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<PenOid[]> GenerateAsync(uint parentStart, uint parentEnd, uint childStart, uint childEnd, CancellationToken token)
        {
            if (parentEnd < parentStart)
                throw new ArgumentException("Parent ending number cannot be less than parent starting number.", nameof(parentEnd));

            if (childEnd < childStart)
                throw new ArgumentException("Child ending number cannot be less than child starting number.", nameof(childEnd));

            ConcurrentBag<PenOid> bag = [];

            var parallelOptions = new ParallelOptions()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount * 2,
                CancellationToken = token,
            };

            await Parallel.ForAsync(parentStart, parentEnd, parallelOptions, async (p, token) =>
            {
                await Task.Run(() =>
                {
                    if (!token.IsCancellationRequested)
                    {
                        var parentPen = new PenOid(this);
                        parentPen.AppendComponent(p);
                        bag.Add(parentPen);

                        foreach (var childPen in parentPen.Generate(childStart, childEnd))
                            bag.Add(childPen);

                        //Parallel.For(childStart, childEnd, parallelOptions, (c) =>
                        //{
                        //    var childPen = new PenOid(parentPen);
                        //    childPen.AppendComponent((uint)c);
                        //    bag.Add(childPen);
                        //});
                    }
                }, token);
            });

            return [.. bag.Order()];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentStart"></param>
        /// <param name="parentEnd"></param>
        /// <param name="childStart"></param>
        /// <param name="childEnd"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<PenOid[]> GenerateAsync(int parentStart, int parentEnd, int childStart, int childEnd)
        {
            if (parentStart < uint.MinValue)
                throw new ArgumentException($"Invalid parent starting number: {parentStart}", nameof(parentStart));

            if (parentEnd < uint.MinValue)
                throw new ArgumentException($"Invalid parent ending number: {parentEnd}", nameof(parentEnd));

            if (childStart < uint.MinValue)
                throw new ArgumentException($"Invalid child starting number: {childStart}", nameof(childStart));

            if (childEnd < uint.MinValue)
                throw new ArgumentException($"Invalid child ending number: {childEnd}", nameof(childEnd));

            return await GenerateAsync((uint)parentStart, (uint)parentEnd, (uint)childStart, (uint)childEnd);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentStart"></param>
        /// <param name="parentEnd"></param>
        /// <param name="childStart"></param>
        /// <param name="childEnd"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public IEnumerable<PenOid> Generate(uint parentStart, uint parentEnd, uint childStart, uint childEnd)
        {
            if (parentEnd < parentStart)
                throw new ArgumentException("Parent ending number cannot be less than parent starting number.", nameof(parentEnd));

            if (childEnd < childStart)
                throw new ArgumentException("Child ending number cannot be less than child starting number.", nameof(childEnd));

            ConcurrentBag<PenOid> bag = [];

            var parallelOptions = new ParallelOptions()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount * 2,
            };

            Parallel.For(parentStart, parentEnd, parallelOptions, (p) =>
            {
                var parentPen = new PenOid(this);
                parentPen.AppendComponent((uint)p);
                bag.Add(parentPen);

                foreach (var childPen in parentPen.Generate(childStart, childEnd))
                    bag.Add(childPen);

                //Parallel.For(childStart, childEnd, parallelOptions , (c) =>
                //{
                //    var childPen = new PenOid(parentPen);
                //    childPen.AppendComponent((uint)c);
                //    bag.Add(childPen);
                //});
            });

            foreach (var _pen in bag.Order())
                yield return _pen;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentStart"></param>
        /// <param name="parentEnd"></param>
        /// <param name="childStart"></param>
        /// <param name="childEnd"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public IEnumerable<PenOid> Generate(int parentStart, int parentEnd, int childStart, int childEnd)
        {
            if (parentStart < uint.MinValue)
                throw new ArgumentException($"Invalid parent starting number: {parentStart}", nameof(parentStart));

            if (parentEnd < uint.MinValue)
                throw new ArgumentException($"Invalid parent ending number: {parentEnd}", nameof(parentEnd));

            if (childStart < uint.MinValue)
                throw new ArgumentException($"Invalid child starting number: {childStart}", nameof(childStart));

            if (childEnd < uint.MinValue)
                throw new ArgumentException($"Invalid child ending number: {childEnd}", nameof(childEnd));

            foreach (var pen in Generate((uint)parentStart, (uint)parentEnd, (uint)childStart, (uint)childEnd))
                yield return pen;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oid"></param>
        /// <returns></returns>
        public static bool IsPenOid(string? oid)
        {
            oid ??= string.Empty;
            return ValidateOid(oid) && oid.StartsWith(PEN_PREFIX);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oid"></param>
        /// <returns></returns>
        public static bool IsOid(string? oid)
        {
            oid ??= string.Empty;
            return ValidateOid(oid);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oid"></param>
        /// <returns></returns>
        public static bool IsPenOid(Oid oid)
        {
            return IsPenOid(oid.Value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oid"></param>
        /// <param name="isFullOid"></param>
        /// <exception cref="ArgumentException"></exception>
        private void ParsePen(string oid, bool isFullOid = true)
        {
            ResetComponents(true);

            var oidStr = oid?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(oidStr))
                throw new ArgumentException("Invalid OID.", nameof(oid));

            string pen;

            if (!oidStr.StartsWith($"{PEN_PREFIX}.") && isFullOid)
                throw new ArgumentException($"Value is not a valid Private Enterprise Number (PEN) OID: {oid}", nameof(oid));

            if (!isFullOid)
                pen = oidStr;
            else
                pen = oidStr[$"{PEN_PREFIX}.".Length..];

            var indexOfDot = pen.IndexOf(OID_SEPARATOR);
            string peStr;
            string? suffix;

            if (indexOfDot == -1)
            {
                peStr = pen;
                suffix = null;
            }
            else
            {
                peStr = pen[..indexOfDot];
                suffix = pen[(indexOfDot + 1)..];
            }

            if (!uint.TryParse(peStr, out var peInt))
                throw new ArgumentException($"Invalid Private Enterprise Number (PEN): {peStr}", nameof(oid));

            Pen = peInt;
            // components = [..components, peInt];

            if (!string.IsNullOrEmpty(suffix))
            {
                var suffixes = suffix.Split(OID_SEPARATOR).Select(s =>
                {
                    if (!uint.TryParse(s, out var suffixInt))
                        throw new ArgumentException($"Invalid OID component: {s} in OID {oid}.", nameof(oid));
                    return suffixInt;
                }).ToArray();
                components = [.. components, .. suffixes];
            }
        }

        [GeneratedRegex(@"^([0-2])((\.0)|(\.[1-9][0-9]*))*$")]
        private static partial Regex OidRegex();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(PenOid? other)
        {
            return CompareTo((object?)other);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(string? other)
        {
            return CompareTo((object?)other);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(Oid? other)
        {
            return CompareTo((object?)other);
        }

        /// <summary>
        /// Clones the current <see cref="PenOid"/> instance.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            if (!IsInitialized)
                return new PenOid();

            if (Components.Clone() is uint[] _components)
                return new PenOid(Name, _components);

            throw new InvalidOperationException("Unable to clone the current instance.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOid"></param>
        /// <returns></returns>
        public bool IsDescendentOf(IPenOid penOid)
        {
            if (penOid.Components.Length >= Components.Length)
                return false;

            for (int c = 0; c < Components.Length; c++)
            {
                if (c < penOid.Components.Length && penOid.Components[c].Equals(Components[c]))
                    continue;

                if (c >= penOid.Components.Length)
                {
                    var thisPrevious = Components[c - 1];
                    var otherPrevious = penOid.Components[c - 1];

                    return thisPrevious.Equals(otherPrevious);
                }
                else
                    return false;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="penOid"></param>
        /// <returns></returns>
        public bool IsChildOf(IPenOid penOid)
        {
            var thisParent = GetParent();
            return Equals(thisParent, penOid);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(IPenOid? other)
        {
            return Equals((object?)other);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(IPenOid? other)
        {
            return CompareTo((object?)other);
        }
    }
}
