﻿
namespace XERP.Client.WPF.WarehouseMaintenance
{//use this class to share properties amonst views, viewmodels etc...
    public class GlobalProperties
    {//WarehouseMaintenace is the exutable program that this Maintenance UI is tied to...
        //All UI's used within this project will inherit this executableprogram's security privelages...
        //this program name coencides with db table ExecutablPrograms...
        //Warehouses are then applied to the ExecutablePrograms allotting for custom form athentication...
        private const string _executableProgramName = "WarehouseMaintenance";
        public string ExecutableProgramName
        {
            get { return _executableProgramName; }
        } 

    }
}
