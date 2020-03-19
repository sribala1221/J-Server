:: Delete contents of initial folder because different deployment groups share the same folder
if exist "C:\Deployments\ServerAPISource" (
	rmdir /s /q C:\Deployments\ServerAPISource
)


:: DenverServer(192.168.240.14)
if "%APPLICATION_NAME%" == "DenverTestV2App" (
	appcmd stop site CCD_ServerAPI
	appcmd stop apppool CCD_ServerAPI
)

if "%APPLICATION_NAME%" == "DenverHeatherV2App" (
	appcmd stop site CCD_ServerAPI_Heather
	appcmd stop apppool CCD_ServerAPI_Heather
)

if "%APPLICATION_NAME%" == "DenverConvV2App" (
	appcmd stop site CCD_Conv_API
	appcmd stop apppool CCD_Conv_API
)

if "%APPLICATION_NAME%" == "DenverTrainingV2App" (
	appcmd stop site CCD_Training_API
	appcmd stop apppool CCD_Training_API
)


:: Web2019-V2
if "%APPLICATION_NAME%" == "DenverV22019App" (
	appcmd stop site 6441_DSD_ServerAPI
	appcmd stop apppool 6441_DSD_ServerAPI
)

if "%APPLICATION_NAME%" == "OrangeV22019App" (
	appcmd stop site 6541_OC_ServerAPI
	appcmd stop apppool 6541_OC.ServerAPI
)

if "%APPLICATION_NAME%" == "SacramentoV22019App" (
	appcmd stop site 6141_SSD_ServerAPI
	appcmd stop apppool 6141_SSD.ServerAPI
)

if "%APPLICATION_NAME%" == "GlennV22019App" (
	appcmd stop site 6741_GCSO_ServerAPI
	appcmd stop apppool 6741_GCSO_ServerAPI
)

if "%APPLICATION_NAME%" == "SeleniumTest2019App" (
	appcmd stop site DSD_ServerAPI_TEST
	appcmd stop apppool DSD_ServerAPI_TEST
)

if "%APPLICATION_NAME%" == "DenverConvV22019App" (
	appcmd stop site 6442_CCD_Conv_API
	appcmd stop apppool CCD_Conv_API
)

if "%APPLICATION_NAME%" == "SacramentoConvV22019App" (
	appcmd stop site 6142_SSD_Conv_API
	appcmd stop apppool 6142_SSD_Conv_API
)

if "%APPLICATION_NAME%" == "GlennConvV22019App" (
	appcmd stop site 6742_GCSO_Conv_API
	appcmd stop apppool 6742_GCSO_Conv_API
)

:: DemoServer (192.168.250.108)
if "%APPLICATION_NAME%" == "DemoV2App" (
	appcmd stop site ServerAPI_V2
	appcmd stop apppool ServerAPI_V2
)

if "%APPLICATION_NAME%" == "DemoV2NewApp" (
	appcmd stop site New_ServerAPI_V2
	appcmd stop apppool New_ServerAPI_V2
)