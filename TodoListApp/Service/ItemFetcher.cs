using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Todoist.Net;
using Todoist.Net.Models;

namespace TodoListApp.Service
{
    public class ItemFetcher
    {

        #region Private Fields

        private TodoistClient _todoistClient;
        private string _syncToken;

        #endregion

        #region Constructor

        public ItemFetcher(string token)
        {
            _syncToken = "*";
            _todoistClient = new TodoistClient(token);
        }

        #endregion

        public async Task<IEnumerable<Item>> FetchAsync()
        {
            //TODO: Think about exception handling here
            var parameters = new LinkedList<KeyValuePair<string, string>>();
            parameters.AddLast(new KeyValuePair<string, string>("sync_token", _syncToken));
            parameters.AddLast(new KeyValuePair<string, string>("resource_types", "[\"items\"]"));
            var payLoad = await _todoistClient.PostFormAsync<Payload>("sync", parameters,
                    new List<ByteArrayContent>()).ConfigureAwait(false);
            _syncToken = payLoad.SyncToken;
            return payLoad.Items;
        }

        private class Payload
        {
            [JsonProperty("sync_token")]
            public string SyncToken { get; set; }

            [JsonProperty("items")]
            public IEnumerable<Item> Items
            {
                get; set;
            }
        }
    }

    
}
