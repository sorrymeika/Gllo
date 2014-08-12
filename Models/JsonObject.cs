using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Script.Serialization;

namespace Gllo.Models
{
    public class JsonObject : Dictionary<string, object>
    {
        public static JsonObject Parse(string input)
        {
            JsonObject json = new JsonObject();
            return new JavaScriptSerializer().Deserialize<JsonObject>(input);
        }

        public new string ToString()
        {
            return new JavaScriptSerializer().Serialize(this);
        }
    }

    public class JsonArray : List<JsonObject>
    {
        public static JsonArray Parse(string input)
        {
            JsonArray json = new JsonArray();
            return new JavaScriptSerializer().Deserialize<JsonArray>(input);
        }

        public new string ToString()
        {
            return new JavaScriptSerializer().Serialize(this);
        }
    }
}
