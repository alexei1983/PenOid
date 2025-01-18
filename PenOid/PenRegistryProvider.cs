using System.Data;

namespace org.goodspace.Utils.Misc
{
    /// <summary>
    /// Provides Private Enterprise Number (PEN) registration details from multiple sources.
    /// </summary>
    public class PenRegistryProvider
    {
        const string IANA_URL = "https://www.iana.org/assignments/enterprise-numbers.txt";
        const string NONE = "---none---";
        const string NO_CONTACT = "----- no contact";
        const string NAME = "name";
        const string ASSIGNEE = "assignee";
        const string EMAIL = "email";
        const string OID = "oid";
        const string PEN = "pen";
        const string PHONE = "phone";
        const string CONTACT = "contact";
        const string PRIVATE_ENTERPRISE_NUMBER = "privateenterprisenumber";
        DateTime lastRetrieved = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        PenOidWithMetadata[] cache = [];
        Action<object>? refresh;
        object? arg;

        /// <summary>
        /// Creates a new instance of the <see cref="PenRegistryProvider"/> class.
        /// </summary>
        public PenRegistryProvider()
        {
        }

        /// <summary>
        /// Retrieves the specified Private Enterprise Number (PEN) from the cache of registration information, 
        /// if available.
        /// </summary>
        /// <param name="pen">Private Enterprise Number (PEN)</param>
        /// <returns><see cref="PenOid"/> or null if the cache does not contain the specified PEN</returns>
        /// <exception cref="ArgumentException"></exception>
        public PenOid? GetPen(int pen)
        {
            if (pen < uint.MinValue)
                throw new ArgumentException($"Invalid Private Enterprise Number (PEN): {pen}", nameof(pen));
            return GetPen((uint)pen);
        }

