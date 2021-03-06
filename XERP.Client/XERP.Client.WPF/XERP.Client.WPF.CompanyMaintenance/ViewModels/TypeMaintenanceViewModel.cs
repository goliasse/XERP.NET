﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Services.Client;
using System.Linq;
using System.Windows;
using ExtensionMethods;
using SimpleMvvmToolkit;
using XERP.Client.Models;
using XERP.Domain.CompanyDomain.CompanyDataService;
using XERP.Domain.CompanyDomain.Services;

namespace XERP.Client.WPF.CompanyMaintenance.ViewModels
{
    public class TypeMaintenanceViewModel : ViewModelBase<TypeMaintenanceViewModel>
    {
        #region Initialization and Cleanup
        //GlobalProperties Class allows us to share properties amonst multiple classes...
        private GlobalProperties _globalProperties = new GlobalProperties();
        private int _newCompanyTypeAutoId;

        private ICompanyServiceAgent _serviceAgent;
        private enum _saveRequiredResultActions
        {
            ChangeKeyLogic,
            SearchLogic,
            ClearLogic
        }
        //required else it generates debug view designer issues 
        public TypeMaintenanceViewModel(){}

        public TypeMaintenanceViewModel(ICompanyServiceAgent serviceAgent)
        {
            this._serviceAgent = serviceAgent;

            SetAsEmptySelection();

            CompanyTypeList = new BindingList<CompanyType>();
            //disable new row feature...
            CompanyTypeList.AllowNew = false;

            //make sure of session authentication...
            if (XERP.Client.ClientSessionSingleton.Instance.SessionIsAuthentic)//make sure user has rights to UI...
                DoFormsAuthentication();
            else
            {//User is not authenticated...
                RegisterToReceiveMessages<bool>(MessageTokens.StartUpLogInToken.ToString(), OnStartUpLogIn);
                FormIsEnabled = false;
            }

            AllowNew = true;
            AllowRowPaste = true;
            //CompanyTypeColumnMetaDataList = new List<ColumnMetaData>();
        }
        #endregion Initialization and Cleanup

        #region Authentication Logic
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

        private void NotifyAuthenticated()
        {
            Notify(AuthenticatedNotice, new NotificationEventArgs());
        }
        #endregion Authentication Logic

        #region Notifications
        public event EventHandler<NotificationEventArgs<Exception>> ErrorNotice;
        public event EventHandler<NotificationEventArgs> MessageNotice;
        public event EventHandler<NotificationEventArgs> SearchNotice;
        public event EventHandler<NotificationEventArgs<bool, MessageBoxResult>> SaveRequiredNotice;
        public event EventHandler<NotificationEventArgs<bool, MessageBoxResult>> NewRecordNeededNotice;
        public event EventHandler<NotificationEventArgs> AuthenticatedNotice;
        public event EventHandler<NotificationEventArgs> NewRecordCreatedNotice;
        #endregion Notifications

        #region Properties
        #region General Form Function/State Properties
        //used to enable/disable rowcopy feature for main datagrid...
        private bool _allowRowCopy;
        public bool AllowRowCopy
        {
            get { return _allowRowCopy; }
            set
            {
                _allowRowCopy = value;
                NotifyPropertyChanged(m => m.AllowRowCopy);
            }
        }

        private bool _allowRowPaste;
        public bool AllowRowPaste
        {
            get { return _allowRowPaste; }
            set
            {
                _allowRowPaste = value;
                NotifyPropertyChanged(m => m.AllowRowPaste);
            }
        }


        private bool _allowNew;
        public bool AllowNew
        {
            get { return _allowNew; }
            set
            {
                _allowNew = value;
                NotifyPropertyChanged(m => m.AllowNew);
            }
        }

        private bool _allowCommit;
        public bool AllowCommit
        {
            get { return _allowCommit; }
            set
            {
                _allowCommit = value;
                NotifyPropertyChanged(m => m.AllowCommit);
            }
        }

