using System.Collections.Generic;
using AccuSync.Application.Resources;
using AccuSync.Application.Resources.Constants;

namespace AccuSync.Application.Models
{
    /// <summary>
    /// Factory that returns a <see cref="RibbonDefinition"/> for each screen tab.
    /// "Content takeover" methods (Profiles, Facilities, etc.) define the toolbar
    /// shown when a sub-screen replaces the parent tab's content area.
    /// </summary>
    // TODO: Many takeover toolbars are identical (Back + CRUD + Save/Revert/Undo + Help).
    //       Create a shared builder method to reduce the copy-paste.
    public static class RibbonDefinitions
    {
        // Patients — Edit/Delete live on the Patient Info panel pills;
        //            Search lives in the Patient List's built-in filter.
        public static RibbonDefinition Patients()
        {
            return new RibbonDefinition(
                new RibbonGroup(Strings.Ribbon_Patient,
                    RibbonItem.CreateButton("Add", Strings.Ribbon_Add, RibbonIcons.Add)
                ),
                new RibbonGroup(Strings.Ribbon_DataManagement,
                    RibbonItem.CreateButton("Import", Strings.Ribbon_Import, RibbonIcons.Import),
                    RibbonItem.CreateButton("Export", Strings.Ribbon_Export, RibbonIcons.Export),
                    RibbonItem.CreateButton("Print", Strings.Ribbon_Print, RibbonIcons.Print)
                ),
                new RibbonGroup(Strings.Ribbon_Device,
                    RibbonItem.CreateButton("Send", Strings.Ribbon_Send, RibbonIcons.Send),
                    RibbonItem.CreateButton("Receive", Strings.Ribbon_Receive, RibbonIcons.Receive),
                    RibbonItem.CreateButton("UpdateDevice", Strings.Ribbon_Update, RibbonIcons.UpdateDevice)
                ),
                new RibbonGroup(Strings.Ribbon_Help,
                    RibbonItem.CreateButton("Help", Strings.Ribbon_Help, RibbonIcons.Help)
                )
            );
        }

        // Users & Profiles
        public static RibbonDefinition Users()
        {
            return new RibbonDefinition(
                new RibbonGroup(Strings.Ribbon_User,
                    RibbonItem.CreateButton("Add", Strings.Ribbon_Add, RibbonIcons.Add),
                    RibbonItem.CreateButton("Edit", Strings.Ribbon_Edit, RibbonIcons.Edit),
                    RibbonItem.CreateButton("Delete", Strings.Ribbon_Delete, RibbonIcons.Delete),
                    RibbonItem.CreateButton("Unlock", Strings.Ribbon_Unlock, RibbonIcons.Unlock)
                ),
                new RibbonGroup(Strings.Ribbon_Actions,
                    RibbonItem.CreateButton("Save", Strings.Ribbon_Save, RibbonIcons.Save),
                    RibbonItem.CreateButton("Revert", Strings.Ribbon_Revert, RibbonIcons.Revert),
                    RibbonItem.CreateButton("Undo", Strings.Ribbon_Undo, RibbonIcons.Undo)
                ),
                new RibbonGroup(Strings.Ribbon_Configuration,
                    RibbonItem.CreateButton("Profiles", Strings.Ribbon_Profiles, RibbonIcons.Profiles)
                ),
                new RibbonGroup(Strings.Ribbon_Device,
                    RibbonItem.CreateButton("UpdateDevice", Strings.Ribbon_Update, RibbonIcons.UpdateDevice)
                ),
                new RibbonGroup(Strings.Ribbon_Help,
                    RibbonItem.CreateButton("Help", Strings.Ribbon_Help, RibbonIcons.Help)
                )
            );
        }

