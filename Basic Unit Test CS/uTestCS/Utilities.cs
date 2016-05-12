using Newtonsoft.Json;

namespace UnitTestCS
{
    public class NormalizedData
    {
        public string Name { get; set; }
        public int Id { get; set; }

        public NormalizedData(string Name, int Id)
        {
            this.Name = Name;
            this.Id = Id;
        }
    }

    public class IncomingData
    {
        public string Name { get; set; }
        public int PersonalIdentifier { get; set; }
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
            var data = JsonConvert.DeserializeObject<IncomingData>(jsonIn);
            return new NormalizedData(data.Name, data.PersonalIdentifier);
        }
    }
}
