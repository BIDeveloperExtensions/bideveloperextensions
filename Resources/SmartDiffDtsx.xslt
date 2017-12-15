<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:DTS="www.microsoft.com/SqlServer/Dts">
	<xsl:output cdata-section-elements="ProjectItem arrayElement"/>

	<xsl:template match="node()">
		<xsl:copy>
      <!-- sort attributes by name -->
      <xsl:apply-templates select="@*">
        <xsl:sort order="ascending" select="name()"/>
      </xsl:apply-templates>
      
      <!-- leave default sort order -->
			<xsl:apply-templates select="node()[name()!='DTS:LogProvider' and name()!='DTS:Executable' and name()!='DTS:ConnectionManager' and name()!='DTS:PrecedenceConstraint' and name()!='component' and name()!='ProjectItem']"/>

			<!-- customize sort order -->
			<xsl:apply-templates select="./DTS:LogProvider">
				<xsl:sort order="ascending" select="./DTS:Property[@DTS:Name='DTSID']"/>
			</xsl:apply-templates>
			<xsl:apply-templates select="./DTS:Executable">
				<xsl:sort order="ascending" select="./DTS:Property[@DTS:Name='DTSID']"/>
			</xsl:apply-templates>
			<xsl:apply-templates select="./DTS:ConnectionManager">
				<xsl:sort order="ascending" select="./DTS:Property[@DTS:Name='DTSID']"/>
			</xsl:apply-templates>
			<xsl:apply-templates select="./DTS:PrecedenceConstraint">
				<xsl:sort order="ascending" select="./DTS:Property[@DTS:Name='DTSID']"/>
			</xsl:apply-templates>
			<xsl:apply-templates select="./component">
				<xsl:sort order="ascending" select="number(@id)"/>
			</xsl:apply-templates>
			<xsl:apply-templates select="./ProjectItem">
				<xsl:sort order="ascending" select="@Name"/>
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

	<!--remove any PackageVariable nodes which have a "Namespace" property of dts-designer-1.0-->
	<xsl:template match="/DTS:Executable/DTS:PackageVariable[DTS:Property[@DTS:Name='Namespace']='dts-designer-1.0']"/>

	<!--remove the ThreadHint attribute-->
	<xsl:template match="//DTS:Executable/@DTS:ThreadHint"/>

	<!--remove the compiled binary of a script task-->
	<xsl:template match="//DTS:ObjectData/ScriptProject/BinaryItem"/>

	<!--remove the compiled binary of a script component-->
	<xsl:template match="//DTS:ObjectData/pipeline/components/component/properties/property[@name='BinaryCode']"/>

	<!--remove the SSIS 2012 layout info-->
	<xsl:template match="//DTS:Executable/DTS:DesignTimeProperties"/>

</xsl:stylesheet>