        // Profiles (takeover inside Users)
        public static RibbonDefinition Profiles()
        {
            return new RibbonDefinition(
                new RibbonGroup(Strings.Ribbon_Navigation,
                    RibbonItem.CreateButton("Back", Strings.Ribbon_Back, RibbonIcons.Back)
                ),
                new RibbonGroup(Strings.Ribbon_Profile,
                    RibbonItem.CreateButton("Add", Strings.Ribbon_Add, RibbonIcons.Add),
                    RibbonItem.CreateButton("Edit", Strings.Ribbon_Edit, RibbonIcons.Edit),
                    RibbonItem.CreateButton("Delete", Strings.Ribbon_Delete, RibbonIcons.Delete)
                ),
                new RibbonGroup(Strings.Ribbon_Actions,
                    RibbonItem.CreateButton("Save", Strings.Ribbon_Save, RibbonIcons.Save),
                    RibbonItem.CreateButton("Revert", Strings.Ribbon_Revert, RibbonIcons.Revert),
                    RibbonItem.CreateButton("Undo", Strings.Ribbon_Undo, RibbonIcons.Undo)
                ),
                new RibbonGroup(Strings.Ribbon_Help,
                    RibbonItem.CreateButton("Help", Strings.Ribbon_Help, RibbonIcons.Help)
                )
            );
        }

        // Sites & Facilities
        public static RibbonDefinition Sites()
        {
            return new RibbonDefinition(
                new RibbonGroup(Strings.Ribbon_Site,
                    RibbonItem.CreateButton("Add", Strings.Ribbon_Add, RibbonIcons.Add),
                    RibbonItem.CreateButton("Edit", Strings.Ribbon_Edit, RibbonIcons.Edit),
                    RibbonItem.CreateButton("Delete", Strings.Ribbon_Delete, RibbonIcons.Delete)
                ),
                new RibbonGroup(Strings.Ribbon_Actions,
                    RibbonItem.CreateButton("Save", Strings.Ribbon_Save, RibbonIcons.Save),
                    RibbonItem.CreateButton("Revert", Strings.Ribbon_Revert, RibbonIcons.Revert),
                    RibbonItem.CreateButton("Undo", Strings.Ribbon_Undo, RibbonIcons.Undo)
                ),
                new RibbonGroup(Strings.Ribbon_Configuration,
                    RibbonItem.CreateButton("Facilities", Strings.Ribbon_Facilities, RibbonIcons.Facilities),
                    RibbonItem.CreateButton("Location", Strings.Ribbon_Location, RibbonIcons.Location)
                ),
                new RibbonGroup(Strings.Ribbon_Device,
                    RibbonItem.CreateButton("UpdateDevice", Strings.Ribbon_Update, RibbonIcons.UpdateDevice)
                ),
                new RibbonGroup(Strings.Ribbon_Help,
                    RibbonItem.CreateButton("Help", Strings.Ribbon_Help, RibbonIcons.Help)
                )
            );
        }

        // Facilities (takeover inside Sites)
        public static RibbonDefinition Facilities()
        {
            return new RibbonDefinition(
                new RibbonGroup(Strings.Ribbon_Navigation,
                    RibbonItem.CreateButton("Back", Strings.Ribbon_Back, RibbonIcons.Back)
                ),
                new RibbonGroup(Strings.Ribbon_Facility,
                    RibbonItem.CreateButton("Add", Strings.Ribbon_Add, RibbonIcons.Add),
                    RibbonItem.CreateButton("Edit", Strings.Ribbon_Edit, RibbonIcons.Edit),
                    RibbonItem.CreateButton("Delete", Strings.Ribbon_Delete, RibbonIcons.Delete)
                ),
                new RibbonGroup(Strings.Ribbon_Actions,
                    RibbonItem.CreateButton("Save", Strings.Ribbon_Save, RibbonIcons.Save),
                    RibbonItem.CreateButton("Revert", Strings.Ribbon_Revert, RibbonIcons.Revert),
                    RibbonItem.CreateButton("Undo", Strings.Ribbon_Undo, RibbonIcons.Undo)
                ),
                new RibbonGroup(Strings.Ribbon_Help,
                    RibbonItem.CreateButton("Help", Strings.Ribbon_Help, RibbonIcons.Help)
                )
            );
        }

        // Locations (takeover inside Sites)
        public static RibbonDefinition Locations()
        {
            return new RibbonDefinition(
                new RibbonGroup(Strings.Ribbon_Navigation,
                    RibbonItem.CreateButton("Back", Strings.Ribbon_Back, RibbonIcons.Back)
                ),
                new RibbonGroup(Strings.Ribbon_Location,
                    RibbonItem.CreateButton("Add", Strings.Ribbon_Add, RibbonIcons.Add),
                    RibbonItem.CreateButton("Edit", Strings.Ribbon_Edit, RibbonIcons.Edit),
                    RibbonItem.CreateButton("Delete", Strings.Ribbon_Delete, RibbonIcons.Delete)
                ),
                new RibbonGroup(Strings.Ribbon_Actions,
                    RibbonItem.CreateButton("Save", Strings.Ribbon_Save, RibbonIcons.Save),
                    RibbonItem.CreateButton("Revert", Strings.Ribbon_Revert, RibbonIcons.Revert),
                    RibbonItem.CreateButton("Undo", Strings.Ribbon_Undo, RibbonIcons.Undo)
                ),
                new RibbonGroup(Strings.Ribbon_Help,
                    RibbonItem.CreateButton("Help", Strings.Ribbon_Help, RibbonIcons.Help)
                )
            );
        }

