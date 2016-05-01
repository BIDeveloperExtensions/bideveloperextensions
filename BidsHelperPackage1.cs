namespace BIDSHelper
{
    using System;
    
    /// <summary>
    /// Helper class that exposes all GUIDs used across VS Package.
    /// </summary>
    internal sealed partial class PackageGuids
    {
        public const string guidBidsHelperPackageString = "d3474f10-475f-4a9d-84f6-85bc892ad3b6";
        public const string guidSsisConnectionMenuString = "96b36e93-f71c-4160-a4ea-26ae801d2f63";
        public const string guidSsisDesignerMenuString = "96b36e93-f71c-4160-a4ea-26ae801d2f63";
        public const string guidBidsHelperPackageCmdSetString = "bd8ea5c7-1cc4-490b-a7b8-8484dc5532e7";
        public const string measureGroupContextMenuGroupString = "fa554dc1-6dd4-11d1-af71-006097df568c";
        public const string guidImagesString = "feec21bd-6b4e-4eca-9c2f-14772de9478c";
        public const string guidImages2String = "feec21bd-6b4e-4eca-9c2f-14772de9478d";
        public const string guidSsasImagesString = "feec21bd-6b4e-4eca-9c2f-14772de9478e";
        public const string guidSsisImagesString = "feec21bd-6b4e-4eca-9c2f-14772de9478f";
        public static Guid guidBidsHelperPackage = new Guid(guidBidsHelperPackageString);
        public static Guid guidSsisConnectionMenu = new Guid(guidSsisConnectionMenuString);
        public static Guid guidSsisDesignerMenu = new Guid(guidSsisDesignerMenuString);
        public static Guid guidBidsHelperPackageCmdSet = new Guid(guidBidsHelperPackageCmdSetString);
        public static Guid measureGroupContextMenuGroup = new Guid(measureGroupContextMenuGroupString);
        public static Guid guidImages = new Guid(guidImagesString);
        public static Guid guidImages2 = new Guid(guidImages2String);
        public static Guid guidSsasImages = new Guid(guidSsasImagesString);
        public static Guid guidSsisImages = new Guid(guidSsisImagesString);
    }
    /// <summary>
    /// Helper class that encapsulates all CommandIDs uses across VS Package.
    /// </summary>
    internal sealed partial class PackageIds
    {
        public const int menuSsisConnectionId = 0x1008;
        public const int menuSsisControlFlowItem = 0x1003;
        public const int menuSsisDataFlowSurface = 0x1014;
        public const int menuSsisProjectNode = 0x1020;
        public const int menuExpressionWindow = 0x4000;
        public const int myMenuGroupTop = 0x1020;
        public const int myMenuGroupBottom = 0x1030;
        public const int MyMenuViewOtherWindowsGroup = 0x1040;
        public const int myMeasureGroupContextMenuGroup = 0x041A;
        public const int myProjectMenuGroup = 0x1060;
        public const int mySsisConnectionMenuGroup = 0x1070;
        public const int mySsisDataFlowGroup = 0x1080;
        public const int mySsisDesignerGroup = 0x1090;
        public const int mySsisProjectNode = 0x10A0;
        public const int DeployMdxScriptId = 0x0100;
        public const int AggregationManagerId = 0x0101;
        public const int PrinterFriendlyDimensionUsageId = 0x0102;
        public const int SyncDescriptionsId = 0x0103;
        public const int TabularDeployDatabaseId = 0x0104;
        public const int TabularActionsEditorId = 0x0105;
        public const int TabularAnnotationsWorkaroundId = 0x0106;
        public const int TabularDisplayFoldersId = 0x0107;
        public const int TabularHideMemberIfId = 0x0108;
        public const int TabularTranslationsEditorId = 0x0109;
        public const int PCDimNaturalizerId = 0x010A;
        public const int AttributeRelationshipNameFixId = 0x010B;
        public const int ExpressionWindowId = 0x010C;
        public const int SmartDiffId = 0x010D;
        public const int MeasureGroupHealthCheckId = 0x010E;
        public const int DataTypeDiscrepancyCheckId = 0x010F;
        public const int DeployAggregationDesignsId = 0x0110;
        public const int DimensionHealthCheckId = 0x0111;
        public const int DeleteUnusedIndexesId = 0x0112;
        public const int DuplicateRolesId = 0x0113;
        public const int DimensionOptimizationReportId = 0x0114;
        public const int RolesReportPluginId = 0x0115;
        public const int NonDefaultPropertiesReportId = 0x0116;
        public const int UnusedColumnsReportId = 0x0117;
        public const int UsedColumnsReportId = 0x0118;
        public const int VisualizeAttributeLatticeId = 0x0119;
        public const int BatchPropertyUpdateId = 0x011A;
        public const int PerformanceVisualizationId = 0x011B;
        public const int DesignPracticeScannerId = 0x011C;
        public const int AddNewBimlFileId = 0x011D;
        public const int ExpandBimlFileId = 0x011E;
        public const int LearnMoreAboutBimlId = 0x011F;
        public const int CheckBimlForErrorsId = 0x0120;
        public const int FixedWidthColumnsId = 0x0121;
        public const int PerformanceBreakdownId = 0x0122;
        public const int SortablePackagePropertiesId = 0x0123;
        public const int SortProjectFilesId = 0x0124;
        public const int ResetGUIDsId = 0x0125;
        public const int FixRelativePathsId = 0x0126;
        public const int DeleteDatasetCacheId = 0x0127;
        public const int UnusedDatasetsId = 0x0128;
        public const int UsedDatasetsId = 0x0129;
        public const int measureGroupContextMenu = 0x1220;
        public const int picBlank = 0x0001;
        public const int picDeployMdxScriptOld = 0x0002;
        public const int picEditAggs = 0x0003;
        public const int picPrinterFriendlyDimUsageOld = 0x0004;
        public const int bmpIndex = 0x0001;
        public const int picAggManager = 0x0001;
        public const int picUsedColumns = 0x0002;
        public const int picUnusedColumns = 0x0003;
        public const int picDeleteAggs = 0x0004;
        public const int picDeleteIndexes = 0x0005;
        public const int picDeployMdxScript = 0x0006;
        public const int picDimDataTypeCheck = 0x0007;
        public const int picDimensionHealthCheck = 0x0008;
        public const int picDimOptimizationReport = 0x0009;
        public const int picDuplicateRole = 0x000A;
        public const int picMeasureGroupHealthCheck = 0x000B;
        public const int picNonDefaultProperties = 0x000C;
        public const int picPCDimNaturalize = 0x000D;
        public const int picPrinterFriendlyDimUsage = 0x000E;
        public const int picRolesReport = 0x000F;
        public const int picSmartDiff = 0x0010;
        public const int picVisualizeAttributeLattice = 0x0011;
        public const int picSyncDescriptions = 0x0012;
        public const int picTabularActions = 0x0013;
        public const int picTabularFolers = 0x0014;
        public const int picTabularTranslations = 0x0015;
        public const int picTabularAnnotationWorkaround = 0x0016;
        public const int picTabularHideMemberIf = 0x0017;
        public const int picBatchProperties = 0x0001;
        public const int picBiml = 0x0002;
        public const int picBimlFile = 0x0003;
        public const int picBimlCheck = 0x0004;
        public const int picBimlHelp = 0x0005;
        public const int picDesignScanner = 0x0006;
        public const int picSsisDeploy = 0x0007;
        public const int picExpressionList = 0x0008;
        public const int picRelativePaths = 0x0009;
        public const int picSsisPerformance = 0x000A;
        public const int picNewGuid = 0x000B;
        public const int picFixedColumns = 0x000C;
        public const int picSortPackages = 0x000D;
    }
}
