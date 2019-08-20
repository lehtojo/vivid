section .text

global function_print
function_print:
mov eax, 0x04 ; System call: sys_write
mov ebx, 1 ; Output mode
mov ecx, [esp] ; Parameter: Text
int 0x80
ret