        private bool _dirty;
        public bool Dirty
        {
            get { return _dirty; }
            set
            {
                _dirty = value;
                NotifyPropertyChanged(m => m.Dirty);
            }
        }

        private bool _allowDelete;
        public bool AllowDelete
        {
            get { return _allowDelete; }
            set
            {
                _allowDelete = value;
                NotifyPropertyChanged(m => m.AllowDelete);
            }
        }

        private bool _allowEdit;
        public bool AllowEdit
        {
            get { return _allowEdit; }
            set
            {
                _allowEdit = value;
                NotifyPropertyChanged(m => m.AllowEdit);
            }
        }


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

        private string _companyTypeListCount;
        public string CompanyTypeListCount
        {
            get { return _companyTypeListCount; }
            set
            {
                _companyTypeListCount = value;
                NotifyPropertyChanged(m => m.CompanyTypeListCount);
            }
        }
        #endregion General Form Function/State Properties

        private BindingList<CompanyType> _companyTypeList;
        public BindingList<CompanyType> CompanyTypeList
        {
            get
            {
                CompanyTypeListCount = _companyTypeList.Count.ToString();
                if (_companyTypeList.Count > 0)
                {
                    AllowEdit = true;
                    AllowDelete = true;
                    AllowRowCopy = true;
                }
                else
                {//no records to edit delete or be dirty...
                    AllowEdit = false;
                    AllowDelete = false;
                    Dirty = false;
                    AllowCommit = false;
                    AllowRowCopy = false;
                }
                return _companyTypeList;
            }
            set
            {
                _companyTypeList = value;
                NotifyPropertyChanged(m => m.CompanyTypeList);
            }
        }

        //this is used to collect previous values as to compare the changed values...
        private CompanyType _selectedCompanyTypeMirror;
        public CompanyType SelectedCompanyTypeMirror
        {
            get { return _selectedCompanyTypeMirror; }
            set { _selectedCompanyTypeMirror = value; }
        }

        private System.Collections.IList _selectedCompanyTypeList;
        public System.Collections.IList SelectedCompanyTypeList
        {
            get { return _selectedCompanyTypeList; }
            set
            {
                if (_selectedCompanyType != value)
                {
                    _selectedCompanyTypeList = value;
                    NotifyPropertyChanged(m => m.SelectedCompanyTypeList);
                }
            }
        }

        private CompanyType _selectedCompanyType;
        public CompanyType SelectedCompanyType
        {
            get
            {
                return _selectedCompanyType;
            }
            set
            {
                if (_selectedCompanyType != value)
                {
                    _selectedCompanyType = value;
                    //set the mirrored SelectedCompanyType to allow to track property changes w/o
                    //explicitly providing a property for each field...
                    SelectedCompanyTypeMirror = new CompanyType();
                    if (value != null)
                    {//default the PreviousKeyID... 
                        foreach (var prop in SelectedCompanyType.GetType().GetProperties())
                        {
                            SelectedCompanyTypeMirror.SetPropertyValue(prop.Name, SelectedCompanyType.GetPropertyValue(prop.Name));
                        }
                        SelectedCompanyTypeMirror.CompanyTypeID = _selectedCompanyType.CompanyTypeID;
                        NotifyPropertyChanged(m => m.SelectedCompanyType);

                        SelectedCompanyType.PropertyChanged += new PropertyChangedEventHandler(SelectedCompanyType_PropertyChanged);
                    }
                }
            }
        }

        private List<ColumnMetaData> _companyTypeColumnMetaDataList;
        public List<ColumnMetaData> CompanyTypeColumnMetaDataList
        {
            get { return _companyTypeColumnMetaDataList; }
            set
            {
                _companyTypeColumnMetaDataList = value;
                NotifyPropertyChanged(m => m.CompanyTypeColumnMetaDataList);
            }
        }

