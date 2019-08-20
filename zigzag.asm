section .text

global _start
_start:
call function_run

mov eax, 1
mov ebx, 0
int 80h


extern function_allocate


extern function_ipow


; Member function '' of type 'Player'
type_Player_constructor:
push ebp
mov ebp, esp
mov esp, ebp
pop ebp
ret

; Member function 'add' of type 'Player'
type_Player_function_add:
push ebp
mov ebp, esp
sub esp, 4
mov eax, 5
mov dword [ebp-4], eax

mov ebx, dword [ebp+16]
add ebx, eax
mov ecx, dword [ebp+12]
imul ecx, ebx
mov esi, dword [ebp+8]
mov ebx, dword [esi]
add ebx, ecx
mov edi, dword [ebp+8]
mov dword [edi], ebx

cmp ebx, 0
jg type_Player_function_add_L2
sub ebx, 1
mov dword [edi], ebx
add ebx, dword [ebp+16]
mov dword [edi], ebx
jmp type_Player_function_add_L1
type_Player_function_add_L2:
mov ecx, 3
imul ecx, ebx
add ecx, dword [ebp+16]
imul ecx, 4
cmp eax, ecx
jle type_Player_function_add_L3
; Push function parameters to stack
push 1
push 3
call type_Player_function_add
; Remove parameters from stack after the call
add esp, 8
mov esi, dword [ebp+8]
mov ebx, esi
mov esi, dword [esi+4]
mov ecx, dword [ebp-4]
sub ecx, dword [esi]
add ecx, eax
mov dword [ebp-4], ecx
jmp type_Player_function_add_L1
type_Player_function_add_L3:
mov ecx, 1
imul ecx, dword [ebp+16]
add eax, ecx
mov dword [ebp-4], eax
type_Player_function_add_L1: 

mov esp, ebp
pop ebp
ret


; Represents global function 'yeet'
function_yeet:
push ebp
mov ebp, esp
sub esp, 4
mov eax, 3
add eax, dword [banana]
mov dword [ebp-4], eax

imul eax, 2
mov ebx, dword [ebp-4]
add ebx, eax
mov eax, dword [ebp+8]
add eax, dword [ebp-4]
mov ecx, dword [ebp-4]
xchg ecx, eax
xor edx, edx
idiv ecx
mov ecx, dword [ebp+8]
add ecx, eax
add ecx, ebx
mov dword [ebp+8], ecx

mov eax, ecx

mov esp, ebp
pop ebp
ret


; Represents global function 'run'
function_run:
push ebp
mov ebp, esp
sub esp, 16
; Push function parameters to stack
push 8
call function_allocate
; Remove parameters from stack after the call
add esp, 4
; Save all critical values
push eax
; Push function parameters to stack
push eax
call type_Player_constructor
; Remove parameters from stack after the call
add esp, 4
; Restore all critical values
pop ebx
mov dword [ebp-16], ebx

mov ecx, 3
mov dword [ebp-4], ecx

mov edx, 5
mov dword [ebp-8], edx

; Push function parameters to stack
push ecx
push edx
call function_ipow
; Remove parameters from stack after the call
add esp, 8
mov dword [ebp-4], eax

add eax, dword [ebp-8]
mov ebx, dword [ebp-4]
add ebx, ebx
imul ebx, eax
mov dword [ebp-12], ebx

mov eax, dword [ebp-4]
imul eax, eax
mov dword [ebp-4], eax

mov ecx, dword [ebp-8]
xchg ecx, eax
xor edx, edx
idiv ecx
mov dword [ebp-4], eax

; Push function parameters to stack
push eax
call function_yeet
; Remove parameters from stack after the call
add esp, 4
imul eax, 2
mov ebx, dword [ebp-12]
imul ebx, dword [ebp-8]
imul ebx, dword [ebp-4]
imul ebx, 7
add ebx, eax
mov dword [ebp-4], ebx

mov eax, dword [ebp-8]
add eax, dword [ebp-12]
mov dword [ebp-4], eax

sub eax, dword [ebp-12]
mov dword [ebp-4], eax

imul eax, dword [ebp-12]
imul eax, 7
mov edi, dword [ebp-16]
mov dword [edi], eax

; Push function parameters to stack
mov esi, dword [ebp-16]
push dword [esi]
push 5
push dword [ebp-16]
call type_Player_function_add
; Remove parameters from stack after the call
add esp, 12

function_run_L1:
mov esi, dword [ebp-16]
mov ebx, dword [esi]
cmp ebx, 0
jle function_run_L2
; Push function parameters to stack
push 1
push 5
push dword [ebp-16]
call type_Player_function_add
; Remove parameters from stack after the call
add esp, 12
jmp function_run_L1
function_run_L2:

mov esi, dword [ebp-16]
mov esi, dword [esi+4]
mov eax, dword [esi]
imul eax, 2
add eax, dword [ebp-4]
mov esi, dword [ebp-16]
mov esi, dword [esi+4]
mov edi, dword [esi+4]
mov dword [edi], eax

mov esp, ebp
pop ebp
ret




section .data
banana dd 0

