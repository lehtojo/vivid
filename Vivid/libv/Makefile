linux:
	yasm -f elf64 -g dwarf2 -o libv64.o libv_linux_x64.asm
	mv libv.o ../

windows:
	yasm -f win64 -g cv8 -o libv64.obj libv_windows_x64.asm
	del ..\libv64.obj
	move libv64.obj ..\