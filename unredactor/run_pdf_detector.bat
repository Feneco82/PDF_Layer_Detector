@echo off
echo.
echo PDF Layer Detector - Quick Launch
echo =================================
echo.

REM Check if a path was provided as an argument
if "%~1"=="" (
    echo Usage: run_pdf_detector.bat "C:\path\to\pdf\folder"
    echo   or just double-click to run interactively
    echo.
    
    REM Run in interactive mode
    "PDF Layer Detector.exe"
) else (
    REM Run with provided path
    "PDF Layer Detector.exe" "%~1"
)

echo.
pause