        // Device Management
        public static RibbonDefinition Devices()
        {
            return new RibbonDefinition(
                new RibbonGroup(Strings.Ribbon_DeviceManagement,
                    RibbonItem.CreateButton("Add", Strings.Ribbon_Add, RibbonIcons.Add),
                    RibbonItem.CreateButton("Edit", Strings.Ribbon_Edit, RibbonIcons.Edit),
                    RibbonItem.CreateButton("Delete", Strings.Ribbon_Delete, RibbonIcons.Delete)
                ),
                new RibbonGroup(Strings.Ribbon_Actions,
                    RibbonItem.CreateButton("Save", Strings.Ribbon_Save, RibbonIcons.Save),
                    RibbonItem.CreateButton("Revert", Strings.Ribbon_Revert, RibbonIcons.Revert),
                    RibbonItem.CreateButton("Undo", Strings.Ribbon_Undo, RibbonIcons.Undo)
                ),
                new RibbonGroup(Strings.Ribbon_Configuration,
                    RibbonItem.CreateButton("ABR", Strings.Ribbon_ABR, RibbonIcons.ABR),
                    RibbonItem.CreateButton("DPOAE", Strings.Ribbon_DPOAE, RibbonIcons.DPOAE),
                    RibbonItem.CreateButton("DeviceFieldSetup", Strings.Ribbon_FieldSetup, RibbonIcons.FieldSetup),
                    RibbonItem.CreateButton("Firmware", Strings.Ribbon_Firmware, RibbonIcons.Firmware)
                ),
                new RibbonGroup(Strings.Ribbon_Device,
                    RibbonItem.CreateButton("UpdateDevice", Strings.Ribbon_Update, RibbonIcons.UpdateDevice)
                ),
                new RibbonGroup(Strings.Ribbon_Help,
                    RibbonItem.CreateButton("Help", Strings.Ribbon_Help, RibbonIcons.Help)
                )
            );
        }

        // ABR (takeover inside Devices)
        public static RibbonDefinition ABR()
        {
            return new RibbonDefinition(
                new RibbonGroup(Strings.Ribbon_Navigation,
                    RibbonItem.CreateButton("Back", Strings.Ribbon_Back, RibbonIcons.Back)
                ),
                new RibbonGroup(Strings.Ribbon_ABRProtocol,
                    RibbonItem.CreateButton("Add", Strings.Ribbon_Add, RibbonIcons.Add),
                    RibbonItem.CreateButton("Edit", Strings.Ribbon_Edit, RibbonIcons.Edit),
                    RibbonItem.CreateButton("Delete", Strings.Ribbon_Delete, RibbonIcons.Delete)
                ),
                new RibbonGroup(Strings.Ribbon_Actions,
                    RibbonItem.CreateButton("Save", Strings.Ribbon_Save, RibbonIcons.Save),
                    RibbonItem.CreateButton("Revert", Strings.Ribbon_Revert, RibbonIcons.Revert),
                    RibbonItem.CreateButton("Undo", Strings.Ribbon_Undo, RibbonIcons.Undo)
                ),
                new RibbonGroup(Strings.Ribbon_Help,
                    RibbonItem.CreateButton("Help", Strings.Ribbon_Help, RibbonIcons.Help)
                )
            );
        }

