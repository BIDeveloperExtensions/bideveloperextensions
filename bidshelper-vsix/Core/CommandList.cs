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
        DimensionOptimizationReportId   = 0x0114
    }
}
