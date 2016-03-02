<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:dwd="http://schemas.microsoft.com/DataWarehouse/Designer/1.0" xmlns:SSAS="http://schemas.microsoft.com/analysisservices/2003/engine" xmlns:msprop="urn:schemas-microsoft-com:xml-msprop" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:ddl200="http://schemas.microsoft.com/analysisservices/2010/engine/200">

	<xsl:output cdata-section-elements="SSAS:Text"/>

	<xsl:template match="node()">
		<xsl:copy>
			
			<!-- sort attributes by name -->
			<xsl:apply-templates select="@*">
				<xsl:sort order="ascending" select="name()"/>
			</xsl:apply-templates>

			<!-- leave default sort order for elements -->
			<xsl:apply-templates select="node()[local-name()!='Action' and name()!='xs:keyref' and local-name()!='Attribute' and local-name()!='Kpi']"/>

			<!-- customize sort order for elements -->
			<xsl:apply-templates select="./SSAS:Action">
				<xsl:sort order="ascending" select="./SSAS:ID"/>
			</xsl:apply-templates>
			
			<xsl:apply-templates select="./xs:keyref">
				<xsl:sort order="ascending" select="@name"/>
			</xsl:apply-templates>

			<xsl:apply-templates select="./SSAS:Attribute">
				<xsl:sort order="ascending" select="./SSAS:AttributeID[local-name(../../../../..)='Perspective']"/> <!--sort Attribute elements inside Perspective/Dimensions/Dimension/Attributes... doesn't change the order of other Attribute tags that don't have an AttributeID child-->
			</xsl:apply-templates>

			<xsl:apply-templates select="./SSAS:Kpi">
				<xsl:sort order="ascending" select="./SSAS:KpiID[local-name(../../..)='Perspective']"/> <!--sort Kpi elements inside Perspective/Kpis... doesn't change the order of other Kpi tags that don't have an KpiID child-->
			</xsl:apply-templates>

		</xsl:copy>
	</xsl:template>
	<xsl:template match="@*">
		<xsl:attribute namespace="{namespace-uri()}" name="{name()}">
			<xsl:value-of select="."/>
		</xsl:attribute>
	</xsl:template>
	<xsl:template match="text()">
		<xsl:value-of select="."/>
	</xsl:template>


	<!--remove dwd:design-time-name attributes-->
	<xsl:template match="@dwd:design-time-name|@msprop:design-time-name"/>

	<!--remove any Microsoft Annotations -->
	<xsl:template match="//SSAS:Annotation[starts-with(SSAS:Name,'http://schemas.microsoft.com')]"/>

	<!--remove nodes which have meaningless dates or values -->
	<xsl:template match="//SSAS:CreatedTimestamp|//SSAS:LastSchemaUpdate|//SSAS:LastProcessed|//SSAS:State|//ddl200:ProcessingState"/>

</xsl:stylesheet>


