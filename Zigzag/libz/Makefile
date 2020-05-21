x64_cdecl:
	yasm -f elf64 -g dwarf2 -o libz64.o libz_x64_cdecl.asm
	mv libz64.o ../

x64:
	yasm -f elf64 -g dwarf2 -o libz64.o libz_x64.asm
	mv libz64.o ../

x64_windows:
	yasm -f win64 -g cv8 -o libz64.obj libz_x64_windows.asm
	del ..\libz64.obj
	move libz64.obj ..\