        // DPOAE (takeover inside Devices)
        public static RibbonDefinition DPOAE()
        {
            return new RibbonDefinition(
                new RibbonGroup(Strings.Ribbon_Navigation,
                    RibbonItem.CreateButton("Back", Strings.Ribbon_Back, RibbonIcons.Back)
                ),
                new RibbonGroup(Strings.Ribbon_DPOAEProtocol,
                    RibbonItem.CreateButton("Add", Strings.Ribbon_Add, RibbonIcons.Add),
                    RibbonItem.CreateButton("Edit", Strings.Ribbon_Edit, RibbonIcons.Edit),
                    RibbonItem.CreateButton("Delete", Strings.Ribbon_Delete, RibbonIcons.Delete)
                ),
                new RibbonGroup(Strings.Ribbon_Actions,
                    RibbonItem.CreateButton("Save", Strings.Ribbon_Save, RibbonIcons.Save),
                    RibbonItem.CreateButton("Revert", Strings.Ribbon_Revert, RibbonIcons.Revert),
                    RibbonItem.CreateButton("Undo", Strings.Ribbon_Undo, RibbonIcons.Undo)
                ),
                new RibbonGroup(Strings.Ribbon_Help,
                    RibbonItem.CreateButton("Help", Strings.Ribbon_Help, RibbonIcons.Help)
                )
            );
        }

        // Device Field Setup (takeover inside Devices)
        public static RibbonDefinition DeviceFieldSetup()
        {
            return new RibbonDefinition(
                new RibbonGroup(Strings.Ribbon_Navigation,
                    RibbonItem.CreateButton("Back", Strings.Ribbon_Back, RibbonIcons.Back)
                ),
                new RibbonGroup(Strings.Ribbon_Actions,
                    RibbonItem.CreateButton("Save", Strings.Ribbon_Save, RibbonIcons.Save),
                    RibbonItem.CreateButton("Revert", Strings.Ribbon_Revert, RibbonIcons.Revert),
                    RibbonItem.CreateButton("Undo", Strings.Ribbon_Undo, RibbonIcons.Undo)
                ),
                new RibbonGroup(Strings.Ribbon_Help,
                    RibbonItem.CreateButton("Help", Strings.Ribbon_Help, RibbonIcons.Help)
                )
            );
        }

        // System Configuration
        public static RibbonDefinition SystemConfig()
        {
            return new RibbonDefinition(
                new RibbonGroup(Strings.Ribbon_Actions,
                    RibbonItem.CreateButton("Save", Strings.Ribbon_Save, RibbonIcons.Save),
                    RibbonItem.CreateButton("Revert", Strings.Ribbon_Revert, RibbonIcons.Revert),
                    RibbonItem.CreateButton("Undo", Strings.Ribbon_Undo, RibbonIcons.Undo)
                ),
                new RibbonGroup(Strings.Ribbon_PatientsTests,
                    RibbonItem.CreateButton("RiskFactors", Strings.Ribbon_RiskFactors, RibbonIcons.RiskFactors),
                    RibbonItem.CreateButton("Comments", Strings.Ribbon_Comments, RibbonIcons.Comments),
                    RibbonItem.CreateButton("FieldSetup", Strings.Ribbon_FieldSetup, RibbonIcons.FieldSetup)
                ),
                new RibbonGroup(Strings.Ribbon_Management,
                    RibbonItem.CreateButton("UserProfile", Strings.Ribbon_UserProfile, RibbonIcons.UserManagement),
                    RibbonItem.CreateButton("SiteFacility", Strings.Ribbon_SiteFacility, RibbonIcons.SiteManagement)
                ),
                new RibbonGroup(Strings.Ribbon_DataExchange,
                    RibbonItem.CreateButton("ImportConfig", Strings.Ribbon_Import, RibbonIcons.Import),
                    RibbonItem.CreateButton("ExportConfig", Strings.Ribbon_Export, RibbonIcons.Export)
                ),
                new RibbonGroup(Strings.Ribbon_Device,
                    RibbonItem.CreateButton("UpdateDevice", Strings.Ribbon_Update, RibbonIcons.UpdateDevice)
                ),
                new RibbonGroup(Strings.Ribbon_Help,
                    RibbonItem.CreateButton("Help", Strings.Ribbon_Help, RibbonIcons.Help)
                )
            );
        }

