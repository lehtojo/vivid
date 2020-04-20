function_run:
push ebx
push esi
push edi
xor ebx, ebx
cmp ebx, 10
jge function_run_L1
function_run_L0:
push S0
call function_print
add ebx, 1
cmp ebx, 10
jl function_run_L0
function_run_L1:
push S1
call type_string_constructor
xor ebx, ebx
mov esi, eax
cmp ebx, 10
jge function_run_L3
function_run_L2:
push esi
mov edi, esi
call function_prints
add ebx, 1
mov esi, edi
cmp ebx, 10
jl function_run_L2
function_run_L3:
pop edi
pop esi
pop ebx
ret

function_length_of:
xor eax, eax
mov ecx, [ebp]
function_length_of_L0:
mov edx, [ecx+eax*4]
cmp edx, 0
jne function_length_of_L1
ret
function_length_of_L1:
add eax, 1
jmp function_length_of_L0
ret

function_prints:
push ebx
mov ebx, [ebp]
push ebx
call type_string_function_length
push eax
push ebx
call type_string_function_data
push eax
call function_sys_print
pop ebx
ret

function_print:
push ebx
mov ebx, [ebp]
push ebx
call function_length_of
push eax
push ebx
call function_sys_print
pop ebx
ret

type_string_constructor:
push 4
call allocate
mov ecx, [ebp+4]
mov [eax], ecx
ret

type_string_function_data:
mov ecx, [ebp]
mov eax, [ecx]
ret

type_string_function_length:
push ebx
xor eax, eax
mov ecx, [ebp]
type_string_function_length_L0:
mov edx, [ecx]
mov ebx, [edx+eax*4]
cmp ebx, 0
jne type_string_function_length_L1
pop ebx
ret
type_string_function_length_L1:
add eax, 1
jmp type_string_function_length_L0
pop ebx
ret