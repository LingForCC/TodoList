using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todoist.Net.Models;

namespace TodoListApp.ViewModel
{
    public class ItemViewModel : BaseViewModel
    {

        #region Private Fields

        private Item _item;

        #endregion

        #region Constructor

        public ItemViewModel(Item item)
        {
            _item = item;
        }

        #endregion

        #region Properties

        public string Content
        {
            get
            {
                return _item?.Content;
            }
        }

        public string Id
        {
            get => _item?.Id.ToString();
        }

        #endregion
    }
}