        // Risk Factors (takeover inside SystemConfig)
        public static RibbonDefinition RiskFactors()
        {
            return new RibbonDefinition(
                new RibbonGroup(Strings.Ribbon_Navigation,
                    RibbonItem.CreateButton("Back", Strings.Ribbon_Back, RibbonIcons.Back)
                ),
                new RibbonGroup(Strings.Ribbon_RiskFactor,
                    RibbonItem.CreateButton("Add", Strings.Ribbon_Add, RibbonIcons.Add),
                    RibbonItem.CreateButton("Edit", Strings.Ribbon_Edit, RibbonIcons.Edit),
                    RibbonItem.CreateButton("Delete", Strings.Ribbon_Delete, RibbonIcons.Delete)
                ),
                new RibbonGroup(Strings.Ribbon_Actions,
                    RibbonItem.CreateButton("Save", Strings.Ribbon_Save, RibbonIcons.Save),
                    RibbonItem.CreateButton("Revert", Strings.Ribbon_Revert, RibbonIcons.Revert),
                    RibbonItem.CreateButton("Undo", Strings.Ribbon_Undo, RibbonIcons.Undo)
                ),
                new RibbonGroup(Strings.Ribbon_Help,
                    RibbonItem.CreateButton("Help", Strings.Ribbon_Help, RibbonIcons.Help)
                )
            );
        }

        // Comments (takeover inside SystemConfig)
        public static RibbonDefinition Comments()
        {
            return new RibbonDefinition(
                new RibbonGroup(Strings.Ribbon_Navigation,
                    RibbonItem.CreateButton("Back", Strings.Ribbon_Back, RibbonIcons.Back)
                ),
                new RibbonGroup(Strings.Ribbon_Comment,
                    RibbonItem.CreateButton("Add", Strings.Ribbon_Add, RibbonIcons.Add),
                    RibbonItem.CreateButton("Edit", Strings.Ribbon_Edit, RibbonIcons.Edit),
                    RibbonItem.CreateButton("Delete", Strings.Ribbon_Delete, RibbonIcons.Delete)
                ),
                new RibbonGroup(Strings.Ribbon_Actions,
                    RibbonItem.CreateButton("Save", Strings.Ribbon_Save, RibbonIcons.Save),
                    RibbonItem.CreateButton("Revert", Strings.Ribbon_Revert, RibbonIcons.Revert),
                    RibbonItem.CreateButton("Undo", Strings.Ribbon_Undo, RibbonIcons.Undo)
                ),
                new RibbonGroup(Strings.Ribbon_Help,
                    RibbonItem.CreateButton("Help", Strings.Ribbon_Help, RibbonIcons.Help)
                )
            );
        }

        // User & Profile Config (takeover inside SystemConfig)
        public static RibbonDefinition UserProfileConfig()
        {
            return new RibbonDefinition(
                new RibbonGroup(Strings.Ribbon_Navigation,
                    RibbonItem.CreateButton("Back", Strings.Ribbon_Back, RibbonIcons.Back)
                ),
                new RibbonGroup(Strings.Ribbon_Actions,
                    RibbonItem.CreateButton("Save", Strings.Ribbon_Save, RibbonIcons.Save),
                    RibbonItem.CreateButton("Revert", Strings.Ribbon_Revert, RibbonIcons.Revert),
                    RibbonItem.CreateButton("Undo", Strings.Ribbon_Undo, RibbonIcons.Undo)
                ),
                new RibbonGroup(Strings.Ribbon_Help,
                    RibbonItem.CreateButton("Help", Strings.Ribbon_Help, RibbonIcons.Help)
                )
            );
        }

        // Field Setup Config (takeover inside SystemConfig)
        public static RibbonDefinition FieldSetupConfig()
        {
            return new RibbonDefinition(
                new RibbonGroup(Strings.Ribbon_Navigation,
                    RibbonItem.CreateButton("Back", Strings.Ribbon_Back, RibbonIcons.Back)
                ),
                new RibbonGroup(Strings.Ribbon_Actions,
                    RibbonItem.CreateButton("Save", Strings.Ribbon_Save, RibbonIcons.Save),
                    RibbonItem.CreateButton("Revert", Strings.Ribbon_Revert, RibbonIcons.Revert),
                    RibbonItem.CreateButton("Undo", Strings.Ribbon_Undo, RibbonIcons.Undo)
                ),
                new RibbonGroup(Strings.Ribbon_Help,
                    RibbonItem.CreateButton("Help", Strings.Ribbon_Help, RibbonIcons.Help)
                )
            );
        }

