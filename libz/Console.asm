section .text

global function_sys_print
function_sys_print:
mov eax, 0x04 ; System call: sys_write
mov ebx, 1 ; Output mode
mov ecx, [esp+4] ; Parameter: Text
mov edx, [esp+8] ; Parameter: Length
int 0x80
ret

global function_sys_read
function_sys_read:
mov eax, 0x03 ; System call: sys_write
mov ebx, 0 ; Input mode
mov ecx, [esp+4] ; Parameter: Buffer
mov edx, [esp+8] ; Parameter: Length
int 0x80
ret