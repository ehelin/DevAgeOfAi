@echo off
set "targetPath=C:\temp\DevAgeTraining\HabitTrackerApps\HabitTracker\bin\Debug\net8.0-windows"

echo Deleting all .json files in %targetPath% ...
del /q "%targetPath%\*historian.json"
del /q "%targetPath%\*strategist.json"
del /q "%targetPath%\*builder.json"
del /q "%targetPath%\*suggestedHabits.json"
del /q "%targetPath%\*habits.json"
del /q "%targetPath%\*agent_log.txt"

echo Done.
