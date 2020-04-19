function_run:
push ebx
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