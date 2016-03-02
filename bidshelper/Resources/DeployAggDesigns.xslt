<xsl:stylesheet version="1.0" 
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:as="http://schemas.microsoft.com/analysisservices/2003/engine"
	xmlns:dwd="http://schemas.microsoft.com/DataWarehouse/Designer/1.0">
	<xsl:param name="TargetDatabase"></xsl:param>
	<xsl:param name="TargetCubeID"></xsl:param>

	<xsl:output indent="yes" omit-xml-declaration="yes" />

	<xsl:template match="/">
		<Batch xmlns="http://schemas.microsoft.com/analysisservices/2003/engine">
			<xsl:apply-templates/>
		</Batch>
	</xsl:template>

	<xsl:template match="/as:Cube/as:MeasureGroups/as:MeasureGroup/as:AggregationDesigns/as:AggregationDesign">
		<Alter AllowCreate="true" ObjectExpansion="ExpandFull" xmlns="http://schemas.microsoft.com/analysisservices/2003/engine">
			<Object>
				<DatabaseID>
					<xsl:value-of select="$TargetDatabase"></xsl:value-of>
				</DatabaseID>
				<CubeID>
					<xsl:value-of select="$TargetCubeID"></xsl:value-of>
					<!--this doesn't work because the ID of the cube in the .partitions file is incorrect: <xsl:value-of select="../../../../as:ID"/>-->
				</CubeID>
				<MeasureGroupID>
					<xsl:value-of select="../../as:ID"/>
				</MeasureGroupID>
				<AggregationDesignID>
					<xsl:value-of select="as:ID"/>
				</AggregationDesignID>
			</Object>
			<ObjectDefinition>
				<AggregationDesign>
					<xsl:apply-templates/>
				</AggregationDesign>
			</ObjectDefinition>
		</Alter>
	</xsl:template>

	<xsl:template match="node()">
		<xsl:copy>
			<xsl:apply-templates select="node()"/>
		</xsl:copy>
	</xsl:template>
	<xsl:template match="text()">
		<xsl:value-of select="."/>
	</xsl:template>

	<!--override the default template for everything outside of the AggregationDesign element so that you don't get the text for all the other elements in the partitions file-->
	<xsl:template match="node()[not(ancestor-or-self::as:AggregationDesign)]">
		<xsl:apply-templates/>
	</xsl:template>


	<!-- effectively remove these child nodes/attributes-->
	<xsl:template match="//as:CreatedTimestamp"></xsl:template>
	<xsl:template match="//as:LastSchemaUpdate"></xsl:template>
	<xsl:template match="@dwd:design-time-name"></xsl:template>

</xsl:stylesheet>
