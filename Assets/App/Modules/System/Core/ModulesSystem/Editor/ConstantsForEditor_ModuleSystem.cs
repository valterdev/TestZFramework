using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EditorConstantsForModuleSystem
{
    public const string ModuleDisableFlagFileName = "module_disabled";
    public const string ModuleInfoFileName = "module-info.json";

    public const string Alpha = "Alpha";
    public const string Beta = "Beta";
    public const string Stable = "Stable";
    public const string Migrated = "Migrated";
    public const string NotCompleted = "NotCompleted";

    public const string FileWasCompressed = "Files successfully compressed";
    public const string Exception = "Exception during processing {0}";
    public const string Description = "Description";

    public const string Assets = "Assets";
    public const string Manager = "Manager";

    public const string System = "System";
    public const string Domain = "Domain";

    public const string System_Core = "System/Core";
    public const string System_Data = "System/Data";
    public const string System_Network = "System/Network";
    public const string System_Other = "System/Other";
    public const string System_Tools = "System/Tools";

    public const string Domain_Mechanics_Features = "Domain/Mechanics_Features";
    public const string Domain_Meta = "Domain/Meta";
    public const string Domain_UI = "Domain/UI";
    public const string Domain_Other = "Domain/Other";
    public const string Domain_Tools = "Domain/Tools";

    #region UI

    // All Modules Window
    public const string L_AllModulesPageTitle = "All modules";
    public const string L_ImportModuleWindowTitle = "Select module's package";

    public const string L_CriticalForWork = "Criticality for system operation: ";
    public const string L_Yes = "<color=#ff0000><b>Yes<b></color>";
    public const string L_No = "<color=#00ff00>No</color>";

    public const string L_Path = "<b>Path:</b> ";
    public const string L_Version = "<b>Version:</b> ";
    public const string L_Type = "<b>Type:</b> ";
    public const string L_State = "<b>State:</b> ";
    public const string L_Author = "<b>Author:</b> ";

    public const string L_Email = " Email: ";
    public const string L_Site = " Site: ";

    // All Utils Window
    public const string L_AllUtilsPageTitle = "All utilities";

    // Create module window
    public const string L_CreateModuleTitle = "Create module";

    public const string L_EnterModuleName = "Enter module name";
    public const string L_EnterModuleVersion = "Enter module version";
    public const string L_SelectModuleType = "Select module type";
    public const string L_SelectModuleState = "Select module state";

    public const string L_ModuleWithNameExist = "Module with this name is exist.";
    public const string L_HashForModules = "Hash for module with name";
    public const string L_GenerateHash = "Generate hash";
    public const string L_GenerateModuleHash = "Generate module hash";

    // Module Helpers
    public const string L_ModuleHelpersTitle = "Module Helpers";

    // Add files to module window
    public const string L_AddFilesToModulePageTitle = "Add files to module";
    public const string L_SelectModule = "Select module";
    public const string L_SuccessAddFiles = "Successfull add files!";

    #endregion

    #region Paths
    public const string RootPath = "App/";
    public const string PathToProjectManifest = "App/ZProjectManifest.json";

    public const string PathToLayoutForAllModulesPage = "Assets/App/Modules/System/Core/ModulesSystem/view/all_modules.uxml";
    public const string PathToLayoutForModuleDetailsPage = "Assets/App/Modules/System/Core/ModulesSystem/view/module_details.uxml";

    public const string PathToLayoutForModuleItem = "Assets/App/Modules/System/Core/ModulesSystem/view/module_item.uxml";

    public const string PathToPreInitScript = "App/Modules/System/00_InitializationCycle/Z0_PreInit.cs";
    public const string PathToPreInitTemplate = "App/Modules/System/Core/ModulesSystem/templates/PreInit.cs.tmpl";

    public const string PathToAppModules = "App/Modules/";
    public const string PathToZFrameworkServiceFolder = "../ZFramework";
    public const string PathToDisabledModules = PathToZFrameworkServiceFolder + "/DisabledModules/";

    // All Utils Window
    public const string PathToLayoutForAllUtilsPage = "Assets/App/Modules/System/Core/ModulesSystem/view/all_utils.uxml";
    public const string PathToLayoutForUtilityItem = "Assets/App/Modules/System/Core/ModulesSystem/view/utility_item.uxml";

    // Create module window
    public const string PathToLayoutForCreateModulePage = "Assets/App/Modules/System/Core/ModulesSystem/view/create_module.uxml";

    public const string PathToPartsScriptTemplate = "App/Modules/System/Core/ModulesSystem/templates/Parts.cs.tmpl";
    public const string PathToManagerScriptTemplate = "App/Modules/System/Core/ModulesSystem/templates/Manager.cs.tmpl";
    public const string PathToHooksScriptTemplate = "App/Modules/System/Core/ModulesSystem/templates/Hooks.cs.tmpl";
    public const string PathToStoreScriptTemplate = "App/Modules/System/Core/ModulesSystem/templates/Store.cs.tmpl";

    // Add files to module window
    public const string PathToLayoutForAddFilesToModulePage = "Assets/App/Modules/System/Core/ModulesSystem/view/add_files_to_module.uxml";
    #endregion
}
