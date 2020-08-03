set dest=C:\projects\BIDS Helper\GitHub\bideveloperextensions2\DLLs\SQL2019\Reference

c:
cd "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\IDE\CommonExtensions\Microsoft\SSAS"
copy /Y /B "Microsoft.AnalysisServices.AdomdClient.dll" "%dest%"
copy /Y /B "Microsoft.AnalysisServices.BackEnd.dll" "%dest%"
copy /Y /B "Microsoft.AnalysisServices.Common.FrontEnd.dll" "%dest%"
copy /Y /B "Microsoft.AnalysisServices.Controls.AS.dll" "%dest%"
copy /Y /B "Microsoft.AnalysisServices.Core.dll" "%dest%"
copy /Y /B "Microsoft.AnalysisServices.Design.AS.dll" "%dest%"
copy /Y /B "Microsoft.AnalysisServices.dll" "%dest%"
copy /Y /B "Microsoft.AnalysisServices.MPFProjectBase.dll" "%dest%"
copy /Y /B "Microsoft.AnalysisServices.Project.AS.dll" "%dest%"
copy /Y /B "Microsoft.AnalysisServices.Tabular.dll" "%dest%"
copy /Y /B "Microsoft.AnalysisServices.VSHost.dll" "%dest%"
copy /Y /B "Microsoft.DataWarehouse.AS.dll" "%dest%"
REM copy /Y /B "Microsoft.DataWarehouse.Interfaces.dll" "%dest%"
copy /Y /B "Microsoft.DataWarehouse.VsIntegration.AS.dll" "%dest%"
copy /Y /B "Microsoft.SqlServer.DlgGrid.dll" "%dest%"
copy /Y /B "Microsoft.SqlServer.GridControl.dll" "%dest%"
copy /Y /B "Microsoft.DataWarehouse.Interfaces.DLL" "%dest%\FromASFolder\Microsoft.DataWarehouse.Interfaces.AS.DLL"


REM cd "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\IDE\CommonExtensions\Microsoft\SSAS\LocalServer"
REM copy /Y /B "Microsoft.AnalysisServices.Server.Tabular.dll" "%dest%"
REM copy /Y /B "Microsoft.AnalysisServices.Server.Tabular.Json.dll" "%dest%"

cd "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\IDE\CommonExtensions\Microsoft\BIShared"
REM copy /Y /B "Microsoft.AnalysisServices.Design.dll" "%dest%"


cd "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\IDE\CommonExtensions\Microsoft\SSIS\150\Binn"
copy /Y /B "Microsoft.DataTransformationServices.Controls.dll" "%dest%"
copy /Y /B "Microsoft.SQLServer.DTSPipelineWrap.dll" "%dest%"
copy /Y /B "Microsoft.SqlServer.DTSRuntimeWrap.dll" "%dest%"
copy /Y /B "Microsoft.SqlServer.ExecPackageTaskWrap.dll" "%dest%"
copy /Y /B "Microsoft.SqlServer.ManagedDTS.dll" "%dest%"

cd "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\IDE\CommonExtensions\Microsoft\SSIS\150\BIShared"
copy /Y /B "Microsoft.DataWarehouse.dll" "%dest%"
copy /Y /B "Microsoft.AnalysisServices.Project.dll" "%dest%"
copy /Y /B "Microsoft.DataWarehouse.VsIntegration.dll" "%dest%"
copy /Y /B "Microsoft.AnalysisServices.Controls.dll" "%dest%"
copy /Y /B "Microsoft.AnalysisServices.Design.dll" "%dest%"
copy /Y /B "Microsoft.DataWarehouse.Interfaces.dll" "%dest%"


cd "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\IDE\CommonExtensions\Microsoft\SSIS"
copy /Y /B "Microsoft.DataTransformationServices.Design.dll" "%dest%"
copy /Y /B "Microsoft.DataTransformationServices.VsIntegration.dll" "%dest%"
copy /Y /B "Microsoft.SqlServer.Dts.Design.dll" "%dest%"
copy /Y /B "Microsoft.SqlServer.Graph.dll" "%dest%"
copy /Y /B "Microsoft.SqlServer.IntegrationServices.Design.dll" "%dest%"
copy /Y /B "Microsoft.SqlServer.IntegrationServices.Graph.dll" "%dest%"


cd "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\IDE\CommonExtensions\Microsoft\SSRS"
copy /Y /B "Microsoft.DataWarehouse.RS.dll" "%dest%"
copy /Y /B "Microsoft.DataWarehouse.VsIntegration.RS.dll" "%dest%"

REM not sure where it's from
REM copy /Y /B "Microsoft.ReportViewer.Common.AppLocal.dll" "%dest%"
REM copy /Y /B "Microsoft.ReportViewer.DataVisualization.AppLocal.dll" "%dest%"
REM copy /Y /B "Microsoft.ReportViewer.Design.AppLocal.dll" "%dest%"
REM copy /Y /B "Microsoft.ReportViewer.ProcessingObjectModel.AppLocal.dll" "%dest%"
REM copy /Y /B "Microsoft.ReportViewer.Winforms.AppLocal.dll" "%dest%"

