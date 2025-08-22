@echo off
echo ======================================
echo Testing Python AI Model HTTP Client
echo ======================================
echo.

echo Step 1: Starting Python AI Model Server...
echo ----------------------------------------
start cmd /k "cd Ai\AiModelRunner && python ai_model_server.py"

echo Waiting for server to start (10 seconds)...
timeout /t 10 /nobreak > nul

echo.
echo Step 2: Running Tests...
echo ----------------------------------------
cd Tests
dotnet test --filter "FullyQualifiedName~AiModelHttpClientTests" --logger:"console;verbosity=detailed"

echo.
echo ======================================
echo Tests completed!
echo ======================================
echo.
echo Note: The Python server is still running in another window.
echo Close it manually when done, or press Ctrl+C in that window.
echo.
pause