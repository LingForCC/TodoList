using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Todoist.Net;
using Todoist.Net.Models;

namespace TodoListApp.Service
{
    public class ItemManager
    {
        #region Private Fields

        private ItemFetcher _itemFetcher;
        private SQLiteStorage _storage;
        private string _token;
        private string _tokenFilePath;
        private List<Item> _items;
        private ItemComparer _itemComparer;

        #endregion

        #region Constructor

        public ItemManager()
        {
            _items = new List<Item>();
            _itemComparer = new ItemComparer();
            _storage = new SQLiteStorage();
            _tokenFilePath = Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, "token");
            GetToken();
        }

        #endregion

        #region Properties

        public IEnumerable<Item> Items
        {
            get
            {
                return _items;
            }
        }

        public string Token
        {
            get
            {
                return _token;
            }
            set
            {
                if(!string.IsNullOrWhiteSpace(value) && value != _token)
                {
                    _token = value;
                    _itemFetcher = new ItemFetcher(_token);
                    SaveToken();
                }
            }
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync()
        {
            await _storage.ConnectAsync();
            _items.AddRange(await _storage.GetAllAsync());
        }

        public async Task<ItemChangeCollection> FetchAsync()
        {
            try
            {
                var data = await _itemFetcher.FetchAsync().ConfigureAwait(false);
                List<Item> toRemoveItems = new List<Item>();
                List<Item> toAddItems = new List<Item>();
                List<Item> toUpdateItems = new List<Item>();

                foreach(var item in data)
                {
                    var index = _items.FindIndex(i => i.Id.ToString() == item.Id.ToString());
                    if (index >= 0 && !IsItemActive(item))
                    {
                        toRemoveItems.Add(item);
                        _items.RemoveAt(index);
                    }
                    else if(index < 0 && IsItemActive(item))
                    {
                        toAddItems.Add(item);
                        _items.Add(item);
                    }
                    else if(index >= 0 && IsItemActive(item))
                    {
                        toUpdateItems.Add(item);
                        _items[index] = item;
                    }
                }

                await _storage.RemoveRangeAsync(toRemoveItems).ConfigureAwait(false);

                await _storage.AddRangeAsync(toAddItems).ConfigureAwait(false);

                await _storage.UpdateRange(toUpdateItems).ConfigureAwait(false);

                return new ItemChangeCollection(toAddItems, toRemoveItems, toUpdateItems);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task ResetAsync()
        {
            await _storage.RemoveAllAsync();
            _items.Clear();
        }

        #endregion

        #region Private Methods

        private bool IsItemActive(Item item)
        {
            return !(item.IsArchived.HasValue ? item.IsArchived.Value : false)
                    && !(item.IsChecked.HasValue ? item.IsChecked.Value : false)
                    && !(item.IsDeleted.HasValue ? item.IsDeleted.Value : false);
        }

        private void GetToken()
        {
            if (File.Exists(_tokenFilePath))
            {
                using (FileStream stream = new FileStream(_tokenFilePath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        _token = reader.ReadLine();
                    }
                }
            }
        }

        private void SaveToken()
        {
            using (FileStream stream = new FileStream(_tokenFilePath, FileMode.OpenOrCreate))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    try
                    {
                        writer.WriteLine(_token);
                    }
                    catch (Exception)
                    {
                        //Do Nothing
                    }
                }
            }
        }

        #endregion

        #region ItemComparer

        private class ItemComparer : IEqualityComparer<Item>
        {
            public bool Equals(Item x, Item y)
            {
                return x.Id.ToString() == y.Id.ToString();
            }

            public int GetHashCode(Item obj)
            {
                return obj.GetHashCode();
            }
        }

        #endregion

    }

    public class ItemChangeCollection
    {

        public IEnumerable<Item> ToAddItems
        {
            get;
            private set;
        }

        public IEnumerable<Item> ToRemoveItems
        {
            get;
            private set;
        }

        public IEnumerable<Item> ToUpdateItems
        {
            get;
            private set;
        }

        public ItemChangeCollection(IEnumerable<Item> toAddItems, IEnumerable<Item> toRemoveItems, IEnumerable<Item> toUpdateItems)
        {
            ToAddItems = toAddItems;
            ToRemoveItems = toRemoveItems;
            ToUpdateItems = toUpdateItems;
        }
    }
}