        /// <summary>
        /// Retrieves the specified Private Enterprise Number (PEN) from the cache of registration information, 
        /// if available.
        /// </summary>
        /// <param name="pen">Private Enterprise Number (PEN)</param>
        /// <returns><see cref="PenOid"/> or null if the cache does not contain the specified PEN</returns>
        public PenOid? GetPen(uint pen)
        {
            return cache.FirstOrDefault(p => p.Pen.Equals(pen));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        bool ShouldRefreshCache(DateTime? dateTime = null)
        {
            return cache.Length == 0 || (dateTime.HasValue && dateTime.Value >= lastRetrieved);
        }

        /// <summary>
        /// Retrieves the Private Enterprise Number (PEN) registration details from the most recently specified source, 
        /// caching the results in the current instance. If the source supports cache management, the local cache may be consulted 
        /// before retrieving and parsing the source on subsequent calls to the method.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void Refresh()
        {
            if (refresh == null || arg == null)
                throw new InvalidOperationException("Cannot refresh Private Enterprise Number (PEN) data: no refresh action is defined.");
            refresh(arg);
        }

        /// <summary>
        /// Retrieves Private Enterprise Number (PEN) registration details from the specified array of 
        /// <see cref="IDictionary{TKey, TValue}"/> objects.
        /// </summary>
        /// <param name="dictionaries">Array of <see cref="IDictionary{TKey, TValue}"/> objects containing 
        /// the registration details</param>
        /// <remarks><para>Each dictionary should contain a key named `oid`, `pen`, or `privateEnterpriseNumber` whose 
        /// value is the Private Enterprise Number (PEN) or OID string representing it. Additionally, each dictionary 
        /// may contain a `name` key whose value is the registrant, an `assignee` key whose value is the individual assignee, 
        /// a `contact`, `email`, or `phone` key whose value is the contact information for the assignee.</para>
        /// <para>The dictionary keys are NOT case sensitive.</para></remarks>
        /// <returns><see cref="IEnumerable{PenOidWithMetadata}"/></returns>
        public IEnumerable<PenOidWithMetadata> FromDictionary(params IDictionary<string, string?>[] dictionaries)
        {
            return FromDictionary(dictionaries.Select(d => new Dictionary<string, object?>(d.Select(kp =>
                                                           new KeyValuePair<string, object?>(kp.Key, kp.Value)))).ToArray());
        }

        /// <summary>
        /// Retrieves Private Enterprise Number (PEN) registration details from the specified array of 
        /// <see cref="IDictionary{TKey, TValue}"/> objects.
        /// </summary>
        /// <param name="dictionaries">Array of <see cref="IDictionary{TKey, TValue}"/> objects containing 
        /// the registration details</param>
        /// <remarks><para>Each dictionary should contain a key named `oid`, `pen`, or `privateEnterpriseNumber` whose 
        /// value is the Private Enterprise Number (PEN) or OID string representing it. Additionally, each dictionary 
        /// may contain a `name` key whose value is the registrant, an `assignee` key whose value is the individual assignee, 
        /// a `contact`, `email`, or `phone` key whose value is the contact information for the assignee.</para>
        /// <para>The dictionary keys are NOT case sensitive.</para></remarks>
        /// <returns><see cref="IEnumerable{PenOidWithMetadata}"/></returns>
        public IEnumerable<PenOidWithMetadata> FromDictionary(params IDictionary<string, object?>[] dictionaries)
        {
            arg = dictionaries;

            refresh = new Action<object>((arg) =>
            {
                if (arg is IDictionary<string, object?>[] _dictionaries)
                {
                    if (_dictionaries.Length > 0)
                    {
                        List<PenOidWithMetadata> list = [];
                        foreach (var dictionary in _dictionaries)
                        {
                            foreach (var keyPair in dictionary)
                            {
                                PenOid? penOid = null;
                                string? name = null, assignee = null, contact = null;

                                switch (keyPair.Key.ToLower())
                                {
                                    case OID:
                                    case PEN:
                                    case PRIVATE_ENTERPRISE_NUMBER:
                                        penOid = GetPen(keyPair.Value);
                                        break;

                                    case NAME:
                                        name = GetName(keyPair.Value is string strName ? strName : keyPair.Value?.ToString());
                                        break;

                                    case ASSIGNEE:
                                        assignee = GetAssignee(keyPair.Value is string strAssignee ? strAssignee : keyPair.Value?.ToString());
                                        break;

                                    case EMAIL:
                                    case CONTACT:
                                    case PHONE:
                                        contact = GetContact(keyPair.Value is string strContact ? strContact : keyPair.Value?.ToString());
                                        break;
                                }

                                if (penOid != null)
                                    list.Add(new PenOidWithMetadata(penOid.Pen, name, assignee, contact));
                            }
                        }
                        cache = [.. list];
                    }
                }
            });
            Refresh();
            return cache;
        }

        /// <summary>
        /// Retrieves Private Enterprise Number (PEN) registration details using the specified 
        /// <see cref="IDbCommand"/> database command.
        /// </summary>
        /// <param name="command">Database command used to retrieve registration details</param>
        /// <remarks><para>The database command must return a dataset with at least one column.</para>
        /// <para>When one column is returned, the PEN or OID value is assumed to be in that column.</para>
        /// <para>When more than one column is returned, the PEN or OID column must be named `oid`, `pen`, or 
        /// `privateEnterpriseNumber`, the registrant name column must be named `name`, the individual assignee column 
        /// must be named `assignee`, and the assignee contact information column must be named `contact`, 
        /// `email`, or `phone`.</para><para>The column names are NOT case sensitive.</para></remarks>
        /// <returns><see cref="IEnumerable{PenOidWithMetadata}"/></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public IEnumerable<PenOidWithMetadata> FromDatabase(IDbCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command), "DB command is required.");

            if (string.IsNullOrEmpty(command.CommandText))
                throw new ArgumentException("DB command text is required.", nameof(command));

            if (command.Connection == null)
                throw new ArgumentException("DB connection is required.", nameof(command));

            arg = command;

            refresh = new Action<object>((arg) =>
            {
                if (arg is IDbCommand _command && _command.Connection != null)
                {
                    if (_command.Connection.State != ConnectionState.Open)
                        _command.Connection.Open();

                    List<PenOidWithMetadata> list = [];

                    using var results = _command.ExecuteReader();

                    if (results.FieldCount > 0)
                    {
                        while (!results.IsClosed && results.Read())
                        {
                            if (results.FieldCount == 1)
                            {
                                // try to get OID/PEN at position 0
                                PenOid? penOid = GetPen(!results.IsDBNull(0) ? results[0] : null);

                                if (penOid != null)
                                    list.Add(new PenOidWithMetadata(penOid.Pen, null, null, null));
                            }
                            else if (results.FieldCount > 1)
                            {
                                string? name = null, assignee = null, contact = null;
                                PenOid? penOid = null;

                                for (int c = 0; c < results.FieldCount; c++)
                                {
                                    if (results.IsDBNull(c))
                                        continue;

                                    var colName = results.GetName(c);

                                    switch (colName.ToLower())
                                    {
                                        case OID:
                                        case PEN:
                                        case PRIVATE_ENTERPRISE_NUMBER:
                                            penOid = GetPen(results[c]);
                                            break;

                                        case NAME:
                                            name = GetName(results.GetString(c));
                                            break;

                                        case ASSIGNEE:
                                            assignee = GetAssignee(results.GetString(c));
                                            break;

                                        case EMAIL:
                                        case CONTACT:
                                        case PHONE:
                                            contact = GetContact(results.GetString(c));
                                            break;
                                    }
                                }

                                if (penOid != null)
                                    list.Add(new PenOidWithMetadata(penOid.Pen, name, assignee, contact));

                            }
                        }
                    }
                    cache = [.. list];
                }
            });
            Refresh();
            return cache;
        }