        #region Validation Properties
        //we use this dictionary to bind all textbox maxLenght properties in the View...
        private Dictionary<string, int> _companyTypeMaxFieldValueDictionary;
        public Dictionary<string, int> CompanyTypeMaxFieldValueDictionary //= new Dictionary<string, int>();
        {
            get
            {//we only need to get this once...
                if (_companyTypeMaxFieldValueDictionary != null)
                    return _companyTypeMaxFieldValueDictionary;

                _companyTypeMaxFieldValueDictionary = new Dictionary<string, int>();
                var metaData = _serviceAgent.GetMetaData("CompanyTypes");

                foreach (var data in metaData)
                {
                    if (data.ShortChar_1 == "String")
                        _companyTypeMaxFieldValueDictionary.Add(data.Name.ToString(), (int)data.Int_1);
                }
                return _companyTypeMaxFieldValueDictionary;
            }
        }
        #endregion Validation Properties
        #endregion Properties

        #region ViewModel Propertie's Events
        private void SelectedCompanyType_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {//these properties are not to be persisted we will igore them...
            if (e.PropertyName == "IsSelected" ||
                e.PropertyName == "IsExpanded" ||
                e.PropertyName == "IsValid" ||
                e.PropertyName == "NotValidMessage" ||
                e.PropertyName == "LastModifiedBy" ||
                e.PropertyName == "LastModifiedByDate")
            {
                return;
            }
               //Key ID Logic...
            if (e.PropertyName == "CompanyTypeID")
            {//make sure it is has changed...
                if (SelectedCompanyTypeMirror.CompanyTypeID != SelectedCompanyType.CompanyTypeID)
                {
                    //if their are no records it is a key change
                    if (CompanyTypeList != null && CompanyTypeList.Count == 0
                        && SelectedCompanyType != null && !string.IsNullOrEmpty(SelectedCompanyType.CompanyTypeID))
                    {
                        ChangeKeyLogic();
                        return;
                    }

                    EntityStates entityState = GetCompanyTypeState(SelectedCompanyType);

                    if (entityState == EntityStates.Unchanged ||
                        entityState == EntityStates.Modified)
                    {//once a key is added it can not be modified...
                        if (Dirty && AllowCommit)//dirty record exists ask if save is required...
                            NotifySaveRequired("Do you want to save changes?", _saveRequiredResultActions.ChangeKeyLogic);
                        else
                            ChangeKeyLogic();
                        return;
                    }
                }
            }//end KeyID logic...

            object propertyChangedValue = SelectedCompanyType.GetPropertyValue(e.PropertyName);
            object prevPropertyValue = SelectedCompanyTypeMirror.GetPropertyValue(e.PropertyName);
            string propertyType = SelectedCompanyType.GetPropertyType(e.PropertyName);
            //in some instances the value is not really changing but yet it still is tripping property change..
            //This will ensure that the field has physically been modified...
            //As well when we revert back it constitutes a property change but they will be = and it will bypass the logic...
            bool objectsAreEqual;
            if (propertyChangedValue == null)
            {
                if (prevPropertyValue == null)//both values are null
                    objectsAreEqual = true;
                else//only one value is null
                    objectsAreEqual = false;
            }
            else
            {
                if (prevPropertyValue == null)//only one value is null
                    objectsAreEqual = false;
                else //both values are not null use .Equals...
                    objectsAreEqual = propertyChangedValue.Equals(prevPropertyValue);
            }
            if (!objectsAreEqual)
            {
                //Here we do property change validation if false is returned we will reset the value
                //Back to its mirrored value and return out of the property change w/o updating the repository...
                if (CompanyTypePropertyChangeIsValid(e.PropertyName, propertyChangedValue, prevPropertyValue, propertyType))
                {
                    Update(SelectedCompanyType);
                    //set the mirrored objects field...
                    SelectedCompanyTypeMirror.SetPropertyValue(e.PropertyName, propertyChangedValue);
                    SelectedCompanyTypeMirror.IsValid = SelectedCompanyType.IsValid;
                    SelectedCompanyTypeMirror.IsExpanded = SelectedCompanyType.IsExpanded;
                    SelectedCompanyTypeMirror.NotValidMessage = SelectedCompanyType.NotValidMessage;

                }
                else
                {//revert back to its previous value... 
                    SelectedCompanyType.SetPropertyValue(e.PropertyName, prevPropertyValue);
                    SelectedCompanyType.IsValid = SelectedCompanyTypeMirror.IsValid;
                    SelectedCompanyType.IsExpanded = SelectedCompanyTypeMirror.IsExpanded;
                    SelectedCompanyType.NotValidMessage = SelectedCompanyTypeMirror.NotValidMessage;
                }
            }
        }
        #endregion ViewModel Propertie's Events

