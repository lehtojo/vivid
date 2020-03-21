function_f:
mov ecx, [ebp]
lea eax, [ecx+ecx]
mov edx, 1
imul edx, [ebp+4]
imul edx, 7
sub eax, edx
mov edx, ecx
imul edx, eax
imul edx, [ebp+4]
sub ecx, edx
imul eax, ecx
ret


function_run:
mov edi, 3
mov ecx, 2
imul ecx, edi
mov edx, edi
sub edx, 1
lea ebx, [edi+1]
imul edx, ebx
push edx
push edi
call function_f
add ecx, eax
push ecx
mov ecx, 1
add ecx, edi
push ecx
call function_f
mov ebx, eax
push ebx
push edi
call type_foo_constructor
mov edi, eax
push edi
call type_foo_function_sum
mov ebx, eax
lea ecx, [edi+edi]
mov [edi], ecx
lea ecx, [edi+edi]
cmp edi, ecx
jle function_run_L1
push edi
push edi
call function_f
jmp function_run_L0
function_run_L1:
cmp edi, 3
jle function_run_L2
lea ecx, [edi+edi]
push ecx
lea ecx, [edi+edi]
push ecx
call function_f
jmp function_run_L0
function_run_L2:
push 2
push 1
call function_f
push 3
push 2
call function_f
cmp eax, eax
jge function_run_L3
mov eax, 13434
ret
jmp function_run_L0
function_run_L3:
lea ecx, [edi+edi]
push ecx
lea ecx, [edi+edi]
push ecx
call function_f
function_run_L0:
mov ecx, [edi+ebx*4]
mov [edi], ecx
function_run_L4:
push 2
push edi
call function_f
add edi, eax
imul edi, 3
jmp function_run_L4
mov eax, 1
add eax, 2
mov ecx, 1
add ecx, 2
lea edx, [eax+ecx]
add eax, ecx
add eax, 1
add eax, 2
ret



































type_foo_constructor:
push 4
call allocate
mov ecx, [ebp+4]
imul ecx, ecx
mov [eax], ecx
mov ecx, [ebp+8]
add ecx, ecx
mov [eax+4], ecx


type_foo_function_sum:
mov ecx, [ebp+8]
mov eax, [ecx]
add eax, [ecx+4]
ret