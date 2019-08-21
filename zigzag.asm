section .text

global _start
_start:
call function_run

mov eax, 1
mov ebx, 0
int 80h


; Member function '' of type 'fruit'
type_fruit_constructor:
push ebp
mov ebp, esp
mov esp, ebp
pop ebp
ret


; Represents global function 'ass'
function_ass:
push ebp
mov ebp, esp
mov esi, dword [ebp+8]
mov esi, dword [ebp+8]
mov esi, dword [ebp+8]
mov eax, dword [esi+4]
imul eax, dword [esi]
sub eax, dword [esi+4]
mov edi, dword [ebp+8]
mov dword [edi], eax

mov esp, ebp
pop ebp
ret


; Represents global function 'my'
function_my:
push ebp
mov ebp, esp
mov esi, dword [ebp+8]
mov esi, dword [ebp+8]
mov eax, dword [esi]
cmp eax, dword [esi+4]
jge function_my_L1
mov esi, dword [ebp+8]
mov eax, dword [esi+4]
mov edi, dword [ebp+8]
mov dword [edi], eax
function_my_L1: 

mov esp, ebp
pop ebp
ret


; Represents global function 'run'
function_run:
push ebp
mov ebp, esp
sub esp, 4
; Push function parameters to stack
push 8
call function_allocate
; Remove parameters from stack after the call
add esp, 4
; Save all critical values
push eax
; Push function parameters to stack
push eax
call type_fruit_constructor
; Remove parameters from stack after the call
add esp, 4
; Restore all critical values
pop ebx
mov dword [ebp-4], ebx

mov ecx, dword [ebx]
mov dword [ebx], ecx

function_run_L1:
mov ecx, dword [ebx]
cmp ecx, dword [ebx]
jge function_run_L2
; Push function parameters to stack
push ebx
call function_ass
; Remove parameters from stack after the call
add esp, 4
jmp function_run_L1
function_run_L2:

mov esp, ebp
pop ebp
ret




section .data

