section .text

global function_print
function_print:
mov eax, 0x04 ; System call: sys_write
mov ebx, 1 ; Output mode
mov ecx, [esp+4] ; Parameter: Text
mov edx, [esp+8] ; Parameter: Length
int 0x80
ret