linux:
	as -o libv64.o libv_linux_x64.asm
	mv libv.o ../

windows:
	as -o libv64.obj libv_windows_x64.asm
	del ..\libv64.obj
	move libv64.obj ..\