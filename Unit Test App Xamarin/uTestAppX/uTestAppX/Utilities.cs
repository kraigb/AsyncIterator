using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Reflection;

namespace uTestAppX
{
    public class NormalizedData
    {
        public string Name { get; set; }
        public UInt64 Id { get; set; }

        public NormalizedData(string Name, UInt64 Id)
        {
            this.Name = Name;
            this.Id = Id;
        }
    }

    public class IncomingData
    {        
        public string Name { get; set; }     
        public UInt64 PersonalIdentifier { get; set; }
    }

    public static class Utilities
    {
        /// <summary>
        /// Converts JSON data that may contain Name and PersonalIdentifier properties to an 
        /// object with the properties name(string) and id (positive integer up to 9999999999.
        /// </summary>
        /// <param name="jsonIn">The JSON data to normalize, expected to match the IncomingData type.</param>
        /// <returns>A NormalizedData object with Name defaulting to "default" and Id
        /// defaulting to 0, or null if the JSON is null or invalid.</returns>

        public static NormalizedData NormalizeData(string jsonIn)
        {

            // [Block 0: empty function]            
            //return null;

            // [Block 1: golden path]
            //var data = JsonConvert.DeserializeObject<IncomingData>(jsonIn);
            //return new NormalizedData(data.Name, data.PersonalIdentifier);

            // [Block 2]
            if (jsonIn == null || jsonIn.Length == 0)
            {
                return null;
            }
            
            JToken token;
            
            try
            {
                // We use token parsing here because DeserializeObject<IncomingData>
                // is case-insensitive, and also makes it difficult to detect certain
                // other conditions, like an integer name property that we want to reject.
                // Using JObject.Parse we can look at each property individually and make
                // our own type-conversion decisions.                
                token = JObject.Parse(jsonIn);                
            }
            catch (JsonReaderException e)
            {
                // This rejects any invalid JSON
                return null;
            }

            // Assign defaults for when tokens aren't found
            string name = "default";
            UInt64 id = 0;

            var tempName = token.SelectToken("Name");
            
            if (tempName != null)
            {
                if (tempName.Type != JTokenType.String)
                {
                    // Rejects integer or any non-string data type
                    return null;
                }

                name = (string)tempName;

                // Truncate if necessary
                name = (name.Length > 255) ? name.Substring(0, 255) : name;
            }

            
            var tempId = token.SelectToken("PersonalIdentifier");
            
            if (tempId != null)
            {
                // A string in the PersonalIdentifier field is OK; we'll check if it can be
                // converted to an integer.
                if (tempId.Type == JTokenType.String)
                {
                    ulong result;
                    if (!UInt64.TryParse((String)tempId, out result))
                    {
                        return null;
                    }

                    // tempId can be converted, so let it pass through to typecast below
                }
                else
                {
                    // Have to use long to check negatives, as max id is more than an int can hold
                    if (tempId.Type != JTokenType.Integer || (long)tempId < 0)
                    {
                        return null;
                    }
                }
                               
                id = (UInt64)tempId;

                if (id > 9999999999) {
                    return null;
                }
            }

            return new NormalizedData(name, id);
        }
    }
}
