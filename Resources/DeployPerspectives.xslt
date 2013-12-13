<xsl:stylesheet version="1.0" 
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:as="http://schemas.microsoft.com/analysisservices/2003/engine">
	<xsl:param name="TargetDatabase"></xsl:param>

	<xsl:output indent="yes" omit-xml-declaration="yes" />


	<!-- effectively remove these two child nodes
	<xsl:template match="//as:CreatedTimestamp"/>
	<xsl:template match="//as:LastSchemaUpdate"/>
	-->

	<xsl:template match="/">
		<Batch xmlns="http://schemas.microsoft.com/analysisservices/2003/engine">
			<xsl:apply-templates/>
		</Batch>
	</xsl:template>
	
	<xsl:template match="//as:Perspective">
		<Alter AllowCreate="true" ObjectExpansion="ExpandFull" xmlns="http://schemas.microsoft.com/analysisservices/2003/engine">
			<Object>
				<DatabaseID>
					<xsl:value-of select="$TargetDatabase"></xsl:value-of>
				</DatabaseID>
				<CubeID>
					<xsl:value-of select="../../as:ID"/>
				</CubeID>
				<PerspectiveID>
					<xsl:value-of select="as:ID"/>
				</PerspectiveID>
			</Object>
			<ObjectDefinition>
				<!--<xsl:copy-of select="." />-->

				<xsl:copy>
					<xsl:copy-of select="node()[local-name()!='CreatedTimestamp' and local-name()!='LastSchemaUpdate']"/>
					<!--<xsl:apply-templates select="node()[local-name()!='CreatedTimestamp' and local-name()!='LastSchemaUpdate']"/>-->
				</xsl:copy>

			</ObjectDefinition>

		</Alter>
	</xsl:template>

	<!--
	<xsl:template match="//as:Perspective/node()[local-name()!='CreatedTimestamp' and local-name()!='LastSchemaUpdate']">
		<xsl:copy-of select="."/>
	</xsl:template>
-->
	<!--
	<xsl:template match="//as:Perspective//text()">
		<xsl:value-of select="."/>
	</xsl:template>
	-->

		

	<xsl:template match="text()"/>
	
	<!--
	<xsl:template match="//as:Perspective">
		<xsl:copy-of select="//as:Perspective" />
	</xsl:template>
	-->

	
</xsl:stylesheet>