        #region Methods
        #region ViewModel Logic Methods
        private void ChangeKeyLogic()
        {
            if (!string.IsNullOrEmpty(SelectedCompanyType.CompanyTypeID))
            {//check to see if key is part of the current itemList...
                CompanyType query = CompanyTypeList.Where(item => item.CompanyTypeID == SelectedCompanyType.CompanyTypeID &&
                                                        item.AutoID != SelectedCompanyType.AutoID).FirstOrDefault();
                if (query != null)
                {//revert it back
                    SelectedCompanyType.CompanyTypeID = SelectedCompanyTypeMirror.CompanyTypeID;
                    //change to the newly selected item...
                    SelectedCompanyType = query;
                    return;
                }
                //it is not part of the existing list try to fetch it from the db...
                CompanyTypeList = GetCompanyTypeByID(SelectedCompanyType.CompanyTypeID);
                if (CompanyTypeList.Count == 0)//it was not found do new record required logic...
                    NotifyNewRecordNeeded("Record " + SelectedCompanyType.CompanyTypeID + " Does Not Exist.  Create A New Record?");
                else
                    SelectedCompanyType = CompanyTypeList.FirstOrDefault();
            }
            else
            {
                string errorMessage = "ID Is Required.";
                NotifyMessage(errorMessage);
                //revert back to the value it was before it was changed...
                if (SelectedCompanyType.CompanyTypeID != SelectedCompanyTypeMirror.CompanyTypeID)
                    SelectedCompanyType.CompanyTypeID = SelectedCompanyTypeMirror.CompanyTypeID;
            }
        }
        //XERP allows for bulk updates we only allow save
        //if all bulk update requirements are met...
        private bool CommitIsAllowed()
        {//Check for any repository changes that are not yet committed to the db...
            Dirty = RepositoryIsDirty();
            //check for any invalid rows...
            int count = (from q in CompanyTypeList where q.IsValid == 1 select q).Count();
            if (count > 0)
                return false;
            return true;
        }

        private void SetAsEmptySelection()
        {
            SelectedCompanyType = new CompanyType();
            AllowEdit = false;
            AllowDelete = false;
            Dirty = false;
            AllowCommit = false;
            AllowRowCopy = false;
        }

        public void ClearLogic()
        {
            CompanyTypeList.Clear();
            SetAsEmptySelection();
        }

        private bool CompanyTypePropertyChangeIsValid(string propertyName, object changedValue, object previousValue, string type)
        {
            string errorMessage = "";
            bool rBool = true;
            switch (propertyName)
            {
                case "CompanyTypeID":
                    rBool = CompanyTypeIsValid(SelectedCompanyType, _itemValidationProperties.CompanyTypeID, out errorMessage);
                    break;
                case "Name":
                    rBool = CompanyTypeIsValid(SelectedCompanyType, _itemValidationProperties.Name, out errorMessage);
                    break;
            }
            if (rBool == false)
            {//here we give a specific error to the specific change
                NotifyMessage(errorMessage);
                SelectedCompanyType.IsValid = 1;
            }
            else //check the enire rows validity...
            {//here we check the entire row for validity the property change may be valid
                //but we still do not know if the entire row is valid...
                //if the row is valid we will set it to 2 (pending changes...)
                //on the commit we will set it to 0 and it will be valid and saved to the db...
                SelectedCompanyType.IsValid = CompanyTypeIsValid(SelectedCompanyType, out errorMessage);
                if (SelectedCompanyType.IsValid == 2)
                    errorMessage = "Pending Changes...";
            }
            SelectedCompanyType.NotValidMessage = errorMessage;
            return rBool;
        }

