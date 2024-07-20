using System.Collections.Generic;
using System.Linq;

namespace Vex.Library
{
    /// <summary>
    /// A class to hold a UI Search Query
    /// </summary>
    public class SearchQuery
    {
        /// <summary>
        /// Gets the Data that can pass the search
        /// </summary>
        public Dictionary<string, List<string>> Pass { get; } = [];

        /// <summary>
        /// Gets the Data to reject
        /// </summary>
        public Dictionary<string, List<string>> Reject { get; } = [];

        /// <summary>
        /// Gets the Data that can pass the search
        /// </summary>
        public Dictionary<string, List<string>> ExplicitPass { get; } = [];

        /// <summary>
        /// Gets the Data to reject
        /// </summary>
        public Dictionary<string, List<string>> ExplicitReject { get; } = [];

        /// <summary>
        /// Initializes a new instance of the Search Query Class
        /// </summary>
        public SearchQuery()
        {

        }

        /// <summary>
        /// Updates the Search Query
        /// </summary>
        /// <param name="searchString">Search String</param>
        public void Update(string searchString)
        {
            Pass.Clear();
            Reject.Clear();
            ExplicitPass.Clear();
            ExplicitReject.Clear();

            if (string.IsNullOrWhiteSpace(searchString))
                return;

            var splitSearch = searchString.Split(' ');

            foreach (var split in splitSearch)
            {
                var itemSplit = split.Split(':');

                var value = itemSplit.Last();
                var clean = itemSplit.Last().Replace("!", "").Replace("\"", "").ToLower();

                if (!string.IsNullOrWhiteSpace(clean))
                {
                    var key = "name";

                    if (itemSplit.Length > 1)
                        key = itemSplit[0];

                    if (split.Contains('!'))
                    {
                        if (value.StartsWith('\'') && value.EndsWith('\''))
                            AddExplicitRejectData(key, clean);
                        else
                            AddRejectData(key, clean);
                    }
                    else
                    {
                        if (value.StartsWith('\'') && value.EndsWith('\''))
                            AddExplicitPassData(key, clean);
                        else
                            AddPassData(key, clean);
                    }
                }
            }
        }

        /// <summary>
        /// Adds the item to the list
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        public void AddPassData(string key, string value)
        {
            if (!Pass.TryGetValue(key, out var list))
            {
                list = [];
                Pass[key] = list;
            }

            list.Add(value);
        }

        /// <summary>
        /// Adds the item to the list
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        public void AddRejectData(string key, string value)
        {
            if (!Reject.TryGetValue(key, out var list))
            {
                list = [];
                Reject[key] = list;
            }

            list.Add(value);
        }

        /// <summary>
        /// Adds the item to the list
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        public void AddExplicitPassData(string key, string value)
        {
            if (!ExplicitPass.TryGetValue(key, out var list))
            {
                list = [];
                ExplicitPass[key] = list;
            }

            list.Add(value);
        }

        /// <summary>
        /// Adds the item to the list
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        public void AddExplicitRejectData(string key, string value)
        {
            if (!ExplicitReject.TryGetValue(key, out var list))
            {
                list = [];
                ExplicitReject[key] = list;
            }

            list.Add(value);
        }
    }
}