        /// <summary>
        /// Attempts to parse the specified object as a Private Enterprise Number (PEN) 
        /// in multiple formats.
        /// </summary>
        /// <param name="pen">Object representing the PEN</param>
        /// <returns><see cref="PenOid"/> or null</returns>
        static PenOid? GetPen(object? pen)
        {
            if (pen == null)
                return default;

            uint? _pen = null;

            if (pen is string str)
            {
                if (string.IsNullOrEmpty(str))
                    return default;

                if (!PenOid.IsOid(str))
                {
                    if (uint.TryParse(str, out var penInt))
                        return new PenOid(penInt);
                }
                else if (PenOid.IsPenOid(str))
                {
                    return new PenOid(str);
                }
            }
            else if (pen is uint uintPen)
                _pen = uintPen;
            else if (pen is int intPen && intPen >= uint.MinValue)
                _pen = (uint)intPen;
            else if (pen is ushort uShortPen)
                _pen = uShortPen;
            else if (pen is sbyte sbytePen && sbytePen >= uint.MinValue)
                _pen = (uint)sbytePen;
            else if (pen is byte bytePen)
                _pen = bytePen;
            else if (pen is short shortPen && shortPen >= uint.MinValue)
                _pen = (uint)shortPen;
            else if (pen is long longPen && longPen >= uint.MinValue && longPen <= uint.MaxValue)
                _pen = (uint)longPen;
            else if (pen is ulong uLongPen && uLongPen <= uint.MaxValue)
                _pen = (uint)uLongPen;
            else if (pen is double doublePen && doublePen >= uint.MinValue && doublePen <= uint.MaxValue)
                _pen = Convert.ToUInt32(doublePen);
            else if (pen is float floatPen && floatPen >= uint.MinValue && floatPen <= uint.MaxValue)
                _pen = Convert.ToUInt32(floatPen);
            else if (pen is decimal decimalPen && decimalPen >= uint.MinValue && decimalPen <= uint.MaxValue)
                _pen = Convert.ToUInt32(decimalPen);

            if (_pen.HasValue)
                return new PenOid(_pen.Value);

            return default;
        }

        /// <summary>
        /// Parses the specified <see cref="string"/> as a Private Enterprise Number (PEN) 
        /// contact email or phone number.
        /// </summary>
        /// <param name="contact">Contact information as a string</param>
        /// <returns><see cref="string"/> or null</returns>
        static string? GetContact(string? contact)
        {
            if (string.IsNullOrEmpty(contact))
                return null;

            if (NONE.Equals(contact) || NO_CONTACT.Equals(contact))
                return null;

            if (contact.Contains('&') && !contact.Contains('@'))
                contact = contact.Replace('&', '@');

            return contact;
        }

        /// <summary>
        /// Parses the specified <see cref="string"/> as a Private Enterprise Number (PEN) 
        /// registrant entity name.
        /// </summary>
        /// <param name="name">PEN registrant entity name</param>
        /// <returns><see cref="string"/> or null</returns>
        static string? GetName(string? name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            if (NONE.Equals(name) || NO_CONTACT.Equals(name))
                return null;

            return name;
        }

