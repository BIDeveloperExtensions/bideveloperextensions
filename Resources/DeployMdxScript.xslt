<xsl:stylesheet version="1.0" 
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:as="http://schemas.microsoft.com/analysisservices/2003/engine">
	<xsl:param name="TargetDatabase"></xsl:param>

	<xsl:output indent="yes" omit-xml-declaration="yes" />
	<xsl:template match="/">
		<Alter ObjectExpansion="ExpandFull" xmlns="http://schemas.microsoft.com/analysisservices/2003/engine">
			<Object>
				<DatabaseID>
					<xsl:value-of select="$TargetDatabase"></xsl:value-of>
				</DatabaseID>
				<CubeID>
					<xsl:value-of select="/as:Cube/as:ID"/>
				</CubeID>
				<MdxScriptID>
					<xsl:value-of select="/as:Cube/as:MdxScripts/as:MdxScript/as:ID"/>
				</MdxScriptID>
			</Object>
			<ObjectDefinition>
				<xsl:copy-of select="/as:Cube/as:MdxScripts/as:MdxScript"/>
				
			</ObjectDefinition>
			
		</Alter>
	</xsl:template>
<!--
	<MdxScript>
		<ID>
			<xsl:value-of select="/as:Cube/as:MdxScripts/as:MdxScript/as:ID"/>
		</ID>
		<Name>
			<xsl:value-of select="/as:Cube/as:MdxScripts/as:MdxScript/as:Name"/>
		</Name>
		<Commands>
			<Command>
				<Text>
					<xsl:value-of select="/as:Cube/as:MdxScripts/as:MdxScript/as:Commands/as:Command/as:Text"/>
				</Text>
			</Command>
		</Commands>
	</MdxScript>
-->
</xsl:stylesheet>
