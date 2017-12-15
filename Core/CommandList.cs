using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIDSHelper.Core
{
    public enum CommandList
    {
        DeployMdxScriptId               = 0x0100,
        AggregationManagerId            = 0x0101,
        PrinterFriendlyDimensionUsageId = 0x0102,
        SyncDescriptionsId              = 0x0103,
        TabularDeployDatabaseId         = 0x0104,
        TabularActionsEditorId          = 0x0105,
        TabularAnnotationsWorkaroundId  = 0x0106,
        TabularDisplayFoldersId         = 0x0107,
        TabularHideMemberIfId           = 0x0108,
        TabularTranslationsEditorId     = 0x0109,
        PCDimNaturalizerId              = 0x010A,
        AttributeRelationshipFixId      = 0x010B,
        ExpressionListId                = 0x010C,
        SmartDiffId                     = 0x010D,
        MeasureGroupHealthCheckId       = 0x010E,
        DataTypeDiscrepancyCheckId      = 0x010F,
        DeployAggDesignsId              = 0x0110,
        DimensionHealthCheckId          = 0x0111,
        DeleteUnusedIndexesId           = 0x0112,
        DuplicateRoleId                 = 0x0113,
        DimensionOptimizationReportId   = 0x0114,
        RolesReportPluginId             = 0x0115,
        NonDefaultPropertiesReportId    = 0x0116,
        UnusedColumnsReportId           = 0x0117,
        UsedColumnsReportId             = 0x0118,
        VisualizeAttributeLatticeId     = 0x0119,
        BatchPropertyUpdateId           = 0x011A,
        PerformanceVisualizationId      = 0x011B,
        DesignPracticeScannerId         = 0x011C,
        AddNewBimlFileId                = 0x011D,
        ExpandBimlFileId                = 0x011E,
        LearnMoreAboutBimlId            = 0x011F,
        CheckBimlForErrorsId            = 0x0120,
        FixedWidthColumnsId             = 0x0121,
        PerformanceBreakdownId          = 0x0122,
        SortablePackagePropertiesId     = 0x0123,
        SortProjectFilesId              = 0x0124,
        ResetGUIDsId                    = 0x0125,
        FixRelativePathsId              = 0x0126,
        DeleteDatasetCacheId            = 0x0127,
        UnusedDatasetsId                = 0x0128,
        UsedDatasetsId                  = 0x0129,
        DeploySSISPackageId             = 0x012A
    }
}
