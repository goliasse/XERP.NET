﻿using System;
using System.ComponentModel;
using System.Linq;
using SimpleMvvmToolkit;
using XERP.Domain.WarehouseDomain.WarehouseDataService;
using XERP.Domain.WarehouseDomain.Services;

namespace XERP.Client.WPF.WarehouseLocationBinMaintenance.ViewModels
{
    public class TypeSearchViewModel : ViewModelBase<TypeSearchViewModel>
    {
        #region Initialization and Cleanup
        //GlobalProperties Class allows us to share properties amonst multiple classes...
        private GlobalProperties _globalProperties = new GlobalProperties();
        private IWarehouseServiceAgent _serviceAgent;

        public TypeSearchViewModel(){}

        public TypeSearchViewModel(IWarehouseServiceAgent serviceAgent)
        {
            this._serviceAgent = serviceAgent;

            SearchObject = new WarehouseLocationBinType();    
            ResultList = new BindingList<WarehouseLocationBinType>();
            SelectedList = new BindingList<WarehouseLocationBinType>();
            //make sure of session authentication...
            if (XERP.Client.ClientSessionSingleton.Instance.SessionIsAuthentic)//make sure user has rights to UI...
                DoFormsAuthentication();
            else
            {//User is not authenticated...
                RegisterToReceiveMessages<bool>(MessageTokens.StartUpLogInToken.ToString(), OnStartUpLogIn);
                FormIsEnabled = false;
            }
        }
        #endregion Initialization and Cleanup

        private void DoFormsAuthentication()
        {//we need to make the system user is allowed access to this UI...
            if (ClientSessionSingleton.Instance.ExecutableProgramIDList.Contains(_globalProperties.ExecutableProgramName))
                FormIsEnabled = true;
            else
                FormIsEnabled = false;
        }

        private void OnStartUpLogIn(object sender, NotificationEventArgs<bool> e)
        {//if true is returned login was successful...
            if (e.Data)
            {
                FormIsEnabled = true;
                DoFormsAuthentication();
                NotifyAuthenticated();
            }
            else
                FormIsEnabled = false;

            UnregisterToReceiveMessages<bool>(MessageTokens.StartUpLogInToken.ToString(), OnStartUpLogIn);
        }


        #region Notifications
        public event EventHandler<NotificationEventArgs<Exception>> ErrorNotice;
        public event EventHandler<NotificationEventArgs> CloseNotice;
        public event EventHandler<NotificationEventArgs> AuthenticatedNotice;
        #endregion Notifications

        private void NotifyAuthenticated()
        {
            Notify(AuthenticatedNotice, new NotificationEventArgs());
        }

        #region Properties
        private bool? _formIsEnabled;
        public bool? FormIsEnabled
        {
            get { return _formIsEnabled; }
            set
            {
                _formIsEnabled = value;
                NotifyPropertyChanged(m => m.FormIsEnabled);
            }
        }

        private WarehouseLocationBinType _searchObject;
        public WarehouseLocationBinType SearchObject
        {
            get { return _searchObject; }
            set
            {
                _searchObject = value;
                NotifyPropertyChanged(m => m.SearchObject);
            }
        }

        private BindingList<WarehouseLocationBinType> _resultList;
        public BindingList<WarehouseLocationBinType> ResultList
        {
            get { return _resultList; }
            set
            {
                _resultList = value;
                NotifyPropertyChanged(m => m.ResultList);
            }
        }

        private System.Collections.IList _selectedList;
        public System.Collections.IList SelectedList
        {
            get { return _selectedList; }
            set
            {
                _selectedList = value;
                NotifyPropertyChanged(m => m.SelectedList);
            }
        }
        #endregion Properties

        #region Methods
        private BindingList<WarehouseLocationBinType> GetWarehouseLocationBinTypes(string companyID)
        {
            return new BindingList<WarehouseLocationBinType>(_serviceAgent.GetWarehouseLocationBinTypes(companyID).ToList());
        }

        private BindingList<WarehouseLocationBinType> GetWarehouseLocationBinTypes(WarehouseLocationBinType itemQueryObject, string companyID)
        {
            return new BindingList<WarehouseLocationBinType>(_serviceAgent.GetWarehouseLocationBinTypes(itemQueryObject, companyID).ToList());
        }
        #endregion Methods

        #region Commands
        public void SearchCommand()
        {
            ResultList = GetWarehouseLocationBinTypes(SearchObject, ClientSessionSingleton.Instance.CompanyID);
        }

        public void CommitSearchCommand()
        {
            if (SelectedList != null)
            {
                BindingList<WarehouseLocationBinType> selectedList = new BindingList<WarehouseLocationBinType>();
                foreach (var item in SelectedList)
                {
                    selectedList.Add((WarehouseLocationBinType)item);
                }
                MessageBus.Default.Notify(MessageTokens.WarehouseLocationBinTypeSearchToken.ToString(), this, new NotificationEventArgs<BindingList<WarehouseLocationBinType>>("", selectedList));
            }
            NotifyClose("");
        }
        #endregion Commands

        #region Helpers
        // Helper method to notify View of an error
        private void NotifyError(string message, Exception error)
        {// Notify view of an error
            Notify(ErrorNotice, new NotificationEventArgs<Exception>(message, error));
        }

        private void NotifyClose(string message)
        {
            Notify(CloseNotice, new NotificationEventArgs(message));
        }
        #endregion Helpers
    }
}