        #region Validation Methods
        //XERP Validation is done by the entire object or by Object property...
        //So we must be sure to add the validation in both places...
        private enum _itemValidationProperties
        {//we list all fields that require validation...
            CompanyTypeID,
            Name
        }

        //Object.Property Scope Validation...
        private bool CompanyTypeIsValid(CompanyType item, _itemValidationProperties validationProperties, out string errorMessage)
        {
            errorMessage = "";
            switch (validationProperties)
            {
                case _itemValidationProperties.CompanyTypeID:
                    //validate key
                    if (string.IsNullOrEmpty(item.CompanyTypeID))
                    {
                        errorMessage = "ID Is Required.";
                        return false;
                    }
                    EntityStates entityState = GetCompanyTypeState(item);
                    if (entityState == EntityStates.Added && CompanyTypeExists(item.CompanyTypeID))
                    {
                        errorMessage = "Item All Ready Exists...";
                        return false;
                    }
                    //check cached list for duplicates...
                    int count = CompanyTypeList.Count(q => q.CompanyTypeID == item.CompanyTypeID);
                    if (count > 1)
                    {
                        errorMessage = "Item All Ready Exists...";
                        return false;
                    }

                    break;
                case _itemValidationProperties.Name:
                    //validate Description
                    if (string.IsNullOrEmpty(item.Description))
                    {
                        errorMessage = "Description Is Required.";
                        return false;
                    }
                    break;
            }
            return true;
        }
        //CompanyType Object Scope Validation check the entire object for validity...
        private byte CompanyTypeIsValid(CompanyType item, out string errorMessage)
        {   //validate key
            errorMessage = "";
            if (string.IsNullOrEmpty(item.CompanyTypeID))
            {
                errorMessage = "ID Is Required.";
                return 1;
            }
            EntityStates entityState = GetCompanyTypeState(item);
            if (entityState == EntityStates.Added && CompanyTypeExists(item.CompanyTypeID))
            {
                errorMessage = "Item AllReady Exists.";
                return 1;
            }
            int count = CompanyTypeList.Count(q => q.CompanyTypeID == item.CompanyTypeID);
            if (count > 1)
            {
                errorMessage = "Item AllReady Exists.";
                return 1;
            }
            //validate Description
            if (string.IsNullOrEmpty(item.Description))
            {
                errorMessage = "Description Is Required.";
                return 1;
            }
            //a value of 2 is pending changes...
            //On Commit we will give it a value of 0...
            return 2;
        }
        #endregion Validation Methods
        #endregion ViewModel Logic Methods

        #region ServiceAgent Call Methods
        private EntityStates GetCompanyTypeState(CompanyType item)
        {
            return _serviceAgent.GetCompanyTypeEntityState(item);
        }

        //check to see if the repository has pending changes...
        private bool RepositoryIsDirty()
        {
            return _serviceAgent.CompanyRepositoryIsDirty();
        }

        #region CompanyType CRUD
        private void Refresh()
        {

            //refetch current records...
            long selectedAutoID = SelectedCompanyType.AutoID;
            string autoIDs = "";
            //bool isFirstItem = true;
            foreach (CompanyType item in CompanyTypeList)
            {//auto seeded starts at 1 any records at 0 or less or not valid records...
                if (item.AutoID > 0)
                    autoIDs = autoIDs + item.AutoID.ToString() + ",";
            }
            if (autoIDs.Length > 0)
            {
                //ditch the extra comma...
                autoIDs = autoIDs.Remove(autoIDs.Length - 1, 1);
                CompanyTypeList = new BindingList<CompanyType>(_serviceAgent.RefreshCompanyType(autoIDs).ToList());
                SelectedCompanyType = (from q in CompanyTypeList
                                   where q.AutoID == selectedAutoID
                                   select q).FirstOrDefault();
                Dirty = false;
                AllowCommit = false;
            }
        }

