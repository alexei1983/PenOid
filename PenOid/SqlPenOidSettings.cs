
namespace org.goodspace.Utils.Misc
{
    /// <summary>
    /// 
    /// </summary>
    public class SqlPenOidSettings
    {
        /// <summary>
        /// 
        /// </summary>
        public ReservedWordEscape ReservedWordEscape { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public SqlParameterPrefix ParameterPrefix { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string TableName
        {
            get
            {
                return tableName ?? string.Empty;
            }

            private set
            {
                if (value != null)
                    value = EscapeReservedWord(value);
                tableName = value;
            }
        }

        string? tableName;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="reservedWordEscape"></param>
        /// <param name="parameterPrefix"></param>
        public SqlPenOidSettings(string tableName, ReservedWordEscape reservedWordEscape, SqlParameterPrefix parameterPrefix)
        {
            ReservedWordEscape = reservedWordEscape;
            ParameterPrefix = parameterPrefix;
            TableName = tableName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public char GetParameterPrefix()
        {
            return ParameterPrefix switch
            {
                SqlParameterPrefix.At => '@',
                SqlParameterPrefix.QuestionMark => '?',
                SqlParameterPrefix.Colon => ':',
                SqlParameterPrefix.Semicolon => ';',
                SqlParameterPrefix.None => ' ',
                _ => '@',
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public char[] GetReservedWordEscape()
        {
            return ReservedWordEscape switch
            {
                ReservedWordEscape.Brackets => ['[', ']'],
                ReservedWordEscape.DoubleQuotes => ['"'],
                ReservedWordEscape.SingleQuotes => ['\''],
                ReservedWordEscape.Backticks => ['`'],
                _ => ['[', ']'],
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string Escape(string text)
        {
            if (IsTextEscaped(text))
                return text;

            return EscapeReservedWord(text);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetParameter(string name)
        {
            return $"{GetParameterPrefix()}{name}";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        bool IsTextEscaped(string text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException("Text value is required.", nameof(text));

            var escapeChars = GetReservedWordEscape();

            char startChar = escapeChars[0];
            char endChar = escapeChars.Length > 1 ? escapeChars[1] : escapeChars[0];

            if (!text.StartsWith(startChar))
                return false;

            if (!text.EndsWith(endChar))
                return false;

            string[] parts;

            if (text.Contains('.'))
                parts = text.Split('.');
            else
                parts = [text];

            foreach (var part in parts)
            {
                if (!part.StartsWith(startChar))
                    return false;

                if (!part.EndsWith(endChar))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Escapes the specified text to ensure it does not have special meaning 
        /// in SQL.
        /// </summary>
        /// <param name="text">Text value to escape.</param>
        string EscapeReservedWord(string text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException("Text value is required.", nameof(text));

            if (IsTextEscaped(text))
                return text;

            string[] parts;

            if (text.Contains('.'))
                parts = text.Split('.');
            else
                parts = [text];

            var escapeChars = GetReservedWordEscape();

            char startChar = escapeChars[0];
            char endChar = escapeChars.Length > 1 ? escapeChars[1] : escapeChars[0];
            var val = string.Empty;

            foreach (var part in parts)
            {
                if (!string.IsNullOrEmpty(val) && parts.Length > 1)
                    val += '.';
                val += $"{startChar}{part}{endChar}";
            }
            return val;
        }
    }
}