        // Site & Facility Config (takeover inside SystemConfig)
        public static RibbonDefinition SiteFacilityConfig()
        {
            return new RibbonDefinition(
                new RibbonGroup(Strings.Ribbon_Navigation,
                    RibbonItem.CreateButton("Back", Strings.Ribbon_Back, RibbonIcons.Back)
                ),
                new RibbonGroup(Strings.Ribbon_Actions,
                    RibbonItem.CreateButton("Save", Strings.Ribbon_Save, RibbonIcons.Save),
                    RibbonItem.CreateButton("Revert", Strings.Ribbon_Revert, RibbonIcons.Revert),
                    RibbonItem.CreateButton("Undo", Strings.Ribbon_Undo, RibbonIcons.Undo)
                ),
                new RibbonGroup(Strings.Ribbon_Help,
                    RibbonItem.CreateButton("Help", Strings.Ribbon_Help, RibbonIcons.Help)
                )
            );
        }

        // Import Config (takeover inside SystemConfig)
        public static RibbonDefinition ImportConfig()
        {
            return new RibbonDefinition(
                new RibbonGroup(Strings.Ribbon_Navigation,
                    RibbonItem.CreateButton("Back", Strings.Ribbon_Back, RibbonIcons.Back)
                ),
                new RibbonGroup(Strings.Ribbon_ImportConfig,
                    RibbonItem.CreateButton("Add", Strings.Ribbon_Add, RibbonIcons.Add),
                    RibbonItem.CreateButton("Edit", Strings.Ribbon_Edit, RibbonIcons.Edit),
                    RibbonItem.CreateButton("Delete", Strings.Ribbon_Delete, RibbonIcons.Delete)
                ),
                new RibbonGroup(Strings.Ribbon_Actions,
                    RibbonItem.CreateButton("Save", Strings.Ribbon_Save, RibbonIcons.Save),
                    RibbonItem.CreateButton("Revert", Strings.Ribbon_Revert, RibbonIcons.Revert),
                    RibbonItem.CreateButton("Undo", Strings.Ribbon_Undo, RibbonIcons.Undo)
                ),
                new RibbonGroup(Strings.Ribbon_Help,
                    RibbonItem.CreateButton("Help", Strings.Ribbon_Help, RibbonIcons.Help)
                )
            );
        }

        // Export Config (takeover inside SystemConfig)
        public static RibbonDefinition ExportConfig()
        {
            return new RibbonDefinition(
                new RibbonGroup(Strings.Ribbon_Navigation,
                    RibbonItem.CreateButton("Back", Strings.Ribbon_Back, RibbonIcons.Back)
                ),
                new RibbonGroup(Strings.Ribbon_ExportConfig,
                    RibbonItem.CreateButton("Add", Strings.Ribbon_Add, RibbonIcons.Add),
                    RibbonItem.CreateButton("Edit", Strings.Ribbon_Edit, RibbonIcons.Edit),
                    RibbonItem.CreateButton("Delete", Strings.Ribbon_Delete, RibbonIcons.Delete)
                ),
                new RibbonGroup(Strings.Ribbon_Actions,
                    RibbonItem.CreateButton("Save", Strings.Ribbon_Save, RibbonIcons.Save),
                    RibbonItem.CreateButton("Revert", Strings.Ribbon_Revert, RibbonIcons.Revert),
                    RibbonItem.CreateButton("Undo", Strings.Ribbon_Undo, RibbonIcons.Undo)
                ),
                new RibbonGroup(Strings.Ribbon_Help,
                    RibbonItem.CreateButton("Help", Strings.Ribbon_Help, RibbonIcons.Help)
                )
            );
        }

        // Settings
        public static RibbonDefinition Settings()
        {
            return new RibbonDefinition(
                new RibbonGroup(Strings.Ribbon_Actions,
                    RibbonItem.CreateButton("Save", Strings.Ribbon_Save, RibbonIcons.Save),
                    RibbonItem.CreateButton("Revert", Strings.Ribbon_Revert, RibbonIcons.Revert),
                    RibbonItem.CreateButton("Undo", Strings.Ribbon_Undo, RibbonIcons.Undo)
                ),
                new RibbonGroup(Strings.Ribbon_Help,
                    RibbonItem.CreateButton("Help", Strings.Ribbon_Help, RibbonIcons.Help)
                )
            );
        }

        // About
        public static RibbonDefinition About()
        {
            return new RibbonDefinition(
                new RibbonGroup(Strings.Ribbon_Actions,
                    RibbonItem.CreateButton("CopyInfo", Strings.Ribbon_CopyInfo, RibbonIcons.Copy)
                )
            );
        }
    }
}