        private BindingList<CompanyType> GetCompanyTypes()
        {
            BindingList<CompanyType> itemList = new BindingList<CompanyType>(_serviceAgent.GetCompanyTypes().ToList());
            Dirty = false;
            AllowCommit = false;
            return itemList;
        }

        private BindingList<CompanyType> GetCompanyTypes(CompanyType item)
        {
            BindingList<CompanyType> itemList = new BindingList<CompanyType>(_serviceAgent.GetCompanyTypes(item).ToList());
            Dirty = false;
            AllowCommit = false;
            return itemList;
        }

        private BindingList<CompanyType> GetCompanyTypeByID(string id)
        {
            BindingList<CompanyType> itemList = new BindingList<CompanyType>(_serviceAgent.GetCompanyTypeByID(id).ToList());
            Dirty = false;
            AllowCommit = false;
            return itemList;
        }

        private bool CompanyTypeExists(string itemID)
        {
            return _serviceAgent.CompanyTypeExists(itemID);
        }
        //udpate merely updates the repository a commit is required 
        //to commit it to the db...
        private bool Update(CompanyType item)
        {
            _serviceAgent.UpdateCompanyTypeRepository(item);
            Dirty = true;
            if (CommitIsAllowed())
                AllowCommit = true;
            else
                AllowCommit = false;
            return AllowCommit;
        }
        //commits repository to the db...
        private bool Commit()
        {//search non respository UI list for pending saved marked records and mark them as valid...
            var items = (from q in CompanyTypeList where q.IsValid == 2 select q).ToList();
            foreach (CompanyType item in items)
            {
                item.IsValid = 0;
                item.NotValidMessage = null;
            }

            _serviceAgent.CommitCompanyTypeRepository();
            Dirty = false;
            AllowCommit = false;
            return true;
        }

        private bool Delete(CompanyType item)
        {//deletes are done indenpendently of the repository as a delete will not commit 
            //dirty records it will simply just delete the record...
            _serviceAgent.DeleteFromCompanyTypeRepository(item);
            return true;
        }

        private bool NewCompanyType(string itemID)
        {
            CompanyType item = new CompanyType();
            _newCompanyTypeAutoId = _newCompanyTypeAutoId - 1;
            item.AutoID = _newCompanyTypeAutoId;
            item.CompanyTypeID = itemID;
            item.IsValid = 1;
            item.NotValidMessage = "New Record Key Field/s Are Required.";
            CompanyTypeList.Add(item);
            _serviceAgent.AddToCompanyTypeRepository(item);
            SelectedCompanyType = CompanyTypeList.LastOrDefault();

            AllowEdit = true;
            Dirty = false;
            return true;
        }

        #endregion CompanyType CRUD
        #endregion ServiceAgent Call Methods
        #endregion Methods

        #region Commands
        public void PasteRowCommand()
        {
            try
            {
                CompanyTypeColumnMetaDataList.Sort(delegate(ColumnMetaData c1, ColumnMetaData c2)
                { return c1.Order.CompareTo(c2.Order); });

                char[] rowSplitter = { '\r', '\n' };
                char[] columnSplitter = { '\t' };
                //get the text from clipboard
                IDataObject dataInClipboard = Clipboard.GetDataObject();
                string stringInClipboard = (string)dataInClipboard.GetData(DataFormats.Text);
                //split it into rows...
                string[] rowsInClipboard = stringInClipboard.Split(rowSplitter, StringSplitOptions.RemoveEmptyEntries);

                foreach (string row in rowsInClipboard)
                {
                    NewCompanyTypeCommand(""); //this will generate a new item and set it as the selected item...
                    //split row into cell values
                    string[] valuesInRow = row.Split(columnSplitter);
                    int i = 0;
                    foreach (string columnValue in valuesInRow)
                    {
                        SelectedCompanyType.SetPropertyValue(CompanyTypeColumnMetaDataList[i].Name, columnValue);
                        i++;
                    }
                }
            }
            catch (Exception ex)
            {
                NotifyMessage(ex.InnerException.ToString());
            }
        }

