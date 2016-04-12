namespace BIDSHelper
{
    using System;
    
    /// <summary>
    /// Helper class that exposes all GUIDs used across VS Package.
    /// </summary>
    internal sealed partial class PackageGuids
    {
        public const string guidBidsHelperPackageString = "d3474f10-475f-4a9d-84f6-85bc892ad3b6";
        public const string guidBidsHelperPackageCmdSetString = "bd8ea5c7-1cc4-490b-a7b8-8484dc5532e7";
        public const string measureGroupContextMenuGroupString = "fa554dc1-6dd4-11d1-af71-006097df568c";
        public const string guidImagesString = "feec21bd-6b4e-4eca-9c2f-14772de9478c";
        public const string guidImages2String = "feec21bd-6b4e-4eca-9c2f-14772de9478d";
        public static Guid guidBidsHelperPackage = new Guid(guidBidsHelperPackageString);
        public static Guid guidBidsHelperPackageCmdSet = new Guid(guidBidsHelperPackageCmdSetString);
        public static Guid measureGroupContextMenuGroup = new Guid(measureGroupContextMenuGroupString);
        public static Guid guidImages = new Guid(guidImagesString);
        public static Guid guidImages2 = new Guid(guidImages2String);
    }
    /// <summary>
    /// Helper class that encapsulates all CommandIDs uses across VS Package.
    /// </summary>
    internal sealed partial class PackageIds
    {
        public const int menuExpressionWindow = 0x4000;
        public const int MyMenuGroupTop = 0x1020;
        public const int MyMenuGroupBottom = 0x1030;
        public const int MyMenuViewOtherWindowsGroup = 0x1040;
        public const int myMeasureGroupContextMenuGroup = 0x041A;
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
        public const int measureGroupContextMenu = 0x1220;
        public const int picBlank = 0x0001;
        public const int picDeployMdxScript = 0x0002;
        public const int picEditAggs = 0x0003;
        public const int picPrinterFriendlyDimUsage = 0x0004;
        public const int bmpIndex = 0x0001;
    }
}
