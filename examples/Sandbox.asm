section .text

global _start
_start:
call function_run

mov eax, 1
mov ebx, 0
int 80h

function_run:
push ebp
mov ebp, esp
sub esp, 12
mov eax, 3
mov ecx, 3
xchg eax, ecx
idiv ecx
mov eax, dword 3
mov [ebp-4], dword 3
mov [ebp-8], dword 5
mov eax, dword [ebp-4]
add eax, dword [ebp-4]
sub eax, dword [ebp-4]
add eax, dword [ebp-8]
mov byte [ebp-12], 1
imul dword [ebp-8]
mov esp, ebp
pop ebp
ret
section .data