        public void SaveCommand()
        {
            if (GetCompanyTypeState(SelectedCompanyType) != EntityStates.Detached)
            {
                if (Update(SelectedCompanyType))
                    Commit();
                else//if and where we have a hole in our allowcommit logic...
                    NotifyMessage("Save Failed Check Your Work And Try Again...");
            }
        }
        public void RefreshCommand()
        {
            Refresh();
        }
        public void DeleteCompanyTypeCommand()
        {
            try
            {
                int i = 0;
                int ii = 0;
                for (int j = SelectedCompanyTypeList.Count - 1; j >= 0; j--)
                {
                    CompanyType item = (CompanyType)SelectedCompanyTypeList[j];
                    //get Max Index...
                    i = CompanyTypeList.IndexOf(item);
                    if (i > ii)
                        ii = i;
                    Delete(item);
                    CompanyTypeList.Remove(item);
                }

                if (CompanyTypeList != null && CompanyTypeList.Count > 0)
                {
                    //back off one index from the max index...
                    ii = ii - 1;

                    //if they delete the first row...
                    if (ii < 0)
                        ii = 0;

                    //make sure it does not exceed the list count...
                    if (ii >= CompanyTypeList.Count())
                        ii = CompanyTypeList.Count - 1;

                    SelectedCompanyType = CompanyTypeList[ii];
                    //we will only enable committ for dirty validated records...
                    if (Dirty == true)
                        AllowCommit = CommitIsAllowed();
                    else
                        AllowCommit = false;
                }
                else//only one record, deleting will result in no records...
                    SetAsEmptySelection();
            }//we try catch item delete as it may be used in another table as a key...
            //As well we will force a refresh to sqare up the UI after the botched delete...
            catch
            {
                NotifyMessage("CompanyType/s Can Not Be Deleted.  Contact XERP Admin For More Details.");
                Refresh();
            }
        }

        
        public void NewCompanyTypeCommand()
        {
            NewCompanyType("");
            AllowCommit = false;
        }

        public void NewCompanyTypeCommand(string itemID)
        {
            NewCompanyType(itemID);
            if (string.IsNullOrEmpty(itemID))//don't allow a save until a securityGroupCodeID is provided...
                AllowCommit = false;
            else
                AllowCommit = CommitIsAllowed();
        }

        public void ClearCommand()
        {
            if (Dirty && AllowCommit)
                NotifySaveRequired("Do you want to save changes?", _saveRequiredResultActions.ClearLogic);
            else
                ClearLogic();
        }

        public void SearchCommand()
        {
            if (Dirty && AllowCommit)
                NotifySaveRequired("Do you want to save changes?", _saveRequiredResultActions.SearchLogic);
            else
                SearchLogic();
        }

        private void SearchLogic()
        {
            RegisterToReceiveMessages<BindingList<CompanyType>>(MessageTokens.CompanyTypeSearchToken.ToString(), OnSearchResult);
            NotifySearch("");
        }

        private void OnSearchResult(object sender, NotificationEventArgs<BindingList<CompanyType>> e)
        {
            if (e.Data != null && e.Data.Count > 0)
            {
                CompanyTypeList = e.Data;
                SelectedCompanyType = CompanyTypeList.FirstOrDefault();
                Dirty = false;
                AllowCommit = false;
            }
            UnregisterToReceiveMessages<BindingList<CompanyType>>(MessageTokens.CompanyTypeSearchToken.ToString(), OnSearchResult);
        }

