yasm -f elf32 -o z.o Zigzag.asm
#ar rcs libz.a z.o
#rm z.o
mv z.o ../