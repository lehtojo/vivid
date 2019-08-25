section .text

global _start
_start:
call function_run

mov eax, 1
mov ebx, 0
int 80h


extern function_allocate


extern function_print


; Member function 'do' of type 'fruit'
type_fruit_function_do:
push ebp
mov ebp, esp
sub esp, 12
mov eax, S1
mov [ebp-12], eax

mov ebx, 15
push ebx
push eax
call function_print
add esp, 8

mov ebx, [ebp+12]
mov ecx, 3
mov edx, eax
mov eax, ebx
xor edx, edx
idiv ecx
mov [ebp-4], eax

mov esi, [ebp+8]
mov edi, [esi+4]
imul edi, ebx
add edi, eax
mov [ebp-8], edi

imul edi, edi
mov eax, edi
mov edi, esi
mov [edi+4], eax

mov eax, eax
mov eax, [ebp-8]
imul eax, [esi+4]
cmp ebx, eax
je type_fruit_function_do_L2
mov eax, eax
mov eax, 5
push eax
push S2
call function_print
add esp, 8
jmp type_fruit_function_do_L1
type_fruit_function_do_L2:
mov eax, 3
xchg ebx, eax
xor edx, edx
idiv ebx
mov [ebp-4], eax
mov eax, eax
mov eax, [esi+4]
imul eax, [ebp+12]
add eax, [ebp-4]
mov [ebp-8], eax
imul eax, eax
mov [edi+4], eax
type_fruit_function_do_L1: 

mov eax, [ebp+12]
xor edx, edx
idiv dword [ebp-4]
xor edx, edx
idiv dword [ebp-8]

mov esp, ebp
pop ebp
ret


; Represents global function 'ass'
function_ass:
push ebp
mov ebp, esp
mov eax, [ebp+8]
mov ebx, [eax+4]
imul ebx, [eax]
sub ebx, [eax+4]
mov [eax], ebx

mov esp, ebp
pop ebp
ret


; Represents global function 'my'
function_my:
push ebp
mov ebp, esp
mov eax, [ebp+8]
mov ebx, [eax]
cmp ebx, [eax+4]
jge function_my_L1
mov ecx, [eax+4]
mov [eax], ecx
function_my_L1: 

mov esp, ebp
pop ebp
ret


; Represents global function 'sum'
function_sum:
push ebp
mov ebp, esp
mov eax, [ebp+8]
add eax, [ebp+12]

mov esp, ebp
pop ebp
ret


; Represents global function 'neg'
function_neg:
push ebp
mov ebp, esp
mov eax, [ebp+8]
sub eax, eax
sub eax, [ebp+8]

mov esp, ebp
pop ebp
ret


; Represents global function 'run'
function_run:
push ebp
mov ebp, esp
sub esp, 24
mov eax, 5
mov [ebp-8], eax

mov ebx, 7
mov [ebp-16], ebx

add eax, ebx
mov [ebp-20], eax

push 8
call function_allocate
add esp, 4
mov [ebp-24], eax

mov ebx, [eax]
mov [eax], ebx

push dword [ebp-8]
push eax
call type_fruit_function_do
add esp, 8

mov ebx, S3
mov [ebp-4], ebx

mov ecx, S4
mov [ebp-12], ecx

push dword [ebp-16]
push dword [ebp-8]
call function_sum
add esp, 8
push eax
push dword [ebp-20]
call function_neg
add esp, 4
pop ebx
cmp ebx, eax
jle function_run_L1
push dword [ebp-20]
call function_neg
add esp, 4
push eax
push dword [ebp-16]
push dword [ebp-8]
call function_sum
add esp, 8
push eax
call function_sum
add esp, 8
function_run_L1: 

function_run_L2:
mov eax, [ebp-24]
mov ebx, [eax]
cmp ebx, ebx
jne function_run_L3
push eax
push 8
call function_allocate
add esp, 4
pop ebx
mov edi, [ebp+8]
mov [edi], eax
jmp function_run_L2
function_run_L3:

function_run_L4:
mov eax, [ebp-24]
mov ebx, [eax]
cmp ebx, ebx
jne function_run_L5
push eax
mov ecx, 12
push ecx
push S5
call function_print
add esp, 8
pop ebx
jmp function_run_L4
function_run_L5:

mov esp, ebp
pop ebp
ret




section .data


section .bss
S1 db 'Tämä on tekstiä', 0
S2 db 'Jaaha', 0
S3 db 'banana', 0
S4 db 'apple', 0
S5 db 'Hello World!', 0

