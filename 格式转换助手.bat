echo. scevo转scmodby把红色赋予黑海1003705691
echo.使用此工具可以直接将scmod转换为对应的scevo格式
echo.将原scmod拖到bat脚本上即可在scmod所在目录生成对应的scevo文件
echo.两种格式压缩方法不同，别想直接修改后缀
echo.不兼容加固模组包！！！
pause
del/Q/S %~dp0\temp
%~dp07za.exe x %1 -o%~dp0\temp\%~n1
pushd %~dp0\temp\%~n1\
%~dp07za.exe a -t7z -mx=9 -ms=off -myx=0 %~dp1%~n1.scevo %~dp0\temp\%~n1\*
pause