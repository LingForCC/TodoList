using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Todoist.Net.Models;
using TodoListApp.Service;

namespace TodoListApp.ViewModel
{
    public class MainViewModel : BaseViewModel
    {

        #region Constant

        private const string SYNC_BUTTON_CONTENT_IDLE = "Sync";
        private const string SYNC_BUTTON_CONTENT_SYNCING = "Syncing";
        private const string TOKEN_NULL_MESSAGE = "Please provide the Token before syncing";
        private const string SYNC_ERROR_MESSAGE = "Something wrong with syncing. Please " +
            "double check your network connection or token and try again";

        #endregion

        #region Private Fields

        #region UI Fields
        private string _token;
        private Visibility _resetIndicationVisibility;
        private Visibility _messageIndicationVisibility;
        private bool _syncButtonEnabled;
        private bool _controlEnabled;
        private string _message;
        private string _syncButtonContent;
        #endregion

        private ObservableCollection<ItemViewModel> _itemVMs;
        private ItemManager _itemManager;

        #endregion

        #region Properties

        public bool SyncButtonEnabled
        {
            get
            {
                return this._syncButtonEnabled;
            }
            set
            {
                this._syncButtonEnabled = value;
                OnPropertyChanged("SyncButtonEnabled");
            }
        }

        public bool ControlEnabled
        {
            get
            {
                return _controlEnabled;
            }
            set
            {
                _controlEnabled = value;
                OnPropertyChanged("ControlEnabled");
            }
        }

        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                _message = value;
                OnPropertyChanged("Message");
            }
        }

        public string SyncButtonContent
        {
            get
            {
                return _syncButtonContent;
            }
            set
            {
                _syncButtonContent = value;
                OnPropertyChanged("SyncButtonContent");
            }
        }

        public ICommand SyncCommand
        {
            get
            {
                return new AppCommand(null, SyncAction);
            }
        }

        public ICommand ResetCommand
        {
            get
            {
                return new AppCommand(null, (parameter) => {
                    Reset();
                });
            }
        }

        private async void Reset()
        {
            ResetIndicationVisibility = Visibility.Hidden;
            await _itemManager.ResetAsync();
            _itemManager.Token = Token;
            _itemVMs.Clear();
            await SyncAsync();
        }

        public ICommand CancelResetCommand
        {
            get
            {
                return new AppCommand(null, (parameter) => {
                    ResetIndicationVisibility = Visibility.Hidden;
                });
            }
        }

        public ICommand CloseMessageCommand
        {
            get
            {
                return new AppCommand(null, (parameter) => {
                    MessageIndicationVisibility = Visibility.Hidden;
                });
            }
        }

        public IEnumerable<ItemViewModel> Items
        {
            get
            {
                return _itemVMs;
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
                _token = value;
                OnPropertyChanged("Token");
            }
        }

        public Visibility ResetIndicationVisibility
        {
            get
            {
                return _resetIndicationVisibility;
            }
            set
            {
                _resetIndicationVisibility = value;
                OnPropertyChanged("ResetIndicationVisibility");
            }
        }

        public Visibility MessageIndicationVisibility
        {
            get
            {
                return _messageIndicationVisibility;
            }
            set
            {
                _messageIndicationVisibility = value;
                OnPropertyChanged("MessageIndicationVisibility");
            }
        }

        #endregion

        #region Constructor

        public MainViewModel(ItemManager itemManager)
        {
            _token = null;
            _resetIndicationVisibility = Visibility.Hidden;
            _messageIndicationVisibility = Visibility.Hidden;
            _syncButtonEnabled = true;
            _syncButtonContent = SYNC_BUTTON_CONTENT_IDLE;
            _controlEnabled = true;
            _itemVMs = new ObservableCollection<ItemViewModel>();
            _itemManager = itemManager;
        }

        #endregion

        public async Task Initialize()
        {
            await _itemManager.InitializeAsync();
            Token = _itemManager.Token;
            AddItemVMs(_itemManager.Items);
        }

        #region Private Methods

        private void SyncAction(object parameter)
        {
            if(string.IsNullOrWhiteSpace(Token))
            {
                Message = TOKEN_NULL_MESSAGE;
                MessageIndicationVisibility = Visibility.Visible;
            }
            else
            {
                if(Token != _itemManager.Token &&
                    _itemManager.Items.Count() != 0)
                {
                    ResetIndicationVisibility = Visibility.Visible;
                }
                else
                {
                    _itemManager.Token = Token;
                    SyncAsync();
                }
            }
        }

        private async Task SyncAsync()
        {
            SyncButtonEnabled = false;
            SyncButtonContent = SYNC_BUTTON_CONTENT_SYNCING;
            var changeCollection = await _itemManager.FetchAsync();

            if (null != changeCollection)
            {
                RemoveItemVMs(changeCollection.ToRemoveItems);

                AddItemVMs(changeCollection.ToAddItems);

                RemoveItemVMs(changeCollection.ToUpdateItems);
                AddItemVMs(changeCollection.ToUpdateItems);
            }
            else
            {
                Message = SYNC_ERROR_MESSAGE;
                MessageIndicationVisibility = Visibility.Visible;
            }
            SyncButtonEnabled = true;
            SyncButtonContent = SYNC_BUTTON_CONTENT_IDLE;
        }

        private void RemoveItemVMs(IEnumerable<Item> toRemoveItems)
        {
            if(null != toRemoveItems)
            {
                foreach (var item in toRemoveItems)
                {
                    var toRemoveItemVM = (from itemVM in _itemVMs
                                          where itemVM.Id == item.Id.ToString()
                                          select itemVM).FirstOrDefault();
                    if (null != toRemoveItemVM)
                    {
                        _itemVMs.Remove(toRemoveItemVM);
                    }
                }
            }
        }

        private void AddItemVMs(IEnumerable<Item> toAddItems)
        {
            if(null != toAddItems)
            {
                foreach (var item in toAddItems)
                {
                    _itemVMs.Add(new ItemViewModel(item));
                }
            }
        }
        
        #endregion
    }


}
