yasm -g dwarf2 -f elf32 -o Firebox.o Sandbox.asm
ld -m elf_i386 -o Firebox Firebox.o
