#!/bin/sh
yasm -f elf32 -o libz.o Zigzag.asm
mv libz.o ../

# yasm -f elf32 -o libz.o Zigzag.asm
# ar rcs libz.a libz.o
# rm libz.o
# mv libz.o ../
