linux-x64:
	x64-as -o libv_x64.o libv_linux_x64.asm
	mv libv_x64.o ../

windows-x64-dynamic:
	x64-as -o libv_x64.obj libv_windows_x64.asm
	ld -shared libv_x64.obj -o libv_x64.dll -lkernel32 -L"C:\Program Files (x86)\Windows Kits\10\Lib\10.0.18362.0\um\x64"
	del ..\libv_x64.dll
	move libv_x64.dll ..\

windows-x64-static:
	x64-as -o libv_x64.obj libv_windows_x64.asm
	ld -static libv_x64.obj -o libv_x64.lib -lkernel32 -L"C:\Program Files (x86)\Windows Kits\10\Lib\10.0.18362.0\um\x64"
	del ..\libv_x64.lib
	move libv_x64.lib ..\

windows-x64:
	x64-as -o libv_x64.obj libv_windows_x64.asm
	del ..\libv_x64.obj
	move libv_x64.obj ..\

linux-arm64:
	arm64-as -o libv_arm64.o libv_linux_arm64.asm
	mv libv_arm64.o ../