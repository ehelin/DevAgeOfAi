@echo off
set "targetPath=C:\temp\DevAgeTraining\HabitTrackerApps\HabitTracker\bin\Debug\net8.0-windows"

echo Deleting all .json files in %targetPath% ...
rd /s /q "%targetPath%\Data"

echo Done.
