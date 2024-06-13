@echo off
rem Pfad von Skriptdatei in variable schreiben und ausgeben
set scriptDir=%~dp0
echo aktueller Projekt-Pfad: %scriptDir%

rem scaffolding befehl ausführen 
echo befehl ist auskommentiert
echo dotnet ef dbcontext scaffold name="AppDb" Microsoft.EntityFrameworkCore.SqlServer -o Data -f

pause