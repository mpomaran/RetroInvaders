set PROGFILES="C:\Program Files (x86)\Microsoft SDKs\Windows Phone\v7.0\Tools\CapDetect"

c:
cd %PROGFILES%

CapabilityDetection.exe Rules.xml "D:\docs\workspace\RetroInvaders\trunk\Game\Retro Invaders\Retro Invaders\bin\Windows Phone\Release\Retro Invaders.dll"

pause