        #endregion Commands

        #region Completion Callbacks

        // TODO: Optionally add callback methods for async calls to the service agent

        #endregion Completion Callbacks

        #region Helpers
        // Helper method to notify View of an error
        private void NotifyError(string message, Exception error)
        {// Notify view of an error
            Notify(ErrorNotice, new NotificationEventArgs<Exception>(message, error));
        }
        private void NotifyMessage(string message)
        {// Notify view of an error message w/o throwing an error...
            Notify(MessageNotice, new NotificationEventArgs<Exception>(message));
        }
        
        private void NotifySearch(string message)
        {//Notify view to launch search...
            Notify(SearchNotice, new NotificationEventArgs(message));
        }

        private void NotifyNewRecordNeeded(string message)
        {//Notify view new record may be required...
            Notify(NewRecordNeededNotice, new NotificationEventArgs<bool, MessageBoxResult>
            (message, true, result => { OnNewRecordNeededResult(result); }));
        }

        private void OnNewRecordNeededResult(MessageBoxResult result)
        {
            switch (result)
            {
                case MessageBoxResult.No:
                    //revert back...
                    SelectedCompanyType.CompanyTypeID = SelectedCompanyTypeMirror.CompanyTypeID;
                    break;
                case MessageBoxResult.Yes:
                    //create new record with CompanyTypeID provided...
                    NewCompanyTypeCommand(SelectedCompanyType.CompanyTypeID);
                    break;
                case MessageBoxResult.Cancel:
                    //revert back...
                    SelectedCompanyType.CompanyTypeID = SelectedCompanyTypeMirror.CompanyTypeID;
                    break;
            }
        }

        private void NotifySaveRequired(string message, _saveRequiredResultActions resultAction)
        {//Notify view save may be required...
            Notify(SaveRequiredNotice, new NotificationEventArgs<bool, MessageBoxResult>
            (message, true, result => { OnSaveResult(result, resultAction); }));
        }

        private void OnSaveResult(MessageBoxResult result, _saveRequiredResultActions resultAction)
        {
            switch (result)
            {
                case MessageBoxResult.No:
                    CaseSaveResultActions(resultAction);
                    break;
                case MessageBoxResult.Yes:
                    //note a commit validation was allready done...
                    _serviceAgent.CommitCompanyTypeRepository();
                    CaseSaveResultActions(resultAction);
                    break;
                case MessageBoxResult.Cancel:
                    //revert back...
                    SelectedCompanyType.CompanyTypeID = SelectedCompanyTypeMirror.CompanyTypeID;
                    break;
            }
        }

        private void CaseSaveResultActions(_saveRequiredResultActions resultAction)
        {
            switch (resultAction)
            {
                case _saveRequiredResultActions.ChangeKeyLogic:
                    ChangeKeyLogic();
                    break;
                case _saveRequiredResultActions.SearchLogic:
                    SearchLogic();
                    break;
                case _saveRequiredResultActions.ClearLogic:
                    ClearLogic();
                    break;
            }
        }
        #endregion Helpers
    }
}

namespace ExtensionMethods
{
    public static partial class XERPExtensions
    {
        public static object GetPropertyValue(this CompanyType myObj, string propertyName)
        {
            var propInfo = typeof(CompanyType).GetProperty(propertyName);
            if (propInfo != null)
                return propInfo.GetValue(myObj, null);
            else
                return string.Empty;
        }

        public static string GetPropertyType(this CompanyType myObj, string propertyName)
        {
            var propInfo = typeof(CompanyType).GetProperty(propertyName);
            if (propInfo != null)
                return propInfo.PropertyType.Name.ToString();
            else
                return null;
        }

        public static void SetPropertyValue(this CompanyType myObj, object propertyName, object propertyValue)
        {
            var propInfo = typeof(CompanyType).GetProperty((string)propertyName);
            if (propInfo != null)
                propInfo.SetValue(myObj, propertyValue, null);
        }
    }
}