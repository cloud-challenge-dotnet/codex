dotnet test ../codex.sln -l trx -r ./ /p:CollectCoverage=true "/p:CoverletOutputFormat=\"json,opencover\"" /p:CoverletOutput=../Coverage /p:MergeWith="../coverage.json" && ReportGenerator -reports:Coverage.opencover.xml -targetdir:reports -reporttypes:html;Badges