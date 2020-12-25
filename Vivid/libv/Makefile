linux-x64:
	x64-as -o libv_x64.o libv_linux_x64.asm
	mv lib_x64.o ../

windows-x64:
	x64-as -o libv_x64.obj libv_windows_x64.asm
	del ..\libv_x64.obj
	move libv_x64.obj ..\

linux-arm64:
	arm64-as -o libv_arm64.o libv_linux_arm64.asm
	mv libv_arm64.o ../