section .text

global _start
_start:
call function_run

mov eax, 1
mov ebx, 0
int 80h


extern function_allocate


; Member function 'do' of type 'fruit'
type_fruit_function_do:
push ebp
mov ebp, esp
sub esp, 8
mov eax, [ebp+12]
mov ebx, 3
xor edx, edx
idiv ebx
mov [ebp-4], eax

mov esi, [ebp+8]
mov ecx, [esi+4]
imul ecx, [ebp+12]
add ecx, eax
mov [ebp-8], ecx

imul ecx, ecx
mov edi, esi
mov [edi+4], ecx

mov edx, [ebp-8]
imul edx, ecx
mov ebp, [ebp+12]
cmp ebp, edx
je type_fruit_function_do_L1
push dword [ebp-8]
push dword [ebp+8]
call type_fruit_function_do
add esp, 8
mov ebx, 9
mov ecx, 9
add ebx, ecx
mov edx, [ebp-8]
imul edx, ebx
mov ebx, [ebp-4]
add ebx, edx
add ebx, eax
push ebx
push dword [ebp+8]
call type_fruit_function_do
add esp, 8
mov ebx, [ebp-8]
mov esi, [ebp+8]
imul ebx, [esi+4]
mov ecx, [ebp+12]
cmp ecx, ebx
je type_fruit_function_do_L3
push dword [ebp-8]
push dword [ebp+8]
call type_fruit_function_do
add esp, 8
mov ebx, 9
mov ecx, 9
add ebx, ecx
mov edx, [ebp-8]
imul edx, ebx
mov ebx, [ebp-4]
add ebx, edx
add ebx, eax
push ebx
push dword [ebp+8]
call type_fruit_function_do
add esp, 8
mov ebx, [ebp-8]
mov esi, [ebp+8]
imul ebx, [esi+4]
mov ecx, [ebp+12]
cmp ecx, ebx
je type_fruit_function_do_L5
push edi
push dword [ebp+8]
call type_fruit_function_do
add esp, 8
mov ebx, 9
mov ecx, 9
add ebx, ecx
mov edx, [ebp-8]
imul edx, ebx
mov ebx, [ebp-4]
add ebx, edx
add ebx, eax
push ebx
push dword [ebp+8]
call type_fruit_function_do
add esp, 8
jmp type_fruit_function_do_L4
type_fruit_function_do_L5:
mov ebx, 0
mov edx, 9
add ebx, edx
mov edi, [ebp-8]
cmp edi, ebx
jle type_fruit_function_do_L4
mov ebx, 0
push ebx
push dword [ebp+8]
call type_fruit_function_do
add esp, 8
type_fruit_function_do_L4: 
jmp type_fruit_function_do_L2
type_fruit_function_do_L3:
mov ebx, 3
mov edx, eax
mov eax, ecx
xor edx, edx
idiv ebx
mov [ebp-4], eax
mov edi, [esi+4]
imul edi, ecx
add edi, eax
mov [ebp-8], edi
imul edi, edi
mov ebp, edi
mov edi, esi
mov [edi+4], ebp
type_fruit_function_do_L2: 
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


; Represents global function 'run'
function_run:
push ebp
mov ebp, esp
sub esp, 7
mov eax, 5
mov [ebp-4], eax

mov ebx, 7
mov [ebp-5], ebx

add eax, ebx
mov [ebp-6], eax

push 8
call function_allocate
add esp, 4
push eax
push eax
call type_fruit_constructor
add esp, 4
pop ebx
mov [ebp-7], ebx

mov ecx, [ebx]
mov [ebx], ecx

function_run_L1:
cmp ecx, ecx
jge function_run_L2
push ebx
call function_ass
add esp, 4
jmp function_run_L1
function_run_L2:

mov esp, ebp
pop ebp
ret




section .data

