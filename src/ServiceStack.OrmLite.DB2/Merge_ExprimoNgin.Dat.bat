C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe ServiceStack.OrmLite.DB2.csproj /t:Rebuild /p:Configuration=Release

ilmerge.exe /target:library /out:ServiceStack.OrmLite.DB2.dll /targetplatform:v4,C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319 bin/Release/ServiceStack.Common.dll  bin/Release/ServiceStack.Interfaces.dll bin/Release/ServiceStack.OrmLite.dll bin/Release/ServiceStack.Text.dll bin/Release/ServiceStack.OrmLite.DB2.dll 

