namespace YS.Knife
{
    public record CodeResult
    {
        public Dictionary<string, object> Errors { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public static CodeResult FromCode(string code, string message)
        {
            return new CodeResult
            {
                Code = code,
                Message = message
            };
        }
        public static CodeResult<T> FromData<T>(string code, string message, T data)
        {
            return new CodeResult<T>
            {
                Code = code,
                Message = message,
                Data = data
            };
        }
        public static CodeResult FromErrors(string code, string message, Dictionary<string, object> errors)
        {
            return new CodeResult
            {
                Code = code,
                Message = message,
                Errors = errors
            };
        }
        public static CodeResult FromErrors(string code, string message, System.Collections.IDictionary errors)
        {
            return FromErrors(code, message, ConvertToStringDictionary(errors));
        }
        static Dictionary<string, object> ConvertToStringDictionary(System.Collections.IDictionary dictionary)
        {

            Dictionary<string, object> result = new Dictionary<string, object>();
            if (dictionary != null)
            {
                foreach (object key in dictionary.Keys)
                {
                    result[key?.ToString() ?? ""] = dictionary[key];
                }
            }
            return result;
        }

    }

    public record CodeResult<T> : CodeResult
    {
        public T Data { get; set; }
    }

}
