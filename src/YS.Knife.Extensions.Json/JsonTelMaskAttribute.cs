namespace System.Text.Json.Serialization;

public class JsonTelMaskAttribute : JsonMaskCharAttribute
{
    public JsonTelMaskAttribute() : base(-8, 4)
    {

    }
}


