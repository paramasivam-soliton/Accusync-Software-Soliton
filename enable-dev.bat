@echo off
REM ============================================================
REM  enable-dev.bat — Creates dev mode flag files
REM  Usage: enable-dev.bat              (both flags)
REM         enable-dev.bat login        (login only)
REM         enable-dev.bat dashboard    (dashboard only)
REM ============================================================

set "DIR=%ProgramData%\AccuSync"
if not exist "%DIR%" mkdir "%DIR%"

if "%~1"=="" goto :BOTH
if /i "%~1"=="login" goto :LOGIN
if /i "%~1"=="dashboard" goto :DASHBOARD
echo Unknown option: %~1
echo Usage: enable-dev.bat [login^|dashboard]
goto :EOF

:BOTH
:LOGIN
echo DevUser^|Admin> "%DIR%\skip_login.flag"
echo [OK] skip_login.flag created (DevUser^|Admin)
if "%~1"=="login" goto :DONE

:DASHBOARD
echo PatientInformation> "%DIR%\skip_dashboard.flag"
echo [OK] skip_dashboard.flag created (target: PatientInformation)

:DONE
echo.
echo Flag directory: %DIR%
goto :EOF


REM ============================================================
REM  Save the section below as: disable-dev.bat
REM ============================================================
REM @echo off
REM set "DIR=%ProgramData%\AccuSync"
REM
REM if "%~1"=="" goto :DBOTH
REM if /i "%~1"=="login" goto :DLOGIN
REM if /i "%~1"=="dashboard" goto :DDASHBOARD
REM echo Usage: disable-dev.bat [login^|dashboard]
REM goto :EOF
REM
REM :DBOTH
REM :DLOGIN
REM if exist "%DIR%\skip_login.flag" (
REM     del "%DIR%\skip_login.flag"
REM     echo [OK] skip_login.flag removed
REM )
REM if "%~1"=="login" goto :EOF
REM
REM :DDASHBOARD
REM if exist "%DIR%\skip_dashboard.flag" (
REM     del "%DIR%\skip_dashboard.flag"
REM     echo [OK] skip_dashboard.flag removed
REM )
REM goto :EOF


REM ============================================================
REM  Save the section below as: dev-status.bat
REM ============================================================
REM @echo off
REM set "DIR=%ProgramData%\AccuSync"
REM echo AccuSync Dev Mode Status
REM echo ========================
REM echo Directory: %DIR%
REM echo.
REM if exist "%DIR%\skip_login.flag" (
REM     echo [ON]  skip_login.flag
REM     type "%DIR%\skip_login.flag"
REM ) else (
REM     echo [OFF] skip_login.flag
REM )
REM if exist "%DIR%\skip_dashboard.flag" (
REM     echo [ON]  skip_dashboard.flag
REM     type "%DIR%\skip_dashboard.flag"
REM ) else (
REM     echo [OFF] skip_dashboard.flag
REM )