        /// <summary>
        /// Parses the specified <see cref="string"/> as a Private Enterprise Number (PEN) 
        /// assignee, usually an individual's name.
        /// </summary>
        /// <param name="assignee">PEN assignee</param>
        /// <returns><see cref="string"/> or null</returns>
        static string? GetAssignee(string? assignee)
        {
            if (string.IsNullOrEmpty(assignee))
                return null;

            if (NONE.Equals(assignee) || NO_CONTACT.Equals(assignee))
                return null;

            return assignee;
        }

        /// <summary>
        /// Parses the specified Private Enterprise Number (PEN) registration file.
        /// </summary>
        /// <remarks>The file must be in the IANA format.</remarks>
        /// <param name="filePath">Path to the file</param>
        /// <returns><see cref="IEnumerable{PenOidWithMetadata}"/></returns>
        public IEnumerable<PenOidWithMetadata> FromFile(string filePath)
        {
            arg = filePath;
            refresh = new Action<object>((arg) =>
            {
                if (arg is string _filePath)
                {
                    var startTime = DateTime.UtcNow;
                    var fileInfo = new FileInfo(Path.GetFullPath(_filePath));
                    if (fileInfo.Exists)
                    {
                        if (ShouldRefreshCache(fileInfo.LastWriteTimeUtc))
                        {
                            var results = ParseIana(File.ReadAllText(fileInfo.FullName));

                            if (results.Any())
                            {
                                lastRetrieved = startTime;
                                cache = [.. results];
                            }
                        }
                    }
                }
            });

            Refresh();
            return cache;
        }

        /// <summary>
        /// Retrieves a Private Enterprise Number (PEN) registration file from 
        /// the specified URL and parses it.
        /// </summary>
        /// <remarks>The file must be in the IANA format.</remarks>
        /// <param name="url">URL where the file is located</param>
        /// <returns><see cref="IEnumerable{PenOidWithMetadata}"/></returns>
        public IEnumerable<PenOidWithMetadata> FromUrl(string url)
        {
            arg = url;
            refresh = new Action<object>((arg) =>
            {
                if (arg is string _url)
                {
                    var startTime = DateTime.UtcNow;
                    using var client = new HttpClient();
                    var req = new HttpRequestMessage(HttpMethod.Get, _url);
                    req.Headers.IfModifiedSince = lastRetrieved;

                    var result = client.SendAsync(req).Result;

                    if (result.IsSuccessStatusCode)
                    {
                        var results = ParseIana(result.Content.ReadAsStringAsync().Result);

                        if (results.Any())
                        {
                            lastRetrieved = startTime;
                            cache = [.. results];
                        }
                    }
                }
            });
            Refresh();
            return cache;
        }

        /// <summary>
        /// Retrieves the Private Enterprise Number (PEN) registration file from 
        /// IANA and parses it.
        /// </summary>
        /// <remarks>The file is located at https://www.iana.org/assignments/enterprise-numbers.txt</remarks>
        /// <returns><see cref="IEnumerable{PenOidWithMetadata}"/></returns>
        public IEnumerable<PenOidWithMetadata> FromIana()
        {
            return FromUrl(IANA_URL);
        }

        /// <summary>
        /// Parses the specified string for Private Enterprise Number (PEN) registrations 
        /// in the IANA format.
        /// </summary>
        /// <param name="contents">File contents as a string</param>
        /// <returns><see cref="IEnumerable{PenOidWithMetadata}"/></returns>
        private static IEnumerable<PenOidWithMetadata> ParseIana(string contents)
        {
            if (!string.IsNullOrEmpty(contents))
            {
                var lines = contents.Split('\n');

                for (var x = 0; x < lines.Length; x++)
                {
                    var line = lines[x].Trim();

                    if (string.IsNullOrEmpty(line))
                        continue;

                    if (char.IsNumber(line[0]))
                    {
                        // parse line as PEN
                        if (uint.TryParse(line, out var penInt))
                        {
                            // skip to next line(s) for name, assignee, and contact info
                            var name = GetName(lines.Length >= (x + 1) ? lines[x + 1].Trim() : null);
                            var assignee = GetAssignee(lines.Length >= (x + 2) ? lines[x + 2].Trim() : null);
                            var contact = GetContact(lines.Length >= (x + 3) ? lines[x + 3].Trim() : null);

                            x += 3;
                            yield return new PenOidWithMetadata(penInt, name, assignee, contact);
                        }
                    }
                }
            }
        }
    }
}
